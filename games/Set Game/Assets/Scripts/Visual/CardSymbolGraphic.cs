using UnityEngine;
using UnityEngine.UI;
using SetGame.Core;

namespace SetGame.Visual
{
    /// <summary>
    /// Custom UI Graphic that draws a single SET card symbol using mesh triangulation.
    /// Supports Diamond, Oval, Squiggle × Solid, Open, Striped.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class CardSymbolGraphic : Graphic
    {
        public CardShape   Shape   = CardShape.Diamond;
        public CardShading Shading = CardShading.Solid;

        const int  OVAL_SEGMENTS    = 32;
        const int  SQUIGGLE_SEGS    = 48;
        const float OUTLINE_WIDTH   = 0.06f;   // fraction of symbol size
        const float STRIPE_SPACING  = 0.11f;   // fraction of height

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();

            switch (Shape)
            {
                case CardShape.Diamond:  DrawDiamond(vh, r); break;
                case CardShape.Oval:     DrawOval(vh, r);    break;
                case CardShape.Squiggle: DrawSquiggle(vh, r); break;
            }
        }

        // ─── Diamond ─────────────────────────────────────────────────────────────

        void DrawDiamond(VertexHelper vh, Rect r)
        {
            float cx = r.x + r.width  * 0.5f;
            float cy = r.y + r.height * 0.5f;
            float hw = r.width  * 0.48f;
            float hh = r.height * 0.48f;

            Vector2 top   = new(cx,      cy + hh);
            Vector2 right = new(cx + hw, cy);
            Vector2 bot   = new(cx,      cy - hh);
            Vector2 left  = new(cx - hw, cy);

            if (Shading == CardShading.Solid)
            {
                AddQuadFan(vh, new[] { top, right, bot, left });
            }
            else if (Shading == CardShading.Open)
            {
                float ow = Mathf.Min(hw, hh) * OUTLINE_WIDTH * 2f;
                AddOutlinePath(vh, new[] { top, right, bot, left }, ow, true);
            }
            else // Striped
            {
                ClipStripesToPolygon(vh, new[] { top, right, bot, left }, r, true);
                float ow = Mathf.Min(hw, hh) * OUTLINE_WIDTH * 2f;
                AddOutlinePath(vh, new[] { top, right, bot, left }, ow, true);
            }
        }

        // ─── Oval ─────────────────────────────────────────────────────────────────

        void DrawOval(VertexHelper vh, Rect r)
        {
            float cx = r.x + r.width  * 0.5f;
            float cy = r.y + r.height * 0.5f;
            float rx = r.width  * 0.47f;
            float ry = r.height * 0.47f;

            var pts = BuildEllipsePoints(cx, cy, rx, ry, OVAL_SEGMENTS);

            if (Shading == CardShading.Solid)
            {
                AddQuadFan(vh, pts);
            }
            else if (Shading == CardShading.Open)
            {
                float ow = Mathf.Min(rx, ry) * OUTLINE_WIDTH * 2f;
                AddOutlinePath(vh, pts, ow, true);
            }
            else
            {
                ClipStripesToPolygon(vh, pts, r, true);
                float ow = Mathf.Min(rx, ry) * OUTLINE_WIDTH * 2f;
                AddOutlinePath(vh, pts, ow, true);
            }
        }

        // ─── Squiggle ─────────────────────────────────────────────────────────────

        void DrawSquiggle(VertexHelper vh, Rect r)
        {
            // S-curve approximated with two arcs
            float cx = r.x + r.width  * 0.5f;
            float cy = r.y + r.height * 0.5f;
            float w  = r.width  * 0.46f;
            float h  = r.height * 0.46f;

            var outer = BuildSquigglePath(cx, cy, w, h, SQUIGGLE_SEGS, false);
            var inner = BuildSquigglePath(cx, cy, w * (1f - OUTLINE_WIDTH * 2f),
                                          h * (1f - OUTLINE_WIDTH * 2f), SQUIGGLE_SEGS, false);

            if (Shading == CardShading.Solid)
            {
                AddQuadFan(vh, outer);
            }
            else if (Shading == CardShading.Open)
            {
                AddRingStrip(vh, outer, inner);
            }
            else
            {
                ClipStripesToPolygon(vh, outer, r, true);
                AddRingStrip(vh, outer, inner);
            }
        }

        // ─── Path builders ────────────────────────────────────────────────────────

        static Vector2[] BuildEllipsePoints(float cx, float cy, float rx, float ry, int segs)
        {
            var pts = new Vector2[segs];
            for (int i = 0; i < segs; i++)
            {
                float a = i / (float)segs * Mathf.PI * 2f;
                pts[i] = new Vector2(cx + Mathf.Cos(a) * rx, cy + Mathf.Sin(a) * ry);
            }
            return pts;
        }

