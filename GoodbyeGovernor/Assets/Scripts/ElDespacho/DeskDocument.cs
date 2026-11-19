using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElDespacho
{
    // Presentation only: rules remain in DespachoGame.
    public sealed class DeskDocument : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        enum Phase { Pile, Dragging, Guided, Returning, Opening, Open, Dismissing }
        Phase phase;
        RectTransform sheet, desk, readingZone;
        Image paper, shadow, zone;
        Text hint;
        GameObject cover, content;
        CanvasGroup contentInput;
        Action onOpened;
        Vector2 pointerOffset;
        int pointerId;
        // Ajustes de presentación: se pueden cambiar ANTES de Initialize (el despacho ilustrado los usa).
        /// <summary>Tamaño del expediente cerrado respecto al abierto.</summary>
        public float closedScale = 0.28f;
        /// <summary>Giro del expediente cuando está en la pila.</summary>
        public float pileAngle = -5f;
        /// <summary>Si se indica, el expediente cerrado descansa sobre este objeto (p. ej. la bandeja).</summary>
        public RectTransform pileAnchor;
        /// <summary>Tamaño del expediente abierto respecto a la zona de la mesa.</summary>
        public Vector2 openSize = new Vector2(0.96f, 0.96f);
        public Color zoneIdle = new Color(0.38f, 0.30f, 0.18f, 0.85f);
        public Color zoneActive = new Color(0.53f, 0.45f, 0.24f, 1f);
        public string hintIdle = "ZONA DE LECTURA\nArrastra aquí el documento de la pila";
        public string hintActive = "SUELTA PARA ABRIR EL EXPEDIENTE";
        /// <summary>
        /// Si es true, el expediente no se ve en la pila: aparece bajo el ratón al pulsar la pila (se saca
        /// de la bandeja) y vuelve a «guardarse» si se suelta fuera de la zona de lectura.
        /// </summary>
        public bool hiddenInPile;
        /// <summary>Con hiddenInPile: escala del área invisible que se puede pulsar sobre la pila (0 = closedScale).</summary>
        public float hiddenPileScale;
        /// <summary>Con hiddenInPile: gráfico que se ilumina al pasar el ratón por la pila (p. ej. la bandeja).</summary>
        public Graphic pileHighlight;
        /// <summary>Se llama cuando el expediente sale de la pila (al cogerlo) y cuando vuelve a ella.</summary>
        public Action leftPile, backInPile;
        CanvasGroup sheetGroup;
        Coroutine pop;
        float RestScale => hiddenInPile && hiddenPileScale > 0 ? hiddenPileScale : closedScale;
        public bool CanDecide => phase == Phase.Open;
        /// <summary>Se lanza cuando el jugador coge un expediente de la pila (lo usa el tutorial).</summary>
        public static event Action Grabbed;
        Vector2 PilePosition
        {
            get
            {
                if (pileAnchor == null) return new Vector2(-desk.rect.width * 0.27f, -desk.rect.height * 0.34f);
                var world = pileAnchor.TransformPoint(pileAnchor.rect.center);
                return (Vector2)desk.InverseTransformPoint(world) - desk.rect.center;
            }
        }

        public void Initialize(RectTransform area, Image tray, Text trayHint, GameObject closedCover,
            GameObject openedContent, Image paperShadow, bool alreadyOpen, Action opened)
        {
            desk = area;
            sheet = (RectTransform)transform;
            paper = transform.Find("PaperSurface").GetComponent<Image>();
            readingZone = tray.rectTransform;
            zone = tray;
            hint = trayHint;
            cover = closedCover;
            content = openedContent;
            shadow = paperShadow;
            onOpened = opened;
            contentInput = content.AddComponent<CanvasGroup>();
            Canvas.ForceUpdateCanvases();
            sheet.sizeDelta = Vector2.Scale(desk.rect.size, openSize);
            phase = alreadyOpen ? Phase.Open : Phase.Pile;
            sheet.anchoredPosition = alreadyOpen ? Vector2.zero : PilePosition;
            sheet.localScale = Vector3.one * (alreadyOpen ? 1f : RestScale);
            if (hiddenInPile) sheetGroup = gameObject.AddComponent<CanvasGroup>();
            SetPileVisible(alreadyOpen);
            sheet.localRotation = Quaternion.Euler(0, 0, alreadyOpen ? 0 : pileAngle);
            cover.SetActive(!alreadyOpen);
            content.SetActive(alreadyOpen);
            contentInput.interactable = contentInput.blocksRaycasts = alreadyOpen;
            Highlight(false);
            gameObject.AddComponent<AccessHitPadding>();
        }
        public void OnPointerDown(PointerEventData e)
        {
            if (DeskPreferences.Paused) return;
            if (e.button != PointerEventData.InputButton.Left || phase != Phase.Pile) return;
            if(AccessPreferences.Current.simpleDocuments){AccessSelect();return;}
            if (!LocalPointer(e, out var local)) return;
            phase = Phase.Dragging;
            pointerId = e.pointerId;
            pointerOffset = sheet.anchoredPosition - local;
            desk.SetAsLastSibling();
            sheet.SetAsLastSibling();
            sheet.localRotation = Quaternion.identity;
            paper.color = Pal.Paper;
            shadow.color = new Color(0, 0, 0, 0.48f);
            if (hiddenInPile)
            {
                // Se saca un expediente de la bandeja: aparece bajo el ratón y crece un poco
                SetPileVisible(true);
                PileHover(false);
                sheet.anchoredPosition = local;
                pointerOffset = Vector2.zero;
                if (pop != null) StopCoroutine(pop);
                pop = StartCoroutine(Pop());
            }
            Sfx.Play(Sfx.Paper, 0.35f);
            leftPile?.Invoke();
            Grabbed?.Invoke();
        }
        public void OnBeginDrag(PointerEventData e) { }
        public void OnDrag(PointerEventData e)
        {
            if (phase != Phase.Dragging || e.pointerId != pointerId) return;
            if (LocalPointer(e, out var local)) sheet.anchoredPosition = local + pointerOffset;
            Highlight(IsOverTray(e));
        }
        public void OnPointerUp(PointerEventData e) => Release(e);
        public void OnEndDrag(PointerEventData e) => Release(e);
        void Release(PointerEventData e)
        {
            if (DeskPreferences.Paused) return;
            if (phase != Phase.Dragging || e.pointerId != pointerId ||
                e.button != PointerEventData.InputButton.Left) return;
            bool accepted = IsOverTray(e);
            if (pop != null) { StopCoroutine(pop); pop = null; }
            AnimatePlacement(accepted);
        }
        void AnimatePlacement(bool accepted) {
            accessSelected=false;
            Highlight(false);
            phase = accepted ? Phase.Opening : Phase.Returning;
            Sfx.Play(Sfx.Paper, 0.3f);
            StartCoroutine(Move(accepted ? Vector2.zero : PilePosition,
                accepted ? 1f : ReturnScale, accepted ? 0 : pileAngle, 0.22f, () =>
                {
                    shadow.color = new Color(0, 0, 0, 0.28f);
                    if (accepted)
                    {
                        cover.SetActive(false);
                        content.SetActive(true);
                        contentInput.interactable = contentInput.blocksRaycasts = true;
                        phase = Phase.Open;
                        Haptics.DocumentFits(); // golpe corto al encajar en el vade (solo con mando)
                        onOpened?.Invoke();
                    }
                    else { phase = Phase.Pile; GoBackToPile(); }
                }));
        }
        /// <summary>Al volver a la bandeja, el expediente oculto se encoge hacia dentro y desaparece.</summary>
        float ReturnScale => hiddenInPile ? closedScale * 0.6f : closedScale;
        void GoBackToPile()
        {
            backInPile?.Invoke();
            if (!hiddenInPile) return;
            SetPileVisible(false);
            sheet.localScale = Vector3.one * RestScale;
        }
        void SetPileVisible(bool visible)
        {
            if (sheetGroup == null) return;
            sheetGroup.alpha = visible ? 1f : 0f; // invisible pero se sigue pudiendo pulsar
        }
        void PileHover(bool on)
        {
            if (pileHighlight != null) pileHighlight.color = on ? new Color(1f, 0.92f, 0.76f, 1f) : Color.white;
        }
        IEnumerator Pop()
        {
            float duration = DeskPreferences.AnimationDuration(0.12f);
            Vector3 from = Vector3.one * closedScale * 0.6f, to = Vector3.one * closedScale;
            for (float t = 0; t < duration; t += DeskPreferences.GameDelta)
            {
                sheet.localScale = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, t / duration));
                yield return null;
            }
            sheet.localScale = to;
            pop = null;
        }
        // ───── Mando: el expediente viaja de la bandeja al vade según el gatillo (0..1) ─────
        public bool GuidedAvailable => phase == Phase.Pile && !accessSelected && !DeskPreferences.Paused;
        public bool IsGuided => phase == Phase.Guided;
        /// <summary>Saca el expediente de la pila para moverlo con el mando. False si ahora no se puede.</summary>
        public bool BeginGuided()
        {
            if (!GuidedAvailable) return false;
            StopAllCoroutines();
            phase = Phase.Guided;
            desk.SetAsLastSibling();
            sheet.SetAsLastSibling();
            SetPileVisible(true);
            PileHover(false);
            paper.color = Pal.Paper;
            shadow.color = new Color(0, 0, 0, 0.48f);
            Sfx.Play(Sfx.Paper, 0.35f);
            leftPile?.Invoke();
            Grabbed?.Invoke();
            SetGuidedProgress(0);
            return true;
        }
        /// <summary>0 = en la bandeja, 1 = sobre la zona de lectura. No abre nada: eso lo hace CommitGuided.</summary>
        public void SetGuidedProgress(float t)
        {
            if (phase != Phase.Guided) return;
            float e = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t));
            float startScale = hiddenInPile ? closedScale * 0.6f : closedScale;
            sheet.anchoredPosition = Vector2.Lerp(PilePosition, Vector2.zero, e);
            sheet.localScale = Vector3.one * Mathf.Lerp(startScale, closedScale * 1.25f, e);
            sheet.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(pileAngle, 0, e));
            Highlight(t >= 0.6f);
        }
        /// <summary>Gatillo a fondo: se abre igual que al soltarlo con el ratón en la zona de lectura.</summary>
        public void CommitGuided() { if (phase == Phase.Guided) AnimatePlacement(true); }
        /// <summary>Gatillo suelto antes del fondo: vuelve a la bandeja.</summary>
        public void CancelGuided() { if (phase == Phase.Guided) AnimatePlacement(false); }

        bool accessSelected;
        public bool AccessSelected=>accessSelected;
        public int AccessSelectionFrame { get; private set; } = -1;
        // The pile hit surface can be transparent; focus belongs on the visible tray instead.
        public Transform AccessFocusTarget => accessSelected ? readingZone : hiddenInPile && pileAnchor != null ? pileAnchor : transform;
        public bool AccessAvailable=>phase==Phase.Pile;
        public bool AccessOnSheet(Vector2 p)=>RectTransformUtility.RectangleContainsScreenPoint(sheet,p) || (GetComponent<AccessHitPadding>()?.Contains(p) ?? false);
        public bool AccessOnTray(Vector2 p)=>AccessHitPadding.ContainsZone(readingZone,p);
        public void AccessSelect(){if(DeskPreferences.Paused||phase!=Phase.Pile||accessSelected)return;accessSelected=true;AccessSelectionFrame=Time.frameCount;if(hiddenInPile){SetPileVisible(true);sheet.localScale=Vector3.one*closedScale;}leftPile?.Invoke();Highlight(true);hint.text="[D] EXPEDIENTE SELECCIONADO\nPulse aquí para abrir. ESC cancela.";Grabbed?.Invoke();AccessController.Cue("Expediente seleccionado. Pulse la zona de lectura.");}
        public void AccessPlace(){if(!accessSelected||DeskPreferences.Paused||phase!=Phase.Pile)return;AnimatePlacement(true);}
        public void AccessCancel(){if(!accessSelected)return;accessSelected=false;GoBackToPile();Highlight(false);AccessController.Cue("Selección cancelada");}
        public void Dismiss(Action finished)
        {
            if (phase != Phase.Open) return;
            phase = Phase.Dismissing;
            contentInput.interactable = contentInput.blocksRaycasts = false;
            Sfx.Play(Sfx.Paper, 0.35f);
            StartCoroutine(Move(new Vector2(desk.rect.width * 0.35f, -desk.rect.height * 0.65f),
                0.12f, 12, 0.2f, finished));
        }
        IEnumerator Move(Vector2 target, float scale, float angle, float duration, Action finished)
        {
            duration = DeskPreferences.AnimationDuration(duration);
            Vector2 start = sheet.anchoredPosition;
            Vector3 startScale = sheet.localScale;
            Quaternion startRotation = sheet.localRotation;
            for (float elapsed = 0; elapsed < duration; elapsed += DeskPreferences.GameDelta)
            {
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                sheet.anchoredPosition = Vector2.Lerp(start, target, t);
                sheet.localScale = Vector3.Lerp(startScale, Vector3.one * scale, t);
                sheet.localRotation = Quaternion.Slerp(startRotation, Quaternion.Euler(0, 0, angle), t);
                yield return null;
            }
            while (DeskPreferences.Paused) yield return null;
            sheet.anchoredPosition = target;
            sheet.localScale = Vector3.one * scale;
            sheet.localRotation = Quaternion.Euler(0, 0, angle);
            finished?.Invoke();
        }
        bool LocalPointer(PointerEventData e, out Vector2 point) =>
            RectTransformUtility.ScreenPointToLocalPointInRectangle(desk, e.position, e.pressEventCamera, out point);
        bool IsOverTray(PointerEventData e) =>
            AccessHitPadding.ContainsZone(readingZone, e.position, e.pressEventCamera);
        void Highlight(bool active)
        {
            zone.color = active ? zoneActive : zoneIdle;
            hint.text = active ? hintActive : hintIdle;
            UpdateHint();
        }
        void LateUpdate() { UpdateHint(); }
        void UpdateHint() {
            if(hint==null || phase==Phase.Open || phase==Phase.Opening || phase==Phase.Dismissing)return;
            if(accessSelected && !AccessPreferences.Current.simpleDocuments){hint.text="EXPEDIENTE SELECCIONADO\nINTRO: colocar en lectura · ESC: cancelar";return;}
            if(AccessPreferences.Current.simpleDocuments)
                hint.text=accessSelected?"2. PULSA AQUÍ PARA ABRIR\nESC cancela la selección":"1. PULSA EL EXPEDIENTE\nEstá en la bandeja de la derecha";
            else if(phase==Phase.Pile)hint.text=hintIdle;
        }
        public void OnPointerEnter(PointerEventData e)
        {
            if (phase != Phase.Pile) return;
            if (hiddenInPile) PileHover(true);
            else paper.color = Color.Lerp(Pal.Paper, Color.white, 0.22f);
        }
        public void OnPointerExit(PointerEventData e)
        {
            if (phase != Phase.Pile) return;
            if (hiddenInPile) PileHover(false);
            else paper.color = Pal.Paper;
        }
        public void CancelDrag() {AccessCancel();OnApplicationFocus(false);}

        void OnApplicationFocus(bool focused)
        {
            if (focused || (phase != Phase.Dragging && phase != Phase.Guided)) return;
            phase = Phase.Returning;
            Highlight(false);
            if (pop != null) { StopCoroutine(pop); pop = null; }
            StartCoroutine(Move(PilePosition, ReturnScale, pileAngle, 0.18f, () =>
            {
                paper.color = Pal.Paper;
                shadow.color = new Color(0, 0, 0, 0.28f);
                phase = Phase.Pile;
                GoBackToPile();
            }));
        }
        void OnDisable() => StopAllCoroutines();
    }
}
