using UnityEngine;

namespace SetGame.Core
{
    /// <summary>
    /// Manages dynamic difficulty progression based on player performance.
    /// Adjusts game parameters to maintain optimal challenge level.
    /// </summary>
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        // Difficulty levels
        public enum DifficultyLevel { Beginner, Easy, Normal, Hard, Expert, Master }

        // Current difficulty state
        public DifficultyLevel CurrentDifficulty { get; private set; } = DifficultyLevel.Normal;
        public float DifficultyProgress { get; private set; } // 0.0 to 1.0 within current level

        // Dynamic parameters (adjusted based on difficulty)
        public float CurrentTimerDuration { get; private set; }
        public int CurrentHintCooldown { get; private set; }
        public int CurrentMinBoardSize { get; private set; }
        public float CurrentScoreMultiplier { get; private set; }

        // Performance tracking
        int _consecutiveValidSets;
        int _consecutiveInvalidSets;
        float _averageSetFindTime;
        int _totalSetsFoundInCurrentGame;
        float _gameStartTime;

        // Difficulty thresholds
        const int SETS_TO_PROMOTE = 8;
        const int SETS_TO_DEMOTE = 3;
        const float FAST_SET_TIME = 3.0f; // seconds
        const float SLOW_SET_TIME = 15.0f; // seconds

        // Base parameters for each difficulty level
        readonly struct DifficultySettings
        {
            public readonly float timerDuration;
            public readonly int hintCooldown;
            public readonly int minBoardSize;
            public readonly float scoreMultiplier;

            public DifficultySettings(float timer, int hintCooldown, int boardSize, float scoreMult)
            {
                timerDuration = timer;
                this.hintCooldown = hintCooldown;
                minBoardSize = boardSize;
                scoreMultiplier = scoreMult;
            }
        }

        readonly DifficultySettings[] _difficultySettings = new DifficultySettings[]
        {
            new DifficultySettings(300f, 10, 12, 0.8f),  // Beginner
            new DifficultySettings(240f, 12, 12, 0.9f),  // Easy
            new DifficultySettings(180f, 15, 12, 1.0f),  // Normal
            new DifficultySettings(150f, 18, 12, 1.2f),  // Hard
            new DifficultySettings(120f, 20, 15, 1.5f),  // Expert
            new DifficultySettings(90f,  25, 15, 2.0f)   // Master
        };

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable()
        {
            GameEvents.OnGameStarted += OnGameStarted;
            GameEvents.OnValidSetFound += OnValidSet;
            GameEvents.OnInvalidSetAttempt += OnInvalidSet;
            GameEvents.OnGameOver += OnGameOver;
        }

        void OnDisable()
        {
            GameEvents.OnGameStarted -= OnGameStarted;
            GameEvents.OnValidSetFound -= OnValidSet;
            GameEvents.OnInvalidSetAttempt -= OnInvalidSet;
            GameEvents.OnGameOver -= OnGameOver;
        }

        void OnGameStarted()
        {
            _consecutiveValidSets = 0;
            _consecutiveInvalidSets = 0;
            _totalSetsFoundInCurrentGame = 0;
            _averageSetFindTime = 0f;
            _gameStartTime = Time.time;

            // Apply current difficulty settings
            ApplyDifficultySettings();
        }

        void OnValidSet(System.Collections.Generic.List<int> _)
        {
            _consecutiveValidSets++;
            _consecutiveInvalidSets = 0;
            _totalSetsFoundInCurrentGame++;

            // Calculate set find time
            float setTime = Time.time - _gameStartTime;
            _averageSetFindTime = (_averageSetFindTime * (_totalSetsFoundInCurrentGame - 1) + setTime) / _totalSetsFoundInCurrentGame;

            // Check for difficulty promotion
            if (_consecutiveValidSets >= SETS_TO_PROMOTE && _averageSetFindTime < FAST_SET_TIME)
            {
                TryPromoteDifficulty();
            }

            // Update progress within current level
            UpdateDifficultyProgress();
        }

