using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ElDespacho
{
    /// <summary>
    /// Despacho ilustrado (v0.10): la pantalla principal montada sobre el pixel art
    /// de Resources/Art/Despacho/Fondo_Despacho (320x180, se muestra a x6 = 1920x1080).
    /// Si falta el fondo, ShowOffice usa el despacho anterior generado por código.
    ///
    /// Distribución (ver el doc de diseño «diseno-pantalla-principal»):
    /// - Pared: ventana (izquierda), corcho con indicadores, mapa, recorte y informe (centro),
    ///   reloj del mandato y botón de cerrar el año (derecha).
    /// - Mesa: nota del asesor y placa (izquierda), vade con el expediente y los dos sellos (centro),
    ///   La Gaceta y la bandeja de expedientes (derecha).
    ///
    /// Nombres que usan el tutorial, la accesibilidad y las pruebas, y que hay que conservar:
    /// HUD, CardArea, ReadingZone, DeskDocument, OpenedContent, Card, StampRack, StampYes, StampNo, YearEndButton.
    /// </summary>
    public partial class DespachoGame
    {
        const float BaseW = 320f, BaseH = 180f; // tamaño del fondo en píxeles de arte
        const float ArtPx = 6f;                  // píxeles de pantalla (a 1920x1080) por píxel de arte

        int newsSeen; // noticias ya leídas en La Gaceta (para el aviso rojo)

        bool IllustratedOffice => Art.Get("Despacho/Fondo_Despacho") != null;

        /// <summary>Un rectángulo del fondo, en píxeles de arte (x, y desde arriba a la izquierda).</summary>
        static RectTransform At(Transform stage, string name, float x, float y, float w, float h) =>
            Kit.Rect(stage, name, new Vector2(x / BaseW, 1f - (y + h) / BaseH), new Vector2((x + w) / BaseW, 1f - y / BaseH));

        /// <summary>Igual que At, pero dentro de un padre que mide pw x ph píxeles de arte.</summary>
        static RectTransform AtIn(Transform parent, float pw, float ph, string name, float x, float y, float w, float h) =>
            Kit.Rect(parent, name, new Vector2(x / pw, 1f - (y + h) / ph), new Vector2((x + w) / pw, 1f - y / ph));

        /// <summary>Pone un sprite del pack en una imagen ya creada. False si no existe (se queda el dibujo de respaldo).</summary>
        static bool UseArt(Image img, string path)
        {
            var sprite = img != null ? Art.Get(path) : null;
            if (sprite == null) return false;
            img.sprite = sprite;
            img.color = Color.white;
            return true;
        }

        /// <summary>Sprite del pack a su tamaño real (x6), centrado en el ancla dada y sin recibir clics.</summary>
        static Image ArtImage(Transform parent, Sprite sprite, Vector2 anchor, string name)
        {
            var rt = Kit.Rect(parent, name, anchor, anchor);
            rt.sizeDelta = ArtSize(sprite);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;
            return img;
        }

        // ───────────── Pantalla ─────────────

        void ShowIllustratedOffice()
        {
            Clear();
            Background(Kit.Hex("#140d08")); // bandas si la pantalla no es 16:9

            // Escenario fijo de 1920x1080 que se escala entero para caber en pantalla
            var stage = Kit.Rect(root, "Stage", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            stage.sizeDelta = new Vector2(BaseW * ArtPx, BaseH * ArtPx);
            stage.gameObject.AddComponent<StageFitter>();

            var bg = Kit.Rect(stage, "Fondo", Vector2.zero, Vector2.one).gameObject.AddComponent<Image>();
            bg.sprite = Art.Get("Despacho/Fondo_Despacho");
            bg.raycastTarget = false;

            float avg = (estabilidad + seguridad + prosperidad) / 3f;
            BuildArtWindow(stage, avg);
            BuildCorkboard(stage);
            BuildMandateClock(stage);
            BuildAdvisorCorner(stage);
            BuildGazette(stage);
            BuildDeskTray(stage);
            BuildDeskCard(stage);
        }

        // ───────────── Ventana ─────────────

        void BuildArtWindow(Transform stage, float avg)
        {
            var window = At(stage, "Window", 15, 15, 44, 42);
            int min = Mathf.Min(estabilidad, Mathf.Min(seguridad, prosperidad));
            // La vista cambia con la situación del país: crisis (media < 40), humo (algún indicador < 35),
            // tranquilo (media >= 60) o nublado. Cada vista es un sprite del cristal entero (44x42).
            string view = avg < 40 ? "Despacho/Ventana_Crisis"
                : min < 35 ? "Despacho/Ventana_Humo"
                : avg >= 60 ? "Despacho/Ventana_Tranquilo"
                : "Despacho/Ventana_Nublado";
            var viewArt = Art.Get(view);
            if (viewArt != null)
            {
                var glass = window.gameObject.AddComponent<Image>();
                glass.sprite = viewArt;
                glass.raycastTarget = false;
            }
            else
            {
                // Respaldo sin arte: el cielo se oscurece y sale humo
                if (avg < 60)
                    Kit.Panel(window, new Color(0.12f, 0.13f, 0.17f, Mathf.InverseLerp(60f, 20f, avg) * 0.5f), Vector2.zero, Vector2.one);
                if (min < 35)
                {
                    Color smoke = new Color(0.22f, 0.22f, 0.24f, 0.7f);
                    Kit.Dot(window, smoke, new Vector2(0.62f, 0.45f), 24);
                    Kit.Dot(window, smoke, new Vector2(0.66f, 0.60f), 36);
                    Kit.Dot(window, smoke, new Vector2(0.72f, 0.77f), 50);
                    Kit.Dot(window, smoke, new Vector2(0.80f, 0.93f), 62);
                }
            }
            string mood = avg >= 60 ? "El país está tranquilo." : avg >= 40 ? "Hay nubes sobre Aurelia." : "Aurelia está en crisis.";
            var sill = At(stage, "WindowCaption", 12, 59, 50, 8);
            Kit.Label(sill, mood, 19, Pal.Cream, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, default, default, FontStyle.Italic, true);
        }

        // ───────────── Corcho ─────────────

        void BuildCorkboard(Transform stage)
        {
            // Hilos rojos (detrás de los papeles): recorte → informe → mapa
            Thread(stage, new Vector2(106, 43), new Vector2(168, 42));
            Thread(stage, new Vector2(168, 42), new Vector2(225, 11));

            // Indicadores clavados (el contenedor se sigue llamando HUD)
            var hud = At(stage, "HUD", 74, 6, 126, 33);
            PinnedIndicator(hud, "ESTABILIDAD", estabilidad, 2, -2f);
            PinnedIndicator(hud, "SEGURIDAD", seguridad, 44, 1.2f);
            PinnedIndicator(hud, "PROSPERIDAD", prosperidad, 86, -1f);

            // Recorte de prensa con el último titular
            var clip = At(stage, "NewsClipping", 80, 42, 52, 22).gameObject.AddComponent<Image>();
            bool clipArt = UseArt(clip, "Despacho/Recorte_Prensa");
            if (!clipArt) clip.color = Kit.Hex("#ddd2b8");
            clip.raycastTarget = false;
            clip.rectTransform.localEulerAngles = new Vector3(0, 0, 1.5f);
            // Con arte: la regla roja dibujada queda entre el borde y el antetítulo; todo el texto va debajo
            float clipTop = clipArt ? 40 : 20;
            Kit.Label(clip.transform, "LA GACETA · ÚLTIMA HORA", 13, Pal.Red, TextAnchor.UpperLeft, Vector2.zero, Vector2.one,
                new Vector2(14, 0), new Vector2(-14, -clipTop), FontStyle.Bold);
            Kit.Label(clip.transform, Headline(news.FirstOrDefault()), 19, Pal.Ink, TextAnchor.UpperLeft, Vector2.zero, Vector2.one,
                new Vector2(14, clipArt ? 14 : 8), new Vector2(-14, -(clipTop + 18)), FontStyle.Bold, true);
            Pin(clip.transform);

            // Informe confidencial con la agenda exterior
            var report = At(stage, "ConfidentialReport", 140, 41, 56, 23).gameObject.AddComponent<Image>();
            bool reportArt = UseArt(report, "Despacho/Informe_Confidencial");
            if (!reportArt) report.color = Kit.Hex("#e8dfc4");
            report.raycastTarget = false;
            report.rectTransform.localEulerAngles = new Vector3(0, 0, -1.8f);
            float reportTop = reportArt ? 30 : 18, reportLeft = reportArt ? 26 : 10; // sin pisar el lomo, el clip ni la regla
            Kit.Label(report.transform, "CONFIDENCIAL", 17, Pal.Red, TextAnchor.UpperCenter, Vector2.zero, Vector2.one,
                new Vector2(reportLeft, 0), new Vector2(-10, -reportTop), FontStyle.Bold);
            var waiting = ThisYear.Where(c => c.decision == Decision.Ninguna).Select(c => c.name).ToList();
            string agenda = waiting.Count > 0 ? "Esperan audiencia:\n" + string.Join(", ", waiting) : "Agenda exterior del año cumplida.";
            Kit.Label(report.transform, agenda, 16, Pal.Ink, TextAnchor.UpperCenter, Vector2.zero, Vector2.one,
                new Vector2(reportLeft + 2, 10), new Vector2(-12, -(reportTop + 22)), FontStyle.Normal, true);
            Pin(report.transform);

            // Mapa del mundo clavado: se pulsa para abrirlo
            int left = PendingThisYear;
            var mapRt = At(stage, "MapButton", 205, 9, 40, 38);
            mapRt.localEulerAngles = new Vector3(0, 0, 2f);
            var map = mapRt.gameObject.AddComponent<Image>();
            if (!UseArt(map, "Despacho/Mapa_Clavado"))
            {
                map.color = Pal.Paper;
                var ocean = Kit.Panel(mapRt, Kit.Hex("#5e8aa0"), new Vector2(0.07f, 0.30f), new Vector2(0.93f, 0.93f), default, default, "Ocean");
                ocean.gameObject.AddComponent<RectMask2D>();
                Kit.Dot(ocean.transform, Pal.Land, new Vector2(0.25f, 0.62f), 90);
                Kit.Dot(ocean.transform, Pal.Land, new Vector2(0.62f, 0.38f), 110);
                Kit.Dot(ocean.transform, Pal.Land, new Vector2(0.85f, 0.80f), 60);
                Kit.Dot(ocean.transform, Pal.Red, new Vector2(0.58f, 0.45f), 14);
            }
            Kit.Label(mapRt, "MAPA DEL MUNDO", 17, Pal.Ink, TextAnchor.MiddleCenter, new Vector2(0, 0.14f), new Vector2(1, 0.30f), default, default, FontStyle.Bold);
            Kit.Label(mapRt, left > 0 ? $"{left} visita{(left == 1 ? "" : "s")}" : "Sin visitas", 16,
                left > 0 ? Pal.Red : Pal.InkSoft, TextAnchor.MiddleCenter, new Vector2(0, 0.01f), new Vector2(1, 0.15f), default, default, FontStyle.Bold);
            Pin(mapRt);
            var mapBtn = mapRt.gameObject.AddComponent<Button>();
            mapBtn.targetGraphic = map;
            mapBtn.onClick.AddListener(() => { Sfx.Play(Sfx.Paper); ShowMap(); });
            // Mismo aviso que antes: tiembla si la mesa está despejada y aún no has abierto el mapa este año
            if (left > 0 && !mapOpenedThisYear && CardsDoneThisYear) mapRt.gameObject.AddComponent<Wiggle>();
        }

        void PinnedIndicator(Transform hud, string name, int v, float x, float angle)
        {
            var rt = AtIn(hud, 126, 33, name, x, 3, 38, 28);
            rt.localEulerAngles = new Vector3(0, 0, angle);
            var card = rt.gameObject.AddComponent<Image>();
            if (!UseArt(card, "Despacho/Ficha_Indicador")) card.color = Pal.Cream;
            card.raycastTarget = false;
            Kit.Label(rt, name, 16, Pal.InkSoft, TextAnchor.UpperCenter, Vector2.zero, Vector2.one,
                new Vector2(6, 0), new Vector2(-6, -22), FontStyle.Bold);
            Kit.Label(rt, v.ToString(), 44, Kit.IndicatorColor(v), TextAnchor.MiddleCenter,
                new Vector2(0, 0.30f), new Vector2(1, 0.80f), default, default, FontStyle.Bold);
            Kit.Bar(rt, v, Kit.IndicatorColor(v), new Vector2(0.10f, 0.10f), new Vector2(0.90f, 0.24f));
            Pin(rt);
            rt.gameObject.AddComponent<GameplayIndicatorLayout>();
        }

        static void Pin(Transform paper)
        {
            var pinArt = Art.Get("Despacho/Chincheta");
            if (pinArt != null)
            {
                // La cabeza de la chincheta está 1 px a la izquierda del centro del sprite (el resto es su sombra)
                ArtImage(paper, pinArt, new Vector2(0.5f, 1f), "Pin").rectTransform.anchoredPosition = new Vector2(ArtPx, -9);
                return;
            }
            var shadow = Kit.Dot(paper, new Color(0, 0, 0, 0.35f), new Vector2(0.5f, 1f), 20, "PinShadow");
            shadow.rectTransform.anchoredPosition = new Vector2(3, -12);
            Kit.Dot(paper, Kit.Hex("#b8322b"), new Vector2(0.5f, 1f), 18, "Pin").rectTransform.anchoredPosition = new Vector2(0, -9);
        }

        /// <summary>Hilo rojo entre dos puntos del fondo (en píxeles de arte).</summary>
        static void Thread(Transform stage, Vector2 a, Vector2 b)
        {
            Vector2 mid = (a + b) / 2f;
            var rt = Kit.Rect(stage, "Thread", new Vector2(mid.x / BaseW, 1f - mid.y / BaseH), new Vector2(mid.x / BaseW, 1f - mid.y / BaseH));
            Vector2 d = new Vector2(b.x - a.x, a.y - b.y) * ArtPx;
            rt.sizeDelta = new Vector2(d.magnitude, 4);
            rt.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = Kit.Hex("#a82a24");
            img.raycastTarget = false;
        }

        static string Headline(string item)
        {
            if (string.IsNullOrEmpty(item)) return "Sin noticias.";
            int s = item.IndexOf("<b>"), e = item.IndexOf("</b>");
            return s >= 0 && e > s ? item.Substring(s + 3, e - s - 3) : item.Split('\n')[0];
        }

        // ───────────── Reloj del mandato y cierre del año ─────────────

        void BuildMandateClock(Transform stage)
        {
            var clock = At(stage, "MandateClock", 267, 20, 36, 30);
            Kit.Dot(clock, new Color(0, 0, 0, 0.4f), new Vector2(0.53f, 0.47f), 176, "Shadow");
            var clockFace = Art.Get("Despacho/Reloj_Esfera");
            var clockRim = Art.Get("Despacho/Reloj_Marco");
            bool artClock = clockFace != null && clockRim != null;
            if (artClock) ArtImage(clock, clockFace, new Vector2(0.5f, 0.5f), "Dial");
            else
            {
                Kit.Dot(clock, Pal.WoodLight, new Vector2(0.5f, 0.5f), 176, "Rim");
                Kit.Dot(clock, Pal.Cream, new Vector2(0.5f, 0.5f), 152, "Dial");
            }
            // Trimestre en curso (suave) y años ya cumplidos (oro)
            Quarter(clock, Kit.WithAlpha(Pal.Gold, 0.35f), year / (float)YearsInMandate);
            Quarter(clock, Pal.Gold, (year - 1) / (float)YearsInMandate);
            // Cuatro marcas (una por año): dos barras cruzadas que el círculo interior tapa por el centro
            // El marco de latón (aro, cruz de los 4 años y círculo central) tapa el borde de los cuartos
            if (artClock) ArtImage(clock, clockRim, new Vector2(0.5f, 0.5f), "Rim");
            else
            {
                for (int i = 0; i < 2; i++)
                    Kit.Box(clock, Pal.Ink, new Vector2(0.5f, 0.5f), new Vector2(4, 152), "Tick").rectTransform.localEulerAngles = new Vector3(0, 0, i * 90);
                Kit.Dot(clock, Pal.Cream, new Vector2(0.5f, 0.5f), 96, "Inner");
            }
            Kit.Label(clock, $"{year}", 44, Pal.Ink, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, default, default, FontStyle.Bold);
            var hand = Kit.Rect(clock, "Hand", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            hand.pivot = new Vector2(0.5f, 0f);
            hand.sizeDelta = new Vector2(6, 70);
            hand.localEulerAngles = new Vector3(0, 0, -(year - 0.5f) / YearsInMandate * 360f);
            Kit.Panel(hand, Kit.Hex("#7a2a22"), new Vector2(0, 0.68f), Vector2.one);

            Kit.Label(At(stage, "MandateYear", 256, 50, 58, 7), $"Año {year} de {YearsInMandate}", 22, Pal.Cream,
                TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, default, default, FontStyle.Bold);

            int left = PendingThisYear;
            bool cardsDone = CardsDoneThisYear;
            string endLabel = year >= YearsInMandate ? "TERMINAR EL MANDATO" : $"CERRAR EL AÑO {year}";
            string label = !cardsDone ? "Atiende tu mesa" : left > 0 ? "Visita los países" : endLabel;
            var plate = At(stage, "YearEndSlot", 256, 58, 58, 11);
            var endBtn = Kit.MakeButton(plate, label, 18, Pal.Gold, Pal.Ink, () => { Sfx.Play(Sfx.Thunk); EndYear(); },
                Vector2.zero, Vector2.one);
            endBtn.gameObject.name = "YearEndButton";
            if (UseArt(endBtn.GetComponent<Image>(), "UI/Boton_Laton"))
            {
                var shade = endBtn.transform.Find("Shade"); // el sprite ya trae su canto inferior
                if (shade != null) shade.gameObject.SetActive(false);
            }
            endBtn.interactable = left == 0 && cardsDone;
            if (endBtn.transition == Selectable.Transition.ColorTint)
            {
                var cb = endBtn.colors;
                cb.disabledColor = new Color(0.72f, 0.68f, 0.62f, 1f); // latón apagado pero opaco
                endBtn.colors = cb;
            }
        }

        static void Quarter(Transform clock, Color color, float amount)
        {
            if (amount <= 0) return;
            var img = Kit.Dot(clock, color, new Vector2(0.5f, 0.5f), 152, "Quarter");
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Radial360;
            img.fillOrigin = (int)Image.Origin360.Top;
            img.fillClockwise = true;
            img.fillAmount = Mathf.Clamp01(amount);
        }

        // ───────────── Mesa izquierda: nota del asesor y placa ─────────────

        void BuildAdvisorCorner(Transform stage)
        {
            var noteRt = At(stage, "AdvisorNote", 5, 82, 63, 66);
            // Sin giro: el recorte del texto con scroll (RectMask2D) no funciona en rectángulos girados
            Kit.Panel(noteRt, new Color(0, 0, 0, 0.3f), Vector2.zero, Vector2.one, new Vector2(8, -8), new Vector2(8, -8), "Shadow");
            UseArt(Kit.Panel(noteRt, Kit.Hex("#efe3a8"), Vector2.zero, Vector2.one, default, default, "Paper"), "Despacho/Nota_Asesor");
            Kit.Label(noteRt, "NOTA DEL ASESOR", 19, Pal.Red, TextAnchor.UpperLeft, Vector2.zero, Vector2.one,
                new Vector2(18, 0), new Vector2(-18, -26), FontStyle.Bold);
            var advisorScroll = Kit.ScrollText(noteRt, AdvisorText(), 18, Pal.Ink, new Vector2(18, 14), new Vector2(-10, -56), false, "AdvisorText");
            noteRt.gameObject.AddComponent<OfficeAccessGuidance>().Initialize(this, advisorScroll);
            Pin(noteRt);

            var plate = At(stage, "NamePlate", 9, 153, 56, 13);
            Kit.Panel(plate, new Color(0, 0, 0, 0.35f), Vector2.zero, Vector2.one, new Vector2(6, -6), new Vector2(6, -6));
            bool plateArt = UseArt(Kit.Panel(plate, Pal.Wood, Vector2.zero, Vector2.one), "Despacho/Placa_Nombre");
            var brass = Kit.Panel(plate, Pal.Gold, Vector2.zero, Vector2.one, new Vector2(10, 10), new Vector2(-10, -10));
            if (plateArt) brass.color = Color.clear; // la chapa de latón ya está en el sprite; aquí solo va el texto
            Kit.Label(brass.transform, "PRESIDENCIA\nDE AURELIA", 17, Pal.Ink, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one,
                default, default, FontStyle.Bold, true);
        }

        // ───────────── Mesa derecha: La Gaceta y la bandeja ─────────────

        void BuildGazette(Transform stage)
        {
            var normal = Art.Get("UI/UI_IconoGaceta_Normal");
            var hover = Art.Get("UI/UI_IconoGaceta_Hover") ?? normal;
            var rt = At(stage, "Gazette", 262, 76, 48, 48);
            var img = rt.gameObject.AddComponent<Image>();
            if (normal != null) img.sprite = normal; else img.color = Pal.Paper;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            if (normal != null)
            {
                btn.transition = Selectable.Transition.SpriteSwap;
                var st = btn.spriteState;
                st.highlightedSprite = hover;
                st.pressedSprite = hover;
                st.selectedSprite = normal;
                btn.spriteState = st;
            }
            else Kit.Label(rt, "LA GACETA", 24, Pal.Ink, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, default, default, FontStyle.Bold);
            btn.onClick.AddListener(() => { Sfx.Play(Sfx.Paper); ShowGazette(stage); });

            int unread = Mathf.Max(0, news.Count - newsSeen);
            if (unread > 0)
            {
                var badge = Kit.Dot(rt, Kit.Hex("#b8322b"), new Vector2(0.9f, 0.9f), 56, "UnreadBadge");
                Kit.Label(badge.transform, unread > 9 ? "9+" : unread.ToString(), 26, Color.white, TextAnchor.MiddleCenter,
                    Vector2.zero, Vector2.one, default, default, FontStyle.Bold);
            }
        }

        void ShowGazette(Transform stage)
        {
            int unread = Mathf.Clamp(news.Count - newsSeen, 0, news.Count); // las noticias más nuevas van primero
            newsSeen = news.Count;
            var overlay = Kit.Panel(stage, new Color(0, 0, 0, 0.6f), Vector2.zero, Vector2.one, default, default, "GazetteOverlay");
            overlay.raycastTarget = true; // bloquea la mesa mientras se lee
            var paper = At(overlay.transform, "GazettePaper", 92, 10, 136, 160).gameObject.AddComponent<Image>();
            if (!UseArt(paper, "UI/Gaceta_Abierta")) paper.color = Pal.Paper;
            var p = paper.transform;

            // Maquetación «portada clásica» (propuesta A), en píxeles de arte del periódico (136×160)
            RectTransform Box(string name, float x, float y, float w, float h) => AtIn(p, 136, 160, name, x, y, w, h);
            Text Line(string name, string text, int size, Color color, TextAnchor align, float x, float y, float w, float h,
                FontStyle style = FontStyle.Normal, bool fit = false) =>
                Kit.Label(Box(name, x, y, w, h), text, size, color, align, Vector2.zero, Vector2.one, default, default, style, fit);

            // Cabecera
            Line("GazetteNumber", $"Nº {Mathf.Max(1, news.Count)} · AÑO {year} DEL MANDATO", 16, Pal.InkSoft, TextAnchor.MiddleLeft, 8, 5, 70, 5);
            Line("GazetteEdition", "EDICIÓN DE LA PRESIDENCIA", 16, Pal.InkSoft, TextAnchor.MiddleRight, 70, 5, 58, 5);
            Line("GazetteMasthead", "LA GACETA", 80, Pal.Ink, TextAnchor.MiddleCenter, 8, 10, 120, 16, FontStyle.Bold, true);
            Line("GazetteSubtitle", "Diario de la República de Aurelia", 18, Pal.InkSoft, TextAnchor.MiddleCenter, 8, 26, 120, 5, FontStyle.Italic);
            Kit.Panel(Box("GazetteRule", 8, 32, 120, 1), Pal.Ink, Vector2.zero, Vector2.one);
            Kit.Panel(Box("GazetteRuleThin", 8, 34, 120, 0.5f), Pal.InkSoft, Vector2.zero, Vector2.one);

            if (news.Count == 0)
            {
                Line("GazetteLead", "Sin noticias por ahora.", 30, Pal.InkSoft, TextAnchor.MiddleCenter, 8, 60, 120, 20, FontStyle.Italic);
            }
            else
            {
                // Noticia principal: titular, ilustración según el tema y texto
                Line("GazetteLeadTitle", Headline(news[0]).ToUpperInvariant(), 40, Pal.Ink, TextAnchor.LowerLeft, 8, 37, 120, 13, FontStyle.Bold, true);
                var pic = Box("GazetteLeadArt", 8, 52, 40, 28).gameObject.AddComponent<Image>();
                pic.raycastTarget = false;
                var illus = Art.Get(NewsIllustration(news[0]));
                if (illus != null) pic.sprite = illus; else pic.color = Pal.PaperDark;
                // Si es nueva, el sello va al pie del texto (sin tapar el titular) y el texto le deja sitio
                bool leadNew = unread > 0;
                Line("GazetteLeadBody", NewsBody(news[0]), 21, Pal.Ink, TextAnchor.UpperLeft, 51, 52, 77, leadNew ? 18 : 28, FontStyle.Normal, true);
                if (leadNew) NewStamp(Box("GazetteLeadNew", 101, 71, 26, 9));

                // Dos breves en columnas
                Kit.Panel(Box("GazetteDivider", 8, 83, 120, 0.5f), Pal.InkSoft, Vector2.zero, Vector2.one);
                if (news.Count > 2) Kit.Panel(Box("GazetteColumnRule", 67.75f, 86, 0.5f, 35), Pal.PaperDark, Vector2.zero, Vector2.one);
                for (int i = 1; i <= 2 && i < news.Count; i++)
                {
                    float x = i == 1 ? 8 : 70;
                    Line("GazetteBriefTitle" + i, Headline(news[i]).ToUpperInvariant(), 24, Pal.Ink, TextAnchor.UpperLeft, x, 86, 58, 10, FontStyle.Bold, true);
                    bool briefNew = i < unread;
                    Line("GazetteBriefBody" + i, NewsBody(news[i]), 18, Pal.Ink, TextAnchor.UpperLeft, x, 97, 58, briefNew ? 15 : 24, FontStyle.Normal, true);
                    if (briefNew) NewStamp(Box("GazetteBriefNew" + i, x + 31, 112, 26, 9));
                }

                // El resto, en una lista corta con scroll
                if (news.Count > 3)
                {
                    Kit.Panel(Box("GazetteMoreRule", 8, 124, 120, 0.5f), Pal.InkSoft, Vector2.zero, Vector2.one);
                    Line("GazetteMoreTitle", "MÁS NOTICIAS", 16, Pal.Red, TextAnchor.MiddleLeft, 8, 125, 60, 5, FontStyle.Bold);
                    var sb = new StringBuilder();
                    foreach (var n in news.Skip(3).Take(12)) sb.Append("· ").Append(Headline(n)).Append('\n');
                    Kit.ScrollText(Box("GazetteMore", 8, 130, 120, 13), sb.ToString().TrimEnd(), 18, Pal.Ink,
                        Vector2.zero, Vector2.zero, false, "GazetteText");
                }
            }

            var close = Kit.MakeButton(Box("GazetteCloseSlot", 39, 145, 58, 11), "DOBLAR EL PERIÓDICO", 22, Pal.Gold, Pal.Ink, () =>
            {
                Sfx.Play(Sfx.Paper);
                overlay.gameObject.SetActive(false);
                Destroy(overlay.gameObject);
                var badge = stage.Find("Gazette/UnreadBadge");
                if (badge != null) badge.gameObject.SetActive(false);
            }, Vector2.zero, Vector2.one);
            close.gameObject.name = "GazetteClose";
            if (UseArt(close.GetComponent<Image>(), "UI/Boton_Laton"))
            {
                var shade = close.transform.Find("Shade"); // el sprite ya trae su canto inferior
                if (shade != null) shade.gameObject.SetActive(false);
            }
        }

        /// <summary>Texto de una noticia sin el titular (lo que va después de la primera línea).</summary>
        static string NewsBody(string item)
        {
            if (string.IsNullOrEmpty(item)) return "";
            int nl = item.IndexOf('\n');
            return nl >= 0 ? item.Substring(nl + 1).Trim() : "";
        }

        /// <summary>Ilustración de la noticia principal según su tema (economía, exterior o política).</summary>
        static string NewsIllustration(string item)
        {
            string t = (item ?? "").ToLowerInvariant();
            string[] economy = { "impuest", "arcas", "comerci", "econom", "económ", "dinero", "presupuest", "empresa", "mercado", "prosperidad", "deuda" };
            string[] foreign = { "país", "países", "audiencia", "visita", "bloque", "alianza", "aliad", "frontera", "embaj", "ataque", "guerra", "sancion", "sanción", "tratado" };
            if (economy.Any(k => t.Contains(k))) return "UI/Gaceta_Ilustracion_Economia";
            if (foreign.Any(k => t.Contains(k))) return "UI/Gaceta_Ilustracion_Exterior";
            return "UI/Gaceta_Ilustracion_Politica";
        }

        /// <summary>Sello rojo «NUEVO» sobre una noticia que aún no se había leído.</summary>
        static void NewStamp(RectTransform slot)
        {
            slot.localEulerAngles = new Vector3(0, 0, 4f);
            var img = slot.gameObject.AddComponent<Image>();
            img.raycastTarget = false;
            var art = Art.Get("UI/Sello_Nuevo");
            if (art != null)
            {
                img.sprite = art;
                img.color = new Color(1, 1, 1, 0.9f);
            }
            else
            {
                img.color = Color.clear;
                Kit.Label(slot, "NUEVO", 20, Pal.Red, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, default, default, FontStyle.Bold);
            }
        }

        // Bandeja de expedientes: se actualiza al coger un expediente (sale de la bandeja) y si vuelve a ella
        Image trayArt;
        GameObject trayBadge;
        Text trayBadgeNumber;

        /// <summary>Expedientes que quedan en la bandeja. El actual cuenta solo mientras sigue dentro.</summary>
        int TrayCount(bool currentInTray) =>
            CardsDoneThisYear ? 0 : Mathf.Max(0, CardsPerYear - cardsThisYear) + (currentInTray && currentCard != null && !cardAnswered ? 1 : 0);

        RectTransform BuildDeskTray(Transform stage)
        {
            var tray = At(stage, "PaperStack", 254, 124, 64, 48);
            trayArt = tray.gameObject.AddComponent<Image>();
            trayArt.raycastTarget = false;

            // Número de expedientes pendientes en la burbuja roja (sprite aparte); con la bandeja vacía no se muestra
            var badge = AtIn(tray, 64, 48, "PendingBadge", 51, 0, 13, 13);
            var bubble = Art.Get("Despacho/Burbuja_Pendientes");
            if (bubble != null)
            {
                var bi = badge.gameObject.AddComponent<Image>();
                bi.sprite = bubble;
                bi.raycastTarget = false;
            }
            else Kit.Dot(badge, Kit.Hex("#b8322b"), new Vector2(0.46f, 0.54f), 64);
            var num = AtIn(badge, 13, 13, "PendingNumber", 0, 0, 12, 12);
            trayBadgeNumber = Kit.Label(num, "", 32, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one,
                new Vector2(-10, 0), new Vector2(10, 0), FontStyle.Bold);
            trayBadge = badge.gameObject;

            Kit.Label(At(stage, "TrayLabel", 252, 172, 68, 7), "EXPEDIENTES", 17, Pal.Cream, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, default, default, FontStyle.Bold);
            RefreshTray(TrayCount(!documentOpened));
            return tray;
        }

        void RefreshTray(int pendingCards)
        {
            if (trayArt == null) return;
            // La bandeja se va vaciando: llena, a medias, casi vacía y vacía (si falta un estado, se usa la llena)
            string trayPath = pendingCards <= 0 ? "Despacho/Bandeja_Expedientes_Vacia"
                            : pendingCards <= CardsPerYear * 2 / 5 ? "Despacho/Bandeja_Expedientes_Poca"
                            : pendingCards <= CardsPerYear * 3 / 5 ? "Despacho/Bandeja_Expedientes_Media"
                            : "Despacho/Bandeja_Expedientes";
            var sprite = Art.Get(trayPath) ?? Art.Get("Despacho/Bandeja_Expedientes");
            if (sprite != null) trayArt.sprite = sprite; else trayArt.color = Pal.WoodMid;
            trayBadge.SetActive(pendingCards > 0);
            trayBadgeNumber.text = pendingCards.ToString();
        }

        // ───────────── Vade: el expediente y los sellos ─────────────

        void BuildDeskCard(Transform stage)
        {
            var desk = At(stage, "CardArea", 109, 81, 103, 90);
            var k = currentCard;
            if (k == null)
            {
                int left = PendingThisYear;
                string msg = left > 0
                    ? $"Tu mesa está despejada.\n\n{left} país{(left == 1 ? "" : "es")} espera{(left == 1 ? "" : "n")} tu visita.\nPulsa el mapa del corcho."
                    : year >= YearsInMandate ? "Todo atendido.\n\nPuedes terminar el mandato." : $"Todo atendido.\n\nPuedes cerrar el año {year}.";
                Kit.Label(At(stage, "EmptyDesk", 80, 88, 161, 76), msg, 34, Pal.Cream, TextAnchor.MiddleCenter,
                    Vector2.zero, Vector2.one, default, default, FontStyle.Italic, true);
                return;
            }

            // Zona de lectura: el propio vade (transparente; se ilumina al acercar el expediente)
            var zone = Kit.Panel(desk, new Color(1, 1, 1, 0), new Vector2(-0.4f, 0f), new Vector2(1.4f, 1f), default, default, "ReadingZone");
            var trayHint = Kit.Label(zone.transform, "", 30, Pal.Cream, TextAnchor.MiddleCenter,
                new Vector2(0.25f, 0.3f), new Vector2(0.75f, 0.7f), default, default, FontStyle.Bold, true);

            // Centro de la bandeja: el expediente sale de aquí al pulsarla (no se ve encima mientras espera)
            var pileSpot = At(stage, "PileSpot", 256, 122, 60, 46);
            var trayImage = stage.Find("PaperStack")?.GetComponent<Image>();

            var sheet = Kit.Panel(desk, Pal.Paper, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), default, default, "DeskDocument");
            sheet.color = new Color(0, 0, 0, 0); // la superficie la pinta PaperSurface
            sheet.raycastTarget = true;
            var shadow = Kit.Panel(sheet.transform, new Color(0, 0, 0, 0.28f), Vector2.zero, Vector2.one,
                new Vector2(14, -14), new Vector2(14, -14), "PaperShadow");
            UseArt(Kit.Panel(sheet.transform, Pal.Paper, Vector2.zero, Vector2.one, default, default, "PaperSurface"), "Despacho/Papel_Expediente");

            // Portada (expediente cerrado en la bandeja)
            var cover = Kit.Rect(sheet.transform, "ClosedCover", Vector2.zero, Vector2.one);
            var dossierArt = Art.Get("UI/UI_Dossier");
            Kit.Label(cover, "AURELIA · PRESIDENCIA", 48, Pal.Ink, TextAnchor.MiddleCenter,
                new Vector2(0.05f, 0.84f), new Vector2(0.95f, 0.97f), default, default, FontStyle.Bold, true);
            if (dossierArt != null) Art.Fill(cover, dossierArt, new Vector2(0.06f, 0.30f), new Vector2(0.94f, 0.83f), "DossierArt");
            Kit.Label(cover, "PENDIENTE DE REVISIÓN", 46, Pal.Red, TextAnchor.MiddleCenter,
                new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.29f), default, default, FontStyle.Bold, true);
            Kit.Label(cover, $"AÑO {year} · ASUNTO {cardsThisYear}", 40, Pal.InkSoft, TextAnchor.MiddleCenter,
                new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.15f), default, default, FontStyle.Bold);

            // Contenido del expediente abierto
            var content = Kit.Rect(sheet.transform, "OpenedContent", Vector2.zero, Vector2.one);
            var card = Kit.Panel(content, Pal.Paper, Vector2.zero, Vector2.one, default, default, "Card");
            bool paperArt = UseArt(card, "Despacho/Papel_Expediente");
            // Con el papel del pack, todo el contenido va dentro de su doble marco (~4 px de arte)
            var ct = paperArt ? (Transform)Kit.Rect(card.transform, "Inner", Vector2.zero, Vector2.one, new Vector2(24, 24), new Vector2(-24, -24))
                              : card.transform;
            Kit.Panel(ct, k.color, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -10), Vector2.zero);
            Kit.Label(ct, "AURELIA / PRESIDENCIA", 16, Pal.Red, TextAnchor.UpperLeft, Vector2.zero, Vector2.one,
                new Vector2(22, 0), new Vector2(-22, -20), FontStyle.Bold);
            Kit.Label(ct, $"Asunto {Mathf.Min(cardsThisYear, CardsPerYear)} de {CardsPerYear}", 16, Pal.InkSoft, TextAnchor.UpperRight,
                Vector2.zero, Vector2.one, new Vector2(22, 0), new Vector2(-22, -20));

            // Retrato (se dibuja al tamaño de siempre y se reduce)
            float portraitY = 0.80f;
            var frame = Kit.Rect(ct, "PortraitSlot", new Vector2(0.18f, portraitY), new Vector2(0.18f, portraitY));
            frame.sizeDelta = new Vector2(184, 176);
            frame.localScale = Vector3.one * (paperArt ? 0.66f : 0.72f); // dentro del marco el sitio es menor
            var portrait = Kit.Panel(frame, Color.Lerp(k.color, Color.white, 0.25f), Vector2.zero, Vector2.one, default, default, "Portrait");
            portrait.gameObject.AddComponent<RectMask2D>();
            BuildFace(portrait.transform, k.color, k.id, cardAnswered && cardResultEffect.estabilidad + cardResultEffect.seguridad + cardResultEffect.prosperidad < 0);

            Kit.Label(ct, $"<b>{k.who}</b>\n<size=18>{k.role}</size>", 25, Pal.Ink, TextAnchor.MiddleLeft,
                new Vector2(0.34f, 0.70f), new Vector2(0.96f, 0.92f), default, default, FontStyle.Normal, true);
            Kit.Panel(ct, Pal.PaperDark, new Vector2(0.05f, 0.675f), new Vector2(0.95f, 0.68f));

            Text preview = null;
            string idle = null;
            if (!cardAnswered)
            {
                Kit.Label(ct, "«" + k.text + "»", 24, Pal.Ink, TextAnchor.UpperCenter,
                    new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.65f), default, default, FontStyle.Normal, true);
                preview = Kit.Label(ct, "<i>Coge un sello de la mesa y estámpalo sobre el expediente.</i>", 17, Pal.InkSoft,
                    TextAnchor.MiddleCenter, new Vector2(0.03f, 0.01f), new Vector2(0.97f, 0.11f), default, default, FontStyle.Normal, true);
                idle = preview.text;
            }
            else
            {
                if (stampPlaced) BuildStampMark(ct);
                Kit.Label(ct, cardResultText, 24, Pal.Ink, TextAnchor.UpperCenter,
                    new Vector2(0.06f, 0.28f), new Vector2(0.94f, 0.65f), default, default, FontStyle.Italic, true);
                Kit.Label(ct, DeltaTextOnPaper(cardResultEffect), 20, Pal.Ink, TextAnchor.MiddleCenter,
                    new Vector2(0.03f, 0.17f), new Vector2(0.97f, 0.28f), default, default, FontStyle.Bold, true);
                Kit.MakeButton(content, "CONTINUAR", 28, Pal.WoodLight, Pal.Cream, ArchiveCard,
                    new Vector2(0.28f, 0.02f), new Vector2(0.72f, 0.15f));
            }

            // Sellos y tampones a los lados del vade (siempre visibles; solo se pueden usar con el expediente abierto)
            var rack = At(stage, "StampRack", 72, 80, 177, 91);
            var rackInput = rack.gameObject.AddComponent<CanvasGroup>();
            // Tras sellar se quedan en la mesa como decorado (sin StampTool: ya no se pueden usar)
            var stampNo = BuildArtStamp(rack, false, StampLabel(false, k), !cardAnswered);
            var stampYes = BuildArtStamp(rack, true, StampLabel(true, k), !cardAnswered);

            void SetStampsReady(bool ready)
            {
                rackInput.blocksRaycasts = ready && !cardAnswered;
                if (ready) rack.SetAsLastSibling(); // que los sellos pasen por encima del papel
            }

            deskDocument = sheet.gameObject.AddComponent<DeskDocument>();
            deskDocument.pileAnchor = pileSpot;
            deskDocument.closedScale = 0.40f;
            deskDocument.pileAngle = 0f;
            deskDocument.hiddenInPile = true;
            deskDocument.hiddenPileScale = 0.62f; // el área invisible que se pulsa cubre toda la bandeja
            deskDocument.pileHighlight = trayImage;
            // Al sacar el expediente de la bandeja se descuenta de los pendientes; si vuelve, se suma otra vez
            deskDocument.leftPile = () => RefreshTray(TrayCount(false));
            deskDocument.backInPile = () => RefreshTray(TrayCount(true));
            deskDocument.zoneIdle = new Color(1, 1, 1, 0);
            deskDocument.zoneActive = new Color(1f, 0.95f, 0.75f, 0.14f);
            deskDocument.hintIdle = "Arrastra un expediente\nde la bandeja hasta el vade";
            deskDocument.hintActive = "SUELTA PARA ABRIR EL EXPEDIENTE";
            bool open = documentOpened || cardAnswered;
            deskDocument.Initialize(desk, zone, trayHint, cover.gameObject, content.gameObject, shadow, open, () =>
            {
                documentOpened = true;
                trayHint.text = "";
                SetStampsReady(true);
                TutorialManager.Instance?.TryAdvance(TutorialAdvanceCondition.DragDocumentToTray);
            });
            if (open) trayHint.text = "";
            SetStampsReady(open);

            if (stampNo != null && stampYes != null)
            {
                var area = rack;
                stampNo.Initialize(area, StampHome(false), card,
                    on => preview.text = on ? "<b>" + StampLabel(false, k) + ":</b> " + PreviewText(Balance.ScaleCardCost(k.no, year)) : idle,
                    pos => StampCard(false, pos, StampLabel(false, k)));
                stampYes.Initialize(area, StampHome(true), card,
                    on => preview.text = on ? "<b>" + StampLabel(true, k) + ":</b> " + PreviewText(Balance.ScaleCardCost(k.yes, year)) : idle,
                    pos => StampCard(true, pos, StampLabel(true, k)));
            }
        }

        // Columnas de los sellos dentro del vade (píxeles del arte, relativas al vade de 177x91)
        const float StampNoX = 19f, StampYesX = 158f; // centro de cada columna
        const float PadTop = 61f;                      // borde superior del tampón

        /// <summary>Tamaño en pantalla de un sprite del pack a x6 (se adapta si el artista cambia la medida).</summary>
        static Vector2 ArtSize(Sprite s) => s.rect.size * ArtPx;

        /// <summary>Centro de reposo del sello, en coordenadas relativas (0..1) del vade.</summary>
        static Vector2 StampHome(bool yes)
        {
            var art = Art.Get(yes ? "Sellos/Sello_Aprobado" : "Sellos/Sello_Denegado");
            float h = art != null ? art.rect.height : 48f;
            float cx = yes ? StampYesX : StampNoX;
            float cy = PadTop + 3f - h / 2f; // el pie del sello apoyado sobre el tampón
            return new Vector2(cx / 177f, 1f - cy / 91f);
        }

        /// <summary>Tampón + sello de madera del pack de arte, en su columna del vade.</summary>
        StampTool BuildArtStamp(RectTransform rack, bool yes, string label, bool interactive)
        {
            var art = Art.Get(yes ? "Sellos/Sello_Aprobado" : "Sellos/Sello_Denegado");
            if (art == null && !interactive) return null;
            float cx = yes ? StampYesX : StampNoX;
            var pad = Art.Get(yes ? "Sellos/Tampon_Verde" : "Sellos/Tampon_Rojo");
            float padH = 9f;
            if (pad != null)
            {
                padH = pad.rect.height;
                var padImg = AtIn(rack, 177, 91, yes ? "PadYes" : "PadNo", cx - pad.rect.width / 2f, PadTop, pad.rect.width, pad.rect.height)
                    .gameObject.AddComponent<Image>();
                padImg.sprite = pad;
                padImg.raycastTarget = false;
            }
            Kit.Label(AtIn(rack, 177, 91, "StampLabel", cx - 20, PadTop + padH + 2, 40, 9), label, 20, Pal.Cream, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, default, default, FontStyle.Bold, true);

            if (art == null) return BuildStamp(rack, label, yes ? Pal.Green : Pal.Red, yes);
            var rt = Kit.Rect(rack, yes ? "StampYes" : "StampNo", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            rt.sizeDelta = ArtSize(art);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = art;
            if (!interactive)
            {
                // Posición de reposo, la misma que calcula StampTool.Initialize
                rt.anchorMin = rt.anchorMax = StampHome(yes);
                img.raycastTarget = false;
                return null;
            }
            img.raycastTarget = true;
            rt.gameObject.AddComponent<AccessName>().label=label;
            return rt.gameObject.AddComponent<StampTool>();
        }

        /// <summary>Marca del sello con el arte del pack cuando el texto es APROBADO o DENEGADO.</summary>
        bool TryArtStampMark(Transform card)
        {
            string file = stampText == "APROBADO" ? "Sellos/Marca_Aprobado" : stampText == "DENEGADO" ? "Sellos/Marca_Denegado" : null;
            var sprite = file != null ? Art.Get(file) : null;
            if (sprite == null) return false;
            var mark = Kit.Rect(card, "StampMark", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            mark.sizeDelta = ArtSize(sprite);
            mark.anchoredPosition = stampPos;
            mark.localEulerAngles = new Vector3(0, 0, stampAngle);
            var img = mark.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = new Color(1, 1, 1, 0.92f);
            img.raycastTarget = false;
            return true;
        }
    }

    /// <summary>Escala el escenario de 1920x1080 para que quepa entero en el lienzo (con bandas si hace falta).</summary>
    public sealed class StageFitter : MonoBehaviour
    {
        RectTransform rt, parent;

        void Awake()
        {
            rt = (RectTransform)transform;
            parent = rt.parent as RectTransform;
            Fit();
        }

        void LateUpdate() => Fit();

        void Fit()
        {
            if (parent == null) return;
            var size = parent.rect.size;
            if (size.x <= 0 || size.y <= 0) return;
            float s = Mathf.Min(size.x / rt.sizeDelta.x, size.y / rt.sizeDelta.y);
            if (!Mathf.Approximately(rt.localScale.x, s)) rt.localScale = new Vector3(s, s, 1);
        }
    }
}
