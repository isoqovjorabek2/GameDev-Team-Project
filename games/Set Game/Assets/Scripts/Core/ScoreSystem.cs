using UnityEngine;

namespace SetGame.Core
{
    public class ScoreSystem : MonoBehaviour
    {
        public static ScoreSystem Instance { get; private set; }

        // Tuning
        const int BASE_POINTS      = 100;
        const int COMBO_MULTIPLIER = 50;
        const int HINT_PENALTY     = 25;
        const int MISTAKE_PENALTY  = 50;
        const int START_LIVES      = 3;

        public int  Score      { get; private set; }
        public int  Combo      { get; private set; }
        public int  Lives      { get; private set; }
        public int  HighScore  { get; private set; }
        public bool IsGameOver => Lives <= 0;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            HighScore = PlayerPrefs.GetInt("HighScore", 0);
        }

        public void ResetForNewGame()
        {
            Score = 0; Combo = 0; Lives = START_LIVES;
            GameEvents.RaiseScoreChanged(Score);
            GameEvents.RaiseComboChanged(Combo);
            GameEvents.RaiseLivesChanged(Lives);
        }

        public void OnValidSet()
        {
            Combo++;
            int points = BASE_POINTS + (Combo - 1) * COMBO_MULTIPLIER;
            Score += points;
            GameEvents.RaiseScoreChanged(Score);
            GameEvents.RaiseComboChanged(Combo);
            CheckHighScore();
        }

        public void OnInvalidSet()
        {
            Combo = 0;
            Score = Mathf.Max(0, Score - MISTAKE_PENALTY);
            Lives--;
            GameEvents.RaiseScoreChanged(Score);
            GameEvents.RaiseComboChanged(Combo);
            GameEvents.RaiseLivesChanged(Lives);
        }

        public void OnHintUsed()
        {
            Score = Mathf.Max(0, Score - HINT_PENALTY);
            GameEvents.RaiseScoreChanged(Score);
        }

        void CheckHighScore()
        {
            if (Score > HighScore)
            {
                HighScore = Score;
                PlayerPrefs.SetInt("HighScore", HighScore);
                PlayerPrefs.Save();
                GameEvents.RaiseHighScoreBeaten(HighScore);
            }
        }
    }
}
