using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ElDespacho
{
    /// <summary>
    /// Acceso al pixel art del juego (Assets/Resources/Art/) y ayudas para colocarlo en la interfaz.
    /// Todo es opcional: si una imagen no está, cada método devuelve null y el juego usa el aspecto
    /// anterior, generado por código. Así se puede añadir arte poco a poco sin romper nada.
    /// </summary>
    public static class Art
    {
        /// <summary>Cuántos píxeles de pantalla (a 1920x1080) ocupa cada píxel del arte en las piezas estiradas.</summary>
        public const float PixelScale = 4f;

        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        /// <summary>Retratos por país (id del país → archivo en Art/Retratos). Añadir aquí los nuevos.</summary>
        static readonly Dictionary<string, string> portraits = new Dictionary<string, string>
        {
            { "korovia", "Retrato_Korovia" },
            { "selvana", "Retrato_Valdoria" }, // provisional: el pack lo llama «Valdoria», país que no existe en el juego
        };

        /// <summary>Carga un sprite de Resources/Art/ (por ejemplo "UI/UI_Dossier"). Null si no existe.</summary>
        public static Sprite Get(string path)
        {
            if (!cache.TryGetValue(path, out var sprite))
            {
                sprite = Resources.Load<Sprite>("Art/" + path);
                cache[path] = sprite;
            }
            return sprite;
        }

        public static Sprite Portrait(string countryId) =>
            countryId != null && portraits.TryGetValue(countryId, out var file) ? Get("Retratos/" + file) : null;

        /// <summary>Icono del mapa según la letra que usaba la versión anterior.</summary>
        public static Sprite MapIcon(string badge)
        {
            switch (badge)
            {
                case "B": return Get("Iconos/Icono_TuPais");
                case "i": return Get("Iconos/Icono_Visitado");
                case "?": return Get("Iconos/Icono_SinVisitar");
                case "!": return Get("Iconos/Icono_Amenaza");
                case "+": return Get("Iconos/Icono_Aliado");
                default: return null;
            }
        }

        /// <summary>Un sprite cuadrado de tamaño fijo centrado en un punto relativo del padre (iconos).</summary>
        public static Image Icon(Transform parent, Sprite sprite, Vector2 anchor, float size, float alpha = 1f, string name = "Icon")
        {
            var rt = Kit.Rect(parent, name, anchor, anchor);
            rt.sizeDelta = new Vector2(size, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.color = new Color(1, 1, 1, alpha);
            img.raycastTarget = false;
            return img;
        }

        /// <summary>Un sprite que ocupa una zona del padre conservando su proporción (retratos, ilustraciones).</summary>
        public static Image Fill(Transform parent, Sprite sprite, Vector2 aMin, Vector2 aMax, string name = "Art")
        {
            var rt = Kit.Rect(parent, name, aMin, aMax);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>Un sprite estirado con 9-slice: los bordes conservan el pixel art a PixelScale.</summary>
        public static Image Sliced(Transform parent, Sprite sprite, Vector2 aMin, Vector2 aMax, string name = "Sliced")
        {
            var rt = Kit.Rect(parent, name, aMin, aMax);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1f / PixelScale; // sprites a 100 PPU: 1 píxel de arte = PixelScale unidades
            img.raycastTarget = false;
            return img;
        }

        /// <summary>
        /// Botón de opción con el arte del pack (normal / seleccionado al pasar el ratón).
        /// Devuelve null si faltan las imágenes, para que quien llama use el botón anterior.
        /// </summary>
        public static Button OptionButton(Transform parent, string text, UnityAction onClick, Vector2 aMin, Vector2 aMax)
        {
            var normal = Get("UI/UI_BotonOpcion_Normal");
            var selected = Get("UI/UI_BotonOpcion_Seleccionado");
            if (normal == null || selected == null) return null;

            var img = Sliced(parent, normal, aMin, aMax, "OptionButton");
            img.raycastTarget = true;
            var button = img.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.SpriteSwap;
            var state = button.spriteState;
            state.highlightedSprite = selected;
            state.pressedSprite = selected;
            state.selectedSprite = normal;
            state.disabledSprite = normal;
            button.spriteState = state;
            if (onClick != null) button.onClick.AddListener(onClick);

            // Flecha del estado seleccionado: sprite aparte, centrada a la izquierda, para que el 9-slice no la estire
            var arrowSprite = Get("UI/UI_FlechaOpcion");
            if (arrowSprite != null)
            {
                var art = Kit.Rect(img.transform, "Arrow", new Vector2(0, 0.5f), new Vector2(0, 0.5f));
                art.pivot = new Vector2(0, 0.5f);
                art.sizeDelta = arrowSprite.rect.size * PixelScale;
                art.anchoredPosition = new Vector2(5 * PixelScale, -0.5f * PixelScale); // el triángulo (sin la sombra) queda centrado
                var arrow = art.gameObject.AddComponent<Image>();
                arrow.sprite = arrowSprite;
                arrow.raycastTarget = false;
                arrow.enabled = false;
                img.gameObject.AddComponent<OptionArrow>().arrow = arrow;
            }

            // Margen izquierdo amplio para no tapar la flecha del estado seleccionado
            Kit.Label(img.transform, text, 20, Pal.Cream, TextAnchor.MiddleLeft, Vector2.zero, Vector2.one,
                new Vector2(13 * PixelScale, 3 * PixelScale), new Vector2(-4 * PixelScale, -3 * PixelScale), FontStyle.Bold, true);
            return button;
        }

        /// <summary>Con SpriteSwap no hay tinte automático: atenúa a mano un botón desactivado.</summary>
        public static void ApplyDisabledLook(Button button)
        {
            if (button == null || button.transition != Selectable.Transition.SpriteSwap) return;
            var alpha = button.interactable ? 1f : 0.45f;
            if (button.targetGraphic != null) button.targetGraphic.color = new Color(1, 1, 1, alpha);
            foreach (var t in button.GetComponentsInChildren<Text>()) t.color = Kit.WithAlpha(t.color, button.interactable ? 1f : 0.6f);
        }
    }

    /// <summary>Enseña la flecha del botón de opción solo mientras el ratón está encima (como el sprite seleccionado).</summary>
    public class OptionArrow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image arrow;
        Button button;

        void Awake() => button = GetComponent<Button>();
        public void OnPointerEnter(PointerEventData e) { if (arrow != null) arrow.enabled = button == null || button.interactable; }
        public void OnPointerExit(PointerEventData e) { if (arrow != null) arrow.enabled = false; }
        void OnDisable() { if (arrow != null) arrow.enabled = false; }
    }
}
