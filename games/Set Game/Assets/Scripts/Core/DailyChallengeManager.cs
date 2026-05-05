using UnityEngine;
using System;

namespace SetGame.Core
{
    /// <summary>
    /// Manages daily challenge mode with seeded randomization for consistent daily puzzles.
    /// </summary>
    public class DailyChallengeManager : MonoBehaviour
    {
        public static DailyChallengeManager Instance { get; private set; }

        // Daily challenge state
        public bool IsDailyChallenge { get; private set; }
        public DateTime CurrentDate { get; private set; }
        public int DailySeed { get; private set; }
        public bool DailyChallengeCompleted { get; private set; }
        public int DailyBestScore { get; private set; }
        public int DailyBestSets { get; private set; }

        // Seeded random state
        System.Random _seededRandom;

        // Session tracking
        int _currentSessionSets;

        // PlayerPrefs keys
        const string KEY_DAILY_DATE = "Daily_Date";
        const string KEY_DAILY_SEED = "Daily_Seed";
        const string KEY_DAILY_COMPLETED = "Daily_Completed";
        const string KEY_DAILY_BEST_SCORE = "Daily_BestScore";
        const string KEY_DAILY_BEST_SETS = "Daily_BestSets";

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            LoadDailyChallengeData();
        }

        void OnEnable()
        {
            GameEvents.OnGameStarted += OnGameStarted;
            GameEvents.OnValidSetFound += OnValidSet;
            GameEvents.OnGameOver += OnGameOver;
        }

        void OnDisable()
        {
            GameEvents.OnGameStarted -= OnGameStarted;
            GameEvents.OnValidSetFound -= OnValidSet;
            GameEvents.OnGameOver -= OnGameOver;
        }

        void OnGameStarted()
        {
            if (IsDailyChallenge)
            {
                // Initialize seeded random for this session
                _seededRandom = new System.Random(DailySeed);
                _currentSessionSets = 0;
            }
        }

        void OnValidSet(System.Collections.Generic.List<int> _)
        {
            if (IsDailyChallenge)
            {
                _currentSessionSets++;
            }
        }

        void OnGameOver()
        {
            if (IsDailyChallenge && ScoreSystem.Instance != null)
            {
                // Update daily best scores
                int currentScore = ScoreSystem.Instance.Score;
                int currentSets = _currentSessionSets;

                if (currentScore > DailyBestScore)
                {
                    DailyBestScore = currentScore;
                }

                if (currentSets > DailyBestSets)
                {
                    DailyBestSets = currentSets;
                }

                // Mark as completed if player found at least 5 sets
                if (currentSets >= 5 && !DailyChallengeCompleted)
                {
                    DailyChallengeCompleted = true;
                }

                SaveDailyChallengeData();
            }
        }

        /// <summary>
        /// Starts a new daily challenge session.
        /// </summary>
        public void StartDailyChallenge()
        {
            CheckForNewDay();
            IsDailyChallenge = true;
            GameManager.Instance?.StartGame();
        }

        /// <summary>
        /// Gets a seeded random value for consistent daily card generation.
        /// </summary>
        public int NextSeededRandom(int min, int max)
        {
            if (_seededRandom == null)
                _seededRandom = new System.Random(DailySeed);
            return _seededRandom.Next(min, max);
        }

        /// <summary>
        /// Gets a seeded random float value.
        /// </summary>
        public float NextSeededRandomFloat()
        {
            if (_seededRandom == null)
                _seededRandom = new System.Random(DailySeed);
            return (float)_seededRandom.NextDouble();
        }

        /// <summary>
        /// Checks if a new day has started and resets daily challenge if needed.
        /// </summary>
        void CheckForNewDay()
        {
            DateTime today = DateTime.Today;
            string savedDateStr = PlayerPrefs.GetString(KEY_DAILY_DATE, string.Empty);

            if (string.IsNullOrEmpty(savedDateStr) || DateTime.Parse(savedDateStr) != today)
            {
                // New day - reset daily challenge
                CurrentDate = today;
                DailySeed = GenerateDailySeed(today);
                DailyChallengeCompleted = false;
                DailyBestScore = 0;
                DailyBestSets = 0;
                SaveDailyChallengeData();
            }
        }

        /// <summary>
        /// Generates a consistent seed based on the date.
        /// </summary>
        int GenerateDailySeed(DateTime date)
        {
            // Use date components to create a consistent seed
            int seed = date.Year * 10000 + date.Month * 100 + date.Day;
            return seed * 17; // Multiply by prime for better distribution
        }

        void LoadDailyChallengeData()
        {
            string dateStr = PlayerPrefs.GetString(KEY_DAILY_DATE, string.Empty);
            if (!string.IsNullOrEmpty(dateStr))
            {
                CurrentDate = DateTime.Parse(dateStr);
            }
            else
            {
                CurrentDate = DateTime.Today;
            }

            DailySeed = PlayerPrefs.GetInt(KEY_DAILY_SEED, GenerateDailySeed(CurrentDate));
            DailyChallengeCompleted = PlayerPrefs.GetInt(KEY_DAILY_COMPLETED, 0) == 1;
            DailyBestScore = PlayerPrefs.GetInt(KEY_DAILY_BEST_SCORE, 0);
            DailyBestSets = PlayerPrefs.GetInt(KEY_DAILY_BEST_SETS, 0);

            // Check if we need to reset for new day
            CheckForNewDay();
        }

        void SaveDailyChallengeData()
        {
            PlayerPrefs.SetString(KEY_DAILY_DATE, CurrentDate.ToString("yyyy-MM-dd"));
            PlayerPrefs.SetInt(KEY_DAILY_SEED, DailySeed);
            PlayerPrefs.SetInt(KEY_DAILY_COMPLETED, DailyChallengeCompleted ? 1 : 0);
            PlayerPrefs.SetInt(KEY_DAILY_BEST_SCORE, DailyBestScore);
            PlayerPrefs.SetInt(KEY_DAILY_BEST_SETS, DailyBestSets);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Gets the remaining time until the next daily challenge.
        /// </summary>
        public string GetTimeUntilNextDaily()
        {
            DateTime tomorrow = DateTime.Today.AddDays(1);
            TimeSpan remaining = tomorrow - DateTime.Now;

            if (remaining.TotalHours > 24)
                remaining = TimeSpan.FromHours(24);

            return $"{remaining.Hours:D2}h {remaining.Minutes:D2}m {remaining.Seconds:D2}s";
        }

        /// <summary>
        /// Gets the daily challenge status display text.
        /// </summary>
        public string GetDailyChallengeStatus()
        {
            string status = DailyChallengeCompleted ? "✓ COMPLETED" : "IN PROGRESS";
            return $"DAILY CHALLENGE - {CurrentDate:MMM dd}\n" +
                   $"Status: {status}\n" +
                   $"Best Score: {DailyBestScore:N0}\n" +
                   $"Best Sets: {DailyBestSets}\n" +
                   $"Resets in: {GetTimeUntilNextDaily()}";
        }

        /// <summary>
        /// Resets the daily challenge (for testing purposes).
        /// </summary>
        public void ResetDailyChallenge()
        {
            PlayerPrefs.DeleteKey(KEY_DAILY_DATE);
            PlayerPrefs.DeleteKey(KEY_DAILY_SEED);
            PlayerPrefs.DeleteKey(KEY_DAILY_COMPLETED);
            PlayerPrefs.DeleteKey(KEY_DAILY_BEST_SCORE);
            PlayerPrefs.DeleteKey(KEY_DAILY_BEST_SETS);
            PlayerPrefs.Save();
            LoadDailyChallengeData();
        }
    }
}