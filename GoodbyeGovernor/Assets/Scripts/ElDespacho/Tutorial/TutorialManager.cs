using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElDespacho
{
    /// <summary>
    /// Tutorial paso a paso para la primera partida.
    ///
    /// - Se crea solo al pulsar Play (igual que DespachoGame) y construye su propio Canvas
    ///   por código, con sortingOrder 200: DespachoGame.Clear() nunca lo toca.
    /// - Oscurece la pantalla con 4 paneles alrededor de un "agujero" (el foco). Los paneles
    ///   bloquean los clics; dentro del agujero el jugador puede usar el juego con normalidad.
    /// - DespachoGame avisa de lo que hace el jugador con TryAdvance(...); el paso solo avanza
    ///   si coincide con su advanceCondition.
    /// - Al terminar (o al pulsar SALTAR TUTORIAL) guarda en PlayerPrefs que ya se vio.
    ///
    /// No conoce las reglas del juego: solo dibuja el overlay y escucha avisos.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        // ── Configuración en Inspector ─────────────────────────────────
        [Header("Pasos del tutorial (arrastrar en orden)")]
        [Tooltip("Vacío = se cargan los assets de Resources/Tutorial por orden de nombre; " +
                 "si tampoco hay, se usan los pasos por defecto de TutorialStepData.")]
        [SerializeField] TutorialStepData[] steps;

        [Header("Opciones")]
        [SerializeField] string playerPrefsKey = DefaultPrefsKey;
        [Tooltip("En el Editor, muestra el tutorial aunque ya se haya completado.")]
        [SerializeField] bool forceShowInEditor = false;

        internal const string DefaultPrefsKey = "tutorial_done_v1";
        internal const string ResourcesFolder = "Tutorial";
        const int SortingOrder = 200;
        const float DimAlpha = 0.72f;
        const float FrameThickness = 4f;
        const float HudHeight = 100f;
        static readonly Vector2 BubbleSize = new Vector2(500f, 210f);

        int current = -1;
        RectTransform overlay;
        Transform gameCanvas;
        RectTransform target;
        readonly Image[] blockers = new Image[4]; // arriba, abajo, izquierda, derecha del foco
        readonly Image[] frame = new Image[4];    // marco dorado que late alrededor del foco
        RectTransform bubble;
        Text header, body, hint;
        Coroutine pulse;

        bool suspendedForSession;
        public bool IsActive => current >= 0;
        public void SuppressLoadedSession() { suspendedForSession = true; SetMenuSuspended(true); }
        public void PrepareNewSession() { suspendedForSession = false; if (IsActive) ShowStep(0); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (!DespachoGame.AutoBoot) return;
            if (FindAnyObjectByType<TutorialManager>() != null) return;
            // Ojo: con objetos de Unity no se usa ?. ni ?? (no respetan el "null" de Unity).
            var game = FindAnyObjectByType<DespachoGame>();
            var host = game != null ? game.gameObject : new GameObject("TutorialManager");
            host.AddComponent<TutorialManager>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            DeskDocument.Grabbed -= OnDocumentGrabbed;
            if (overlay != null) Destroy(overlay.gameObject);
        }

        public void SetMenuSuspended(bool suspended)
        {
            if (overlay != null) overlay.GetComponent<Canvas>().enabled = !suspended && !suspendedForSession;
        }

        IEnumerator Start()
        {
            bool force = forceShowInEditor && Application.isEditor;
            if (!force && PlayerPrefs.GetInt(playerPrefsKey, 0) == 1) { Destroy(this); yield break; }

            // DespachoGame puede arrancar antes o después que nosotros: esperamos a su Canvas.
            while (gameCanvas == null)
            {
                var game = FindAnyObjectByType<DespachoGame>();
                var canvas = game != null ? game.GetComponentInChildren<Canvas>() : null;
                if (canvas != null) gameCanvas = canvas.transform;
                else yield return null;
            }

            // Wait for the introduction, not the new main menu. Loading a save never starts the tutorial.
            while (true)
            {
                var game = FindAnyObjectByType<DespachoGame>();
                if (game != null && game.InIntroduction && DeskPreferences.Current.tutorial) break;
                yield return null;
            }
            if (steps == null || steps.Length == 0) steps = LoadSteps();
            if (steps.Length == 0) { Destroy(this); yield break; }

            BuildOverlay();
            DeskDocument.Grabbed += OnDocumentGrabbed;
            ShowStep(0);
        }

        static TutorialStepData[] LoadSteps()
        {
            var assets = Resources.LoadAll<TutorialStepData>(ResourcesFolder).OrderBy(s => s.name).ToArray();
            return assets.Length > 0 ? assets : TutorialStepData.CreateDefaults();
        }

        // ───────────── API pública ─────────────

        /// <summary>DespachoGame avisa de una acción del jugador. Si es la que espera el paso actual, avanza.</summary>
        public void TryAdvance(TutorialAdvanceCondition trigger)
        {
            if (!IsActive || suspendedForSession || !DeskPreferences.Current.tutorial || steps[current].advanceCondition != trigger) return;
            Sfx.Play(Sfx.Paper, 0.5f);
            ShowStep(current + 1);
        }

        /// <summary>Termina el tutorial (último paso o SALTAR) y no vuelve a mostrarse.</summary>
        public void CompleteTutorial()
        {
            PlayerPrefs.SetInt(playerPrefsKey, 1);
            PlayerPrefs.Save();
            current = -1;
            StopAllCoroutines();
            Destroy(this); // OnDestroy borra el overlay y se desuscribe
        }

        void OnDocumentGrabbed() => TryAdvance(TutorialAdvanceCondition.GrabDocument);

        // ───────────── Construcción del overlay (una sola vez) ─────────────

        void BuildOverlay()
        {
            var go = new GameObject("TutorialCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            overlay = (RectTransform)go.transform;

            for (int i = 0; i < blockers.Length; i++)
            {
                var b = Kit.Panel(overlay, new Color(0, 0, 0, DimAlpha), Vector2.zero, Vector2.zero, default, default, "Blocker" + i);
                b.raycastTarget = true;
                AddClick(b.gameObject);
                blockers[i] = b;
            }
            for (int i = 0; i < frame.Length; i++)
                frame[i] = Kit.Panel(overlay, Pal.Gold, Vector2.zero, Vector2.zero, default, default, "Frame" + i);

            var paper = Kit.Panel(overlay, Pal.Paper, Vector2.zero, Vector2.zero, default, default, "Bubble");
            paper.raycastTarget = true; // la burbuja no deja pasar clics al juego
            AddClick(paper.gameObject);
            var outline = paper.gameObject.AddComponent<Outline>();
            outline.effectColor = Pal.Gold;
            outline.effectDistance = new Vector2(3, -3);
            bubble = paper.rectTransform;
            bubble.pivot = Vector2.zero;
            bubble.sizeDelta = BubbleSize;

            header = Kit.Label(bubble, "", 15, Pal.Red, TextAnchor.UpperLeft, Vector2.zero, Vector2.one,
                new Vector2(22, 0), new Vector2(-22, -12), FontStyle.Bold);
            body = Kit.Label(bubble, "", 23, Pal.Ink, TextAnchor.UpperLeft, Vector2.zero, Vector2.one,
                new Vector2(22, 58), new Vector2(-22, -38), FontStyle.Normal, true);
            hint = Kit.Label(bubble, "", 15, Pal.InkSoft, TextAnchor.LowerLeft, Vector2.zero, Vector2.one,
                new Vector2(22, 16), new Vector2(-220, -12), FontStyle.Italic);
            Kit.MakeButton(bubble, "SALTAR TUTORIAL", 15, Pal.WoodMid, Pal.Cream, CompleteTutorial,
                new Vector2(1, 0), new Vector2(1, 0), new Vector2(-196, 12), new Vector2(-16, 48));
        }

        void AddClick(GameObject go)
        {
            var button = go.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => TryAdvance(TutorialAdvanceCondition.ClickAnywhere));
        }

        // ───────────── Pasos ─────────────

        void ShowStep(int index)
        {
            if (index >= steps.Length) { CompleteTutorial(); return; }
            current = index;
            target = null;
            var step = steps[index];
            header.text = $"TUTORIAL  ·  PASO {index + 1} DE {steps.Length}";
            body.text = step.instructionText;
            hint.text = step.advanceCondition == TutorialAdvanceCondition.ClickAnywhere
                ? "Haz clic en cualquier sitio para continuar." : "";
            if (pulse != null) StopCoroutine(pulse);
            // DespachoGame suele redibujar la pantalla en este mismo frame (Clear + Show...):
            // esperamos uno para que el objeto a destacar ya exista.
            pulse = StartCoroutine(StepRoutine());
        }

        IEnumerator StepRoutine()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            Layout();
            float t = 0f, relayout = 0f;
            while (true)
            {
                t += Time.unscaledDeltaTime;
                relayout += Time.unscaledDeltaTime;
                // Pulso del marco del foco
                float alpha = DeskPreferences.Current.reduceMotion ? 1f : 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(t * 3f));
                foreach (var f in frame) f.color = Kit.WithAlpha(Pal.Gold, alpha);
                // El juego destruye y recrea su interfaz a menudo: si el objeto destacado
                // desaparece o se mueve, se vuelve a buscar y a colocar el foco.
                if (relayout >= 0.15f)
                {
                    relayout = 0f;
                    if (target == null || target.hasChanged)
                    {
                        if (target != null) target.hasChanged = false;
                        Layout();
                    }
                }
                yield return null;
            }
        }

        // ───────────── Colocación del foco, los bloqueadores y la burbuja ─────────────
        // Todo en píxeles de referencia, con origen en la esquina inferior izquierda.

        void Layout()
        {
            if (!IsActive) return;
            var step = steps[current];
            var size = overlay.rect.size;
            bool hasHole = TryGetHole(step, size, out var hole);

            if (!hasHole)
            {
                Place(blockers[0], new Rect(Vector2.zero, size));
                for (int i = 1; i < blockers.Length; i++) Place(blockers[i], Rect.zero);
                foreach (var f in frame) Place(f, Rect.zero);
            }
            else
            {
                Place(blockers[0], Rect.MinMaxRect(0, hole.yMax, size.x, size.y));           // arriba
                Place(blockers[1], Rect.MinMaxRect(0, 0, size.x, hole.yMin));                // abajo
                Place(blockers[2], Rect.MinMaxRect(0, hole.yMin, hole.xMin, hole.yMax));     // izquierda
                Place(blockers[3], Rect.MinMaxRect(hole.xMax, hole.yMin, size.x, hole.yMax)); // derecha
                const float k = FrameThickness;
                Place(frame[0], Rect.MinMaxRect(hole.xMin - k, hole.yMax, hole.xMax + k, hole.yMax + k));
                Place(frame[1], Rect.MinMaxRect(hole.xMin - k, hole.yMin - k, hole.xMax + k, hole.yMin));
                Place(frame[2], Rect.MinMaxRect(hole.xMin - k, hole.yMin, hole.xMin, hole.yMax));
                Place(frame[3], Rect.MinMaxRect(hole.xMax, hole.yMin, hole.xMax + k, hole.yMax));
            }

            // Si un paso pide interactuar pero no encuentra qué destacar, no bloqueamos nada:
            // mejor un tutorial sin foco que un jugador atascado.
            bool block = hasHole || step.advanceCondition == TutorialAdvanceCondition.ClickAnywhere;
            foreach (var b in blockers) b.raycastTarget = block;

            PlaceBubble(hasHole, hole, size);
        }

        bool TryGetHole(TutorialStepData step, Vector2 size, out Rect hole)
        {
            hole = default;
            if (!string.IsNullOrEmpty(step.targetName))
            {
                if (target == null || !target.gameObject.activeInHierarchy) target = FindTarget(step.targetName);
                if (target != null)
                {
                    var corners = new Vector3[4];
                    target.GetWorldCorners(corners); // en un Canvas Overlay, mundo = píxeles de pantalla
                    var min = new Vector2(float.MaxValue, float.MaxValue);
                    var max = new Vector2(float.MinValue, float.MinValue);
                    foreach (var c in corners)
                    {
                        var p = ToOverlay(c);
                        min = Vector2.Min(min, p);
                        max = Vector2.Max(max, p);
                    }
                    float pad = step.targetPadding;
                    hole = Rect.MinMaxRect(min.x - pad, min.y - pad, max.x + pad, max.y + pad);
                    hole = Clamp(hole, size);
                    return true;
                }
            }
            if (step.spotlightAnchor.x >= 0f && step.spotlightAnchor.y >= 0f)
            {
                var center = Vector2.Scale(step.spotlightAnchor, size);
                float r = step.spotlightRadius;
                hole = Clamp(new Rect(center.x - r, center.y - r, 2 * r, 2 * r), size);
                return true;
            }
            return false;
        }

        RectTransform FindTarget(string name)
        {
            if (gameCanvas == null) return null;
            foreach (var rt in gameCanvas.GetComponentsInChildren<RectTransform>())
                if (rt.name == name) return rt;
            return null;
        }

        Vector2 ToOverlay(Vector3 screenPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay, screenPoint, null, out var local);
            return local + overlay.rect.size * 0.5f; // de centro a esquina inferior izquierda
        }

        /// <summary>Burbuja al lado del foco donde quepa (derecha, izquierda, abajo, arriba), sin tapar el HUD.</summary>
        void PlaceBubble(bool hasHole, Rect hole, Vector2 size)
        {
            var b = BubbleSize;
            const float gap = 24f, margin = 16f;
            float maxY = size.y - HudHeight - b.y;
            Vector2 pos;
            if (!hasHole)
                pos = (size - b) * 0.5f;
            else
            {
                float midY = Mathf.Clamp(hole.center.y - b.y / 2, margin, maxY);
                float midX = Mathf.Clamp(hole.center.x - b.x / 2, margin, size.x - b.x - margin);
                if (size.x - hole.xMax >= b.x + gap + margin) pos = new Vector2(hole.xMax + gap, midY);
                else if (hole.xMin >= b.x + gap + margin) pos = new Vector2(hole.xMin - gap - b.x, midY);
                else if (hole.yMin >= b.y + gap + margin) pos = new Vector2(midX, hole.yMin - gap - b.y);
                else if (size.y - HudHeight - hole.yMax >= b.y + gap) pos = new Vector2(midX, hole.yMax + gap);
                else pos = new Vector2(midX, margin); // no cabe en ningún lado: abajo, encima de lo que haya
            }
            bubble.anchorMin = bubble.anchorMax = Vector2.zero;
            bubble.anchoredPosition = pos;
            bubble.SetAsLastSibling();
        }

        static void Place(Graphic g, Rect r)
        {
            var rt = g.rectTransform;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = Vector2.zero;
            rt.anchoredPosition = r.position;
            rt.sizeDelta = new Vector2(Mathf.Max(0, r.width), Mathf.Max(0, r.height));
        }

        static Rect Clamp(Rect r, Vector2 size) =>
            Rect.MinMaxRect(Mathf.Max(0, r.xMin), Mathf.Max(0, r.yMin), Mathf.Min(size.x, r.xMax), Mathf.Min(size.y, r.yMax));
    }

