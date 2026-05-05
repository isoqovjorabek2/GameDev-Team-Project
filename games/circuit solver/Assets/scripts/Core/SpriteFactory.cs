using UnityEngine;

namespace CircuitSolver.Core
{
    /// <summary>
    /// Procedurally generates the small set of sprites we need at runtime
    /// (solid panels, zigzag resistor, battery symbol, checkmark icon...).
    /// Keeps the project asset-free so the whole game can run from code.
    /// </summary>
    public static class SpriteFactory
    {
        static Sprite _solid;
        static Sprite _rounded;
        static Sprite _ring;
        static Sprite _resistor;
        static Sprite _battery;
        static Sprite _voltageSource;
        static Sprite _questionMark;
        static Sprite _nodeDot;
        static Sprite _shadow;

        public static Sprite Solid
        {
            get { if (_solid == null) _solid = MakeSolid(8, 8, Color.white); return _solid; }
        }

        public static Sprite Rounded(int radius = 16)
        {
            if (_rounded == null) _rounded = MakeRounded(64, 64, radius, Color.white);
            return _rounded;
        }

        public static Sprite Ring(int size = 48, int thickness = 4)
        {
            if (_ring == null) _ring = MakeRing(size, thickness);
            return _ring;
        }

        public static Sprite Resistor
        {
            get { if (_resistor == null) _resistor = MakeResistor(); return _resistor; }
        }

        public static Sprite Battery
        {
            get { if (_battery == null) _battery = MakeBattery(); return _battery; }
        }

        public static Sprite VoltageSourceCircle
        {
            get { if (_voltageSource == null) _voltageSource = MakeVoltageSource(); return _voltageSource; }
        }

        public static Sprite QuestionMark
        {
            get { if (_questionMark == null) _questionMark = MakeQuestionMark(); return _questionMark; }
        }

        public static Sprite NodeDot
        {
            get { if (_nodeDot == null) _nodeDot = MakeNodeDot(); return _nodeDot; }
        }

        public static Sprite SoftShadow
        {
            get { if (_shadow == null) _shadow = MakeRadialShadow(); return _shadow; }
        }

        // ---------------- generators -----------------

        static Sprite MakeSolid(int w, int h, Color c)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var px = new Color[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = c;
            t.SetPixels(px);
            t.Apply();
            var s = Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            s.name = "Solid";
            return s;
        }