        static Vector2[] BuildSquigglePath(float cx, float cy, float w, float h, int segs, bool inner)
        {
            // The squiggle is an S-shape: combine two half-ellipses offset vertically
            var pts = new Vector2[segs];
            for (int i = 0; i < segs; i++)
            {
                float t  = i / (float)(segs - 1);   // 0..1
                float a  = t * Mathf.PI * 2f - Mathf.PI * 0.5f;

                // S-curve: upper half-circle offset right, lower half-circle offset left
                float ox = Mathf.Sin(a * 0.5f) * w * 0.35f;
                float x  = cx + Mathf.Cos(a) * w + ox;
                float y  = cy + Mathf.Sin(a) * h;
                pts[i]   = new Vector2(x, y);
            }
            return pts;
        }

        // ─── Mesh helpers ─────────────────────────────────────────────────────────

        // ─── Mesh helpers ─────────────────────────────────────────────────────────

        // Filled convex polygon via fan triangulation from centroid
        void AddQuadFan(VertexHelper vh, Vector2[] pts)
        {
            int baseIdx = vh.currentVertCount;

            Vector2 ctr = Vector2.zero;
            foreach (var p in pts) ctr += p;
            ctr /= pts.Length;

            vh.AddVert(V(ctr));
            foreach (var p in pts) vh.AddVert(V(p));

            for (int i = 0; i < pts.Length; i++)
                vh.AddTriangle(baseIdx,
                               baseIdx + 1 + i,
                               baseIdx + 1 + (i + 1) % pts.Length);
        }

        // Outline quad-strip around a closed polygon
        void AddOutlinePath(VertexHelper vh, Vector2[] pts, float width, bool closed)
        {
            int n       = pts.Length;
            int baseIdx = vh.currentVertCount;

            for (int i = 0; i < n; i++)
            {
                Vector2 prev   = pts[(i - 1 + n) % n];
                Vector2 curr   = pts[i];
                Vector2 next   = pts[(i + 1) % n];
                Vector2 dir    = ((curr - prev).normalized + (next - curr).normalized).normalized;
                Vector2 normal = new(-dir.y, dir.x);
                float   half   = width * 0.5f;

                vh.AddVert(V(curr + normal * half));
                vh.AddVert(V(curr - normal * half));
            }

            for (int i = 0; i < n; i++)
            {
                int a  = baseIdx + i * 2;
                int b  = baseIdx + i * 2 + 1;
                int c2 = baseIdx + ((i + 1) % n) * 2;
                int d  = baseIdx + ((i + 1) % n) * 2 + 1;
                vh.AddTriangle(a, c2, b);
                vh.AddTriangle(b, c2, d);
            }
        }

        // Ring between outer and inner closed paths
        void AddRingStrip(VertexHelper vh, Vector2[] outer, Vector2[] inner)
        {
            int n       = Mathf.Min(outer.Length, inner.Length);
            int baseIdx = vh.currentVertCount;

            for (int i = 0; i < n; i++)
            {
                vh.AddVert(V(outer[i]));
                vh.AddVert(V(inner[i]));
            }

            for (int i = 0; i < n; i++)
            {
                int a  = baseIdx + i * 2;
                int b  = baseIdx + i * 2 + 1;
                int c2 = baseIdx + ((i + 1) % n) * 2;
                int d  = baseIdx + ((i + 1) % n) * 2 + 1;
                vh.AddTriangle(a, c2, b);
                vh.AddTriangle(b, c2, d);
            }
        }

        // Horizontal stripe quads clipped inside a polygon (scanline-based)
        void ClipStripesToPolygon(VertexHelper vh, Vector2[] poly, Rect bounds, bool closed)
        {
            float stripeH = bounds.height * STRIPE_SPACING;
            float halfSH  = stripeH * 0.25f;

            for (float yc = bounds.yMin + stripeH; yc < bounds.yMax; yc += stripeH)
            {
                if (!PolygonScanline(poly, yc, out float xMin, out float xMax)) continue;
                if (xMax <= xMin) continue;

                int b = vh.currentVertCount;
                vh.AddVert(V(new Vector2(xMin, yc - halfSH)));
                vh.AddVert(V(new Vector2(xMax, yc - halfSH)));
                vh.AddVert(V(new Vector2(xMax, yc + halfSH)));
                vh.AddVert(V(new Vector2(xMin, yc + halfSH)));
                vh.AddTriangle(b, b + 1, b + 2);
                vh.AddTriangle(b, b + 2, b + 3);
            }
        }

        // ─── Scanline ─────────────────────────────────────────────────────────────

        static bool PolygonScanline(Vector2[] poly, float y, out float xMin, out float xMax)
        {
            xMin = float.MaxValue; xMax = float.MinValue;
            int n = poly.Length;
            for (int i = 0; i < n; i++)
            {
                Vector2 a = poly[i];
                Vector2 b2 = poly[(i + 1) % n];
                if ((a.y <= y && b2.y > y) || (b2.y <= y && a.y > y))
                {
                    float t = (y - a.y) / (b2.y - a.y);
                    float x = a.x + t * (b2.x - a.x);
                    if (x < xMin) xMin = x;
                    if (x > xMax) xMax = x;
                }
            }
            return xMin < xMax;
        }

        // ─── Vertex factory ───────────────────────────────────────────────────────

        UIVertex V(Vector2 p)
        {
            var v    = UIVertex.simpleVert;
            v.position = new Vector3(p.x, p.y, 0);
            v.color    = color;
            return v;
        }
    }

}