#if UNITY_EDITOR
    /// <summary>Utilidades de editor del tutorial (menú Tools > El Despacho > Tutorial).</summary>
    static class TutorialEditorMenu
    {
        const string Folder = "Assets/Resources/" + TutorialManager.ResourcesFolder;

        [UnityEditor.MenuItem("Tools/El Despacho/Tutorial/Crear assets de los pasos")]
        static void CreateStepAssets()
        {
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources"))
                UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
            if (!UnityEditor.AssetDatabase.IsValidFolder(Folder))
                UnityEditor.AssetDatabase.CreateFolder("Assets/Resources", TutorialManager.ResourcesFolder);
            int created = 0;
            foreach (var step in TutorialStepData.CreateDefaults())
            {
                string path = $"{Folder}/{step.name}.asset";
                if (UnityEditor.AssetDatabase.LoadAssetAtPath<TutorialStepData>(path) != null) continue; // no pisar ediciones
                UnityEditor.AssetDatabase.CreateAsset(step, path);
                created++;
            }
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"[Tutorial] {created} paso(s) creados en {Folder}. Edítalos en el Inspector; se muestran por orden de nombre.");
        }

        [UnityEditor.MenuItem("Tools/El Despacho/Tutorial/Volver a mostrar el tutorial")]
        static void ResetTutorial()
        {
            PlayerPrefs.DeleteKey(TutorialManager.DefaultPrefsKey);
            PlayerPrefs.Save();
            Debug.Log("[Tutorial] Se volverá a mostrar en la próxima partida.");
        }
    }
#endif
}
