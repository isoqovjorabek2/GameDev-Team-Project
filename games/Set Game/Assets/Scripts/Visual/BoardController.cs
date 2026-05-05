using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SetGame.Core;

namespace SetGame.Visual
{
    /// <summary>
    /// Owns the visible card grid. Talks to GameManager for logical state.
    /// </summary>
    public class BoardController : MonoBehaviour
    {
        public static BoardController Instance { get; private set; }

        public RectTransform GridContainer;  // assigned by Bootstrap

        const int COLS = 4;
        const float CELL_W = CardFactory.CARD_W + 14f;
        const float CELL_H = CardFactory.CARD_H + 14f;

        readonly List<CardView> _views = new();
        readonly List<CardData> _board = new();   // mirrors GameManager.Board
        readonly List<int>      _selection = new();

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable()
        {
            GameEvents.OnBoardRefreshed   += OnBoardRefreshed;
            GameEvents.OnValidSetFound    += OnValidSet;
            GameEvents.OnInvalidSetAttempt+= OnInvalidSet;
            GameEvents.OnHintRequested    += OnHint;
            GameEvents.OnCardSelected     += OnCardSelected;
            GameEvents.OnCardDeselected   += OnCardDeselected;
        }

        void OnDisable()
        {
            GameEvents.OnBoardRefreshed   -= OnBoardRefreshed;
            GameEvents.OnValidSetFound    -= OnValidSet;
            GameEvents.OnInvalidSetAttempt-= OnInvalidSet;
            GameEvents.OnHintRequested    -= OnHint;
            GameEvents.OnCardSelected     -= OnCardSelected;
            GameEvents.OnCardDeselected   -= OnCardDeselected;
        }

        // ─── Event handlers ───────────────────────────────────────────────────────

        void OnBoardRefreshed() => SyncWithManager();

        void OnValidSet(List<int> indices) => StartCoroutine(PlayValidSet(indices));
        void OnInvalidSet(List<int> indices) => StartCoroutine(PlayInvalidSet(indices));

        void OnHint(int idx)
        {
            if (idx < _views.Count) _views[idx]?.ShowHint();
        }

        void OnCardSelected(int idx)
        {
            if (!_selection.Contains(idx)) _selection.Add(idx);
        }

        void OnCardDeselected(int idx)
        {
            _selection.Remove(idx);
        }

        // ─── Board sync ───────────────────────────────────────────────────────────

        public void SyncWithManager()
        {
            if (GameManager.Instance == null) return;
            var board = GameManager.Instance.Board;

            // Clear extra views
            while (_views.Count > board.Count)
            {
                int last = _views.Count - 1;
                if (_views[last] != null) Destroy(_views[last].gameObject);
                _views.RemoveAt(last);
            }

            // Update or create views
            for (int i = 0; i < board.Count; i++)
            {
                if (i < _views.Count && _views[i] != null)
                {
                    if (!_views[i].Data.Equals(board[i]))
                    {
                        Destroy(_views[i].gameObject);
                        _views[i] = SpawnCard(board[i], i);
                    }
                }
                else
                {
                    if (i >= _views.Count) _views.Add(null);
                    _views[i] = SpawnCard(board[i], i);
                }
            }

            LayoutCards();
        }

        CardView SpawnCard(CardData data, int idx)
        {
            var cv = CardFactory.Create(data, idx, GridContainer);
            StartCoroutine(cv.DealIn(idx * 0.04f));
            return cv;
        }

        void LayoutCards()
        {
            int n = _views.Count;
            int cols = COLS;
            int rows = Mathf.CeilToInt(n / (float)cols);
            float totalW = cols * CELL_W;
            float totalH = rows * CELL_H;

            for (int i = 0; i < _views.Count; i++)
            {
                if (_views[i] == null) continue;
                int col = i % cols;
                int row = i / cols;
                float x = -totalW * 0.5f + col * CELL_W + CELL_W * 0.5f;
                float y =  totalH * 0.5f - row * CELL_H - CELL_H * 0.5f;
                var rt = _views[i].GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(x, y);
            }
        }

        // ─── Animations ───────────────────────────────────────────────────────────

        IEnumerator PlayValidSet(List<int> indices)
        {
            SetAllInteractive(false);
            var coroutines = new List<Coroutine>();
            foreach (int idx in indices)
                if (idx < _views.Count && _views[idx] != null)
                    coroutines.Add(StartCoroutine(_views[idx].ValidSetAnimation()));

            // Wait for animations to complete
            yield return new WaitForSeconds(0.45f);

            // Replace views with null (destroyed by animation)
            foreach (int idx in indices)
                if (idx < _views.Count) _views[idx] = null;

            yield return new WaitForSeconds(0.1f);
            SyncWithManager();
            SetAllInteractive(true);
        }

        IEnumerator PlayInvalidSet(List<int> indices)
        {
            SetAllInteractive(false);
            var tasks = new List<Coroutine>();
            foreach (int idx in indices)
                if (idx < _views.Count && _views[idx] != null)
                    tasks.Add(StartCoroutine(_views[idx].InvalidSetAnimation()));

            yield return new WaitForSeconds(0.5f);
            _selection.Clear();
            SetAllInteractive(true);
        }

        void SetAllInteractive(bool on)
        {
            foreach (var v in _views) v?.SetInteractive(on);
        }

        public void ClearHints()
        {
            foreach (var v in _views) v?.ClearHint();
        }

        public List<int> GetSelection() => new(_selection);
    }
}
