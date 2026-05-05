using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SetGame.Core;

namespace SetGame.UI
{
    /// <summary>
    /// Manages all UI panels: Main Menu, HUD, Pause, Game Over.
    /// All panels are created by GameBootstrap and referenced here.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        // Panels (assigned by Bootstrap)
        public GameObject MainMenuPanel;
        public GameObject HUDPanel;
        public GameObject PausePanel;
        public GameObject GameOverPanel;
        public GameObject StatisticsPanel;

        // HUD elements
        public TMP_Text ScoreText;
        public TMP_Text HighScoreText;
        public TMP_Text LivesText;
        public TMP_Text TimerText;
        public TMP_Text ComboText;
        public TMP_Text DeckText;
        public Slider   TimerSlider;

        // Game Over panel
        public TMP_Text FinalScoreText;
        public TMP_Text FinalHighScoreText;
        public TMP_Text GameOverTitleText;

        // Main Menu
        public TMP_Text MenuHighScoreText;
        public TMP_Text DailyStatusText;

        // Statistics panel
        public TMP_Text StatisticsText;

        // Difficulty selector
        public TMP_Text CurrentDifficultyText;

        float _maxTimer;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable()
        {
            GameEvents.OnGameStarted       += OnGameStarted;
            GameEvents.OnGameOver          += OnGameOver;
            GameEvents.OnGamePaused        += OnGamePaused;
            GameEvents.OnGameResumed       += OnGameResumed;
            GameEvents.OnScoreChanged      += OnScoreChanged;
            GameEvents.OnComboChanged      += OnComboChanged;
            GameEvents.OnLivesChanged      += OnLivesChanged;
            GameEvents.OnTimerTick         += OnTimerTick;
            GameEvents.OnHighScoreBeaten   += OnHighScoreBeaten;
        }

        void OnDisable()
        {
            GameEvents.OnGameStarted       -= OnGameStarted;
            GameEvents.OnGameOver          -= OnGameOver;
            GameEvents.OnGamePaused        -= OnGamePaused;
            GameEvents.OnGameResumed       -= OnGameResumed;
            GameEvents.OnScoreChanged      -= OnScoreChanged;
            GameEvents.OnComboChanged      -= OnComboChanged;
            GameEvents.OnLivesChanged      -= OnLivesChanged;
            GameEvents.OnTimerTick         -= OnTimerTick;
            GameEvents.OnHighScoreBeaten   -= OnHighScoreBeaten;
        }

        void Start()
        {
            ShowMainMenu();
            _maxTimer = GameManager.Instance != null ? GameManager.Instance.TimerDuration : 180f;
            UpdateMenuHighScore();
            UpdateDifficultyDisplay();
            UpdateDailyStatus();
        }

        // ─── Panel transitions ────────────────────────────────────────────────────

        public void ShowMainMenu()
        {
            SetPanelActive(MainMenuPanel,  true);
            SetPanelActive(HUDPanel,       false);
            SetPanelActive(PausePanel,     false);
            SetPanelActive(GameOverPanel,  false);
            SetPanelActive(StatisticsPanel, false);
            UpdateMenuHighScore();
        }

        public void ShowStatistics()
        {
            SetPanelActive(StatisticsPanel, true);
            if (StatisticsPanel) StartCoroutine(ScaleIn(StatisticsPanel.transform, 0.3f));
        }

        void OnGameStarted()
        {
            _maxTimer = GameManager.Instance != null ? GameManager.Instance.TimerDuration : 180f;
            SetPanelActive(MainMenuPanel,  false);
            SetPanelActive(HUDPanel,       true);
            SetPanelActive(PausePanel,     false);
            SetPanelActive(GameOverPanel,  false);

            // Reset HUD values
            OnScoreChanged(0);
            OnLivesChanged(ScoreSystem.Instance != null ? ScoreSystem.Instance.Lives : 3);
            OnTimerTick(_maxTimer);
            if (ComboText) ComboText.gameObject.SetActive(false);
        }

        void OnGameOver()
        {
            StartCoroutine(ShowGameOverDelayed(0.6f));
        }

        IEnumerator ShowGameOverDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            SetPanelActive(HUDPanel,      false);
            SetPanelActive(GameOverPanel, true);

            var ss = ScoreSystem.Instance;
            if (FinalScoreText)     FinalScoreText.text     = $"{(ss != null ? ss.Score : 0):N0}";
            if (FinalHighScoreText) FinalHighScoreText.text = $"BEST: {(ss != null ? ss.HighScore : 0):N0}";
            if (GameOverTitleText)
            {
                bool lifeOut = ss != null && ss.Lives <= 0;
                GameOverTitleText.text = lifeOut ? "NO MORE LIVES" : "TIME'S UP";
            }

