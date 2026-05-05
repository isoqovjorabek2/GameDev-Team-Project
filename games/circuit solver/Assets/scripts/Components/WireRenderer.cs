using CircuitSolver.Core;
using CircuitSolver.Data;
using CircuitSolver.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitSolver.Components
{
    /// <summary>
    /// A simple wire drawn with two orthogonal segments (L-bend) using UI
    /// Image rectangles. Each segment is a thin rounded strip that sits on
    /// the circuit canvas.
    /// </summary>
    public class WireRenderer : MonoBehaviour
    {
        public Color wireColor = Theme.WireOrange;
        public float thickness = 6f;

        RectTransform _hSeg;
        RectTransform _vSeg;
        RectTransform _dotA;
        RectTransform _dotB;

        public void Build(RectTransform parent, Color color)
        {
            wireColor = color;
            var rt = (RectTransform)transform;
            rt.pivot = new Vector2(0.5f, 0.5f);
            UIFactory.Stretch(rt, 0, 0, 0, 0);
            _hSeg = MakeSegment("H");
            _vSeg = MakeSegment("V");
            _dotA = MakeDot("A");
            _dotB = MakeDot("B");
        }

        RectTransform MakeSegment(string name)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(transform, false);
            var img = go.AddComponent<Image>();
            img.sprite = SpriteFactory.Rounded();
            img.type = Image.Type.Sliced;
            img.color = wireColor;
            img.raycastTarget = false;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            return rt;
        }

        RectTransform MakeDot(string name)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(transform, false);
            var img = go.AddComponent<Image>();
            img.sprite = SpriteFactory.NodeDot;
            img.color = wireColor;
            img.raycastTarget = false;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(14, 14);
            return rt;
        }

        public void SetEndpoints(Vector2 a, Vector2 b)
        {
            Vector2 mid = new Vector2(b.x, a.y);

            _hSeg.anchoredPosition = (a + mid) * 0.5f;
            _hSeg.sizeDelta = new Vector2(Mathf.Abs(b.x - a.x), thickness);

            _vSeg.anchoredPosition = (mid + b) * 0.5f;
            _vSeg.sizeDelta = new Vector2(thickness, Mathf.Abs(b.y - a.y));

            _dotA.anchoredPosition = a;
            _dotB.anchoredPosition = b;
        }
    }
}
