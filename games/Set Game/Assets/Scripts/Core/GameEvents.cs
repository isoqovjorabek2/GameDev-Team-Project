using System;
using System.Collections.Generic;

namespace SetGame.Core
{
    /// <summary>
    /// Lightweight static event bus so systems don't need direct references.
    /// </summary>
    public static class GameEvents
    {
        // --- Game flow ---
        public static event Action OnGameStarted;
        public static event Action OnGameOver;
        public static event Action OnGamePaused;
        public static event Action OnGameResumed;

        // --- Card events ---
        public static event Action<int> OnCardSelected;       // board index
        public static event Action<int> OnCardDeselected;

        // --- Set events ---
        public static event Action<List<int>> OnValidSetFound;
        public static event Action<List<int>> OnInvalidSetAttempt;

        // --- Score / lives ---
        public static event Action<int>   OnScoreChanged;     // new total
        public static event Action<int>   OnComboChanged;     // combo count
        public static event Action<int>   OnLivesChanged;     // remaining lives
        public static event Action<float> OnTimerTick;        // seconds remaining

        // --- Board ---
        public static event Action        OnBoardRefreshed;
        public static event Action<int>   OnHintRequested;    // hint card index
        public static event Action        OnHintUsed;         // hint was used
        public static event Action        OnDeckEmpty;

        // --- High score ---
        public static event Action<int>   OnHighScoreBeaten;

        // Raise helpers
        public static void RaiseGameStarted()                     => OnGameStarted?.Invoke();
        public static void RaiseGameOver()                        => OnGameOver?.Invoke();
        public static void RaiseGamePaused()                      => OnGamePaused?.Invoke();
        public static void RaiseGameResumed()                     => OnGameResumed?.Invoke();
        public static void RaiseCardSelected(int idx)             => OnCardSelected?.Invoke(idx);
        public static void RaiseCardDeselected(int idx)           => OnCardDeselected?.Invoke(idx);
        public static void RaiseValidSet(List<int> indices)       => OnValidSetFound?.Invoke(indices);
        public static void RaiseInvalidSet(List<int> indices)     => OnInvalidSetAttempt?.Invoke(indices);
        public static void RaiseScoreChanged(int score)           => OnScoreChanged?.Invoke(score);
        public static void RaiseComboChanged(int combo)           => OnComboChanged?.Invoke(combo);
        public static void RaiseLivesChanged(int lives)           => OnLivesChanged?.Invoke(lives);
        public static void RaiseTimerTick(float secs)             => OnTimerTick?.Invoke(secs);
        public static void RaiseBoardRefreshed()                  => OnBoardRefreshed?.Invoke();
        public static void RaiseHint(int idx)                     => OnHintRequested?.Invoke(idx);
        public static void RaiseHintUsed()                        => OnHintUsed?.Invoke();
        public static void RaiseDeckEmpty()                       => OnDeckEmpty?.Invoke();
        public static void RaiseHighScoreBeaten(int score)        => OnHighScoreBeaten?.Invoke(score);

        public static void ClearAllListeners()
        {
            OnGameStarted = null;
            OnGameOver = null;
            OnGamePaused = null;
            OnGameResumed = null;
            OnCardSelected = null;
            OnCardDeselected = null;
            OnValidSetFound = null;
            OnInvalidSetAttempt = null;
            OnScoreChanged = null;
            OnComboChanged = null;
            OnLivesChanged = null;
            OnTimerTick = null;
            OnBoardRefreshed = null;
            OnHintRequested = null;
            OnHintUsed = null;
            OnDeckEmpty = null;
            OnHighScoreBeaten = null;
        }
    }
}
