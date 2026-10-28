using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElDespacho
{
    /// <summary>
    /// El Despacho — prototipo jugable.
    /// Se crea solo al pulsar Play en cualquier escena (ver Boot). Toda la interfaz se genera por código.
    /// </summary>
    public class DespachoGame : MonoBehaviour
    {
        /// <summary>Pon esto a false si más adelante quieres colocar el juego a mano en una escena concreta.</summary>
        public static bool AutoBoot = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (!AutoBoot) return;
            if (FindAnyObjectByType<DespachoGame>() != null) return;
            new GameObject("El Despacho").AddComponent<DespachoGame>();
        }

        // ───────────── Estado ─────────────

        const int YearsInMandate = 4;

        int estabilidad, seguridad, prosperidad;
        int year;
        List<Country> countries;
        readonly List<string> news = new List<string>();

        class Pending { public int year; public DelayedEvent ev; public Country country; }
        readonly List<Pending> pending = new List<Pending>();

        class Drain { public Effect perTurn; public int turns; public string reason; }
        readonly List<Drain> drains = new List<Drain>();

        int allies, destroyed, sanctioned;

        Transform root;
        Country current;
        int docTab;
        readonly List<string> dialogLog = new List<string>();

        int QuestionsPerVisit => year >= 4 ? 2 : 3;

        // ───────────── Arranque ─────────────

        void Start()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem));
                es.transform.SetParent(transform, false);
#if ENABLE_INPUT_SYSTEM
                var module = es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                module.AssignDefaultActions();
#else
                es.AddComponent<StandaloneInputModule>();