        void OnInvalidSet(System.Collections.Generic.List<int> _)
        {
            _consecutiveValidSets = 0;
            _consecutiveInvalidSets++;

            // Check for difficulty demotion
            if (_consecutiveInvalidSets >= SETS_TO_DEMOTE)
            {
                TryDemoteDifficulty();
            }
        }

        void OnGameOver()
        {
            // Evaluate overall performance and adjust difficulty for next game
            EvaluateGamePerformance();
        }

        void TryPromoteDifficulty()
        {
            if (CurrentDifficulty < DifficultyLevel.Master)
            {
                CurrentDifficulty++;
                _consecutiveValidSets = 0;
                ApplyDifficultySettings();
                Debug.Log($"Difficulty promoted to {CurrentDifficulty}");
            }
        }

        void TryDemoteDifficulty()
        {
            if (CurrentDifficulty > DifficultyLevel.Beginner)
            {
                CurrentDifficulty--;
                _consecutiveInvalidSets = 0;
                ApplyDifficultySettings();
                Debug.Log($"Difficulty demoted to {CurrentDifficulty}");
            }
        }

        void UpdateDifficultyProgress()
        {
            // Calculate progress toward next difficulty level
            float progress = Mathf.Clamp01((float)_consecutiveValidSets / SETS_TO_PROMOTE);
            DifficultyProgress = progress;
        }

        void ApplyDifficultySettings()
        {
            int levelIndex = (int)CurrentDifficulty;
            var settings = _difficultySettings[levelIndex];

            CurrentTimerDuration = settings.timerDuration;
            CurrentHintCooldown = settings.hintCooldown;
            CurrentMinBoardSize = settings.minBoardSize;
            CurrentScoreMultiplier = settings.scoreMultiplier;

            // Update GameManager if available
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TimerDuration = CurrentTimerDuration;
                GameManager.Instance.HintCooldown = CurrentHintCooldown;
                GameManager.Instance.MinBoardSize = CurrentMinBoardSize;
            }
        }

        void EvaluateGamePerformance()
        {
            // Adjust difficulty based on overall game performance
            float avgTimePerSet = _totalSetsFoundInCurrentGame > 0 ?
                (Time.time - _gameStartTime) / _totalSetsFoundInCurrentGame : float.MaxValue;

            if (_totalSetsFoundInCurrentGame >= 15 && avgTimePerSet < FAST_SET_TIME)
            {
                // Excellent performance - consider promotion
                if (CurrentDifficulty < DifficultyLevel.Master)
                    CurrentDifficulty++;
            }
            else if (_totalSetsFoundInCurrentGame < 5 || avgTimePerSet > SLOW_SET_TIME)
            {
                // Poor performance - consider demotion
                if (CurrentDifficulty > DifficultyLevel.Beginner)
                    CurrentDifficulty--;
            }
        }

        /// <summary>
        /// Manually sets the difficulty level.
        /// </summary>
        public void SetDifficulty(DifficultyLevel level)
        {
            CurrentDifficulty = level;
            ApplyDifficultySettings();
        }

        /// <summary>
        /// Gets the display name for the current difficulty level.
        /// </summary>
        public string GetDifficultyDisplayName()
        {
            return CurrentDifficulty.ToString();
        }

        /// <summary>
        /// Gets a description of the current difficulty settings.
        /// </summary>
        public string GetDifficultyDescription()
        {
            int levelIndex = (int)CurrentDifficulty;
            var settings = _difficultySettings[levelIndex];

            return $"Time: {(int)settings.timerDuration}s | " +
                   $"Hints: {settings.hintCooldown}s cooldown | " +
                   $"Board: {settings.minBoardSize} cards | " +
                   $"Score: ×{settings.scoreMultiplier}";
        }
    }
}