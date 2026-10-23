using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ElDespacho
{
    /// <summary>Paleta del juego.</summary>
    public static class Pal
    {
        public static readonly Color Wood = Kit.Hex("#2e1d12");
        public static readonly Color WoodMid = Kit.Hex("#4a3020");
        public static readonly Color WoodLight = Kit.Hex("#6b4a30");
        public static readonly Color Paper = Kit.Hex("#e9dcc0");
        public static readonly Color PaperDark = Kit.Hex("#d6c49c");
        public static readonly Color Ink = Kit.Hex("#2a2018");
        public static readonly Color InkSoft = Kit.Hex("#5a4a3a");
        public static readonly Color Gold = Kit.Hex("#d9a441");
        public static readonly Color Green = Kit.Hex("#3f8f4a");
        public static readonly Color Red = Kit.Hex("#a8322d");
        public static readonly Color Blue = Kit.Hex("#3d6fa8");
        public static readonly Color Gray = Kit.Hex("#7a7a7a");
        public static readonly Color Ocean = Kit.Hex("#2f5d7c");
        public static readonly Color Land = Kit.Hex("#c9b98a");
        public static readonly Color Cream = Kit.Hex("#f3ead6");
    }

    /// <summary>Utilidades para construir la interfaz uGUI por código.</summary>
    public static class Kit
    {
        static Font _font;
        public static Font Font
        {
            get
            {
                if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return _font;
            }
        }

        static Sprite _circle;
        public static Sprite Circle
        {
            get
            {
                if (_circle == null) _circle = MakeCircle(128);
                return _circle;
            }
        }

        static Sprite MakeCircle(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color32[size * size];
            float r = size / 2f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - r, dy = y + 0.5f - r;
                float a = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy));
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        public static Color WithAlpha(Color c, float a)
        {
            c.a = a;
            return c;
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 aMin, Vector2 aMax,
            Vector2 offMin = default, Vector2 offMax = default)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = offMin;
            rt.offsetMax = offMax;
            return rt;
        }

        public static Image Panel(Transform parent, Color color, Vector2 aMin, Vector2 aMax,
            Vector2 offMin = default, Vector2 offMax = default, string name = "Panel")
        {
            var rt = Rect(parent, name, aMin, aMax, offMin, offMax);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>Un círculo centrado en un punto relativo del padre, con tamaño en píxeles.</summary>
        public static Image Dot(Transform parent, Color color, Vector2 anchor, float size, string name = "Dot")
        {
            var rt = Rect(parent, name, anchor, anchor);
            rt.sizeDelta = new Vector2(size, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = Circle;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>Un rectángulo centrado en un punto relativo del padre, con tamaño en píxeles.</summary>
        public static Image Box(Transform parent, Color color, Vector2 anchor, Vector2 size, string name = "Box")
        {
            var rt = Rect(parent, name, anchor, anchor);
            rt.sizeDelta = size;
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static Text Label(Transform parent, string text, int size, Color color, TextAnchor align,
            Vector2 aMin, Vector2 aMax, Vector2 offMin = default, Vector2 offMax = default,
            FontStyle style = FontStyle.Normal, bool bestFit = false)
        {
            var rt = Rect(parent, "Text", aMin, aMax, offMin, offMax);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = style;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.lineSpacing = 1.1f;
            t.raycastTarget = false;
            if (bestFit)
            {
                t.resizeTextForBestFit = true;
                t.resizeTextMaxSize = size;
                t.resizeTextMinSize = Mathf.Max(10, size / 2);
            }
            return t;
        }

        public static Button MakeButton(Transform parent, string text, int size, Color bg, Color fg, UnityAction onClick,
            Vector2 aMin, Vector2 aMax, Vector2 offMin = default, Vector2 offMax = default)
        {
            var img = Panel(parent, bg, aMin, aMax, offMin, offMax, "Button");
            img.raycastTarget = true;

            // Sombra inferior para dar volumen de botón físico
            var shadow = Panel(img.transform, new Color(0, 0, 0, 0.35f), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 5), "Shade");
            shadow.raycastTarget = false;

            var b = img.gameObject.AddComponent<Button>();
            var cb = b.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            cb.pressedColor = new Color(0.65f, 0.65f, 0.65f, 1f);
            cb.selectedColor = Color.white;
            cb.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.55f);
            cb.fadeDuration = 0.06f;
            b.colors = cb;
            if (onClick != null) b.onClick.AddListener(onClick);

            Label(img.transform, text, size, fg, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one,
                new Vector2(10, 6), new Vector2(-10, -4), FontStyle.Bold, true);
            return b;
        }

        /// <summary>Barra horizontal de 0 a 100.</summary>
        public static void Bar(Transform parent, int value, Color fill, Vector2 aMin, Vector2 aMax,
            Vector2 offMin = default, Vector2 offMax = default)
        {
            var bg = Panel(parent, new Color(0, 0, 0, 0.35f), aMin, aMax, offMin, offMax, "Bar");
            float v = Mathf.Clamp01(value / 100f);
            Panel(bg.transform, fill, new Vector2(0, 0), new Vector2(v, 1), new Vector2(2, 2), new Vector2(v > 0 ? -2 : 0, -2), "Fill");
        }

        public static Color IndicatorColor(int v)
        {
            if (v >= 60) return Hex("#5fae6e");
            if (v >= 35) return Hex("#d9a441");
            return Hex("#c94a3a");
        }
    }

    /// <summary>Sonidos sintetizados por código (no requiere assets).</summary>
    public static class Sfx
    {
        static AudioSource _src;
        public static AudioClip Click, Thunk, Paper, Low;

        public static void Init(GameObject host)
        {
            _src = host.AddComponent<AudioSource>();
            _src.playOnAwake = false;
            _src.spatialBlend = 0f;

            Click = Make("click", 0.05f, (t, r) => r * Mathf.Exp(-t * 120f) * 0.4f);
            Thunk = Make("thunk", 0.35f, (t, r) => (Mathf.Sin(2f * Mathf.PI * 65f * t) * 0.9f + r * 0.25f) * Mathf.Exp(-t * 14f));
            Paper = Make("paper", 0.25f, (t, r) => r * 0.22f * Mathf.Exp(-t * 10f) * (0.6f + 0.4f * Mathf.Sin(t * 120f)));
            Low = Make("low", 1.4f, (t, r) => Mathf.Sin(2f * Mathf.PI * 48f * t) * 0.5f * Mathf.Exp(-t * 2.5f));
        }

        static AudioClip Make(string name, float dur, System.Func<float, float, float> f)
        {
            const int rate = 44100;
            int n = Mathf.Max(1, (int)(rate * dur));
            var data = new float[n];
            var rnd = new System.Random(name.GetHashCode());
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float r = (float)rnd.NextDouble() * 2f - 1f;
                data[i] = Mathf.Clamp(f(t, r), -1f, 1f);
            }
            var clip = AudioClip.Create(name, n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static void Play(AudioClip clip, float volume = 1f)
        {
            if (_src != null && clip != null) _src.PlayOneShot(clip, volume);
        }
    }
}