        static Sprite MakeRounded(int w, int h, int r, Color c)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int cx = x < r ? r : (x > w - r - 1 ? w - r - 1 : x);
                int cy = y < r ? r : (y > h - r - 1 ? h - r - 1 : y);
                int dx = x - cx, dy = y - cy;
                bool inside = (dx == 0 && dy == 0) || (dx * dx + dy * dy <= r * r);
                px[y * w + x] = inside ? c : new Color(0, 0, 0, 0);
            }
            t.SetPixels(px);
            t.Apply();
            var border = new Vector4(r, r, r, r);
            var s = Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            s.name = "Rounded";
            return s;
        }

        static Sprite MakeRing(int size, int thickness)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var px = new Color[size * size];
            int r = size / 2;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int dx = x - r, dy = y - r;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                bool inRing = d <= r - 1 && d >= r - thickness;
                px[y * size + x] = inRing ? Color.white : new Color(0, 0, 0, 0);
            }
            t.SetPixels(px);
            t.Apply();
            var s = Sprite.Create(t, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            s.name = "Ring";
            return s;
        }

        static Sprite MakeResistor()
        {
            // Classic zigzag resistor, horizontal. Body 256x96.
            int w = 256, h = 96;
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var clear = new Color(0, 0, 0, 0);
            var px = new Color[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = clear;

            // Leads on both sides
            DrawLine(px, w, 0, h / 2, 40, h / 2, Color.white, 6);
            DrawLine(px, w, w - 1, h / 2, w - 41, h / 2, Color.white, 6);

            // Zigzag: 6 peaks between x=40 and x=w-40
            int x0 = 40, x1 = w - 40;
            int peaks = 6;
            float step = (x1 - x0) / (float)(peaks * 2);
            int topY = h / 2 - 32, botY = h / 2 + 32;
            Vector2Int prev = new Vector2Int(x0, h / 2);
            for (int i = 0; i < peaks * 2; i++)
            {
                int x = x0 + Mathf.RoundToInt(step * (i + 1));
                int y = (i % 2 == 0) ? topY : botY;
                DrawLine(px, w, prev.x, prev.y, x, y, Color.white, 6);
                prev = new Vector2Int(x, y);
            }
            DrawLine(px, w, prev.x, prev.y, x1, h / 2, Color.white, 6);

            t.SetPixels(px);
            t.Apply();
            var s = Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
            s.name = "Resistor";
            return s;
        }

        static Sprite MakeBattery()
        {
            // Battery cell pair symbol, horizontal. Long line (+), short line (-).
            int w = 180, h = 120;
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var px = new Color[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = new Color(0, 0, 0, 0);

            // Leads
            DrawLine(px, w, 0, h / 2, 60, h / 2, Color.white, 6);
            DrawLine(px, w, w - 1, h / 2, w - 61, h / 2, Color.white, 6);

            // Long line (plus terminal, left)
            DrawLine(px, w, 70, h / 2 - 40, 70, h / 2 + 40, Color.white, 8);
            // Short line (minus terminal, right)
            DrawLine(px, w, 110, h / 2 - 22, 110, h / 2 + 22, Color.white, 8);

            t.SetPixels(px);
            t.Apply();
            var s = Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
            s.name = "Battery";
            return s;
        }

        static Sprite MakeVoltageSource()
        {
            // Circle with +/- symbols inside. Horizontal orientation.
            int size = 160;
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var px = new Color[size * size];
            for (int i = 0; i < px.Length; i++) px[i] = new Color(0, 0, 0, 0);

            int r = size / 2 - 8;
            int cx = size / 2, cy = size / 2;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int dx = x - cx, dy = y - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d <= r && d >= r - 6) px[y * size + x] = Color.white;
            }
            // Plus on left
            DrawLine(px, size, cx - 36, cy, cx - 16, cy, Color.white, 5);
            DrawLine(px, size, cx - 26, cy - 10, cx - 26, cy + 10, Color.white, 5);
            // Minus on right
            DrawLine(px, size, cx + 16, cy, cx + 36, cy, Color.white, 5);

            t.SetPixels(px);
            t.Apply();
            var s = Sprite.Create(t, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            s.name = "VoltageSource";
            return s;
        }

        static Sprite MakeQuestionMark()
        {
            int size = 128;
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var px = new Color[size * size];
            for (int i = 0; i < px.Length; i++) px[i] = new Color(0, 0, 0, 0);

            int cx = size / 2;
            // Arc top of question mark
            int rOuter = 30, rInner = 18;
            for (int y = 30; y < 80; y++)
            for (int x = 0; x < size; x++)
            {
                int dx = x - cx, dy = y - 60;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d <= rOuter && d >= rInner && dy <= 0) px[y * size + x] = Color.white;
            }
            // Stem
            DrawLine(px, size, cx, 60, cx, 88, Color.white, 10);
            // Dot
            for (int y = 100; y < 112; y++)
            for (int x = cx - 6; x < cx + 6; x++)
                px[y * size + x] = Color.white;

            t.SetPixels(px);
            t.Apply();
            var s = Sprite.Create(t, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            s.name = "QuestionMark";
            return s;
        }

        static Sprite MakeNodeDot()
        {
            int size = 24;
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var px = new Color[size * size];
            int r = size / 2 - 2;
            int cx = size / 2, cy = size / 2;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int dx = x - cx, dy = y - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                px[y * size + x] = d <= r ? Color.white : new Color(0, 0, 0, 0);
            }
            t.SetPixels(px);
            t.Apply();
            var s = Sprite.Create(t, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            s.name = "NodeDot";
            return s;
        }

        static Sprite MakeRadialShadow()
        {
            int size = 128;
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var px = new Color[size * size];
            int cx = size / 2, cy = size / 2;
            float maxD = size / 2f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                float a = Mathf.Clamp01(1f - d / maxD);
                a = a * a;
                px[y * size + x] = new Color(0, 0, 0, a * 0.35f);
            }
            t.SetPixels(px);
            t.Apply();
            var s = Sprite.Create(t, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            s.name = "Shadow";
            return s;
        }

        // Bresenham with thickness
        public static void DrawLine(Color[] px, int w, int x0, int y0, int x1, int y1, Color c, int thickness = 1)
        {
            int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            int half = thickness / 2;
            int h = px.Length / w;
            while (true)
            {
                for (int oy = -half; oy <= half; oy++)
                for (int ox = -half; ox <= half; ox++)
                {
                    int xx = x0 + ox, yy = y0 + oy;
                    if (xx < 0 || xx >= w || yy < 0 || yy >= h) continue;
                    px[yy * w + xx] = c;
                }
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 <  dx) { err += dx; y0 += sy; }
            }
        }
    }
}
