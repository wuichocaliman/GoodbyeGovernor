using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElDespacho
{
    /// <summary>
    /// Un sello que el jugador arrastra sobre el expediente abierto (estilo Papers, Please).
    /// Solo presentación: la decisión la aplica DespachoGame a través del callback.
    /// </summary>
    public sealed class StampTool : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        RectTransform self, area, target;
        Image targetImage;
        Color targetIdle;
        Vector2 home, offset;
        bool dragging, used;
        int pointerId;
        Action<bool> onHover;
        Action<Vector2> onStamped;

        /// <param name="area">Rect donde se mueve el sello (su padre).</param>
        /// <param name="homeAnchor">Posición de reposo, en coordenadas relativas (0..1) del área.</param>
        /// <param name="paper">Superficie del expediente sobre la que se puede sellar.</param>
        public void Initialize(RectTransform area, Vector2 homeAnchor, Image paper, Action<bool> hover, Action<Vector2> stamped)
        {
            self = (RectTransform)transform;
            this.area = area;
            targetImage = paper;
            target = paper.rectTransform;
            targetIdle = paper.color;
            onHover = hover;
            onStamped = stamped;
            Canvas.ForceUpdateCanvases();
            var r = area.rect;
            home = new Vector2(r.width * (homeAnchor.x - 0.5f), r.height * (homeAnchor.y - 0.5f));
            self.anchoredPosition = home;
            gameObject.AddComponent<AccessHitPadding>();
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (DeskPreferences.Paused) return;
            if (used || dragging || guided || e.button != PointerEventData.InputButton.Left) return;
            if(AccessPreferences.Current.simpleDocuments){AccessController.Instance?.SelectStamp(this);return;}
            if (!Local(area, e, out var local)) return;
            StopAllCoroutines();
            dragging = true;
            pointerId = e.pointerId;
            offset = self.anchoredPosition - local;
            self.SetAsLastSibling();
            self.localScale = Vector3.one * 1.12f;
            self.localRotation = Quaternion.Euler(0, 0, -6);
            onHover?.Invoke(true);
            Sfx.Play(Sfx.Click, 0.6f);
        }

        public void OnBeginDrag(PointerEventData e) { }

        public void OnDrag(PointerEventData e)
        {
            if (!dragging || e.pointerId != pointerId) return;
            if (Local(area, e, out var local)) self.anchoredPosition = local + offset;
            targetImage.color = OverPaper(e) ? Color.Lerp(targetIdle, Color.white, 0.35f) : targetIdle;
        }

        public void OnPointerUp(PointerEventData e) => Release(e);
        public void OnEndDrag(PointerEventData e) => Release(e);

        void Release(PointerEventData e)
        {
            if (DeskPreferences.Paused) return;
            if (!dragging || e.pointerId != pointerId || e.button != PointerEventData.InputButton.Left) return;
            dragging = false;
            targetImage.color = targetIdle;
            self.localScale = Vector3.one;
            self.localRotation = Quaternion.identity;

            if (OverPaper(e) && Local(target, e, out var onPaper))
            {
                used = true;
                // Que la marca del sello no se salga del papel
                var r = target.rect;
                onPaper.x = ClampInside(onPaper.x, r.xMin + 170, r.xMax - 170);
                onPaper.y = ClampInside(onPaper.y, r.yMin + 65, r.yMax - 65);
                onStamped?.Invoke(onPaper);
                return;
            }
            onHover?.Invoke(false);
            StartCoroutine(GoHome());
        }

        // ───── Mando: el sello baja hacia el papel según el gatillo (0..1) ─────
        bool guided;
        Vector2 guidedMark;
        public bool IsGuided => guided;
        public bool GuidedAvailable => !used && !dragging && !guided && isActiveAndEnabled && self != null && !DeskPreferences.Paused;
        public bool BeginGuided()
        {
            if (!GuidedAvailable) return false;
            StopAllCoroutines();
            guided = true;
            self.SetAsLastSibling();
            // Dónde quedará la marca: zona baja del papel, con algo de azar (como al sellar a mano)
            var r = target.rect;
            guidedMark = new Vector2(UnityEngine.Random.Range(-r.width * 0.18f, r.width * 0.18f),
                                     UnityEngine.Random.Range(-r.height * 0.30f, -r.height * 0.12f));
            onHover?.Invoke(true);
            Sfx.Play(Sfx.Click, 0.6f);
            return true;
        }
        public void SetGuidedProgress(float t)
        {
            if (!guided) return;
            float e = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t));
            // El centro del sello va hacia la marca, un poco por encima para que sea el pie el que toque el papel
            var world = target.TransformPoint(guidedMark);
            var goal = (Vector2)area.InverseTransformPoint(world) - area.rect.center + new Vector2(0, self.rect.height * 0.38f);
            self.anchoredPosition = Vector2.Lerp(home, goal, e);
            self.localScale = Vector3.one * (1f + 0.12f * Mathf.Sin(e * Mathf.PI));
            self.localRotation = Quaternion.Euler(0, 0, -6f * e);
            targetImage.color = t >= 0.6f ? Color.Lerp(targetIdle, Color.white, 0.35f) : targetIdle;
        }
        public void CommitGuided()
        {
            if (!guided) return;
            guided = false;
            targetImage.color = targetIdle;
            self.localScale = Vector3.one;
            self.localRotation = Quaternion.identity;
            used = true;
            onStamped?.Invoke(guidedMark);
        }
        public void CancelGuided()
        {
            if (!guided) return;
            guided = false;
            targetImage.color = targetIdle;
            self.localScale = Vector3.one;
            self.localRotation = Quaternion.identity;
            onHover?.Invoke(false);
            StartCoroutine(GoHome());
        }

        public bool AccessAvailable=>!used && !dragging && !guided && isActiveAndEnabled;
        public bool AccessOnTool(Vector2 p)=>RectTransformUtility.RectangleContainsScreenPoint(self,p) || (GetComponent<AccessHitPadding>()?.Contains(p) ?? false);
        public Transform AccessFocusTarget=>target;
        public bool AccessContains(Vector2 p)=>RectTransformUtility.RectangleContainsScreenPoint(target,p);
        public void AccessPreview(){if(AccessAvailable)onHover?.Invoke(true);}
        public void AccessApply(){if(!AccessAvailable||DeskPreferences.Paused)return;used=true;onStamped?.Invoke(Vector2.zero);}
        IEnumerator GoHome()
        {
            Vector2 start = self.anchoredPosition;
            float duration = DeskPreferences.AnimationDuration(0.18f);
            for (float t = 0; t < duration; t += DeskPreferences.GameDelta)
            {
                self.anchoredPosition = Vector2.Lerp(start, home, Mathf.SmoothStep(0, 1, t / duration));
                yield return null;
            }
            self.anchoredPosition = home;
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if (!dragging && !used) onHover?.Invoke(true);
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (!dragging && !used) onHover?.Invoke(false);
        }

        public void CancelDrag() => OnApplicationFocus(false);

        void OnApplicationFocus(bool focused)
        {
            if (!focused && guided) { CancelGuided(); return; }
            if (focused || !dragging) return;
            dragging = false;
            targetImage.color = targetIdle;
            self.localScale = Vector3.one;
            self.localRotation = Quaternion.identity;
            StartCoroutine(GoHome());
        }

        void OnDisable() => StopAllCoroutines();

        bool OverPaper(PointerEventData e) =>
            RectTransformUtility.RectangleContainsScreenPoint(target, e.position, e.pressEventCamera);

        static bool Local(RectTransform rt, PointerEventData e, out Vector2 p) =>
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, e.position, e.pressEventCamera, out p);

        static float ClampInside(float v, float min, float max) => min > max ? (min + max) / 2f : Mathf.Clamp(v, min, max);
    }
}
