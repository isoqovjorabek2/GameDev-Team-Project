using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SetGame.Core;
using SetGame.Visual;
using SetGame.Effects;
using SetGame.Audio;
using SetGame.UI;

namespace SetGame.Bootstrap
{
    /// <summary>
    /// Builds the entire game scene programmatically.
    /// Add this script to any empty GameObject in your scene and press Play.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameBootstrap : MonoBehaviour
    {
        // ─── Color palette ────────────────────────────────────────────────────────
        static readonly Color BG_DARK       = new(0.04f, 0.04f, 0.10f);
        static readonly Color BG_PANEL      = new(0.07f, 0.07f, 0.16f, 0.97f);
        static readonly Color BG_PANEL_DARK = new(0.04f, 0.04f, 0.09f, 0.98f);
        static readonly Color ACCENT        = new(0.67f, 0.33f, 0.97f);
        static readonly Color ACCENT2       = new(0.18f, 0.83f, 0.45f);
        static readonly Color GOLD          = new(1f, 0.85f, 0.2f);
        static readonly Color TEXT_WHITE    = Color.white;
        static readonly Color TEXT_DIM      = new(0.65f, 0.65f, 0.80f);
        static readonly Color RED           = new(1f, 0.27f, 0.34f);
        static readonly Color BTN_PRIMARY   = new(0.67f, 0.33f, 0.97f);
        static readonly Color BTN_SECONDARY = new(0.20f, 0.20f, 0.40f);
        static readonly Color BTN_DANGER    = new(0.85f, 0.20f, 0.25f);

        void Awake()
        {
            SetupCamera();
            var canvas  = CreateCanvas();
            SetupEventSystem(); // Create EventSystem before UI elements
            BuildScene(canvas);
        }

        // ─── Camera ───────────────────────────────────────────────────────────────

        void SetupCamera()
        {
            var cam = Camera.main ?? new GameObject("Main Camera").AddComponent<Camera>();
            cam.backgroundColor   = BG_DARK;
            cam.clearFlags        = CameraClearFlags.SolidColor;
            cam.orthographic      = true;
            cam.orthographicSize  = 5;
            cam.transform.position = new Vector3(0, 0, -10);
            if (!cam.GetComponent<AudioListener>())
                cam.gameObject.AddComponent<AudioListener>();
        }

        // ─── Canvas ───────────────────────────────────────────────────────────────

        Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas");
            var c  = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 0;

            var cs = go.AddComponent<CanvasScaler>();
            cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            cs.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight  = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return c;
        }

        // ─── Scene assembly ───────────────────────────────────────────────────────

        void BuildScene(Canvas canvas)
        {
            var root = canvas.transform;

            // Background gradient quad
            CreateBackground(root);

            // Particle system layer
            CreateParticleLayer(root);

            // Managers (non-visual) - create first
            CreateManagers(root);

            // Board (where cards live)
            var boardArea = CreateBoardArea(root);

            // UI Panels
            var ui = CreateUIManager();
            CreateMainMenuPanel(root, ui);
            CreateHUDPanel(root, ui);
            CreatePausePanel(root, ui);
            CreateGameOverPanel(root, ui);
            CreateStatisticsPanel(root, ui);

            // Screen effects overlay canvas
            CreateEffectsCanvas();
        }

        // ─── Event System ───────────────────────────────────────────────────────────

        void SetupEventSystem()
        {
            // EventSystem - use legacy input for UI simplicity
            if (!FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>())
            {
                new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }
        }

        // ─── Effects canvas ───────────────────────────────────────────────────────

        void CreateEffectsCanvas()
        {
            var go = new GameObject("EffectsCanvas");
            var c  = go.AddComponent<Canvas>();
            c.renderMode   = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 100;  // always on top
            go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            go.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);

