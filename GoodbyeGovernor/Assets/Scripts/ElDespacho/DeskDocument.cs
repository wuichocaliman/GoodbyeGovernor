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
        enum Phase { Pile, Dragging, Returning, Opening, Open, Dismissing }
        Phase phase;
        RectTransform sheet, desk, readingZone;
        Image paper, shadow, zone;
        Text hint;
        GameObject cover, content;
        CanvasGroup contentInput;
        Action onOpened;
        Vector2 pointerOffset;
        int pointerId;
        const float ClosedScale = 0.28f;
        static readonly Color ZoneIdle = new Color(0.38f, 0.30f, 0.18f, 0.85f);
        public bool CanDecide => phase == Phase.Open;
        Vector2 PilePosition => new Vector2(-desk.rect.width * 0.27f, -desk.rect.height * 0.34f);

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
            sheet.sizeDelta = Vector2.Scale(desk.rect.size, new Vector2(0.96f, 0.96f));
            phase = alreadyOpen ? Phase.Open : Phase.Pile;
            sheet.anchoredPosition = alreadyOpen ? Vector2.zero : PilePosition;
            sheet.localScale = Vector3.one * (alreadyOpen ? 1f : ClosedScale);
            sheet.localRotation = Quaternion.Euler(0, 0, alreadyOpen ? 0 : -5);
            cover.SetActive(!alreadyOpen);
            content.SetActive(alreadyOpen);
            contentInput.interactable = contentInput.blocksRaycasts = alreadyOpen;
            Highlight(false);
        }
        public void OnPointerDown(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left || phase != Phase.Pile) return;
            if (!LocalPointer(e, out var local)) return;
            phase = Phase.Dragging;
            pointerId = e.pointerId;
            pointerOffset = sheet.anchoredPosition - local;
            desk.SetAsLastSibling();
            sheet.SetAsLastSibling();
            sheet.localRotation = Quaternion.identity;
            paper.color = Pal.Paper;
            shadow.color = new Color(0, 0, 0, 0.48f);
            Sfx.Play(Sfx.Paper, 0.35f);
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
            if (phase != Phase.Dragging || e.pointerId != pointerId ||
                e.button != PointerEventData.InputButton.Left) return;
            bool accepted = IsOverTray(e);
            Highlight(false);
            phase = accepted ? Phase.Opening : Phase.Returning;
            Sfx.Play(Sfx.Paper, 0.3f);
            StartCoroutine(Move(accepted ? Vector2.zero : PilePosition,
                accepted ? 1f : ClosedScale, accepted ? 0 : -5, 0.22f, () =>
                {
                    shadow.color = new Color(0, 0, 0, 0.28f);
                    if (accepted)
                    {
                        cover.SetActive(false);
                        content.SetActive(true);
                        contentInput.interactable = contentInput.blocksRaycasts = true;
                        phase = Phase.Open;
                        onOpened?.Invoke();
                    }
                    else phase = Phase.Pile;
                }));
        }
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
            Vector2 start = sheet.anchoredPosition;
            Vector3 startScale = sheet.localScale;
            Quaternion startRotation = sheet.localRotation;
            for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float t = Mathf.SmoothStep(0, 1, elapsed / duration);
                sheet.anchoredPosition = Vector2.Lerp(start, target, t);
                sheet.localScale = Vector3.Lerp(startScale, Vector3.one * scale, t);
                sheet.localRotation = Quaternion.Slerp(startRotation, Quaternion.Euler(0, 0, angle), t);
                yield return null;
            }
            sheet.anchoredPosition = target;
            sheet.localScale = Vector3.one * scale;
            sheet.localRotation = Quaternion.Euler(0, 0, angle);
            finished?.Invoke();
        }
        bool LocalPointer(PointerEventData e, out Vector2 point) =>
            RectTransformUtility.ScreenPointToLocalPointInRectangle(desk, e.position, e.pressEventCamera, out point);
        bool IsOverTray(PointerEventData e) =>
            RectTransformUtility.RectangleContainsScreenPoint(readingZone, e.position, e.pressEventCamera);
        void Highlight(bool active)
        {
            zone.color = active ? new Color(0.53f, 0.45f, 0.24f, 1f) : ZoneIdle;
            hint.text = active ? "SUELTA PARA ABRIR EL EXPEDIENTE" : "ZONA DE LECTURA\nArrastra aquí el documento de la pila";
        }
        public void OnPointerEnter(PointerEventData e)
        {
            if (phase == Phase.Pile) paper.color = Color.Lerp(Pal.Paper, Color.white, 0.22f);
        }
        public void OnPointerExit(PointerEventData e)
        {
            if (phase == Phase.Pile) paper.color = Pal.Paper;
        }
        void OnApplicationFocus(bool focused)
        {
            if (focused || phase != Phase.Dragging) return;
            phase = Phase.Returning;
            Highlight(false);
            StartCoroutine(Move(PilePosition, ClosedScale, -5, 0.18f, () =>
            {
                paper.color = Pal.Paper;
                shadow.color = new Color(0, 0, 0, 0.28f);
                phase = Phase.Pile;
            }));
        }
        void OnDisable() => StopAllCoroutines();
    }
}