            // Animate panel in
            if (GameOverPanel) StartCoroutine(ScaleIn(GameOverPanel.transform, 0.4f));
        }

        void OnGamePaused()
        {
            SetPanelActive(PausePanel, true);
            if (PausePanel) StartCoroutine(FadeIn(PausePanel, 0.2f));
        }

        void OnGameResumed()
        {
            SetPanelActive(PausePanel, false);
        }

        // ─── HUD updates ──────────────────────────────────────────────────────────

        void OnScoreChanged(int score)
        {
            if (ScoreText) ScoreText.text = score.ToString("N0");
        }

        void OnComboChanged(int combo)
        {
            if (ComboText == null) return;
            if (combo > 1)
            {
                ComboText.gameObject.SetActive(true);
                ComboText.text = $"×{combo} COMBO!";
                StopCoroutine("ComboFade");
                StartCoroutine(ComboPopEffect(ComboText));
            }
            else
            {
                ComboText.gameObject.SetActive(false);
            }
        }

        void OnLivesChanged(int lives)
        {
            if (LivesText)
            {
                string hearts = "";
                for (int i = 0; i < 3; i++) hearts += (i < lives) ? "♥ " : "♡ ";
                LivesText.text = hearts.Trim();
            }
        }

        void OnTimerTick(float secs)
        {
            if (TimerText)
            {
                int m = (int)(secs / 60);
                int s = (int)(secs % 60);
                TimerText.text = $"{m:00}:{s:00}";
                TimerText.color = secs < 30f ? Color.Lerp(Color.red, Color.white, secs / 30f) : Color.white;
            }
            if (TimerSlider)
                TimerSlider.value = Mathf.Clamp01(secs / _maxTimer);

            if (DeckText && GameManager.Instance != null)
                DeckText.text = $"DECK: {GameManager.Instance.DeckRemaining}";
        }

        void OnHighScoreBeaten(int score)
        {
            if (HighScoreText)
            {
                HighScoreText.text = $"BEST: {score:N0}";
                StartCoroutine(FlashText(HighScoreText, new Color(1f, 0.85f, 0.2f), Color.white, 1f));
            }
        }

        void UpdateMenuHighScore()
        {
            if (MenuHighScoreText)
                MenuHighScoreText.text = $"BEST: {PlayerPrefs.GetInt("HighScore", 0):N0}";
            if (HighScoreText)
                HighScoreText.text = $"BEST: {PlayerPrefs.GetInt("HighScore", 0):N0}";
        }

        // ─── Button callbacks (wired by Bootstrap) ────────────────────────────────

        public void OnPlayPressed()   => GameManager.Instance?.StartGame();
        public void OnDailyChallengePressed() => DailyChallengeManager.Instance?.StartDailyChallenge();
        public void OnPausePressed()  => GameManager.Instance?.PauseGame();
        public void OnResumePressed() => GameManager.Instance?.ResumeGame();
        public void OnRestartPressed()
        {
            SetPanelActive(GameOverPanel, false);
            GameManager.Instance?.StartGame();
        }
        public void OnMenuPressed()
        {
            GameManager.Instance?.ReturnToMenu();
            ShowMainMenu();
        }
        public void OnHintPressed()
        {
            GameManager.Instance?.RequestHint();
        }

        public void OnCloseStatsPressed()
        {
            SetPanelActive(StatisticsPanel, false);
        }

        public void OnDifficultySelected(int difficultyIndex)
        {
            if (DifficultyManager.Instance != null)
            {
                var difficulty = (DifficultyManager.DifficultyLevel)difficultyIndex;
                DifficultyManager.Instance.SetDifficulty(difficulty);
                UpdateDifficultyDisplay();
            }
        }

        void UpdateDifficultyDisplay()
        {
            if (DifficultyManager.Instance != null && CurrentDifficultyText != null)
            {
                CurrentDifficultyText.text = DifficultyManager.Instance.GetDifficultyDisplayName();
            }
        }

        void UpdateDailyStatus()
        {
            if (DailyChallengeManager.Instance != null && DailyStatusText != null)
            {
                string status = DailyChallengeManager.Instance.DailyChallengeCompleted ? "✓ DONE" : "NEW";
                DailyStatusText.text = $"DAILY: {status}";
            }
        }

        // ─── Animation helpers ────────────────────────────────────────────────────

        IEnumerator ComboPopEffect(TMP_Text txt)
        {
            Transform t = txt.transform;
            yield return ScaleTo(t, Vector3.one * 1.4f, 0.1f);
            yield return ScaleTo(t, Vector3.one, 0.15f);
        }

        IEnumerator ScaleIn(Transform t, float dur)
        {
            t.localScale = Vector3.zero;
            yield return ScaleTo(t, Vector3.one, dur);
        }

        IEnumerator ScaleTo(Transform t, Vector3 target, float dur)
        {
            Vector3 from = t.localScale;
            float   tt   = 0;
            while (tt < dur)
            {
                float p = 1f - Mathf.Pow(1f - tt / dur, 3f);
                t.localScale = Vector3.Lerp(from, target, p);
                tt += Time.unscaledDeltaTime;
                yield return null;
            }
            t.localScale = target;
        }

        IEnumerator FadeIn(GameObject panel, float dur)
        {
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();
            cg.alpha = 0;
            float t = 0;
            while (t < dur)
            {
                cg.alpha = t / dur;
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            cg.alpha = 1;
        }

        IEnumerator FlashText(TMP_Text txt, Color flash, Color normal, float dur)
        {
            float t = 0;
            while (t < dur)
            {
                txt.color = Color.Lerp(flash, normal, t / dur);
                t += Time.deltaTime;
                yield return null;
            }
            txt.color = normal;
        }

        static void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null) panel.SetActive(active);
        }
    }
}