            var se = go.AddComponent<ScreenEffects>();
            se.EffectCanvas = c;
        }

        // ─── Background ───────────────────────────────────────────────────────────

        void CreateBackground(Transform parent)
        {
            var go = MakeRect("Background", parent);
            Stretch(go);
            var img = go.AddComponent<Image>();

            // Create a simple gradient texture (dark blue to deep purple)
            var tex = new Texture2D(2, 128, TextureFormat.RGBA32, false);
            for (int y = 0; y < 128; y++)
            {
                float t = y / 127f;
                Color c = Color.Lerp(new Color(0.03f, 0.03f, 0.08f), new Color(0.06f, 0.02f, 0.12f), t);
                tex.SetPixel(0, y, c);
                tex.SetPixel(1, y, c);
            }
            tex.Apply();
            img.sprite = Sprite.Create(tex, new Rect(0, 0, 2, 128), new Vector2(0.5f, 0.5f));
            img.type   = Image.Type.Simple;
            img.preserveAspect = false;
            img.raycastTarget  = false;
        }

        void CreateParticleLayer(Transform parent)
        {
            var go = MakeRect("Particles", parent);
            Stretch(go);
            go.AddComponent<ParticleBackground>();
            // behind everything
            go.transform.SetSiblingIndex(1);
        }

        // ─── Board area ───────────────────────────────────────────────────────────

        RectTransform CreateBoardArea(Transform parent)
        {
            var go = MakeRect("BoardArea", parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.02f, 0.05f);
            rt.anchorMax = new Vector2(0.98f, 0.90f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var board = go.AddComponent<BoardController>();
            board.GridContainer = rt;
            return rt;
        }

        // ─── Managers ─────────────────────────────────────────────────────────────

        void CreateManagers(Transform parent)
        {
            var mgr = new GameObject("Managers");
            mgr.transform.SetParent(parent, false);
            DontDestroyOnLoad(mgr); // Ensure managers persist across scene loads
            mgr.AddComponent<GameManager>();
            mgr.AddComponent<ScoreSystem>();
            mgr.AddComponent<AudioManager>();
            mgr.AddComponent<StatisticsManager>();
            mgr.AddComponent<DifficultyManager>();
            mgr.AddComponent<DailyChallengeManager>();
        }

        UIManager CreateUIManager()
        {
            var go = new GameObject("UIManager");
            DontDestroyOnLoad(go); // Ensure UIManager persists
            return go.AddComponent<UIManager>();
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  MAIN MENU PANEL
        // ─────────────────────────────────────────────────────────────────────────

        void CreateMainMenuPanel(Transform parent, UIManager ui)
        {
            var panel = MakePanel("MainMenuPanel", parent, BG_PANEL_DARK);
            Stretch(panel);

            // Title
            var title = MakeText("Title", panel.transform, "SET", 120, ACCENT);
            var titleRt = title.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.2f, 0.6f);
            titleRt.anchorMax = new Vector2(0.8f, 0.85f);
            titleRt.offsetMin = titleRt.offsetMax = Vector2.zero;
            title.alignment   = TextAlignmentOptions.Center;
            title.fontStyle   = FontStyles.Bold;

            // Subtitle
            var sub = MakeText("Subtitle", panel.transform, "PATTERN RECOGNITION", 28, TEXT_DIM);
            var subRt = sub.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0.2f, 0.54f);
            subRt.anchorMax = new Vector2(0.8f, 0.62f);
            subRt.offsetMin = subRt.offsetMax = Vector2.zero;
            sub.alignment   = TextAlignmentOptions.Center;
            sub.fontStyle   = FontStyles.Bold;
            sub.characterSpacing = 8f;

            // High score
            var hs = MakeText("MenuHighScore", panel.transform, "BEST: 0", 26, GOLD);
            var hsRt = hs.GetComponent<RectTransform>();
            hsRt.anchorMin = new Vector2(0.25f, 0.49f);
            hsRt.anchorMax = new Vector2(0.75f, 0.55f);
            hsRt.offsetMin = hsRt.offsetMax = Vector2.zero;
            hs.alignment   = TextAlignmentOptions.Center;
            ui.MenuHighScoreText = hs;

            // Daily challenge status
            var dailyStatus = MakeText("DailyStatus", panel.transform, "DAILY: NEW", 18, ACCENT2);
            var dailyStatusRt = dailyStatus.GetComponent<RectTransform>();
            dailyStatusRt.anchorMin = new Vector2(0.25f, 0.44f);
            dailyStatusRt.anchorMax = new Vector2(0.75f, 0.49f);
            dailyStatusRt.offsetMin = dailyStatusRt.offsetMax = Vector2.zero;
            dailyStatus.alignment = TextAlignmentOptions.Center;
            ui.DailyStatusText = dailyStatus;

            // Play button
            var playBtn = MakeButton("PlayBtn", panel.transform, "PLAY", BTN_PRIMARY, TEXT_WHITE, 40);
            var playBtnRt = playBtn.GetComponent<RectTransform>();
            playBtnRt.anchorMin = new Vector2(0.35f, 0.37f);
            playBtnRt.anchorMax = new Vector2(0.65f, 0.47f);
            playBtnRt.offsetMin = playBtnRt.offsetMax = Vector2.zero;
            playBtn.onClick.AddListener(ui.OnPlayPressed);

            // Daily challenge button
            var dailyBtn = MakeButton("DailyBtn", panel.transform, "DAILY CHALLENGE", ACCENT2, TEXT_WHITE, 32);
            var dailyBtnRt = dailyBtn.GetComponent<RectTransform>();
            dailyBtnRt.anchorMin = new Vector2(0.35f, 0.28f);
            dailyBtnRt.anchorMax = new Vector2(0.65f, 0.36f);
            dailyBtnRt.offsetMin = dailyBtnRt.offsetMax = Vector2.zero;
            dailyBtn.onClick.AddListener(ui.OnDailyChallengePressed);

            // How-to text
            var how = MakeText("HowTo", panel.transform,
                "Select 3 cards that form a SET.\nEach attribute must be all-same or all-different.",
                20, TEXT_DIM);
            var howRt = how.GetComponent<RectTransform>();
            howRt.anchorMin = new Vector2(0.2f, 0.26f);
            howRt.anchorMax = new Vector2(0.8f, 0.36f);
            howRt.offsetMin = howRt.offsetMax = Vector2.zero;
            how.alignment   = TextAlignmentOptions.Center;

            // Attribute labels row
            CreateAttributePreview(panel.transform);

            // Statistics button
            var statsBtn = MakeButton("StatsBtn", panel.transform, "STATS", BTN_SECONDARY, TEXT_DIM, 24);
            var statsBtnRt = statsBtn.GetComponent<RectTransform>();
            statsBtnRt.anchorMin = new Vector2(0.35f, 0.08f);
            statsBtnRt.anchorMax = new Vector2(0.65f, 0.14f);
            statsBtnRt.offsetMin = statsBtnRt.offsetMax = Vector2.zero;
            statsBtn.onClick.AddListener(() => ShowStatisticsPanel(ui));

            // Difficulty selector
            CreateDifficultySelector(panel.transform, ui);

            ui.MainMenuPanel = panel;
        }

        void CreateAttributePreview(Transform parent)
        {
            string[] labels  = { "SHAPE", "COLOR", "COUNT", "SHADING" };
            string[] values  = { "Diamond Oval Squiggle", "Red Green Purple", "1  2  3", "Solid Striped Open" };
            Color[]  colors2 = { ACCENT, new Color(1f,0.27f,0.34f), ACCENT2, TEXT_DIM };

            for (int i = 0; i < labels.Length; i++)
            {
                float xMin = 0.05f + i * 0.23f;
                float xMax = xMin + 0.21f;

                var lbl = MakeText($"AttrLabel_{i}", parent, labels[i], 16, TEXT_DIM);
                var lRt = lbl.GetComponent<RectTransform>();
                lRt.anchorMin = new Vector2(xMin, 0.18f);
                lRt.anchorMax = new Vector2(xMax, 0.24f);
                lRt.offsetMin = lRt.offsetMax = Vector2.zero;
                lbl.alignment = TextAlignmentOptions.Center;
                lbl.fontStyle = FontStyles.Bold;
                lbl.characterSpacing = 4f;

                var val = MakeText($"AttrVal_{i}", parent, values[i], 17, colors2[i]);
                var vRt = val.GetComponent<RectTransform>();
                vRt.anchorMin = new Vector2(xMin, 0.12f);
                vRt.anchorMax = new Vector2(xMax, 0.18f);
                vRt.offsetMin = vRt.offsetMax = Vector2.zero;
                val.alignment = TextAlignmentOptions.Center;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  HUD PANEL
        // ─────────────────────────────────────────────────────────────────────────

        void CreateHUDPanel(Transform parent, UIManager ui)
        {
            var panel = MakeRect("HUDPanel", parent);
            Stretch(panel);
            panel.gameObject.SetActive(false);

            // Top bar background
            var topBar = MakePanel("TopBar", panel.transform, new Color(0,0,0,0.45f));
            var tbRt   = topBar.GetComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0, 0.91f);
            tbRt.anchorMax = Vector2.one;
            tbRt.offsetMin = tbRt.offsetMax = Vector2.zero;

            // Score
            MakeHUDLabel("SCORE", topBar.transform, new Vector2(0.01f, 0.55f), new Vector2(0.15f, 0.95f), TEXT_DIM, 18);
            var scoreText = MakeText("ScoreValue", topBar.transform, "0", 36, TEXT_WHITE);
            PlaceRt(scoreText.GetComponent<RectTransform>(), 0.01f, 0.05f, 0.15f, 0.58f);
            scoreText.fontStyle = FontStyles.Bold;
            ui.ScoreText = scoreText;

            // High score
            var hsText = MakeText("HighScore", topBar.transform, "BEST: 0", 22, GOLD);
            PlaceRt(hsText.GetComponent<RectTransform>(), 0.01f, 0f, 0.25f, 0.55f);
            ui.HighScoreText = hsText;

            // Lives
            MakeHUDLabel("LIVES", topBar.transform, new Vector2(0.37f, 0.55f), new Vector2(0.57f, 0.95f), TEXT_DIM, 18);
            var livesText = MakeText("LivesValue", topBar.transform, "♥ ♥ ♥", 28, RED);
            PlaceRt(livesText.GetComponent<RectTransform>(), 0.35f, 0.05f, 0.60f, 0.58f);
            livesText.alignment = TextAlignmentOptions.Center;
            ui.LivesText = livesText;

            // Timer
            MakeHUDLabel("TIME", topBar.transform, new Vector2(0.60f, 0.55f), new Vector2(0.75f, 0.95f), TEXT_DIM, 18);
            var timerText = MakeText("TimerValue", topBar.transform, "03:00", 36, TEXT_WHITE);
            PlaceRt(timerText.GetComponent<RectTransform>(), 0.60f, 0.05f, 0.75f, 0.58f);
            timerText.fontStyle = FontStyles.Bold;
            timerText.alignment = TextAlignmentOptions.Center;
            ui.TimerText = timerText;

            // Timer slider
            var sliderGO = new GameObject("TimerSlider", typeof(RectTransform));
            sliderGO.transform.SetParent(topBar.transform, false);
            PlaceRt(sliderGO.GetComponent<RectTransform>(), 0.59f, 0.04f, 0.76f, 0.08f);
            var slider  = sliderGO.AddComponent<Slider>();
            slider.minValue = 0; slider.maxValue = 1; slider.value = 1;
            var fillArea   = MakeRect("FillArea", sliderGO.transform);
            Stretch(fillArea);
            var fill = fillArea.gameObject.AddComponent<Image>();
            fill.color = ACCENT2;
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.interactable = false;
            ui.TimerSlider = slider;

            // Deck count
            var deckText = MakeText("Deck", topBar.transform, "DECK: 81", 20, TEXT_DIM);
            PlaceRt(deckText.GetComponent<RectTransform>(), 0.78f, 0.05f, 0.90f, 0.58f);
            deckText.alignment = TextAlignmentOptions.Center;
            ui.DeckText = deckText;

            // Pause button
            var pauseBtn = MakeButton("PauseBtn", topBar.transform, "❚❚", BTN_SECONDARY, TEXT_WHITE, 24);
            PlaceRt(pauseBtn.GetComponent<RectTransform>(), 0.91f, 0.10f, 0.99f, 0.90f);
            pauseBtn.onClick.AddListener(ui.OnPausePressed);

            // Hint button (bottom right)
            var hintBtn = MakeButton("HintBtn", panel.transform, "HINT", BTN_SECONDARY, TEXT_DIM, 22);
            PlaceRt(hintBtn.GetComponent<RectTransform>(), 0.88f, 0.01f, 0.99f, 0.07f);
            hintBtn.onClick.AddListener(ui.OnHintPressed);

            // Combo display (center, appears/disappears)
            var comboText = MakeText("ComboText", panel.transform, "×2 COMBO!", 42, GOLD);
            PlaceRt(comboText.GetComponent<RectTransform>(), 0.30f, 0.83f, 0.70f, 0.92f);
            comboText.alignment = TextAlignmentOptions.Center;
            comboText.fontStyle = FontStyles.Bold;
            comboText.gameObject.SetActive(false);
            ui.ComboText = comboText;

            ui.HUDPanel = panel.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  PAUSE PANEL
        // ─────────────────────────────────────────────────────────────────────────

        void CreatePausePanel(Transform parent, UIManager ui)
        {
            var overlay = MakePanel("PauseOverlay", parent, new Color(0, 0, 0, 0.7f));
            Stretch(overlay);
            overlay.gameObject.SetActive(false);

            var box = MakePanel("PauseBox", overlay.transform, BG_PANEL);
            PlaceRt(box.GetComponent<RectTransform>(), 0.35f, 0.3f, 0.65f, 0.75f);
            AddRoundedOutline(box.transform, ACCENT, 2f);

            var pauseTitle = MakeText("PauseTitle", box.transform, "PAUSED", 52, ACCENT);
            pauseTitle.alignment = TextAlignmentOptions.Center;
            pauseTitle.fontStyle = FontStyles.Bold;
            PlaceRtRelative(pauseTitle.GetComponent<RectTransform>(), 0.1f, 0.72f, 0.9f, 0.95f);

            var resumeBtn = MakeButton("ResumeBtn", box.transform, "RESUME", BTN_PRIMARY, TEXT_WHITE, 32);
            PlaceRtRelative(resumeBtn.GetComponent<RectTransform>(), 0.15f, 0.50f, 0.85f, 0.66f);
            resumeBtn.onClick.AddListener(ui.OnResumePressed);

            var menuBtn = MakeButton("MenuBtn", box.transform, "MAIN MENU", BTN_SECONDARY, TEXT_DIM, 28);
            PlaceRtRelative(menuBtn.GetComponent<RectTransform>(), 0.15f, 0.32f, 0.85f, 0.48f);
            menuBtn.onClick.AddListener(ui.OnMenuPressed);

            ui.PausePanel = overlay.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  GAME OVER PANEL
        // ─────────────────────────────────────────────────────────────────────────

        void CreateGameOverPanel(Transform parent, UIManager ui)
        {
            var overlay = MakePanel("GameOverOverlay", parent, new Color(0, 0, 0, 0.75f));
            Stretch(overlay);
            overlay.gameObject.SetActive(false);

            var box = MakePanel("GameOverBox", overlay.transform, BG_PANEL);
            PlaceRt(box.GetComponent<RectTransform>(), 0.3f, 0.22f, 0.7f, 0.82f);
            AddRoundedOutline(box.transform, GOLD, 2f);

            var goTitle = MakeText("GameOverTitle", box.transform, "TIME'S UP", 54, RED);
            PlaceRtRelative(goTitle.GetComponent<RectTransform>(), 0.05f, 0.80f, 0.95f, 0.96f);
            goTitle.alignment = TextAlignmentOptions.Center;
            goTitle.fontStyle = FontStyles.Bold;
            ui.GameOverTitleText = goTitle;

            MakeHUDLabelRelative(box.transform, "YOUR SCORE", new Vector2(0.1f, 0.64f), new Vector2(0.9f, 0.74f), TEXT_DIM, 22);

            var finalScore = MakeText("FinalScore", box.transform, "0", 72, TEXT_WHITE);
            PlaceRtRelative(finalScore.GetComponent<RectTransform>(), 0.1f, 0.48f, 0.9f, 0.66f);
            finalScore.alignment = TextAlignmentOptions.Center;
            finalScore.fontStyle = FontStyles.Bold;
            ui.FinalScoreText = finalScore;

            var finalHS = MakeText("FinalHighScore", box.transform, "BEST: 0", 28, GOLD);
            PlaceRtRelative(finalHS.GetComponent<RectTransform>(), 0.15f, 0.38f, 0.85f, 0.50f);
            finalHS.alignment = TextAlignmentOptions.Center;
            ui.FinalHighScoreText = finalHS;

            var restartBtn = MakeButton("RestartBtn", box.transform, "PLAY AGAIN", BTN_PRIMARY, TEXT_WHITE, 34);
            PlaceRtRelative(restartBtn.GetComponent<RectTransform>(), 0.1f, 0.22f, 0.9f, 0.37f);
            restartBtn.onClick.AddListener(ui.OnRestartPressed);

            var menuBtn = MakeButton("MenuBtnGO", box.transform, "MAIN MENU", BTN_SECONDARY, TEXT_DIM, 28);
            PlaceRtRelative(menuBtn.GetComponent<RectTransform>(), 0.1f, 0.06f, 0.9f, 0.21f);
            menuBtn.onClick.AddListener(ui.OnMenuPressed);

            ui.GameOverPanel = overlay.gameObject;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  STATISTICS PANEL
        // ─────────────────────────────────────────────────────────────────────────

        void CreateStatisticsPanel(Transform parent, UIManager ui)
        {
            var overlay = MakePanel("StatsOverlay", parent, new Color(0, 0, 0, 0.8f));
            Stretch(overlay);
            overlay.gameObject.SetActive(false);

            var box = MakePanel("StatsBox", overlay.transform, BG_PANEL);
            PlaceRt(box.GetComponent<RectTransform>(), 0.25f, 0.15f, 0.75f, 0.85f);
            AddRoundedOutline(box.transform, ACCENT, 2f);

            var statsTitle = MakeText("StatsTitle", box.transform, "STATISTICS", 48, ACCENT);
            statsTitle.alignment = TextAlignmentOptions.Center;
            statsTitle.fontStyle = FontStyles.Bold;
            PlaceRtRelative(statsTitle.GetComponent<RectTransform>(), 0.1f, 0.88f, 0.9f, 0.98f);

            var statsText = MakeText("StatsContent", box.transform, "Loading...", 22, TEXT_WHITE);
            statsText.alignment = TextAlignmentOptions.Center;
            PlaceRtRelative(statsText.GetComponent<RectTransform>(), 0.1f, 0.25f, 0.9f, 0.80f);
            ui.StatisticsText = statsText;

            var closeBtn = MakeButton("CloseStatsBtn", box.transform, "CLOSE", BTN_PRIMARY, TEXT_WHITE, 28);
            PlaceRtRelative(closeBtn.GetComponent<RectTransform>(), 0.25f, 0.08f, 0.75f, 0.20f);
            closeBtn.onClick.AddListener(ui.OnCloseStatsPressed);

            ui.StatisticsPanel = overlay.gameObject;
        }

        void ShowStatisticsPanel(UIManager ui)
        {
            if (StatisticsManager.Instance != null && ui.StatisticsText != null)
            {
                ui.StatisticsText.text = StatisticsManager.Instance.GetStatisticsDisplay();
            }
            ui.ShowStatistics();
        }

        void CreateDifficultySelector(Transform parent, UIManager ui)
        {
            // Difficulty label
            var diffLabel = MakeText("DiffLabel", parent, "DIFFICULTY", 18, TEXT_DIM);
            var diffLabelRt = diffLabel.GetComponent<RectTransform>();
            diffLabelRt.anchorMin = new Vector2(0.15f, 0.24f);
            diffLabelRt.anchorMax = new Vector2(0.35f, 0.30f);
            diffLabelRt.offsetMin = diffLabelRt.offsetMax = Vector2.zero;
            diffLabel.alignment = TextAlignmentOptions.Center;
            diffLabel.fontStyle = FontStyles.Bold;

            // Difficulty buttons container
            var diffContainer = MakeRect("DiffContainer", parent);
            var diffContainerRt = diffContainer.GetComponent<RectTransform>();
            diffContainerRt.anchorMin = new Vector2(0.15f, 0.16f);
            diffContainerRt.anchorMax = new Vector2(0.85f, 0.24f);
            diffContainerRt.offsetMin = diffContainerRt.offsetMax = Vector2.zero;

            // Create difficulty buttons
            string[] difficulties = { "BEGINNER", "EASY", "NORMAL", "HARD", "EXPERT", "MASTER" };
            float buttonWidth = 1f / difficulties.Length;

            for (int i = 0; i < difficulties.Length; i++)
            {
                var btn = MakeButton($"DiffBtn_{i}", diffContainer.transform, difficulties[i],
                    i == 2 ? BTN_PRIMARY : BTN_SECONDARY, i == 2 ? TEXT_WHITE : TEXT_DIM, 16);
                var btnRt = btn.GetComponent<RectTransform>();
                btnRt.anchorMin = new Vector2(i * buttonWidth, 0f);
                btnRt.anchorMax = new Vector2((i + 1) * buttonWidth, 1f);
                btnRt.offsetMin = btnRt.offsetMax = Vector2.zero;

                int difficultyIndex = i;
                btn.onClick.AddListener(() => ui.OnDifficultySelected(difficultyIndex));
            }

            // Current difficulty display
            var currentDiffText = MakeText("CurrentDiffText", parent, "NORMAL", 20, ACCENT);
            var currentDiffRt = currentDiffText.GetComponent<RectTransform>();
            currentDiffRt.anchorMin = new Vector2(0.40f, 0.24f);
            currentDiffRt.anchorMax = new Vector2(0.60f, 0.30f);
            currentDiffRt.offsetMin = currentDiffRt.offsetMax = Vector2.zero;
            currentDiffText.alignment = TextAlignmentOptions.Center;
            currentDiffText.fontStyle = FontStyles.Bold;
            ui.CurrentDifficultyText = currentDiffText;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  UI CREATION HELPERS
        // ─────────────────────────────────────────────────────────────────────────

        static GameObject MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static GameObject MakePanel(string name, Transform parent, Color color)
        {
            var go = MakeRect(name, parent);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = true;
            return go;
        }

        static TMP_Text MakeText(string name, Transform parent, string text, int size, Color color)
        {
            var go = MakeRect(name, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = size;
            tmp.color     = color;
            tmp.raycastTarget = false;
            tmp.enableWordWrapping = true;
            return tmp;
        }

        static Button MakeButton(string name, Transform parent, string label,
                                  Color bgColor, Color textColor, int fontSize)
        {
            var go  = MakePanel(name, parent, bgColor);
            var btn = go.AddComponent<Button>();

            var colors = ColorBlock.defaultColorBlock;
            colors.normalColor      = bgColor;
            colors.highlightedColor = bgColor * 1.25f;
            colors.pressedColor     = bgColor * 0.75f;
            colors.fadeDuration     = 0.1f;
            btn.colors = colors;

            var txt = MakeText(name + "Label", go.transform, label, fontSize, textColor);
            Stretch(txt.GetComponent<RectTransform>());
            txt.alignment = TextAlignmentOptions.Center;
            txt.fontStyle = FontStyles.Bold;
            txt.raycastTarget = false;

            return btn;
        }

        static TMP_Text MakeHUDLabel(string name, Transform parent, Vector2 min, Vector2 max,
                                      Color color, int size)
        {
            var t = MakeText(name, parent, name.Replace("Label_",""), size, color);
            PlaceRt(t.GetComponent<RectTransform>(), min.x, min.y, max.x, max.y);
            t.alignment    = TextAlignmentOptions.Left;
            t.fontStyle    = FontStyles.Bold;
            t.characterSpacing = 3f;
            return t;
        }

        static TMP_Text MakeHUDLabelRelative(Transform parent, string text,
                                              Vector2 min, Vector2 max, Color color, int size)
        {
            var t = MakeText("Label_"+text, parent, text, size, color);
            PlaceRtRelative(t.GetComponent<RectTransform>(), min.x, min.y, max.x, max.y);
            t.alignment    = TextAlignmentOptions.Center;
            t.fontStyle    = FontStyles.Bold;
            t.characterSpacing = 3f;
            return t;
        }

        static void AddRoundedOutline(Transform parent, Color color, float _ = 2f)
        {
            var go  = MakeRect("Outline", parent);
            var rt  = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-2, -2); rt.offsetMax = new Vector2(2, 2);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            // Move behind
            go.transform.SetAsFirstSibling();
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static void Stretch(GameObject go) => Stretch(go.GetComponent<RectTransform>());

        static void PlaceRt(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
        {
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static void PlaceRtRelative(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
        {
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