#endif
            }

            if (Camera.main == null)
            {
                var cam = new GameObject("Main Camera", typeof(Camera));
                cam.tag = "MainCamera";
                cam.transform.SetParent(transform, false);
            }
            if (FindAnyObjectByType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();

            Sfx.Init(gameObject);

            var cgo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            cgo.transform.SetParent(transform, false);
            var canvas = cgo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = cgo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            root = cgo.transform;

            NewGame();
            ShowTitle();
        }

        void NewGame()
        {
            estabilidad = 60;
            seguridad = 55;
            prosperidad = 50;
            year = 1;
            countries = CountryDatabase.Create();
            news.Clear();
            pending.Clear();
            drains.Clear();
            allies = destroyed = sanctioned = 0;
            cards = CardDatabase.Create();
            usedCards.Clear();
            flags.Clear();
            cardQueue.Clear();
            rng = new System.Random();
            currentCard = null;
            cardAnswered = false;
            documentOpened = false;
            cardsThisYear = 0;
            current = null;
            news.Insert(0, "<b>Nuevo líder en Aurelia</b>\nTomas posesión del cargo. El mundo observa qué tipo de líder serás.");
            news.Insert(0, "<b>Tres países piden audiencia</b>\nLindmark, Pavonia y Tarvessa esperan tu visita este año.");
        }

        // ───────────── Utilidades de estado ─────────────

        Country Find(string id) => countries.FirstOrDefault(c => c.id == id);

        IEnumerable<Country> ThisYear => countries.Where(c => c.year == year);

        int PendingThisYear => ThisYear.Count(c => c.decision == Decision.Ninguna);

        void Apply(Effect e)
        {
            estabilidad = Mathf.Clamp(estabilidad + e.estabilidad, 0, 100);
            seguridad = Mathf.Clamp(seguridad + e.seguridad, 0, 100);
            prosperidad = Mathf.Clamp(prosperidad + e.prosperidad, 0, 100);
        }

        bool IsDefeated => estabilidad <= 0 || seguridad <= 0 || prosperidad <= 0;

        static string Signed(int v) => v > 0 ? "+" + v : v.ToString();

        static string DeltaText(Effect e)
        {
            if (e.IsZero) return "<color=#bbbbbb>Sin cambios en los indicadores</color>";
            var parts = new List<string>();
            void Add(string name, int v)
            {
                if (v == 0) return;
                string col = v > 0 ? "#7fd18b" : "#ff7a6a";
                parts.Add($"<color={col}>{name} {Signed(v)}</color>");
            }
            Add("Estabilidad", e.estabilidad);
            Add("Seguridad", e.seguridad);
            Add("Prosperidad", e.prosperidad);
            return string.Join("     ", parts);
        }

        static string ProfileName(Profile p)
        {
            switch (p)
            {
                case Profile.Honesto: return "Honesto";
                case Profile.Mentiroso: return "Mentiroso";
                case Profile.Ambiguo: return "Ambiguo";
                default: return "Mal informado";
            }
        }

        void Clear()
        {
            // Destroy is deferred: deactivate first to stop stale input and document animations.
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
            deskDocument = null;
        }

        Image Background(Color c) => Kit.Panel(root, c, Vector2.zero, Vector2.one, default, default, "Background");

        // ───────────── HUD (indicadores siempre visibles) ─────────────

        void BuildHud()
        {
            var bar = Kit.Panel(root, Kit.Hex("#1a110a"), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -100), Vector2.zero, "HUD");
            Kit.Panel(bar.transform, Pal.Gold, new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, 3), "Line");

            Kit.Label(bar.transform, $"<b>{CountryDatabase.PlayerCountry.ToUpper()}</b>\n<size=20>Año {year} de {YearsInMandate}</size>", 30,
                Pal.Gold, TextAnchor.MiddleLeft, new Vector2(0, 0), new Vector2(0.18f, 1), new Vector2(30, 0), Vector2.zero);

            Indicator(bar.transform, "ESTABILIDAD", estabilidad, 0.19f);
            Indicator(bar.transform, "SEGURIDAD", seguridad, 0.42f);
            Indicator(bar.transform, "PROSPERIDAD", prosperidad, 0.65f);

            int left = PendingThisYear;
            Kit.Label(bar.transform, left > 0 ? $"Países por atender\n<b><size=30>{left}</size></b>" : "Año completado", 20,
                Pal.Cream, TextAnchor.MiddleRight, new Vector2(0.88f, 0), new Vector2(1, 1), Vector2.zero, new Vector2(-30, 0));
        }

        void Indicator(Transform p, string name, int v, float x)
        {
            const float w = 0.21f;
            Kit.Label(p, name, 20, Pal.Cream, TextAnchor.LowerLeft, new Vector2(x, 0.52f), new Vector2(x + w, 1), Vector2.zero, new Vector2(0, -10), FontStyle.Bold);
            Kit.Label(p, v.ToString(), 24, Kit.IndicatorColor(v), TextAnchor.LowerRight, new Vector2(x, 0.52f), new Vector2(x + w, 1), Vector2.zero, new Vector2(0, -10), FontStyle.Bold);
            Kit.Bar(p, v, Kit.IndicatorColor(v), new Vector2(x, 0.2f), new Vector2(x + w, 0.46f));
        }

        // ───────────── PANTALLA: Título ─────────────

        void ShowTitle()
        {
            Clear();
            Background(Pal.Wood);
            var paper = Kit.Panel(root, Pal.Paper, new Vector2(0.25f, 0.12f), new Vector2(0.75f, 0.88f));
            Kit.Panel(paper.transform, Pal.PaperDark, Vector2.zero, Vector2.one, new Vector2(14, 14), new Vector2(-14, -14)).color = Kit.WithAlpha(Pal.PaperDark, 0.35f);

            Kit.Label(paper.transform, "EL DESPACHO", 84, Pal.Ink, TextAnchor.MiddleCenter, new Vector2(0, 0.76f), new Vector2(1, 0.95f), default, default, FontStyle.Bold);
            Kit.Label(paper.transform, "<i>¿Puedo confiar en lo que me dicen?</i>", 30, Pal.InkSoft, TextAnchor.MiddleCenter, new Vector2(0, 0.68f), new Vector2(1, 0.77f));

            Kit.Label(paper.transform,
                "Acabas de llegar al poder en <b>Aurelia</b>, un país pequeño y próspero en un mundo inestable.\n\n" +
                "Cada día te llegarán problemas a la mesa y tendrás que responder <b>SÍ</b> o <b>NO</b>. Y cada año visitarás a otros líderes, leerás sus documentos y decidirás: " +
                "<color=#2f7a3a><b>alianza</b></color> o <color=#9a2a25><b>cortar lazos</b></color>.\n\n" +
                "Si la <b>Estabilidad</b>, la <b>Seguridad</b> o la <b>Prosperidad</b> llegan a cero, tu mandato termina antes de tiempo.\n\n" +
                "Los líderes no siempre dicen la verdad. Los informes, tampoco.",
                26, Pal.Ink, TextAnchor.UpperCenter, new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.65f), default, default, FontStyle.Normal, true);

            Kit.MakeButton(paper.transform, "ASUMIR EL CARGO", 32, Pal.Green, Color.white, () =>
            {
                Sfx.Play(Sfx.Thunk);
                ShowOffice();
            }, new Vector2(0.3f, 0.07f), new Vector2(0.7f, 0.18f));
        }

        // ───────────── PANTALLA 1: El despacho (cartas al estilo Reigns) ─────────────

        void ShowOffice()
        {
            if (currentCard == null) DrawCard();

            Clear();
            Background(Pal.Wood);
            // Vetas de madera
            for (int i = 0; i < 9; i++)
                Kit.Panel(root, Kit.WithAlpha(Pal.WoodMid, 0.35f), new Vector2(0, i / 9f), new Vector2(1, i / 9f), Vector2.zero, new Vector2(0, 4));
            BuildHud();

            // Ventana con vistas al castillo (refleja el estado del país)
            BuildWindow(new Vector2(0.015f, 0.47f), new Vector2(0.27f, 0.86f));

            // Nota del asesor
            var note = Kit.Panel(root, Kit.Hex("#efe3a8"), new Vector2(0.015f, 0.05f), new Vector2(0.27f, 0.43f));
            note.rectTransform.localEulerAngles = new Vector3(0, 0, 0.8f);
            Kit.Label(note.transform, "NOTA DEL ASESOR", 20, Pal.Red, TextAnchor.UpperLeft, Vector2.zero, Vector2.one, new Vector2(20, 0), new Vector2(-20, -14), FontStyle.Bold);
            Kit.Label(note.transform, AdvisorText(), 19, Pal.Ink, TextAnchor.UpperLeft, Vector2.zero, Vector2.one, new Vector2(20, 14), new Vector2(-20, -44), FontStyle.Italic, true);

            // La carta del día
            BuildCard(new Vector2(0.30f, 0.04f), new Vector2(0.70f, 0.87f));

            // Periódico
            var paper = Kit.Panel(root, Pal.Paper, new Vector2(0.73f, 0.40f), new Vector2(0.985f, 0.86f));
            Kit.Label(paper.transform, "LA GACETA", 32, Pal.Ink, TextAnchor.UpperCenter, Vector2.zero, Vector2.one, new Vector2(16, 0), new Vector2(-16, -12), FontStyle.Bold);
            Kit.Label(paper.transform, $"Año {year} del mandato", 16, Pal.InkSoft, TextAnchor.UpperCenter, Vector2.zero, Vector2.one, new Vector2(16, 0), new Vector2(-16, -52));
            Kit.Panel(paper.transform, Pal.Ink, new Vector2(0, 1), new Vector2(1, 1), new Vector2(18, -78), new Vector2(-18, -75));
            var sb = new StringBuilder();
            foreach (var n in news.Take(4)) sb.Append(n).Append("\n\n");
            Kit.Label(paper.transform, sb.ToString().TrimEnd(), 18, Pal.Ink, TextAnchor.UpperLeft, Vector2.zero, Vector2.one, new Vector2(20, 14), new Vector2(-20, -88), FontStyle.Normal, true);

            // Acciones
            int left = PendingThisYear;
            var mapBtn = Kit.MakeButton(root, left > 0 ? $"MAPA DEL MUNDO\n<size=18>{left} país{(left == 1 ? "" : "es")} esperan tu visita</size>" : "MAPA DEL MUNDO", 24, Pal.Blue, Color.white, () =>
            {
                Sfx.Play(Sfx.Paper);
                ShowMap();
            }, new Vector2(0.73f, 0.22f), new Vector2(0.985f, 0.36f));

            bool cardsDone = CardsDoneThisYear;
            string endLabel = year >= YearsInMandate ? "TERMINAR EL MANDATO" : $"CERRAR EL AÑO {year}";
            string label = !cardsDone ? "Atiende los asuntos de tu mesa" : left > 0 ? "Visita a los países pendientes" : endLabel;
            var endBtn = Kit.MakeButton(root, label, 22, Pal.Gold, Pal.Ink, () =>
            {
                Sfx.Play(Sfx.Thunk);
                EndYear();
            }, new Vector2(0.73f, 0.05f), new Vector2(0.985f, 0.18f));
            endBtn.interactable = left == 0 && cardsDone;
        }

        // ───────────── Mazo de cartas ─────────────

        const int CardsPerYear = 5;
        List<Card> cards;
        readonly HashSet<string> usedCards = new HashSet<string>();
        readonly HashSet<string> flags = new HashSet<string>();
        readonly List<string> cardQueue = new List<string>();
        System.Random rng = new System.Random();
        Card currentCard;
        bool cardAnswered;
        bool documentOpened;
        float cardAnsweredAt;
        DeskDocument deskDocument;
        string cardResultText;
        Effect cardResultEffect;
        int cardsThisYear;

        IEnumerable<Card> EligibleCards()
        {
            return cards.Where(k => !k.followUpOnly && !usedCards.Contains(k.id) &&
                                    year >= k.minYear && year <= k.maxYear &&
                                    (string.IsNullOrEmpty(k.requires) || flags.Contains(k.requires)));
        }

        bool CardsDoneThisYear => currentCard == null && cardQueue.Count == 0 &&
                                  (cardsThisYear >= CardsPerYear || !EligibleCards().Any());

        void DrawCard()
        {
            currentCard = null;
            cardAnswered = false;
            documentOpened = false;
            while (cardQueue.Count > 0 && currentCard == null)
            {
                string id = cardQueue[0];
                cardQueue.RemoveAt(0);
                var k = cards.FirstOrDefault(x => x.id == id);
                if (k != null && !usedCards.Contains(id)) currentCard = k;
            }
            if (currentCard == null && cardsThisYear < CardsPerYear)
            {
                var pool = EligibleCards().ToList();
                // Las cartas que reaccionan a tus decisiones tienen prioridad
                var story = pool.Where(k => !string.IsNullOrEmpty(k.requires)).ToList();
                if (story.Count > 0) pool = story;
                if (pool.Count > 0) currentCard = pool[rng.Next(pool.Count)];
            }
            if (currentCard != null)
            {
                usedCards.Add(currentCard.id);
                cardsThisYear++;
            }
        }

        void AnswerCard(bool yes)
        {
            var k = currentCard;
            if (k == null || cardAnswered || !documentOpened || deskDocument == null || !deskDocument.CanDecide) return;
            var eff = yes ? k.yes : k.no;
            Apply(eff);
            string flag = yes ? k.yesFlag : k.noFlag;
            if (!string.IsNullOrEmpty(flag)) flags.Add(flag);
            string next = yes ? k.yesNext : k.noNext;
            if (!string.IsNullOrEmpty(next)) cardQueue.Insert(0, next);
            cardAnswered = true;
            cardAnsweredAt = Time.unscaledTime;
            cardResultText = yes ? k.yesResult : k.noResult;
            cardResultEffect = eff;
            news.Insert(0, $"<b>{(yes ? k.yesLabel : k.noLabel)} a {k.who}</b>\n{cardResultText}");
            Sfx.Play(Sfx.Thunk);
            ShowOffice();
        }

        void ArchiveCard()
        {
            // A second click on a decision must not immediately dismiss its result.
            if (!cardAnswered || Time.unscaledTime - cardAnsweredAt < 0.3f ||
                deskDocument == null || !deskDocument.CanDecide) return;
            deskDocument.Dismiss(ContinueCard);
        }

        void ContinueCard()
        {
            Sfx.Play(Sfx.Click);
            currentCard = null;
            cardAnswered = false;
            documentOpened = false;
            if (IsDefeated) { ShowGameOver(); return; }
            ShowOffice();
        }

        void BuildCard(Vector2 aMin, Vector2 aMax)
        {
            var holder = Kit.Rect(root, "CardArea", aMin, aMax);
            var k = currentCard;
            if (k == null)
            {
                var empty = Kit.Panel(holder, Kit.WithAlpha(Pal.Paper, 0.12f), new Vector2(0.02f, 0.18f), new Vector2(0.98f, 1f));
                int left = PendingThisYear;
                string msg = left > 0
                    ? $"Tu mesa está despejada.\n\n{left} país{(left == 1 ? "" : "es")} espera{(left == 1 ? "" : "n")} tu visita.\nAbre el mapa del mundo."
                    : year >= YearsInMandate ? "Todo atendido.\n\nPuedes terminar el mandato." : $"Todo atendido.\n\nPuedes cerrar el año {year}.";
                Kit.Label(empty.transform, msg, 30, Pal.Cream, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(30, 30), new Vector2(-30, -30), FontStyle.Italic, true);
                return;
            }

            var desk = holder;
            var tray = Kit.Panel(desk, Pal.WoodLight, new Vector2(0.02f, 0.30f), new Vector2(0.98f, 0.99f), default, default, "ReadingZone");
            Kit.Panel(tray.transform, Pal.Gold, new Vector2(0, 1), Vector2.one, new Vector2(0, -3), Vector2.zero);
            var trayHint = Kit.Label(tray.transform, "", 24, Pal.Cream, TextAnchor.MiddleCenter,
                new Vector2(0.07f, 0.35f), new Vector2(0.93f, 0.65f), default, default, FontStyle.Normal, true);
            for (int i = 2; i >= 0; i--)
            {
                var pile = Kit.Panel(desk, i == 0 ? Pal.PaperDark : Pal.Paper,
                    new Vector2(0.10f, 0.025f), new Vector2(0.36f, 0.29f),
                    new Vector2(i * 5, -i * 4), new Vector2(i * 5, -i * 4), "PaperStack");
                pile.rectTransform.localEulerAngles = new Vector3(0, 0, -5 + i * 3);
            }
            Kit.Label(desk, "ASUNTOS PENDIENTES\nMantén pulsado y arrastra", 21, Pal.Cream, TextAnchor.MiddleLeft,
                new Vector2(0.42f, 0.06f), new Vector2(0.98f, 0.23f), default, default, FontStyle.Normal, true);
            var sheet = Kit.Panel(desk, Pal.Paper, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), default, default, "DeskDocument");
            sheet.raycastTarget = true;
            var shadow = Kit.Panel(sheet.transform, new Color(0, 0, 0, 0.28f), Vector2.zero, Vector2.one,
                new Vector2(12, -12), new Vector2(12, -12), "PaperShadow");
            Kit.Panel(sheet.transform, Pal.Paper, Vector2.zero, Vector2.one, default, default, "PaperSurface");
            var cover = Kit.Rect(sheet.transform, "ClosedCover", Vector2.zero, Vector2.one);
            Kit.Panel(cover, Pal.Gold, new Vector2(0.09f, 0.81f), new Vector2(0.91f, 0.83f));
            Kit.Label(cover, "AURELIA\nPRESIDENCIA", 58, Pal.Ink, TextAnchor.MiddleCenter,
                new Vector2(0.08f, 0.53f), new Vector2(0.92f, 0.8f), default, default, FontStyle.Bold, true);
            Kit.Label(cover, "EXPEDIENTE\nPENDIENTE DE REVISIÓN", 44, Pal.Red, TextAnchor.MiddleCenter,
                new Vector2(0.08f, 0.22f), new Vector2(0.92f, 0.5f), default, default, FontStyle.Bold, true);
            Kit.Label(cover, $"AÑO {year}  /  ASUNTO {cardsThisYear}", 38, Pal.InkSoft, TextAnchor.MiddleCenter,
                new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.2f));
            holder = Kit.Rect(sheet.transform, "OpenedContent", Vector2.zero, Vector2.one);

            var card = Kit.Panel(holder, Pal.Paper, new Vector2(0.02f, 0.18f), new Vector2(0.98f, 1f), default, default, "Card");
            var ct = card.transform;
            Kit.Panel(ct, k.color, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -10), Vector2.zero);

            Kit.Label(ct, "AURELIA / PRESIDENCIA", 18, Pal.Red, TextAnchor.UpperLeft,
                new Vector2(0.06f, 0.94f), new Vector2(0.63f, 0.99f), default, default, FontStyle.Bold);
            Kit.Panel(ct, Pal.PaperDark, new Vector2(0.06f, 0.64f), new Vector2(0.94f, 0.643f));
            Kit.Label(ct, $"Asunto {Mathf.Min(cardsThisYear, CardsPerYear)} de {CardsPerYear}", 17, Pal.InkSoft, TextAnchor.UpperRight, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-20, -18));

            var portrait = Kit.Panel(ct, Color.Lerp(k.color, Color.white, 0.25f), new Vector2(0.06f, 0.68f), new Vector2(0.30f, 0.92f), default, default, "Portrait");
            portrait.gameObject.AddComponent<RectMask2D>();
            BuildFace(portrait.transform, k.color, k.id, cardAnswered && cardResultEffect.estabilidad + cardResultEffect.seguridad + cardResultEffect.prosperidad < 0);

            Kit.Label(ct, $"<b>{k.who}</b>\n<size=19>{k.role}</size>", 27, Pal.Ink, TextAnchor.UpperCenter,
                new Vector2(0.34f, 0.69f), new Vector2(0.94f, 0.88f), default, default, FontStyle.Normal, true);

            if (!cardAnswered)
            {
                Kit.Label(ct, "«" + k.text + "»", 26, Pal.Ink, TextAnchor.UpperCenter,
                    new Vector2(0.07f, 0.18f), new Vector2(0.93f, 0.59f), default, default, FontStyle.Normal, true);
                var preview = Kit.Label(ct, "<i>Pasa el ratón por un botón para ver a qué afecta.</i>", 18, Pal.InkSoft, TextAnchor.MiddleCenter,
                    new Vector2(0.03f, 0.0f), new Vector2(0.97f, 0.09f));
                string idle = preview.text;

                var noBtn = Kit.MakeButton(holder, k.noLabel, 38, Pal.Red, Color.white, () => AnswerCard(false),
                    new Vector2(0.02f, 0f), new Vector2(0.48f, 0.15f));
                var yesBtn = Kit.MakeButton(holder, k.yesLabel, 38, Pal.Green, Color.white, () => AnswerCard(true),
                    new Vector2(0.52f, 0f), new Vector2(0.98f, 0.15f));
                AddHover(noBtn.gameObject, () => preview.text = PreviewText(k.no), () => preview.text = idle);
                AddHover(yesBtn.gameObject, () => preview.text = PreviewText(k.yes), () => preview.text = idle);
            }
            else
            {
                Kit.Label(ct, cardResultText, 26, Pal.Ink, TextAnchor.UpperCenter,
                    new Vector2(0.07f, 0.20f), new Vector2(0.93f, 0.59f), default, default, FontStyle.Italic, true);
                Kit.Label(ct, DeltaTextOnPaper(cardResultEffect), 24, Pal.Ink, TextAnchor.MiddleCenter,
                    new Vector2(0.03f, 0.02f), new Vector2(0.97f, 0.13f), default, default, FontStyle.Bold);
                Kit.MakeButton(holder, "CONTINUAR", 30, Pal.WoodLight, Pal.Cream, ArchiveCard,
                    new Vector2(0.25f, 0f), new Vector2(0.75f, 0.15f));
            }
            deskDocument = sheet.gameObject.AddComponent<DeskDocument>();
            deskDocument.Initialize(desk, tray, trayHint, cover.gameObject, holder.gameObject, shadow,
                documentOpened || cardAnswered, () => documentOpened = true);
        }

        /// <summary>Como en Reigns: dice QUÉ indicadores cambian, pero no si suben o bajan.</summary>
        static string PreviewText(Effect e)
        {
            var parts = new List<string>();
            void Add(string n, int v)
            {
                if (v == 0) return;
                parts.Add(Mathf.Abs(v) >= 6 ? $"<b>{n} (mucho)</b>" : n);
            }
            Add("Estabilidad", e.estabilidad);
            Add("Seguridad", e.seguridad);
            Add("Prosperidad", e.prosperidad);
            return parts.Count == 0 ? "No afecta a ningún indicador." : "Afectará a: " + string.Join("  ·  ", parts);
        }

        static void AddHover(GameObject go, System.Action enter, System.Action exit)
        {
            var et = go.AddComponent<EventTrigger>();
            var e1 = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            e1.callback.AddListener(_ => enter());
            et.triggers.Add(e1);
            var e2 = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            e2.callback.AddListener(_ => exit());
            et.triggers.Add(e2);
        }

        void BuildFace(Transform p, Color baseColor, string seed, bool frown)
        {
            int h = 0;
            foreach (char ch in seed) h = (h * 31 + ch) & 0x7fffffff;
            Color[] skins = { Kit.Hex("#f1c9a5"), Kit.Hex("#d9a47a"), Kit.Hex("#a8704a"), Kit.Hex("#7a4a2e"), Kit.Hex("#e8b896") };
            Color[] hairs = { Kit.Hex("#2a1a10"), Kit.Hex("#6a4a2a"), Kit.Hex("#c9c9c9"), Kit.Hex("#e0c070"), Kit.Hex("#1a1a1a") };
            Color skin = skins[h % skins.Length];
            Color hair = hairs[(h / 7) % hairs.Length];
            Color suit = Color.Lerp(baseColor, Color.black, 0.45f);

            Kit.Box(p, suit, new Vector2(0.5f, 0.05f), new Vector2(250, 190));
            Kit.Box(p, Color.white, new Vector2(0.5f, 0.2f), new Vector2(40, 90));
            Kit.Box(p, Color.Lerp(baseColor, Color.black, 0.2f), new Vector2(0.5f, 0.2f), new Vector2(14, 80));
            Kit.Dot(p, hair, new Vector2(0.5f, 0.66f), 132);
            Kit.Dot(p, skin, new Vector2(0.5f, 0.58f), 118);
            Kit.Dot(p, Pal.Ink, new Vector2(0.43f, 0.60f), 12);
            Kit.Dot(p, Pal.Ink, new Vector2(0.57f, 0.60f), 12);
            Kit.Box(p, frown ? Pal.Ink : Color.Lerp(skin, Pal.Red, 0.4f), new Vector2(0.5f, 0.50f), new Vector2(frown ? 30 : 38, frown ? 4 : 7));
        }

        string AdvisorText()
        {
            var sb = new StringBuilder(CountryDatabase.YearHint(year));
            int min = Mathf.Min(estabilidad, Mathf.Min(seguridad, prosperidad));
            string need = min == prosperidad ? "la PROSPERIDAD."
                        : min == seguridad ? "la SEGURIDAD."
                        : "la ESTABILIDAD.";
            sb.Append("\n\nLo que más me preocupa ahora es ").Append(need);
            foreach (var d in drains)
                sb.Append($"\nEfecto en curso: {d.reason} ({Signed(d.perTurn.prosperidad)} Prosperidad, {d.turns} decisiones más).");
            return sb.ToString();
        }

        void BuildWindow(Vector2 aMin, Vector2 aMax)
        {
            float avg = (estabilidad + seguridad + prosperidad) / 3f;
            int min = Mathf.Min(estabilidad, Mathf.Min(seguridad, prosperidad));
            float t = Mathf.InverseLerp(20f, 75f, avg);

            var frame = Kit.Panel(root, Pal.WoodLight, aMin, aMax, default, default, "Window");
            var sky = Kit.Panel(frame.transform, Color.Lerp(Kit.Hex("#5a5e64"), Kit.Hex("#8fcaf0"), t), Vector2.zero, Vector2.one, new Vector2(16, 16), new Vector2(-16, -16), "Sky");
            sky.gameObject.AddComponent<RectMask2D>();
            var s = sky.transform;

            if (avg >= 50) Kit.Dot(s, Kit.Hex("#ffe27a"), new Vector2(0.84f, 0.80f), 90);
            else
            {
                Kit.Dot(s, Kit.Hex("#8a8d92"), new Vector2(0.20f, 0.85f), 140);
                Kit.Dot(s, Kit.Hex("#7d8085"), new Vector2(0.32f, 0.88f), 120);
                Kit.Dot(s, Kit.Hex("#8a8d92"), new Vector2(0.75f, 0.84f), 150);
            }

            Color ground = Color.Lerp(Kit.Hex("#6b5a3a"), Kit.Hex("#5f9a4a"), Mathf.InverseLerp(20f, 75f, prosperidad));
            Kit.Dot(s, Color.Lerp(ground, Color.black, 0.1f), new Vector2(0.15f, -0.35f), 700);
            Kit.Dot(s, ground, new Vector2(0.85f, -0.40f), 800);
            Kit.Dot(s, ground, new Vector2(0.50f, -0.55f), 900);

            // Castillo
            Color stone = Kit.Hex("#b9b2a4");
            Color roof = estabilidad >= 35 ? Pal.Red : Kit.Hex("#5a4040");
            var keep = Kit.Box(s, stone, new Vector2(0.5f, 0.33f), new Vector2(170, 150), "Keep");
            for (int i = 0; i < 5; i++) Kit.Box(keep.transform, stone, new Vector2(0.1f + 0.2f * i, 1f), new Vector2(22, 26));
            Kit.Panel(keep.transform, Kit.Hex("#3a2a1a"), new Vector2(0.38f, 0), new Vector2(0.62f, 0.42f));
            foreach (float x in new[] { 0.33f, 0.67f })
            {
                var tower = Kit.Box(s, Color.Lerp(stone, Color.black, 0.07f), new Vector2(x, 0.38f), new Vector2(62, 220), "Tower");
                var r = Kit.Box(tower.transform, roof, new Vector2(0.5f, 1f), new Vector2(46, 46));
                r.rectTransform.localEulerAngles = new Vector3(0, 0, 45);
                Kit.Box(tower.transform, Kit.Hex("#2a2a2a"), new Vector2(0.5f, 0.7f), new Vector2(14, 22));

                if (estabilidad >= 35)
                {
                    var pole = Kit.Panel(tower.transform, Kit.Hex("#2a2a2a"), new Vector2(0.48f, 1f), new Vector2(0.52f, 1f), new Vector2(0, 30), new Vector2(0, 90));
                    Color flag = estabilidad >= 60 ? Pal.Gold : Kit.Hex("#9a8a6a");
                    Kit.Panel(pole.transform, flag, new Vector2(1, 0.55f), new Vector2(1, 1), Vector2.zero, new Vector2(38, 0));
                }
            }

            // Humo cuando hay crisis
            if (min < 35)
            {
                Color smoke = new Color(0.25f, 0.25f, 0.27f, 0.65f);
                Kit.Dot(s, smoke, new Vector2(0.60f, 0.50f), 40);
                Kit.Dot(s, smoke, new Vector2(0.63f, 0.62f), 60);
                Kit.Dot(s, smoke, new Vector2(0.68f, 0.76f), 85);
                Kit.Dot(s, smoke, new Vector2(0.74f, 0.92f), 110);
            }

            // Marco de la ventana
            Kit.Panel(frame.transform, Pal.WoodLight, new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(-7, 0), new Vector2(7, 0));
            Kit.Panel(frame.transform, Pal.WoodLight, new Vector2(0, 0.58f), new Vector2(1, 0.58f), new Vector2(0, -7), new Vector2(0, 7));

            string mood = avg >= 60 ? "El país está tranquilo." : avg >= 40 ? "Hay nubes sobre Aurelia." : "Aurelia está en crisis.";
            var tag = Kit.Panel(frame.transform, Kit.WithAlpha(Pal.Wood, 0.85f), new Vector2(0, 0), new Vector2(1, 0), new Vector2(16, 16), new Vector2(-16, 52));
            Kit.Label(tag.transform, mood, 20, Pal.Cream, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, default, default, FontStyle.Italic);
        }

        // ───────────── PANTALLA 2: Mapa del mundo ─────────────

        void ShowMap()
        {
            Clear();
            Background(Pal.Wood);
            BuildHud();

            var mapFrame = Kit.Panel(root, Pal.WoodLight, new Vector2(0.015f, 0.03f), new Vector2(0.78f, 0.885f), default, default, "MapFrame");
            var map = Kit.Panel(mapFrame.transform, Pal.Ocean, Vector2.zero, Vector2.one, new Vector2(12, 12), new Vector2(-12, -12), "Map");
            map.gameObject.AddComponent<RectMask2D>();
            var m = map.transform;

            // Masas de tierra decorativas
            var land = new (float x, float y, float size)[]
            {
                (0.38f, 0.55f, 560), (0.68f, 0.58f, 470), (0.22f, 0.80f, 360), (0.53f, 0.20f, 360),
                (0.84f, 0.27f, 330), (0.12f, 0.36f, 330), (0.47f, 0.84f, 330), (0.80f, 0.80f, 300),
                (0.28f, 0.35f, 300), (0.10f, 0.62f, 250), (0.62f, 0.38f, 260), (0.30f, 0.93f, 200),
            };
            foreach (var l in land) Kit.Dot(m, Pal.Land, new Vector2(l.x, l.y), l.size * 0.72f);
            foreach (var l in land) Kit.Dot(m, Kit.Hex("#d5c696"), new Vector2(l.x, l.y), l.size * 0.62f);

            // Líneas del bloque: de Aurelia a cada aliado
            Canvas.ForceUpdateCanvases();
            Vector2 mapSize = ((RectTransform)m).rect.size;
            foreach (var c in countries.Where(k => k.decision == Decision.Alianza))
                Line(m, CountryDatabase.PlayerMapPos, c.mapPos, mapSize, Kit.WithAlpha(Pal.Gold, 0.85f), 5);

            // Tu país
            Node(m, CountryDatabase.PlayerCountry + " (tú)", CountryDatabase.PlayerMapPos, Pal.Gold, "B", Pal.Gold, null, 1f, 70);

            // Resto de países
            foreach (var c in countries)
            {
                bool future = c.year > year;
                bool available = !future && c.decision == Decision.Ninguna;
                Color col = c.color;
                string label = c.name;
                string badge; Color badgeCol;

                if (future)
                {
                    col = Kit.Hex("#9a9a9a"); label = "???"; badge = ""; badgeCol = Color.clear;
                }
                else if (c.decision == Decision.Alianza)
                {
                    col = Pal.Gold; badge = "B"; badgeCol = Pal.Gold; label += "\n<size=15>tu bloque</size>";
                }
                else if (c.decision == Decision.Corte)
                {
                    col = c.attacked ? Kit.Hex("#3a3532") : Color.Lerp(c.color, Kit.Hex("#666666"), 0.7f);
                    badge = "i"; badgeCol = Pal.Blue;
                    label += c.attacked ? "\n<size=15>en ruinas</size>" : "\n<size=15>sancionado</size>";
                }
                else if (c.rumor == Rumor.Amenaza) { badge = "!"; badgeCol = Pal.Red; }
                else if (c.rumor == Rumor.Aliado) { badge = "+"; badgeCol = Pal.Green; }
                else { badge = "?"; badgeCol = Pal.Gray; }

                var target = c;
                Node(m, label, c.mapPos, col, badge, badgeCol,
                    available ? () => StartVisit(target) : (UnityEngine.Events.UnityAction)null,
                    future ? 0.35f : 1f, available ? 62 : 54);
            }

            // Barra lateral: leyenda y pendientes
            var side = Kit.Panel(root, Pal.Paper, new Vector2(0.79f, 0.03f), new Vector2(0.985f, 0.885f), default, default, "Side");
            var st = side.transform;
            Kit.Label(st, "MAPA DEL MUNDO", 26, Pal.Ink, TextAnchor.UpperCenter, Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, -16), FontStyle.Bold);
            Kit.Label(st, "Elige un país con el ratón.", 18, Pal.InkSoft, TextAnchor.UpperCenter, Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, -52), FontStyle.Italic);

            var legend = new (string b, Color c, string text)[]
            {
                ("B", Pal.Gold, "Tu país y tu bloque"),
                ("i", Pal.Blue, "Ya visitado"),
                ("?", Pal.Gray, "Sin visitar"),
                ("!", Pal.Red, "Posible amenaza"),
                ("+", Pal.Green, "Posible aliado"),
            };
            for (int i = 0; i < legend.Length; i++)
            {
                float y = 0.87f - i * 0.05f;
                var d = Kit.Dot(st, legend[i].c, new Vector2(0.13f, y), 30);
                Kit.Label(d.transform, legend[i].b, 18, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, default, default, FontStyle.Bold);
                Kit.Label(st, legend[i].text, 19, Pal.Ink, TextAnchor.MiddleLeft, new Vector2(0.24f, y - 0.025f), new Vector2(1, y + 0.025f));
            }
            Kit.Label(st, "<i>Ojo: amenaza y aliado son sospechas, no verdades.</i>", 17, Pal.Red, TextAnchor.UpperCenter,
                new Vector2(0.05f, 0.56f), new Vector2(0.95f, 0.63f), default, default, FontStyle.Normal, true);

            var sb = new StringBuilder($"<b>Agenda del año {year}</b>\n");
            foreach (var c in ThisYear)
            {
                string state = c.decision == Decision.Ninguna ? "<color=#8a6a2a>pendiente</color>"
                             : c.decision == Decision.Alianza ? "<color=#2f7a3a>alianza</color>"
                             : "<color=#9a2a25>lazos cortados</color>";
                sb.Append($"\n• {c.name}: {state}");
            }
            Kit.Label(st, sb.ToString(), 20, Pal.Ink, TextAnchor.UpperLeft, new Vector2(0.08f, 0.17f), new Vector2(0.95f, 0.55f), default, default, FontStyle.Normal, true);

            Kit.MakeButton(st, "VOLVER AL DESPACHO", 22, Pal.WoodMid, Pal.Cream, () =>
            {
                Sfx.Play(Sfx.Click);
                ShowOffice();
            }, new Vector2(0.08f, 0.03f), new Vector2(0.92f, 0.12f));
        }

        void Node(Transform map, string label, Vector2 pos, Color col, string badge, Color badgeCol,
            UnityEngine.Events.UnityAction onClick, float alpha, float size)
        {
            var ring = Kit.Dot(map, Kit.WithAlpha(Pal.Wood, 0.8f * alpha), pos, size + 8, "NodeRing");
            var dot = Kit.Dot(map, Kit.WithAlpha(col, alpha), pos, size, "Node");
            if (onClick != null)
            {
                dot.raycastTarget = true;
                var b = dot.gameObject.AddComponent<Button>();
                var cb = b.colors;
                cb.highlightedColor = new Color(1f, 1f, 1f, 0.75f);
                cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
                b.colors = cb;
                b.onClick.AddListener(() => { Sfx.Play(Sfx.Click); onClick(); });
                // Pulso: anillo dorado para los países que esperan visita
                ring.color = Kit.WithAlpha(Pal.Cream, 0.9f);
            }

            if (!string.IsNullOrEmpty(badge))
            {
                var bd = Kit.Dot(dot.transform, Kit.WithAlpha(badgeCol, alpha), new Vector2(0.88f, 0.88f), 30, "Badge");
                Kit.Label(bd.transform, badge, 20, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, default, default, FontStyle.Bold);
            }

            var lbl = Kit.Label(map, label, 20, Kit.WithAlpha(Pal.Ink, Mathf.Max(alpha, 0.6f)), TextAnchor.UpperCenter, pos, pos,
                new Vector2(-110, -size / 2f - 52), new Vector2(110, -size / 2f - 4), FontStyle.Bold);
            lbl.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        static void Line(Transform parent, Vector2 a, Vector2 b, Vector2 size, Color col, float width)
        {
            Vector2 pa = new Vector2(a.x * size.x, a.y * size.y);
            Vector2 pb = new Vector2(b.x * size.x, b.y * size.y);
            Vector2 d = pb - pa;
            var rt = Kit.Rect(parent, "Line", Vector2.zero, Vector2.zero);
            rt.sizeDelta = new Vector2(d.magnitude, width);
            rt.anchoredPosition = (pa + pb) / 2f;
            rt.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = col;
            img.raycastTarget = false;
        }

        // ───────────── PANTALLA 3 y 4: Visita (diálogo + documentos) ─────────────

        void StartVisit(Country c)
        {
            current = c;
            c.visited = true;
            c.questionsAsked = 0;
            foreach (var q in c.questions) q.asked = false;
            dialogLog.Clear();
            dialogLog.Add($"<b>{c.leader}:</b> «{c.speech}»");
            docTab = 0;
            Sfx.Play(Sfx.Paper);
            ShowVisit();
        }

        void ShowVisit()
        {
            var c = current;
            Clear();
            Background(Pal.Wood);
            BuildHud();

            // ── Diálogo (izquierda)
            var left = Kit.Panel(root, Pal.WoodMid, new Vector2(0.015f, 0.155f), new Vector2(0.49f, 0.885f), default, default, "Dialog");
            var lt = left.transform;

            var portrait = Kit.Panel(lt, Color.Lerp(c.color, Color.black, 0.25f), new Vector2(0.03f, 0.63f), new Vector2(0.28f, 0.97f), default, default, "Portrait");
            portrait.gameObject.AddComponent<RectMask2D>();
            BuildPortrait(portrait.transform, c);

            Kit.Label(lt, $"<b>{c.leader}</b>\n<size=20>{c.name} · {c.government}</size>", 28, Pal.Cream, TextAnchor.UpperLeft,
                new Vector2(0.31f, 0.78f), new Vector2(0.98f, 0.97f), default, default, FontStyle.Normal, true);

            int remaining = QuestionsPerVisit - c.questionsAsked;
            Kit.Label(lt, remaining > 0 ? $"Puedes hacer <b>{remaining}</b> pregunta{(remaining == 1 ? "" : "s")} más." : "No te quedan preguntas. Revisa los documentos y decide.",
                20, Pal.Gold, TextAnchor.MiddleLeft, new Vector2(0.31f, 0.64f), new Vector2(0.98f, 0.76f), default, default, FontStyle.Italic, true);

            var log = Kit.Panel(lt, Pal.Paper, new Vector2(0.03f, 0.27f), new Vector2(0.97f, 0.61f), default, default, "Log");
            Kit.Label(log.transform, string.Join("\n\n", dialogLog), 22, Pal.Ink, TextAnchor.UpperLeft, Vector2.zero, Vector2.one,
                new Vector2(18, 12), new Vector2(-18, -12), FontStyle.Normal, true);

            for (int i = 0; i < c.questions.Count; i++)
            {
                var q = c.questions[i];
                int col = i % 2, row = i / 2;
                var aMin = new Vector2(0.03f + col * 0.475f, 0.14f - row * 0.12f);
                var aMax = new Vector2(0.495f + col * 0.475f, 0.25f - row * 0.12f);
                var btn = Kit.MakeButton(lt, q.text, 20, q.asked ? Pal.Gray : Pal.WoodLight, Pal.Cream, () => Ask(q), aMin, aMax);
                btn.interactable = !q.asked && remaining > 0;
            }

            // ── Documentos (derecha)
            var right = Kit.Panel(root, Pal.WoodMid, new Vector2(0.505f, 0.155f), new Vector2(0.985f, 0.885f), default, default, "Docs");
            var rt = right.transform;
            string[] tabs = { "DOSSIER", "INTELIGENCIA", "COMERCIO", "REGIÓN Y RIESGO" };
            for (int i = 0; i < tabs.Length; i++)
            {
                int idx = i;
                bool on = i == docTab;
                Kit.MakeButton(rt, tabs[i], 18, on ? Pal.Paper : Pal.WoodLight, on ? Pal.Ink : Pal.Cream, () =>
                {
                    Sfx.Play(Sfx.Paper, 0.8f);
                    docTab = idx;
                    ShowVisit();
                }, new Vector2(0.02f + i * 0.24f, 0.91f), new Vector2(0.25f + i * 0.24f, 0.985f));
            }
            var doc = Kit.Panel(rt, Pal.Paper, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.9f), default, default, "Paper");
            BuildDocument(doc.transform, c);

            // ── Los dos botones
            var desk = Kit.Panel(root, Kit.Hex("#1a110a"), new Vector2(0, 0), new Vector2(1, 0.14f), default, default, "Buttons");
            var dt = desk.transform;
            Kit.MakeButton(dt, "HACER ALIANZA", 34, Pal.Green, Color.white, () => Decide(Decision.Alianza),
                new Vector2(0.08f, 0.15f), new Vector2(0.40f, 0.85f));
            Kit.MakeButton(dt, c.attackable ? "CORTAR LAZOS / ATACAR" : "CORTAR LAZOS / SANCIONAR", 34, Pal.Red, Color.white, () => Decide(Decision.Corte),
                new Vector2(0.60f, 0.15f), new Vector2(0.92f, 0.85f));
            Kit.Label(dt, "No hay opción de esperar.\nDecide.", 20, Pal.Cream, TextAnchor.MiddleCenter,
                new Vector2(0.40f, 0), new Vector2(0.60f, 1), default, default, FontStyle.Italic);
        }

        void BuildPortrait(Transform p, Country c)
        {
            int h = 0;
            foreach (char ch in c.id) h = (h * 31 + ch) & 0x7fffffff;
            Color[] skins = { Kit.Hex("#f1c9a5"), Kit.Hex("#d9a47a"), Kit.Hex("#a8704a"), Kit.Hex("#7a4a2e"), Kit.Hex("#e8b896") };
            Color[] hairs = { Kit.Hex("#2a1a10"), Kit.Hex("#6a4a2a"), Kit.Hex("#c9c9c9"), Kit.Hex("#e0c070"), Kit.Hex("#1a1a1a") };
            Color skin = skins[h % skins.Length];
            Color hair = hairs[(h / 7) % hairs.Length];
            Color suit = Color.Lerp(c.color, Color.black, 0.55f);

            // Bandera detrás
            Kit.Panel(p, Color.Lerp(c.color, Color.white, 0.25f), new Vector2(0.62f, 0.45f), new Vector2(1f, 1f));
            Kit.Panel(p, Color.Lerp(c.color, Color.black, 0.2f), new Vector2(0.62f, 0.72f), new Vector2(1f, 0.82f));

            Kit.Box(p, suit, new Vector2(0.5f, 0.05f), new Vector2(230, 190));
            Kit.Box(p, Color.white, new Vector2(0.5f, 0.2f), new Vector2(40, 90));
            Kit.Box(p, Color.Lerp(Pal.Red, Color.black, 0.1f), new Vector2(0.5f, 0.2f), new Vector2(14, 80));
            Kit.Dot(p, hair, new Vector2(0.5f, 0.66f), 128);
            Kit.Dot(p, skin, new Vector2(0.5f, 0.58f), 116);
            Kit.Dot(p, Pal.Ink, new Vector2(0.42f, 0.60f), 12);
            Kit.Dot(p, Pal.Ink, new Vector2(0.58f, 0.60f), 12);
            Kit.Box(p, current.annoyed ? Pal.Ink : Color.Lerp(skin, Pal.Red, 0.4f), new Vector2(0.5f, 0.50f), new Vector2(current.annoyed ? 30 : 36, current.annoyed ? 4 : 6));
            if (c.profile == Profile.Mentiroso || c.army.StartsWith("Grande"))
                Kit.Box(p, Pal.Gold, new Vector2(0.36f, 0.22f), new Vector2(22, 10)); // medalla
        }

        void BuildDocument(Transform d, Country c)
        {
            string title; string body;
            switch (docTab)
            {
                case 0:
                    title = "DOSSIER — " + c.name.ToUpper();
                    string rel = c.relation;
                    if (c.annoyed) rel += "  <color=#9a2a25>→ TENSA</color>";
                    if (c.grateful) rel += "  <color=#2f7a3a>→ AGRADECIDA</color>";
                    body = $"<b>Jefe de Estado:</b> {c.leader}\n\n<b>Población:</b> {c.population}\n\n<b>Tipo de gobierno:</b> {c.government}\n\n" +
                           $"<b>Tamaño del ejército:</b> {c.army}\n\n<b>Relación actual con Aurelia:</b> {rel}";
                    Kit.Panel(d, c.color, new Vector2(0.78f, 0.80f), new Vector2(0.95f, 0.93f));
                    Kit.Panel(d, Color.Lerp(c.color, Color.white, 0.5f), new Vector2(0.78f, 0.845f), new Vector2(0.95f, 0.885f));
                    break;
                case 1:
                    title = "INFORME DE INTELIGENCIA";
                    body = $"<size=18><color=#5a4a3a>Fuente: {c.intelSource}\nFecha: {c.intelDate}</color></size>\n\n{c.intel}";
                    var stamp = Kit.Label(d, "CONFIDENCIAL", 36, Kit.WithAlpha(Pal.Red, 0.55f), TextAnchor.MiddleCenter,
                        new Vector2(0.6f, 0.86f), new Vector2(0.98f, 0.98f), default, default, FontStyle.Bold);
                    stamp.rectTransform.localEulerAngles = new Vector3(0, 0, 12);
                    if (!string.IsNullOrEmpty(c.photo))
                    {
                        var photo = Kit.Panel(d, Kit.Hex("#f5f0e4"), new Vector2(0.56f, 0.06f), new Vector2(0.95f, 0.56f));
                        photo.rectTransform.localEulerAngles = new Vector3(0, 0, -2.5f);
                        var img = Kit.Panel(photo.transform, Kit.Hex("#6e6e6e"), new Vector2(0.06f, 0.26f), new Vector2(0.94f, 0.95f));
                        Kit.Dot(img.transform, Kit.Hex("#9a9a9a"), new Vector2(0.3f, 0.35f), 70);
                        Kit.Box(img.transform, Kit.Hex("#4a4a4a"), new Vector2(0.65f, 0.3f), new Vector2(120, 50));
                        Kit.Dot(img.transform, Kit.Hex("#bdbdbd"), new Vector2(0.8f, 0.8f), 36);
                        Kit.Label(photo.transform, "<i>" + c.photo + "</i>", 17, Pal.Ink, TextAnchor.MiddleCenter,
                            new Vector2(0.04f, 0f), new Vector2(0.96f, 0.26f), default, default, FontStyle.Normal, true);
                        Kit.Label(d, body, 22, Pal.Ink, TextAnchor.UpperLeft, new Vector2(0, 0), new Vector2(0.54f, 1),
                            new Vector2(28, 24), new Vector2(-10, -80), FontStyle.Normal, true);
                        Kit.Label(d, title, 28, Pal.Ink, TextAnchor.UpperLeft, Vector2.zero, Vector2.one, new Vector2(28, 0), new Vector2(-28, -24), FontStyle.Bold);
                        return;
                    }
                    break;
                case 2:
                    title = "COMERCIO";
                    body = $"<b>Exporta:</b> {c.exports}\n\n<b>Importa:</b> {c.imports}\n\n<b>¿Depende nuestra economía de este país?</b>\n{c.dependency}";
                    break;
                default:
                    title = "REGIÓN, ESTABILIDAD Y RIESGO";
                    var rivals = c.rivals.Select(Find).Where(r => r != null && r.year <= year).Select(r => r.name).ToList();
                    body = $"{c.region}\n\n<b>Rivales conocidos:</b> {(rivals.Count > 0 ? string.Join(", ", rivals) : "ninguno conocido")}";
                    Kit.Label(d, "Estabilidad interna", 22, Pal.Ink, TextAnchor.LowerLeft, new Vector2(0.06f, 0.30f), new Vector2(0.94f, 0.38f), default, default, FontStyle.Bold);
                    Kit.Bar(d, c.stabilityBar, Kit.Hex("#5fae6e"), new Vector2(0.06f, 0.24f), new Vector2(0.94f, 0.30f));
                    Kit.Label(d, "Riesgo para Aurelia", 22, Pal.Ink, TextAnchor.LowerLeft, new Vector2(0.06f, 0.13f), new Vector2(0.94f, 0.21f), default, default, FontStyle.Bold);
                    Kit.Bar(d, c.riskBar, Kit.Hex("#c94a3a"), new Vector2(0.06f, 0.07f), new Vector2(0.94f, 0.13f));
                    Kit.Label(d, title, 28, Pal.Ink, TextAnchor.UpperLeft, Vector2.zero, Vector2.one, new Vector2(28, 0), new Vector2(-28, -24), FontStyle.Bold);
                    Kit.Label(d, body, 22, Pal.Ink, TextAnchor.UpperLeft, new Vector2(0, 0.42f), new Vector2(1, 1), new Vector2(28, 0), new Vector2(-28, -80), FontStyle.Normal, true);
                    return;
            }
            Kit.Label(d, title, 28, Pal.Ink, TextAnchor.UpperLeft, Vector2.zero, Vector2.one, new Vector2(28, 0), new Vector2(-28, -24), FontStyle.Bold);
            Kit.Label(d, body, 23, Pal.Ink, TextAnchor.UpperLeft, Vector2.zero, Vector2.one, new Vector2(28, 24), new Vector2(-28, -80), FontStyle.Normal, true);
        }

        void Ask(Question q)
        {
            var c = current;
            if (q.asked || c.questionsAsked >= QuestionsPerVisit) return;
            q.asked = true;
            c.questionsAsked++;
            dialogLog.Add($"<color=#3d5f8a><b>Tú:</b> {q.text}</color>\n<b>{c.leader}:</b> «{q.answer}»");
            if (q.incomoda)
            {
                c.annoyed = true;
                dialogLog.Add("<color=#9a2a25><i>(La pregunta no le ha gustado nada. La relación se enfría.)</i></color>");
            }
            Sfx.Play(Sfx.Click);
            ShowVisit();
        }

        // ───────────── PANTALLA 5 y 6: Decisión y consecuencias ─────────────

        void Decide(Decision d)
        {
            var c = current;
            c.decision = d;
            flags.Add((d == Decision.Alianza ? "ally_" : "cut_") + c.id);
            Effect eff;
            var chain = new List<string>();

            if (d == Decision.Alianza)
            {
                allies++;
                eff = c.allyEffect;
                if (c.annoyed) { eff.prosperidad -= 3; chain.Add($"La relación con {c.name} estaba tensa: el acuerdo es menos generoso. (Prosperidad -3)"); }
                if (c.grateful) { eff.prosperidad += 3; chain.Add($"{c.name} te estaba agradecido: el acuerdo es más generoso. (Prosperidad +3)"); }
                foreach (var r in c.rivals.Select(Find).Where(r => r != null))
                {
                    if (r.decision == Decision.Alianza)
                    {
                        eff.estabilidad -= 6;
                        chain.Add($"{r.name}, ya en tu bloque, protesta: no entiende que pactes con {c.name}. (Estabilidad -6)");
                    }
                    else if (r.decision == Decision.Ninguna)
                    {
                        r.annoyed = true;
                        if (r.year <= year) chain.Add($"{r.name} toma nota de tu alianza con {c.name} y enfría la relación.");
                    }
                }
                if (c.allyLater != null) pending.Add(new Pending { year = year + c.allyLater.yearsLater, ev = c.allyLater, country = c });
                news.Insert(0, $"<b>{c.name} se une al bloque de Aurelia</b>\n{c.allyText}");
                Sfx.Play(Sfx.Thunk);
            }
            else
            {
                eff = c.cutEffect;
                c.attacked = c.attackable;
                if (c.attacked) destroyed++; else sanctioned++;
                foreach (var r in c.rivals.Select(Find).Where(r => r != null && r.decision == Decision.Ninguna))
                {
                    r.grateful = true;
                    if (r.year <= year) chain.Add($"{r.name}, rival de {c.name}, aplaude tu firmeza.");
                }
                if (c.cutLater != null) pending.Add(new Pending { year = year + c.cutLater.yearsLater, ev = c.cutLater, country = c });
                news.Insert(0, c.attacked
                    ? $"<b>Aurelia ataca {c.name}</b>\n{c.cutText}"
                    : $"<b>Aurelia rompe relaciones con {c.name}</b>\n{c.cutText}");
                Sfx.Play(c.attacked ? Sfx.Low : Sfx.Thunk);
            }

            if (year >= 4)
            {
                eff += new Effect(-2, -2, -2);
                chain.Add("La crisis global desgasta al país. (-2 en todo)");
            }

            // Efectos que duran varios turnos
            Effect drainTotal = default;
            foreach (var dr in drains)
            {
                if (dr.turns <= 0) continue;
                drainTotal += dr.perTurn;
                dr.turns--;
                chain.Add($"Efecto en curso: {dr.reason}. {DeltaText(dr.perTurn)}");
            }
            drains.RemoveAll(x => x.turns <= 0);
            if (d == Decision.Corte && c.cutTurns > 0)
                drains.Add(new Drain { perTurn = c.cutPerTurn, turns = c.cutTurns, reason = $"sin los suministros de {c.name}" });

            Effect total = eff + drainTotal;
            Apply(total);
            ShowConsequence(c, d, total, chain);
        }

        void ShowConsequence(Country c, Decision d, Effect total, List<string> chain)
        {
            Clear();
            bool attack = d == Decision.Corte && c.attacked;
            Color bg = d == Decision.Alianza ? Pal.Wood : attack ? Kit.Hex("#161616") : Kit.Hex("#2a1414");
            Background(bg);
            BuildHud();

            string title = d == Decision.Alianza ? $"{c.name} se une a tu bloque"
                         : attack ? $"{c.name} ha sido atacado"
                         : $"Rompes relaciones con {c.name}";
            Color titleCol = d == Decision.Alianza ? Pal.Gold : attack ? Kit.Hex("#9a9a9a") : Kit.Hex("#e08a7a");
            string text = d == Decision.Alianza ? c.allyText : c.cutText;

            Kit.Label(root, title, attack ? 52 : 58, titleCol, TextAnchor.MiddleCenter, new Vector2(0.1f, 0.68f), new Vector2(0.9f, 0.84f), default, default, FontStyle.Bold, true);
            Kit.Label(root, text, 28, attack ? Kit.Hex("#b0b0b0") : Pal.Cream, TextAnchor.UpperCenter, new Vector2(0.15f, 0.52f), new Vector2(0.85f, 0.67f), default, default, FontStyle.Italic, true);
            Kit.Label(root, DeltaText(total), 30, Color.white, TextAnchor.MiddleCenter, new Vector2(0.1f, 0.44f), new Vector2(0.9f, 0.51f), default, default, FontStyle.Bold);

            if (chain.Count > 0)
                Kit.Label(root, "<b>Reacciones:</b>\n" + string.Join("\n", chain.Select(x => "• " + x)), 22, Kit.WithAlpha(Pal.Cream, 0.85f), TextAnchor.UpperCenter,
                    new Vector2(0.15f, 0.18f), new Vector2(0.85f, 0.42f), default, default, FontStyle.Normal, true);

            Kit.MakeButton(root, "VOLVER AL DESPACHO", 26, attack ? Kit.Hex("#3a3a3a") : Pal.WoodLight, Pal.Cream, () =>
            {
                Sfx.Play(Sfx.Click);
                if (IsDefeated) ShowGameOver(); else ShowOffice();
            }, new Vector2(0.38f, 0.05f), new Vector2(0.62f, 0.14f));
        }

        // ───────────── Fin de año / mandato ─────────────

        List<Pending> FirePending(int upToYear)
        {
            var fired = pending.Where(p => p.year <= upToYear).ToList();
            foreach (var p in fired)
            {
                Apply(p.ev.effect);
                news.Insert(0, $"<b>{p.ev.headline}</b>\n{p.ev.body}");
            }
            pending.RemoveAll(p => p.year <= upToYear);
            return fired;
        }

        void EndYear()
        {
            if (year >= YearsInMandate)
            {
                var last = FirePending(int.MaxValue);
                ShowYearEvents("FIN DEL MANDATO", "Antes de dejar el despacho, llegan las últimas noticias...", last, "VER EL VEREDICTO", () =>
                {
                    if (IsDefeated) ShowGameOver(); else ShowEnding();
                });
                return;
            }

            year++;
            cardsThisYear = 0;
            var events = FirePending(year);
            var names = string.Join(", ", ThisYear.Select(c => c.name));
            news.Insert(0, $"<b>Comienza el año {year}</b>\nPiden audiencia: {names}.");
            ShowYearEvents($"AÑO {year} DE MANDATO", CountryDatabase.YearHint(year), events, "ENTRAR AL DESPACHO", () =>
            {
                if (IsDefeated) ShowGameOver(); else ShowOffice();
            });
        }

        void ShowYearEvents(string title, string subtitle, List<Pending> events, string button, UnityEngine.Events.UnityAction next)
        {
            Clear();
            Background(Kit.Hex("#140d08"));
            BuildHud();
            Kit.Label(root, title, 64, Pal.Gold, TextAnchor.MiddleCenter, new Vector2(0.1f, 0.74f), new Vector2(0.9f, 0.86f), default, default, FontStyle.Bold);
            Kit.Label(root, subtitle, 24, Pal.Cream, TextAnchor.UpperCenter, new Vector2(0.18f, 0.62f), new Vector2(0.82f, 0.73f), default, default, FontStyle.Italic, true);

            var paper = Kit.Panel(root, Pal.Paper, new Vector2(0.15f, 0.17f), new Vector2(0.85f, 0.6f));
            string body;
            if (events.Count == 0) body = "<i>Nada de lo que hiciste el año pasado ha vuelto a por ti... todavía.</i>";
            else body = string.Join("\n\n", events.Select(e => $"<b>{e.ev.headline}</b>\n{e.ev.body}\n{DeltaTextOnPaper(e.ev.effect)}"));
            Kit.Label(paper.transform, "<b>CONSECUENCIAS QUE LLEGAN TARDE</b>\n\n" + body, 23, Pal.Ink, TextAnchor.UpperLeft, Vector2.zero, Vector2.one,
                new Vector2(30, 20), new Vector2(-30, -20), FontStyle.Normal, true);

            Kit.MakeButton(root, button, 26, Pal.Gold, Pal.Ink, () => { Sfx.Play(Sfx.Thunk); next(); },
                new Vector2(0.38f, 0.04f), new Vector2(0.62f, 0.13f));
        }

        static string DeltaTextOnPaper(Effect e)
        {
            return DeltaText(e).Replace("#7fd18b", "#2f7a3a").Replace("#ff7a6a", "#9a2a25").Replace("#bbbbbb", "#5a4a3a");
        }

        // ───────────── Finales ─────────────

        void ShowGameOver()
        {
            Clear();
            Background(Kit.Hex("#120808"));
            string title, text;
            if (estabilidad <= 0)
            {
                title = "GOLPE DE ESTADO";
                text = "El pueblo ha perdido la confianza en ti. Una mañana, los tanques rodean el palacio. Tu mandato termina antes de tiempo.";
            }
            else if (seguridad <= 0)
            {
                title = "INVASIÓN";
                text = "Aurelia ya no puede defenderse. Tropas extranjeras cruzan la frontera y llegan hasta la capital.";
            }
            else
            {
                title = "COLAPSO ECONÓMICO";
                text = "Los bancos cierran, las tiendas se vacían y el país se para. Dimites entre abucheos.";
            }
            Kit.Label(root, title, 80, Kit.Hex("#c94a3a"), TextAnchor.MiddleCenter, new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.8f), default, default, FontStyle.Bold);
            Kit.Label(root, text, 30, Pal.Cream, TextAnchor.UpperCenter, new Vector2(0.18f, 0.42f), new Vector2(0.82f, 0.6f), default, default, FontStyle.Italic, true);
            Kit.Label(root, $"Caíste en el año {year} de {YearsInMandate}.", 24, Pal.Gray, TextAnchor.MiddleCenter, new Vector2(0.2f, 0.32f), new Vector2(0.8f, 0.4f));
            RevealAndRestart(new Vector2(0.2f, 0.15f), new Vector2(0.8f, 0.31f));
        }

        void ShowEnding()
        {
            Clear();
            Background(Pal.Wood);
            var decided = countries.Where(c => c.decision != Decision.Ninguna).ToList();
            int total = Mathf.Max(1, decided.Count);
            int correct = decided.Count(c => c.WasCorrect);
            int liarsAllied = decided.Count(c => c.profile == Profile.Mentiroso && c.decision == Decision.Alianza);
            int min = Mathf.Min(estabilidad, Mathf.Min(seguridad, prosperidad));
            int max = Mathf.Max(estabilidad, Mathf.Max(seguridad, prosperidad));
            float acc = correct / (float)total;

            string title, text;
            if (destroyed >= 5 || destroyed >= total * 0.5f)
            {
                title = "EL CONQUISTADOR";
                text = "Destruiste a la mayoría de quienes se cruzaron en tu camino. Aurelia es segura... y está sola, rodeada de ruinas. Nadie se atreve a llamarte.";
            }
            else if (liarsAllied >= 3)
            {
                title = "EL INGENUO";
                text = "Creíste demasiadas promesas. Tu bloque estaba lleno de mentirosos y se rompió desde dentro. Al menos nadie podrá decir que no confiabas en la gente.";
            }
            else if (acc >= 0.8f)
            {
                title = "EL ESTRATEGA";
                text = "Leíste entre líneas. Tus aliados fueron fieles y las amenazas reales quedaron neutralizadas. Pocos líderes ven tan claro en un mundo tan turbio.";
            }
            else if (allies >= total * 0.6f && destroyed <= 2 && min >= 30 && max - min <= 35)
            {
                title = "EL PACIFICADOR";
                text = "Tendiste la mano más veces de las que cerraste el puño. Aurelia termina el mandato en equilibrio y con muchos amigos.";
            }
            else
            {
                title = "EL SUPERVIVIENTE";
                text = "Aciertos, errores y mucha improvisación. No pasarás a la historia como un gran líder, pero Aurelia sigue en pie.";
            }

            Kit.Label(root, "FIN DEL MANDATO", 26, Pal.Cream, TextAnchor.MiddleCenter, new Vector2(0.1f, 0.88f), new Vector2(0.9f, 0.95f));
            Kit.Label(root, title, 76, Pal.Gold, TextAnchor.MiddleCenter, new Vector2(0.1f, 0.76f), new Vector2(0.9f, 0.88f), default, default, FontStyle.Bold);
            Kit.Label(root, text, 26, Pal.Cream, TextAnchor.UpperCenter, new Vector2(0.15f, 0.64f), new Vector2(0.85f, 0.76f), default, default, FontStyle.Italic, true);
            Kit.Label(root,
                $"Alianzas: {allies}   ·   Ataques: {destroyed}   ·   Sanciones: {sanctioned}   ·   Decisiones acertadas: {correct}/{decided.Count}\n" +
                $"Estabilidad {estabilidad}   ·   Seguridad {seguridad}   ·   Prosperidad {prosperidad}",
                22, Pal.Cream, TextAnchor.MiddleCenter, new Vector2(0.1f, 0.56f), new Vector2(0.9f, 0.64f));
            RevealAndRestart(new Vector2(0.12f, 0.15f), new Vector2(0.88f, 0.55f));
        }

        /// <summary>Revela los perfiles ocultos (invita a rejugar) y ofrece volver a empezar.</summary>
        void RevealAndRestart(Vector2 aMin, Vector2 aMax)
        {
            var paper = Kit.Panel(root, Pal.Paper, aMin, aMax);
            var sb = new StringBuilder("<b>LO QUE NO SABÍAS</b>\n");
            foreach (var c in countries.Where(k => k.year <= year))
            {
                string dec = c.decision == Decision.Alianza ? "alianza" : c.decision == Decision.Corte ? (c.attacked ? "atacado" : "sancionado") : "sin decidir";
                string ok = c.decision == Decision.Ninguna ? "" : c.WasCorrect ? " <color=#2f7a3a>(acierto)</color>" : " <color=#9a2a25>(error)</color>";
                sb.Append($"{c.name}: <b>{ProfileName(c.profile)}</b> ({dec}){ok}     ");
            }
            Kit.Label(paper.transform, sb.ToString(), 21, Pal.Ink, TextAnchor.UpperLeft, Vector2.zero, Vector2.one,
                new Vector2(24, 16), new Vector2(-24, -16), FontStyle.Normal, true);

            Kit.MakeButton(root, "JUGAR OTRA VEZ", 26, Pal.Green, Color.white, () =>
            {
                Sfx.Play(Sfx.Thunk);
                NewGame();
                ShowTitle();
            }, new Vector2(0.4f, 0.03f), new Vector2(0.6f, 0.12f));
        }
    }
}
