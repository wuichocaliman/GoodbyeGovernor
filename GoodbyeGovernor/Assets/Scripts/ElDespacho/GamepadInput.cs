using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ElDespacho
{
    /// <summary>
    /// Mando (Xbox, Steam Deck, DualSense…) junto al ratón, sin sustituirlo. Esquema híbrido:
    /// - Menús, mapa, visitas y mesa: navegación nativa de uGUI con cruceta / stick izquierdo, A pulsa, B vuelve.
    ///   Cada vez que cambia la pantalla se elige un botón válido (nunca una decisión irreversible) y un marco
    ///   dorado muestra dónde está el foco.
    /// - Mesa: R2 saca el expediente de la bandeja y lo lleva al vade según la presión; con el expediente abierto,
    ///   L2 baja el sello DENEGADO y R2 el APROBADO. Solo cuenta al llegar al fondo; soltar antes cancela.
    ///   LB / RB manteniendo pulsado hacen lo mismo para mandos con gatillos digitales.
    /// - Stick derecho: desplaza el texto largo visible (nota del asesor, La Gaceta, diálogo).
    /// En cuanto se mueve el ratón o se pulsa el teclado, el mando deja de mandar: se cancela su acción,
    /// se paran los motores y se quita la selección (el juego con ratón se ve exactamente como antes).
    /// La vibración está en Kit.cs (Haptics); aquí solo se aplica cada frame y se para por seguridad.
    /// </summary>
    public sealed class GamepadInput : MonoBehaviour
    {
        public static bool UsingGamepad { get; private set; }
        public static Gamepad ActivePad { get; private set; }

        // ── Ajustes de la mecánica analógica ──
        const float TriggerDeadZone = 0.08f;  // por debajo, el gatillo cuenta como suelto
        const float RearmBelow = 0.15f;       // tras una acción hay que soltar hasta aquí antes de la siguiente
        const float CommitAt = 0.95f;         // «a fondo» (algunos gatillos gastados no llegan a 1)
        const float MinTriggerTime = 0.3f;    // un gatillo digital (0/1) tarda esto en confirmar: nada se sella de golpe
        const float ShoulderHoldTime = 0.35f; // LB / RB manteniendo pulsado
        const float ScrollSpeed = 900f;       // unidades de UI por segundo con el stick derecho

        DespachoGame game;
        DeskMenus menus;

        enum Act { None, Document, StampNo, StampYes }
        Act act;
        float held;
        bool needRearm;
        DeskDocument actDoc;
        StampTool actStamp;

        RectTransform focusLayer;
        readonly Image[] frame = new Image[4];
        readonly Dictionary<string, string> lastFocus = new Dictionary<string, string>();
        string lastTutorialStep = "";
        float navRepeatAt;
        bool navHeld;

        // Botones que se prefieren al llegar a una pantalla (por texto y por nombre)
        static readonly string[] PreferredLabels = { "CONTINUAR", "VOLVER AL DESPACHO", "ENTRAR AL DESPACHO", "ASUMIR EL CARGO", "DOBLAR EL PERIÓDICO", "VOLVER" };
        static readonly string[] PreferredNames = { "AssumeButton", "GazetteClose", "YearEndButton", "MapButton", "Gazette" };
        // Nunca se seleccionan solos: un A pulsado por inercia no puede tomar una decisión irreversible
        static readonly string[] NeverDefault = { "ALIANZA", "CORTAR", "ATACAR", "SANCION", "SANCIÓN", "SALIR", "BORRAR", "ELIMINAR", "NUEVA PARTIDA", "RESTABLECER", "SOBRESCRIBIR" };

        public void Initialize(DespachoGame owner, DeskMenus deskMenus)
        {
            game = owner;
            menus = deskMenus;
            BuildFocusLayer();
            InputSystem.onDeviceChange += OnDeviceChange;
            Application.quitting += Haptics.Stop;
        }

        // ───────────── Seguridad: los motores nunca se quedan encendidos ─────────────

        void OnDisable() { CancelAction(); Haptics.Stop(); }
        void OnDestroy()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
            Application.quitting -= Haptics.Stop;
            Haptics.Stop();
            UsingGamepad = false;
            ActivePad = null;
        }
        void OnApplicationFocus(bool focused) { if (!focused) { CancelAction(); Haptics.Stop(); } }
        void OnApplicationPause(bool paused) { if (paused) { CancelAction(); Haptics.Stop(); } }
        void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (!(device is Gamepad)) return;
            if (change != InputDeviceChange.Removed && change != InputDeviceChange.Disconnected) return;
            CancelAction();
            Haptics.Stop();
            if (ActivePad == device) { ActivePad = null; UsingGamepad = false; }
        }

        // ───────────── Bucle ─────────────

        void Update()
        {
            if (game == null) return;
            TrackDevice();
            if (!UsingGamepad || ActivePad == null) return;
            var es = EventSystem.current;
            Buttons(es);
            DeskActions();
            Scroll(es);
        }

        void LateUpdate()
        {
            if (game == null) return;
            var es = EventSystem.current;
            ManageFocus(es);
            DrawFocusFrame(es);
            Haptics.Tick(); // después de toda la lógica del frame (incluida una pantalla reconstruida)
        }

        // ───────────── Último dispositivo usado ─────────────

        void TrackDevice()
        {
            foreach (var pad in Gamepad.all)
                if (Touched(pad)) { UsingGamepad = true; ActivePad = pad; return; }

            var m = Mouse.current;
            var k = Keyboard.current;
            bool mouse = m != null && (m.delta.ReadValue().sqrMagnitude > 9f || m.leftButton.wasPressedThisFrame ||
                                       m.rightButton.wasPressedThisFrame || m.scroll.ReadValue().sqrMagnitude > 0.01f);
            bool keys = k != null && k.anyKey.wasPressedThisFrame;
            if (UsingGamepad && (mouse || keys))
            {
                UsingGamepad = false;
                CancelAction();
                Haptics.Stop();
                if (mouse) EventSystem.current?.SetSelectedGameObject(null); // con ratón no se ve ningún foco, como antes
            }
        }

        static bool Touched(Gamepad p) =>
            p.leftStick.ReadValue().sqrMagnitude > 0.12f || p.rightStick.ReadValue().sqrMagnitude > 0.12f ||
            p.dpad.ReadValue().sqrMagnitude > 0.1f || p.leftTrigger.ReadValue() > 0.15f || p.rightTrigger.ReadValue() > 0.15f ||
            p.buttonSouth.wasPressedThisFrame || p.buttonEast.wasPressedThisFrame || p.buttonWest.wasPressedThisFrame ||
            p.buttonNorth.wasPressedThisFrame || p.leftShoulder.wasPressedThisFrame || p.rightShoulder.wasPressedThisFrame ||
            p.startButton.wasPressedThisFrame || p.selectButton.wasPressedThisFrame;

        // ───────────── Botones: B (volver) y puente con el modo teclado accesible ─────────────

        void Buttons(EventSystem es)
        {
            var p = ActivePad;
            if (p.buttonEast.wasPressedThisFrame)
            {
                if (menus.IsVisible) menus.RequestBack();
                else
                {
                    var close = GazetteClose();
                    if (close != null && close.IsInteractable()) close.onClick.Invoke();
                    else AccessController.CancelOrBack();
                }
            }

            // Con el modo teclado de Accesibilidad (o sus páginas) la navegación nativa está apagada:
            // el mando mueve el mismo foco que Tab / flechas y A equivale a Intro.
            if (es != null && !es.sendNavigationEvents && AccessController.Instance != null)
            {
                float y = p.dpad.ReadValue().y + p.leftStick.ReadValue().y;
                int dir = y > 0.5f ? -1 : y < -0.5f ? 1 : 0;
                if (dir != 0)
                {
                    if (Time.unscaledTime >= navRepeatAt)
                    {
                        AccessController.Instance.MoveFocus(dir);
                        navRepeatAt = Time.unscaledTime + (navHeld ? 0.12f : 0.35f);
                        navHeld = true;
                    }
                }
                else { navHeld = false; navRepeatAt = 0; }
                if (p.buttonSouth.wasPressedThisFrame) AccessController.Instance.Activate();
            }
        }

        // ───────────── Mesa: gatillos analógicos ─────────────

        void DeskActions()
        {
            var p = ActivePad;
            if (menus.IsVisible || DeskPreferences.Paused || game.ScreenName != "Despacho" || GazetteOpen())
            {
                if (act != Act.None) { CancelAction(); needRearm = true; }
                return;
            }
            float l2 = p.leftTrigger.ReadValue(), r2 = p.rightTrigger.ReadValue();
            bool lb = p.leftShoulder.isPressed, rb = p.rightShoulder.isPressed;

            // Anti-repetición: tras abrir o sellar hay que soltar todo antes de la siguiente acción
            if (needRearm)
            {
                if (l2 < RearmBelow && r2 < RearmBelow && !lb && !rb) needRearm = false;
                else return;
            }

            if (act == Act.None)
            {
                var doc = game.GetComponentInChildren<DeskDocument>();
                if (doc == null) return;
                bool left = l2 > TriggerDeadZone || lb, right = r2 > TriggerDeadZone || rb;
                if (doc.GuidedAvailable)
                {
                    if (right && doc.BeginGuided()) { act = Act.Document; actDoc = doc; held = 0; }
                    return;
                }
                if (!doc.CanDecide || left == right) return; // los dos a la vez: no se elige ninguno
                var stamp = game.GetComponentsInChildren<StampTool>().FirstOrDefault(s => s.name == (right ? "StampYes" : "StampNo"));
                if (stamp != null && stamp.BeginGuided()) { act = right ? Act.StampYes : Act.StampNo; actStamp = stamp; held = 0; }
                return;
            }

            bool useRight = act != Act.StampNo;
            float trigger = useRight ? r2 : l2;
            bool shoulder = useRight ? rb : lb;
            if (trigger < TriggerDeadZone && !shoulder)
            {
                CancelAction();
                Haptics.Released();
                return;
            }
            held += DeskPreferences.GameDelta;
            // Analógico: sigue la presión, pero con velocidad máxima (un gatillo digital no confirma de golpe)
            float t = Mathf.Max(Mathf.Min(trigger, held / MinTriggerTime), shoulder ? held / ShoulderHoldTime : 0f);
            t = Mathf.Clamp01(t);

            if (act == Act.Document)
            {
                if (actDoc == null || !actDoc.IsGuided) { ResetAction(); return; }
                actDoc.SetGuidedProgress(t);
            }
            else
            {
                if (actStamp == null || !actStamp.IsGuided) { ResetAction(); return; }
                actStamp.SetGuidedProgress(t);
            }
            Haptics.Tension(t);

            if (t >= CommitAt)
            {
                var doc = actDoc;
                var stamp = actStamp;
                ResetAction();
                needRearm = true;
                if (doc != null) doc.CommitGuided();   // se abre igual que al soltarlo con el ratón
                if (stamp != null) stamp.CommitGuided(); // sella: puede reconstruir la pantalla entera
            }
        }

        void CancelAction()
        {
            if (act == Act.Document && actDoc != null) actDoc.CancelGuided();
            if ((act == Act.StampNo || act == Act.StampYes) && actStamp != null) actStamp.CancelGuided();
            ResetAction();
        }

        void ResetAction() { act = Act.None; actDoc = null; actStamp = null; held = 0; }

        // ───────────── Stick derecho: textos largos ─────────────

        void Scroll(EventSystem es)
        {
            float y = ActivePad.rightStick.ReadValue().y;
            if (Mathf.Abs(y) < 0.2f) return;
            var sr = FindScroll(es);
            if (sr == null || sr.content == null) return;
            var view = sr.viewport != null ? sr.viewport : (RectTransform)sr.transform;
            float overflow = sr.content.rect.height - view.rect.height;
            if (overflow <= 1f) return;
            sr.verticalNormalizedPosition = Mathf.Clamp01(sr.verticalNormalizedPosition + y * ScrollSpeed * Time.unscaledDeltaTime / overflow);
        }

        ScrollRect FindScroll(EventSystem es)
        {
            var sel = es != null ? es.currentSelectedGameObject : null;
            var own = sel != null ? sel.GetComponentInParent<ScrollRect>() : null;
            if (own != null && own.isActiveAndEnabled) return own;
            var scope = Scope() ?? GameCanvas;
            if (scope == null) return null;
            ScrollRect best = null;
            float bestOverflow = 1f;
            foreach (var s in scope.GetComponentsInChildren<ScrollRect>())
            {
                if (!s.isActiveAndEnabled || s.content == null) continue;
                var view = s.viewport != null ? s.viewport : (RectTransform)s.transform;
                float o = s.content.rect.height - view.rect.height;
                if (o > bestOverflow) { best = s; bestOverflow = o; }
            }
            return best;
        }

        // ───────────── Foco: nunca sin selección ─────────────

        Transform GameCanvas => game.transform.Find("Canvas");
        Transform MenusRoot => menus.transform.Find("PresidentialMenus");
        Transform GazetteOverlay
        {
            get
            {
                var overlay = GameCanvas != null ? GameCanvas.Find("Stage/GazetteOverlay") : null;
                return overlay != null && overlay.gameObject.activeInHierarchy ? overlay : null;
            }
        }
        bool GazetteOpen() => GazetteOverlay != null;
        Button GazetteClose()
        {
            var overlay = GazetteOverlay;
            var close = overlay != null ? overlay.GetComponentsInChildren<Button>().FirstOrDefault(b => b.name == "GazetteClose") : null;
            return close;
        }

        /// <summary>Dónde tiene que estar el foco: el menú abierto, La Gaceta abierta o (null) el juego.</summary>
        Transform Scope()
        {
            if (menus.IsVisible) return MenusRoot;
            return GazetteOverlay;
        }

        string ScreenKey(Transform scope) =>
            menus.IsVisible ? "menu:" + menus.PageName : scope != null ? "gaceta" : "juego:" + game.ScreenName;

        void ManageFocus(EventSystem es)
        {
            if (es == null || !UsingGamepad || !es.sendNavigationEvents) return; // el modo teclado accesible lleva su propio foco
            var scope = Scope();

            // Nota nueva del tutorial con SIGUIENTE disponible: el foco salta a ella
            var tutorial = TutorialManager.Instance;
            string step = tutorial != null && tutorial.IsActive ? tutorial.ActiveId : "";
            if (step != lastTutorialStep)
            {
                lastTutorialStep = step;
                var next = step != "" && scope == null ? TutorialNext() : null;
                if (Usable(next)) { es.SetSelectedGameObject(next.gameObject); return; }
            }

            string key = ScreenKey(scope);
            var current = es.currentSelectedGameObject != null ? es.currentSelectedGameObject.GetComponent<Selectable>() : null;
            if (Usable(current) && (scope == null || current.transform.IsChildOf(scope)))
            {
                lastFocus[key] = Id(current);
                return;
            }
            var pick = PickDefault(scope, key);
            es.SetSelectedGameObject(pick != null ? pick.gameObject : null);
        }

        Selectable PickDefault(Transform scope, string key)
        {
            var root = scope ?? GameCanvas;
            if (root == null) return null;
            var all = root.GetComponentsInChildren<Selectable>()
                .Where(Usable)
                .Where(s => !NeverDefault.Any(w => Label(s).Contains(w)))
                .ToList();
            if (all.Count == 0) return scope == null && Usable(TutorialNext()) ? TutorialNext() : null;

            // En el juego, CONTINUAR (tras sellar o tras un resultado) va primero
            if (scope == null)
            {
                var cont = all.FirstOrDefault(s => Label(s) == "CONTINUAR");
                if (cont != null) return cont;
            }
            // Si se vuelve a una pantalla, el último botón usado en ella
            if (lastFocus.TryGetValue(key, out var id))
            {
                var same = all.FirstOrDefault(s => Id(s) == id);
                if (same != null) return same;
            }
            foreach (var w in PreferredLabels)
            {
                var m = all.FirstOrDefault(s => Label(s).StartsWith(w));
                if (m != null) return m;
            }
            foreach (var n in PreferredNames)
            {
                var m = all.FirstOrDefault(s => s.name == n);
                if (m != null) return m;
            }
            // Si no, el de más arriba a la izquierda
            return all.OrderByDescending(s => Mathf.Round(Corner(s, top: true) / 40f)).ThenBy(s => Corner(s, top: false)).First();
        }

        Selectable TutorialNext()
        {
            var tutorial = TutorialManager.Instance;
            var canvas = tutorial != null ? tutorial.transform.Find("TutorialCanvas") : null;
            return canvas != null ? canvas.GetComponentsInChildren<Button>().FirstOrDefault(b => Label(b) == "SIGUIENTE") : null;
        }

        static bool Usable(Selectable s)
        {
            if (s == null || !s.isActiveAndEnabled || !s.IsInteractable()) return false;
            var canvas = s.GetComponentInParent<Canvas>();
            if (canvas == null || !canvas.isActiveAndEnabled) return false;
            foreach (var g in s.GetComponentsInParent<CanvasGroup>())
                if (!g.blocksRaycasts || g.alpha < 0.05f) return false;
            return true;
        }

        static string Label(Selectable s)
        {
            var t = s != null ? s.GetComponentInChildren<Text>() : null;
            return t == null ? "" : Regex.Replace(t.text, "<.*?>", "").Replace('\n', ' ').Trim().ToUpperInvariant();
        }

        static string Id(Selectable s) => s.name + "|" + Label(s);

        static float Corner(Selectable s, bool top)
        {
            var c = new Vector3[4];
            ((RectTransform)s.transform).GetWorldCorners(c);
            return top ? c[1].y : c[0].x;
        }

        // ───────────── Marco dorado alrededor del foco (solo con mando) ─────────────

        void BuildFocusLayer()
        {
            var go = new GameObject("GamepadFocus", typeof(RectTransform), typeof(Canvas));
            go.transform.SetParent(transform, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 650; // sobre el juego, menús y tutorial; debajo de Accesibilidad (700)
            focusLayer = (RectTransform)go.transform;
            for (int i = 0; i < 4; i++)
            {
                frame[i] = Kit.Panel(focusLayer, Kit.Hex("#E8C35A"), Vector2.zero, Vector2.zero, default, default, "FocusEdge");
                frame[i].raycastTarget = false;
            }
        }

        void DrawFocusFrame(EventSystem es)
        {
            var sel = es != null && es.currentSelectedGameObject != null ? es.currentSelectedGameObject.GetComponent<Selectable>() : null;
            bool show = UsingGamepad && es != null && es.sendNavigationEvents && Usable(sel);
            focusLayer.gameObject.SetActive(show);
            if (!show) return;
            var c = new Vector3[4];
            ((RectTransform)sel.transform).GetWorldCorners(c); // en un Canvas Overlay, mundo = píxeles de pantalla
            float xMin = c.Min(v => v.x) - 6, xMax = c.Max(v => v.x) + 6, yMin = c.Min(v => v.y) - 6, yMax = c.Max(v => v.y) + 6;
            float k = Mathf.Max(3f, Screen.height / 270f); // grosor proporcional a la pantalla
            Place(frame[0], xMin - k, yMax, xMax + k, yMax + k);
            Place(frame[1], xMin - k, yMin - k, xMax + k, yMin);
            Place(frame[2], xMin - k, yMin, xMin, yMax);
            Place(frame[3], xMax, yMin, xMax + k, yMax);
            float pulse = DeskPreferences.Current.reduceMotion || AccessPreferences.Current.reduceFlashes
                ? 1f : 0.75f + 0.25f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 3f));
            foreach (var f in frame) f.color = Kit.WithAlpha(Kit.Hex("#E8C35A"), pulse);
        }

        void Place(Image img, float x0, float y0, float x1, float y1)
        {
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = Vector2.zero;
            var scale = focusLayer.lossyScale.x > 0 ? focusLayer.lossyScale.x : 1f;
            rt.anchoredPosition = new Vector2(x0, y0) / scale;
            rt.sizeDelta = new Vector2(x1 - x0, y1 - y0) / scale;
        }
    }
}
