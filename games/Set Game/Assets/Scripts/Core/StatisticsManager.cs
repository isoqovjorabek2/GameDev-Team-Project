using UnityEngine;

namespace SetGame.Core
{
    /// <summary>
    /// Tracks comprehensive game statistics and persists them between sessions.
    /// </summary>
    public class StatisticsManager : MonoBehaviour
    {
        public static StatisticsManager Instance { get; private set; }

        // Statistics
        public int TotalGamesPlayed { get; private set; }
        public int TotalSetsFound { get; private set; }
        public int TotalHintsUsed { get; private set; }
        public int BestCombo { get; private set; }
        public int BestSingleGameScore { get; private set; }
        public float TotalPlayTime { get; private set; }
        public int GamesWon { get; private set; }
        public int GamesLostByLives { get; private set; }
        public int GamesLostByTime { get; private set; }
        public float AverageSetsPerGame { get; private set; }

        // Session tracking
        int _currentGameSets;
        int _currentGameCombo;
        float _sessionStartTime;

        // PlayerPrefs keys
        const string KEY_TOTAL_GAMES = "Stats_TotalGames";
        const string KEY_TOTAL_SETS = "Stats_TotalSets";
        const string KEY_TOTAL_HINTS = "Stats_TotalHints";
        const string KEY_BEST_COMBO = "Stats_BestCombo";
        const string KEY_BEST_SCORE = "Stats_BestScore";
        const string KEY_TOTAL_TIME = "Stats_TotalTime";
        const string KEY_GAMES_WON = "Stats_GamesWon";
        const string KEY_LOST_LIVES = "Stats_LostLives";
        const string KEY_LOST_TIME = "Stats_LostTime";

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            LoadStatistics();
        }

        void OnEnable()
        {
            GameEvents.OnGameStarted += OnGameStarted;
            GameEvents.OnGameOver += OnGameOver;
            GameEvents.OnValidSetFound += OnValidSet;
            GameEvents.OnHintUsed += OnHintUsed;
        }

        void OnDisable()
        {
            GameEvents.OnGameStarted -= OnGameStarted;
            GameEvents.OnGameOver -= OnGameOver;
            GameEvents.OnValidSetFound -= OnValidSet;
            GameEvents.OnHintUsed -= OnHintUsed;
        }

        void OnGameStarted()
        {
            _currentGameSets = 0;
            _sessionStartTime = Time.time;
        }

        void OnValidSet(System.Collections.Generic.List<int> _)
        {
            _currentGameSets++;
            TotalSetsFound++;

            // Track best combo
            if (ScoreSystem.Instance != null && ScoreSystem.Instance.Combo > BestCombo)
            {
                BestCombo = ScoreSystem.Instance.Combo;
            }
        }

        void OnHintUsed()
        {
            TotalHintsUsed++;
        }

        void OnGameOver()
        {
            TotalGamesPlayed++;
            TotalPlayTime += Time.time - _sessionStartTime;

            // Track win/loss
            if (ScoreSystem.Instance != null)
            {
                if (ScoreSystem.Instance.Lives <= 0)
                    GamesLostByLives++;
                else
                    GamesLostByTime++;

                // Track best single game score
                if (ScoreSystem.Instance.Score > BestSingleGameScore)
                {
                    BestSingleGameScore = ScoreSystem.Instance.Score;
                }

                // Consider games with 10+ sets as "won"
                if (_currentGameSets >= 10)
                    GamesWon++;
            }

            // Update average
            if (TotalGamesPlayed > 0)
                AverageSetsPerGame = (float)TotalSetsFound / TotalGamesPlayed;

            SaveStatistics();
        }

        void LoadStatistics()
        {
            TotalGamesPlayed = PlayerPrefs.GetInt(KEY_TOTAL_GAMES, 0);
            TotalSetsFound = PlayerPrefs.GetInt(KEY_TOTAL_SETS, 0);
            TotalHintsUsed = PlayerPrefs.GetInt(KEY_TOTAL_HINTS, 0);
            BestCombo = PlayerPrefs.GetInt(KEY_BEST_COMBO, 0);
            BestSingleGameScore = PlayerPrefs.GetInt(KEY_BEST_SCORE, 0);
            TotalPlayTime = PlayerPrefs.GetFloat(KEY_TOTAL_TIME, 0f);
            GamesWon = PlayerPrefs.GetInt(KEY_GAMES_WON, 0);
            GamesLostByLives = PlayerPrefs.GetInt(KEY_LOST_LIVES, 0);
            GamesLostByTime = PlayerPrefs.GetInt(KEY_LOST_TIME, 0);

            if (TotalGamesPlayed > 0)
                AverageSetsPerGame = (float)TotalSetsFound / TotalGamesPlayed;
        }

        void SaveStatistics()
        {
            PlayerPrefs.SetInt(KEY_TOTAL_GAMES, TotalGamesPlayed);
            PlayerPrefs.SetInt(KEY_TOTAL_SETS, TotalSetsFound);
            PlayerPrefs.SetInt(KEY_TOTAL_HINTS, TotalHintsUsed);
            PlayerPrefs.SetInt(KEY_BEST_COMBO, BestCombo);
            PlayerPrefs.SetInt(KEY_BEST_SCORE, BestSingleGameScore);
            PlayerPrefs.SetFloat(KEY_TOTAL_TIME, TotalPlayTime);
            PlayerPrefs.SetInt(KEY_GAMES_WON, GamesWon);
            PlayerPrefs.SetInt(KEY_LOST_LIVES, GamesLostByLives);
            PlayerPrefs.SetInt(KEY_LOST_TIME, GamesLostByTime);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Resets all statistics to zero.
        /// </summary>
        public void ResetAllStatistics()
        {
            TotalGamesPlayed = 0;
            TotalSetsFound = 0;
            TotalHintsUsed = 0;
            BestCombo = 0;
            BestSingleGameScore = 0;
            TotalPlayTime = 0f;
            GamesWon = 0;
            GamesLostByLives = 0;
            GamesLostByTime = 0;
            AverageSetsPerGame = 0f;
            SaveStatistics();
        }

        /// <summary>
        /// Gets formatted statistics string for display.
        /// </summary>
        public string GetStatisticsDisplay()
        {
            return $"Games Played: {TotalGamesPlayed}\n" +
                   $"Sets Found: {TotalSetsFound}\n" +
                   $"Best Combo: {BestCombo}x\n" +
                   $"Best Score: {BestSingleGameScore:N0}\n" +
                   $"Win Rate: {(TotalGamesPlayed > 0 ? (GamesWon * 100f / TotalGamesPlayed) : 0f):F1}%\n" +
                   $"Avg Sets/Game: {AverageSetsPerGame:F1}\n" +
                   $"Total Play Time: {FormatPlayTime(TotalPlayTime)}";
        }

        string FormatPlayTime(float seconds)
        {
            int hours = (int)(seconds / 3600);
            int mins = (int)((seconds % 3600) / 60);
            return hours > 0 ? $"{hours}h {mins}m" : $"{mins}m";
        }
    }
}