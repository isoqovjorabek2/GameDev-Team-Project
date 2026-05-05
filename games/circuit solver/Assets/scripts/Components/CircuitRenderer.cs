using System.Collections.Generic;
using CircuitSolver.Core;
using CircuitSolver.Data;
using CircuitSolver.Managers;
using CircuitSolver.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitSolver.Components
{
    /// <summary>
    /// Lays out all components of a CircuitPuzzle on a cream-colored grid
    /// board. Each component is positioned from its grid cell (x, y).
    /// Wires are drawn as L-shaped UI rectangles between shared node
    /// terminals. Hidden components get a ComponentInputField child.
    /// </summary>
    public class CircuitRenderer : MonoBehaviour
    {
        public float cellSize = 130f;
        public Vector2 gridOrigin = Vector2.zero;

        readonly Dictionary<string, ComponentSprite> _sprites = new Dictionary<string, ComponentSprite>();
        readonly List<WireRenderer> _wires = new List<WireRenderer>();
        readonly Dictionary<string, ComponentInputField> _inputs = new Dictionary<string, ComponentInputField>();
        RectTransform _board;
        RectTransform _wireLayer;
        RectTransform _componentLayer;
        RectTransform _inputLayer;

        public IReadOnlyDictionary<string, ComponentSprite> Sprites => _sprites;

        public void Build(RectTransform parent)
        {
            transform.SetParent(parent, false);
            var rt = (RectTransform)transform;
            UIFactory.Stretch(rt, 0, 0, 0, 0);

            _board = UIFactory.AddImage(rt, "Board", Theme.BoardCream, SpriteFactory.Rounded()).rectTransform;
            _board.GetComponent<Image>().type = Image.Type.Sliced;
            _board.pivot = new Vector2(0.5f, 0.5f);
            UIFactory.Stretch(_board, 0, 0, 0, 0);

            var gridLines = UIFactory.AddImage(_board, "Grid", new Color(1, 1, 1, 0));
            UIFactory.Stretch(gridLines.rectTransform, 0, 0, 0, 0);
            gridLines.raycastTarget = false;

            _wireLayer = UIFactory.AddChild(_board, "Wires");
            _wireLayer.pivot = new Vector2(0.5f, 0.5f);
            UIFactory.Stretch(_wireLayer, 0, 0, 0, 0);

            _componentLayer = UIFactory.AddChild(_board, "Components");
            _componentLayer.pivot = new Vector2(0.5f, 0.5f);
            UIFactory.Stretch(_componentLayer, 0, 0, 0, 0);

            _inputLayer = UIFactory.AddChild(_board, "Inputs");
            _inputLayer.pivot = new Vector2(0.5f, 0.5f);
            UIFactory.Stretch(_inputLayer, 0, 0, 0, 0);
        }

        public void Clear()
        {
            foreach (var s in _sprites.Values) if (s) Destroy(s.gameObject);
            foreach (var w in _wires) if (w) Destroy(w.gameObject);
            foreach (var i in _inputs.Values) if (i) Destroy(i.gameObject);
            _sprites.Clear();
            _wires.Clear();
            _inputs.Clear();
        }

        public void Render(CircuitPuzzle puzzle)
        {
            Clear();
            if (puzzle == null) return;

            // Compute board bounds so we can center the grid inside the cream panel.
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            foreach (var c in puzzle.components)
            {
                min = Vector2.Min(min, c.position);
                max = Vector2.Max(max, c.position);
            }
            Vector2 span = (max - min);
            gridOrigin = new Vector2(-span.x * 0.5f * cellSize, -span.y * 0.5f * cellSize);

            // Spawn components first so terminals exist for wires.
            foreach (var c in puzzle.components)
            {
                var go = new GameObject(c.id, typeof(RectTransform));
                var cs = go.AddComponent<ComponentSprite>();
                go.transform.SetParent(_componentLayer, false);
                cs.Build(c);
                var rt = (RectTransform)cs.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = gridOrigin + new Vector2((c.position.x - min.x) * cellSize,
                                                                 (c.position.y - min.y) * cellSize);
                _sprites[c.id] = cs;

                if (c.isHidden)
                {
                    var inputGo = new GameObject($"Input_{c.id}", typeof(RectTransform));
                    inputGo.transform.SetParent(_inputLayer, false);
                    var field = inputGo.AddComponent<ComponentInputField>();
                    field.Build(c);
                    var irt = (RectTransform)inputGo.transform;
                    irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
                    irt.pivot = new Vector2(0.5f, 0f);
                    irt.anchoredPosition = rt.anchoredPosition + new Vector2(0, 60);
                    field.OnValueSubmitted += (id, v) =>
                    {
                        PuzzleManager.Instance.SetHiddenValue(id, v);
                        if (_sprites.TryGetValue(id, out var sprite)) sprite.SetValueDisplay(v);
                    };
                    _inputs[c.id] = field;
                }
            }

            // Wire pass: group terminals by shared node id.
            var nodeToTerminals = new Dictionary<int, List<Vector2>>();
            foreach (var c in puzzle.components)
            {
                AddTerminal(nodeToTerminals, c.nodeA, c, true);
                AddTerminal(nodeToTerminals, c.nodeB, c, false);
            }

            foreach (var kv in nodeToTerminals)
            {
                var list = kv.Value;
                if (list.Count < 2) continue;
                var root = list[0];
                for (int i = 1; i < list.Count; i++)
                {
                    var wireGo = new GameObject($"Wire_{kv.Key}_{i}", typeof(RectTransform));
                    wireGo.transform.SetParent(_wireLayer, false);
                    var wire = wireGo.AddComponent<WireRenderer>();
                    wire.Build(_wireLayer, Theme.WireOrange);
                    wire.SetEndpoints(root, list[i]);
                    _wires.Add(wire);
                }
            }
        }

        void AddTerminal(Dictionary<int, List<Vector2>> dict, int node, CircuitComponent comp, bool isA)
        {
            if (!dict.TryGetValue(node, out var list))
            {
                list = new List<Vector2>();
                dict[node] = list;
            }
            list.Add(ComputeTerminalLocalPos(comp, isA));
        }

        /// <summary>
        /// Return a terminal's (x,y) in _wireLayer local coordinates (relative
        /// to its center pivot). Since the wire and component layers share
        /// the same bounds, this is just the component's anchored position
        /// plus the terminal offset rotated by the component's orientation.
        /// </summary>
        Vector2 ComputeTerminalLocalPos(CircuitComponent comp, bool isA)
        {
            if (!_sprites.TryGetValue(comp.id, out var sprite)) return Vector2.zero;
            var rt = (RectTransform)sprite.transform;
            Vector2 center = rt.anchoredPosition;
            // Default horizontal component: terminal A at -size.x/2, B at +size.x/2.
            Vector2 size = rt.sizeDelta;
            Vector2 localOffset = new Vector2((isA ? -1f : 1f) * size.x * 0.5f, 0f);
            // Apply orientation rotation.
            float rad = comp.orientationDegrees * Mathf.Deg2Rad;
            float cs = Mathf.Cos(rad), sn = Mathf.Sin(rad);
            Vector2 rotated = new Vector2(localOffset.x * cs - localOffset.y * sn,
                                          localOffset.x * sn + localOffset.y * cs);
            return center + rotated;
        }

        public void FocusHidden()
        {
            foreach (var kv in _inputs) kv.Value.Focus();
        }
    }
}
