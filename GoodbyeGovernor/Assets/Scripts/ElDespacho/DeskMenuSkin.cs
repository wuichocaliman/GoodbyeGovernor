using UnityEngine;
using UnityEngine.UI;

namespace ElDespacho
{
    /// <summary>Visual styling only. Button callbacks and interactability remain owned by DeskMenus.</summary>
    public static class DeskMenuSkin
    {
        static Sprite normal, hover, pressed, disabled;
        static Sprite Frame(string name)
        {
            var source = Resources.Load<Sprite>("Menus/" + name);
            if (source == null) return null;
            // Runtime view of the original texture: remove excess transparent margins,
            // without resampling or changing any PNG. Same viewport for all four states.
            var frame = Sprite.Create(source.texture, new Rect(64, 100, 2048, 550),
                new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect,
                new Vector4(330, 80, 330, 140), false);
            frame.name = name;
            return frame;
        }
        public static void Apply(Button button, bool compact = false)
        {
            if (normal == null) normal = Frame("button_default");
            if (hover == null) hover = Frame("button_hover");
            if (pressed == null) pressed = Frame("button_pressed");
            if (disabled == null) disabled = Frame("button_disabled");
            if (normal == null || hover == null || pressed == null || disabled == null) return;

            var graphic = button.GetComponent<Image>();
            graphic.color = Color.white;
            graphic.sprite = normal;
            graphic.type = Image.Type.Sliced;
            graphic.pixelsPerUnitMultiplier = compact ? 16f : 10f;
            button.targetGraphic = graphic;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState {
                highlightedSprite = hover, selectedSprite = hover,
                pressedSprite = pressed, disabledSprite = disabled
            };
            // Kit's solid underline belongs to the previous flat button design.
            var shade = button.transform.Find("Shade");
            if (shade != null) shade.gameObject.SetActive(false);
            var label = button.GetComponentInChildren<Text>();
            label.color = DeskPreferences.Current.highContrast ? Color.black : Kit.Hex("#291b12");
            label.rectTransform.offsetMin = new Vector2(compact ? 14 : 50, compact ? 3 : 8);
            label.rectTransform.offsetMax = new Vector2(compact ? -14 : -50, compact ? -2 : -6);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = compact ? 12 : 18;
            label.resizeTextMaxSize = label.fontSize;
        }
    }
}
