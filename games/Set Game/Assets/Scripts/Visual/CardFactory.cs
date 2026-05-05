using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SetGame.Core;

namespace SetGame.Visual
{
    /// <summary>
    /// Creates and configures CardView GameObjects entirely in code.
    /// </summary>
    public static class CardFactory
    {
        // Card visual dimensions (UI units)
        public const float CARD_W = 130f;
        public const float CARD_H = 185f;

        // Symbol color palette
        static readonly Color COLOR_RED    = new(1f,   0.27f, 0.34f, 1f);
        static readonly Color COLOR_GREEN  = new(0.18f,0.83f, 0.45f, 1f);
        static readonly Color COLOR_PURPLE = new(0.67f,0.33f, 0.97f, 1f);

        public static CardView Create(CardData data, int boardIndex, Transform parent)
        {
            // Root card object
            var root = new GameObject($"Card_{boardIndex}", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(CARD_W, CARD_H);

            var cv = root.AddComponent<CardView>();

            // Glow (behind card, slightly larger)
            var glow = CreateImage("Glow", root.transform, new Color(0,0,0,0));
            var glowRt = glow.GetComponent<RectTransform>();
            glowRt.anchorMin = Vector2.zero;
            glowRt.anchorMax = Vector2.one;
            glowRt.offsetMin = new Vector2(-10, -10);
            glowRt.offsetMax = new Vector2(10, 10);
            glow.sprite  = MakeRoundedSprite(20);
            cv.GlowImage = glow;

            // Card background
            var bg = CreateImage("Background", root.transform, new Color(0.09f, 0.09f, 0.18f));
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
            bg.sprite = MakeRoundedSprite(14);
            cv.CardBackground = bg;

            // Selection ring
            var ring = CreateImage("Ring", root.transform, new Color(1f, 0.85f, 0.2f, 0f));
            var ringRt = ring.GetComponent<RectTransform>();
            ringRt.anchorMin = Vector2.zero;
            ringRt.anchorMax = Vector2.one;
            ringRt.offsetMin = new Vector2(4, 4);
            ringRt.offsetMax = new Vector2(-4, -4);
            ring.sprite = MakeRoundedSprite(12);
            ring.type   = Image.Type.Simple;
            ring.fillMethod = Image.FillMethod.Radial360;
            ring.enabled = false;
            cv.SelectRing = ring;

            // Symbol container
            var symContainer = new GameObject("Symbols", typeof(RectTransform));
            symContainer.transform.SetParent(root.transform, false);
            var symRt = symContainer.GetComponent<RectTransform>();
            symRt.anchorMin = new Vector2(0.1f, 0.08f);
            symRt.anchorMax = new Vector2(0.9f, 0.92f);
            symRt.offsetMin = symRt.offsetMax = Vector2.zero;

            // Place symbols
            var symColor = GetSymbolColor(data.Color);
            int count = (int)data.Count;
            var symbols = BuildSymbols(data, symColor, count, symContainer.transform);
            cv.Symbols = symbols;

            cv.Setup(data, boardIndex);
            return cv;
        }

        static List<CardSymbolGraphic> BuildSymbols(CardData data, Color color,
                                                     int count, Transform parent)
        {
            // Symbol sizing: scale so 3 symbols fit with padding
            float symH = 50f;
            float symW = 78f;
            float gap  = 8f;
            float totalH = count * symH + (count - 1) * gap;

            var list = new List<CardSymbolGraphic>();
            for (int i = 0; i < count; i++)
            {
                float yOffset = (count == 1) ? 0 :
                                (count == 2) ? (i == 0 ? (symH + gap) * 0.5f : -(symH + gap) * 0.5f) :
                                               (i - 1) * (symH + gap);

                var symGO = new GameObject($"Symbol_{i}", typeof(RectTransform));
                symGO.transform.SetParent(parent, false);

                var rt = symGO.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, yOffset);
                rt.sizeDelta        = new Vector2(symW, symH);

                var g = symGO.AddComponent<CardSymbolGraphic>();
                g.Shape   = data.Shape;
                g.Shading = data.Shading;
                g.color   = color;
                g.raycastTarget = false;

                list.Add(g);
            }
            return list;
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        static Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = (name == "Background"); // only bg receives raycasts
            return img;
        }

        static Color GetSymbolColor(CardColor c) => c switch
        {
            CardColor.Red    => COLOR_RED,
            CardColor.Green  => COLOR_GREEN,
            CardColor.Purple => COLOR_PURPLE,
            _                => Color.white
        };

        // Creates a simple circular sprite (approximated rounded rect) for card BG
        static Sprite MakeRoundedSprite(int radius)
        {
            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float fx = x + 0.5f;
                float fy = y + 0.5f;
                bool inside = IsInsideRoundedRect(fx, fy, size, radius);
                pixels[y * size + x] = inside
                    ? new Color32(255, 255, 255, 255)
                    : new Color32(0, 0, 0, 0);
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                (uint)radius,
                SpriteMeshType.FullRect,
                new Vector4(radius, radius, radius, radius));
        }

        static bool IsInsideRoundedRect(float x, float y, int size, int r)
        {
            float x0 = r, y0 = r;
            float x1 = size - r, y1 = size - r;
            if (x >= x0 && x <= x1 && y >= 0 && y <= size) return true;
            if (x >= 0  && x <= size && y >= y0 && y <= y1) return true;

            // corners
            float dx = 0, dy = 0;
            if      (x < x0 && y < y0) { dx = x0 - x; dy = y0 - y; }
            else if (x > x1 && y < y0) { dx = x - x1; dy = y0 - y; }
            else if (x < x0 && y > y1) { dx = x0 - x; dy = y - y1; }
            else if (x > x1 && y > y1) { dx = x - x1; dy = y - y1; }
            else return true;

            return dx * dx + dy * dy <= (float)(r * r);
        }
    }
}
