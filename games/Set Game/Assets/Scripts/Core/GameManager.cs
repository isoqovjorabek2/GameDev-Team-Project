using System.Collections.Generic;
using UnityEngine;
using SetGame.Visual;

namespace SetGame.Core
{
    public enum GameState { MainMenu, Playing, Paused, GameOver }

    /// <summary>
    /// Central game logic: deck, board state, selection validation, timer.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // Config
        public float TimerDuration  = 180f;   // seconds per game
        public int   MinBoardSize   = 12;
        public int   HintCooldown   = 15;     // seconds between free hints

        public GameState        State  { get; private set; } = GameState.MainMenu;
        public List<CardData>   Board  { get; private set; } = new();
        public int              DeckRemaining => _deck.Remaining;
        public float            TimeRemaining { get; private set; }
        public bool             TimerMode     = true;

        Deck            _deck;
        List<int>       _selection = new();
        float           _hintCooldownTimer;
        bool            _running;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Update()
        {
            if (State != GameState.Playing || !_running) return;

            if (TimerMode)
            {
                TimeRemaining -= Time.deltaTime;
                GameEvents.RaiseTimerTick(TimeRemaining);
                if (TimeRemaining <= 0)
                {
                    TimeRemaining = 0;
                    TriggerGameOver();
                    return;
                }
            }

            _hintCooldownTimer -= Time.deltaTime;
        }

        // ─── Public API ───────────────────────────────────────────────────────────

        public void StartGame()
        {
            GameEvents.ClearAllListeners();
            ReRegisterEvents();

            _deck = new Deck();
            Board.Clear();
            _selection.Clear();
            TimeRemaining      = TimerDuration;
            _hintCooldownTimer = 0;
            State              = GameState.Playing;
            _running           = true;

            ScoreSystem.Instance?.ResetForNewGame();

            // Deal initial board (12 cards, guaranteed to have at least one SET)
            DealToMinimum();
            GameEvents.RaiseGameStarted();
            GameEvents.RaiseBoardRefreshed();
        }

        public void HandleCardSelected(int boardIndex)
        {
            if (State != GameState.Playing) return;
            if (_selection.Contains(boardIndex)) return;

            _selection.Add(boardIndex);
            BoardController.Instance?.ClearHints();

            if (_selection.Count == 3)
                EvaluateSelection();
        }

        public void HandleCardDeselected(int boardIndex)
        {
            _selection.Remove(boardIndex);
        }

        public void RequestHint()
        {
            if (State != GameState.Playing) return;
            if (_hintCooldownTimer > 0) return;

            var hint = SetValidator.FindHint(Board);
            if (hint.HasValue)
            {
                ScoreSystem.Instance?.OnHintUsed();
                GameEvents.RaiseHintUsed();
                _hintCooldownTimer = HintCooldown;
                GameEvents.RaiseHint(hint.Value.i);
                GameEvents.RaiseHint(hint.Value.j);
                GameEvents.RaiseHint(hint.Value.k);
            }
        }

        public void PauseGame()
        {
            if (State != GameState.Playing) return;
            State    = GameState.Paused;
            _running = false;
            Time.timeScale = 0f;
            GameEvents.RaiseGamePaused();
        }

        public void ResumeGame()
        {
            if (State != GameState.Paused) return;
            State    = GameState.Playing;
            _running = true;
            Time.timeScale = 1f;
            GameEvents.RaiseGameResumed();
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            State    = GameState.MainMenu;
            _running = false;
            Board.Clear();
            _selection.Clear();
        }

        // ─── Internal logic ───────────────────────────────────────────────────────

        void EvaluateSelection()
        {
            var a = Board[_selection[0]];
            var b = Board[_selection[1]];
            var c = Board[_selection[2]];

            if (SetValidator.IsValidSet(a, b, c))
            {
                ScoreSystem.Instance?.OnValidSet();
                var removed = new List<int>(_selection);
                _selection.Clear();

                // Remove cards in descending index order to preserve indices
                removed.Sort((x, y) => y.CompareTo(x));
                foreach (int idx in removed) Board.RemoveAt(idx);

                DealToMinimum();
                GameEvents.RaiseValidSet(removed);
                GameEvents.RaiseBoardRefreshed();

                if (_deck.Remaining == 0 && !SetValidator.BoardContainsSet(Board))
                {
                    GameEvents.RaiseDeckEmpty();
                    TriggerGameOver();
                }
            }
            else
            {
                ScoreSystem.Instance?.OnInvalidSet();
                var wrongIndices = new List<int>(_selection);
                _selection.Clear();

                GameEvents.RaiseInvalidSet(wrongIndices);

                if (ScoreSystem.Instance != null && ScoreSystem.Instance.IsGameOver)
                    TriggerGameOver();
            }
        }

        void DealToMinimum()
        {
            while (Board.Count < MinBoardSize && _deck.Remaining > 0)
            {
                var card = _deck.Draw();
                if (card.HasValue) Board.Add(card.Value);
            }

            // Ensure at least one SET exists; if not, deal 3 more cards
            while (!SetValidator.BoardContainsSet(Board) && _deck.Remaining >= 3)
            {
                for (int i = 0; i < 3 && _deck.Remaining > 0; i++)
                {
                    var card = _deck.Draw();
                    if (card.HasValue) Board.Add(card.Value);
                }
            }
        }

        void TriggerGameOver()
        {
            _running = false;
            State    = GameState.GameOver;
            Time.timeScale = 1f;
            GameEvents.RaiseGameOver();
        }

        void ReRegisterEvents()
        {
            GameEvents.OnCardSelected   += HandleCardSelected;
            GameEvents.OnCardDeselected += HandleCardDeselected;
        }
    }
}
