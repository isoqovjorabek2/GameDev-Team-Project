using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // ── Panel references ────────────────────────────────────────────────
    private GameObject _menuPanel;
    private GameObject _hudPanel;
    private GameObject _gameOverPanel;
    private GameObject _victoryPanel;

    // ── HUD refs ────────────────────────────────────────────────────────
    private TextMeshProUGUI[] _hearts;     // 3 heart labels
    private TextMeshProUGUI   _scoreVal;
    private TextMeshProUGUI   _timerVal;
    private TextMeshProUGUI   _comboPopup;
    private Coroutine         _comboRoutine;

    // ── Overlay refs ────────────────────────────────────────────────────
    private TextMeshProUGUI _goScore;
    private TextMeshProUGUI _winScore;
    private TextMeshProUGUI _winTime;
    private TextMeshProUGUI _winBest;

    private void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
    }

    public void Initialize(Canvas canvas)
    {
        BuildAll(canvas);
        GameManager.Instance.OnStateChanged += HandleState;
        GameManager.Instance.OnStatsUpdated  += RefreshHUD;
        ShowOnly(_menuPanel);
    }

    // ── Public API ──────────────────────────────────────────────────────
    public void ShowComboPopup(int combo)
    {
        if (_comboRoutine != null) StopCoroutine(_comboRoutine);
        if (combo >= 2)
        {
            _comboPopup.text  = $"COMBO  x{combo}!";
            _comboPopup.color = new Color(1f, 0.84f, 0.10f, 1f);
            _comboRoutine     = StartCoroutine(FadeCombo(1.6f));
        }
        else
        {
            _comboPopup.text = "";
        }
    }

    // ── State & HUD ─────────────────────────────────────────────────────
    private void HandleState(GameManager.State state)
    {
        switch (state)
        {
            case GameManager.State.Menu:
                ShowOnly(_menuPanel);
                break;

            case GameManager.State.Playing:
                ShowOnly(_hudPanel);
                RefreshHUD();
                break;

            case GameManager.State.GameOver:
                SoundManager.Instance.PlayGameOver();
                _goScore.text = $"Final Score:  {GameManager.Instance.Score}";
                ShowOnly(_gameOverPanel);
                break;

            case GameManager.State.Victory:
                SoundManager.Instance.PlayVictory();
                var gm = GameManager.Instance;
                _winScore.text = $"Score  {gm.Score}";
                _winTime.text  = $"Time  {FormatTime(gm.Timer)}";
                _winBest.text  = $"Best  {gm.BestScore}";
                ShowOnly(_victoryPanel);
                _victoryPanel.GetComponentInChildren<ConfettiSystem>()?.Burst();
                break;
        }
    }

    private void RefreshHUD()
    {
        var gm = GameManager.Instance;
        if (!_hudPanel.activeSelf) return;

        _scoreVal.text = gm.Score.ToString("N0");
        _timerVal.text = FormatTime(gm.Timer);

        for (int i = 0; i < 3; i++)
            _hearts[i].color = i < gm.Lives
                ? new Color(0.90f, 0.22f, 0.35f)
                : new Color(0.28f, 0.22f, 0.35f);
    }

    private static string FormatTime(float t)
    {
        int m = (int)t / 60;
        int s = (int)t % 60;
        return $"{m}:{s:D2}";
    }

    private IEnumerator FadeCombo(float dur)
    {
        yield return new WaitForSeconds(dur * 0.55f);
        float t = 0f, fade = dur * 0.45f;
        var   c = _comboPopup.color;
        while (t < fade)
        {
            _comboPopup.color = new Color(c.r, c.g, c.b, 1f - t / fade);
            t += Time.deltaTime;
            yield return null;
        }
        _comboPopup.text = "";
    }

    // ── Panel Visibility ────────────────────────────────────────────────
    private void ShowOnly(GameObject target)
    {
        _menuPanel.SetActive(_menuPanel    == target);
        _hudPanel.SetActive(_hudPanel      == target);
        _gameOverPanel.SetActive(_gameOverPanel == target);
        _victoryPanel.SetActive(_victoryPanel  == target);
    }

    // ── UI Construction ─────────────────────────────────────────────────
    private void BuildAll(Canvas canvas)
    {
        Transform root = canvas.transform;

        // Animated star background (sibling 0)
        var bg = Panel(root, "BG", new Color(0.07f, 0.04f, 0.15f));
        FullStretch(bg.GetComponent<RectTransform>());
        bg.AddComponent<AnimatedBackground>();
        bg.transform.SetSiblingIndex(0);

        // Combo popup overlay (always on top, canvas child)
        var comboGo = new GameObject("ComboOverlay");
        comboGo.transform.SetParent(root, false);
        FullStretch(comboGo.AddComponent<RectTransform>());
        _comboPopup = Txt(comboGo.transform, "ComboTxt", "", 48f,
                          new Color(1f, 0.84f, 0.1f), FontStyles.Bold);
        PlaceCenter(_comboPopup.rectTransform, new Vector2(0, 80), new Vector2(700, 80));

        _menuPanel    = BuildMenu(root);
        _hudPanel     = BuildHUD(root);
        _gameOverPanel = BuildGameOver(root);
        _victoryPanel = BuildVictory(root);
    }

    // ── MAIN MENU ───────────────────────────────────────────────────────
    private GameObject BuildMenu(Transform root)
    {
        var p = FullPanel(root, "MenuPanel", Color.clear);

        // Title shadow
        var shadow = Txt(p.transform, "TitleShadow", "MEMORY\nMATCH",
            96f, new Color(0f, 0f, 0f, 0.4f), FontStyles.Bold);
        PlaceCenter(shadow.rectTransform, new Vector2(4f, 156f), new Vector2(740f, 250f));

        // Title
        var title = Txt(p.transform, "Title", "MEMORY\nMATCH",
            96f, new Color(1f, 0.85f, 0.15f), FontStyles.Bold);
        PlaceCenter(title.rectTransform, new Vector2(0f, 160f), new Vector2(740f, 250f));

        // Subtitle
        var sub = Txt(p.transform, "Sub", "Find all the matching pairs!",
            28f, new Color(0.72f, 0.82f, 1f));
        PlaceCenter(sub.rectTransform, new Vector2(0f, 60f), new Vector2(640f, 46f));

        // Difficulty label
        var dLabel = Txt(p.transform, "DLabel", "─── DIFFICULTY ───",
            20f, new Color(0.50f, 0.60f, 0.82f));
        PlaceCenter(dLabel.rectTransform, new Vector2(0f, -20f), new Vector2(480f, 34f));

        // Difficulty row
        var row = Container(p.transform, "DiffRow", new Vector2(0f, -82f), new Vector2(580f, 64f));
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 18f; hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false; hlg.childControlHeight = false;

        var diffs = new (string lbl, GameManager.Difficulty diff, Color col)[]
        {
            ("EASY",   GameManager.Difficulty.Easy,   new Color(0.18f, 0.74f, 0.36f)),
            ("MEDIUM", GameManager.Difficulty.Medium, new Color(0.20f, 0.50f, 0.92f)),
            ("HARD",   GameManager.Difficulty.Hard,   new Color(0.86f, 0.22f, 0.28f)),
        };

        foreach (var (lbl, diff, col) in diffs)
        {
            var btn = Btn(row.transform, lbl, col, () =>
            {
                GameManager.Instance.SetDifficulty(diff);
                SoundManager.Instance.PlayClick();
                UpdateDiffButtons(row, lbl);
            });
            btn.name = lbl;
            btn.GetComponent<RectTransform>().sizeDelta = new Vector2(166f, 58f);
        }

        // Play button
        var play = Btn(p.transform, "PLAY!", new Color(0.96f, 0.66f, 0.10f), () =>
        {
            SoundManager.Instance.PlayClick();
            GameManager.Instance.StartGame();
        });
        PlaceCenter(play.GetComponent<RectTransform>(), new Vector2(0f, -178f), new Vector2(228f, 74f));
        var playTmp = play.GetComponentInChildren<TextMeshProUGUI>();
        playTmp.fontSize  = 38f;
        playTmp.fontStyle = FontStyles.Bold;

        // Highlight Medium by default after one frame
        StartCoroutine(DefaultHighlight(row));
        return p;
    }

    private IEnumerator DefaultHighlight(GameObject row)
    {
        yield return null;
        UpdateDiffButtons(row, "MEDIUM");
    }

    private static void UpdateDiffButtons(GameObject row, string selected)
    {
        foreach (Transform child in row.transform)
        {
            var img = child.GetComponent<Image>();
            if (img == null) continue;
            Color c = img.color;
            img.color = child.name == selected
                ? new Color(c.r, c.g, c.b, 1.0f)
                : new Color(c.r * 0.4f, c.g * 0.4f, c.b * 0.4f, 0.60f);
        }
    }

    // ── HUD ─────────────────────────────────────────────────────────────
    private GameObject BuildHUD(Transform root)
    {
        var p  = Panel(root, "HUD", new Color(0.04f, 0.03f, 0.12f, 0.85f));
        var rt = p.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = new Vector2(0f, -80f);
        rt.offsetMax = Vector2.zero;

        // Hearts
        _hearts = new TextMeshProUGUI[3];
        for (int i = 0; i < 3; i++)
        {
            var h = Txt(p.transform, $"H{i}", "♥", 36f, new Color(0.90f, 0.22f, 0.35f), FontStyles.Bold);
            PlaceAtAnchor(h.rectTransform, new Vector2(0.5f, 0.5f),
                new Vector2(-490f + i * 56f, -40f), new Vector2(52f, 52f));
            _hearts[i] = h;
        }

        // Score
        var sLbl = Txt(p.transform, "SLbl", "SCORE", 16f, new Color(0.55f, 0.65f, 0.88f));
        PlaceAtAnchor(sLbl.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -18f), new Vector2(160f, 24f));
        _scoreVal = Txt(p.transform, "SVal", "0", 32f, Color.white, FontStyles.Bold);
        PlaceAtAnchor(_scoreVal.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -54f), new Vector2(240f, 38f));

        // Timer
        var tLbl = Txt(p.transform, "TLbl", "TIME", 16f, new Color(0.55f, 0.65f, 0.88f));
        PlaceAtAnchor(tLbl.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(310f, -18f), new Vector2(120f, 24f));
        _timerVal = Txt(p.transform, "TVal", "0:00", 32f, new Color(0.80f, 0.90f, 1f), FontStyles.Bold);
        PlaceAtAnchor(_timerVal.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(310f, -54f), new Vector2(170f, 38f));

        return p;
    }

    // ── GAME OVER ───────────────────────────────────────────────────────
    private GameObject BuildGameOver(Transform root)
    {
        var p = FullPanel(root, "GOPanel", new Color(0f, 0f, 0f, 0.80f));

        var card = Panel(p.transform, "Card", new Color(0.12f, 0.06f, 0.22f));
        PlaceCenter(card.GetComponent<RectTransform>(), Vector2.zero, new Vector2(520f, 390f));

        var title = Txt(card.transform, "Title", "GAME\nOVER",
            74f, new Color(0.94f, 0.27f, 0.35f), FontStyles.Bold);
        PlaceCenter(title.rectTransform, new Vector2(0f, 90f), new Vector2(460f, 195f));

        _goScore = Txt(card.transform, "Score", "Final Score: 0", 28f, Color.white);
        PlaceCenter(_goScore.rectTransform, new Vector2(0f, -12f), new Vector2(440f, 44f));

        var retry = Btn(card.transform, "PLAY AGAIN", new Color(0.20f, 0.50f, 0.92f), () =>
        {
            SoundManager.Instance.PlayClick();
            GameManager.Instance.StartGame();
        });
        PlaceCenter(retry.GetComponent<RectTransform>(), new Vector2(-115f, -115f), new Vector2(202f, 58f));

        var menu = Btn(card.transform, "MENU", new Color(0.32f, 0.26f, 0.44f), () =>
        {
            SoundManager.Instance.PlayClick();
            GameManager.Instance.ReturnToMenu();
        });
        PlaceCenter(menu.GetComponent<RectTransform>(), new Vector2(115f, -115f), new Vector2(162f, 58f));

        return p;
    }

    // ── VICTORY ─────────────────────────────────────────────────────────
    private GameObject BuildVictory(Transform root)
    {
        var p = FullPanel(root, "VictoryPanel", new Color(0f, 0f, 0f, 0.74f));

        // Confetti lives here so it appears behind the card
        var confettiGo = new GameObject("ConfettiHost");
        confettiGo.transform.SetParent(p.transform, false);
        FullStretch(confettiGo.AddComponent<RectTransform>());
        confettiGo.AddComponent<ConfettiSystem>();

        var card = Panel(p.transform, "Card", new Color(0.07f, 0.12f, 0.22f));
        PlaceCenter(card.GetComponent<RectTransform>(), Vector2.zero, new Vector2(560f, 430f));

        // Glow title
        var glow = Txt(card.transform, "TitleGlow", "YOU WIN!",
            76f, new Color(1f, 0.90f, 0.10f, 0.35f), FontStyles.Bold);
        PlaceCenter(glow.rectTransform, new Vector2(3f, 113f), new Vector2(510f, 104f));

        var title = Txt(card.transform, "Title", "YOU WIN!",
            76f, new Color(0.98f, 0.82f, 0.10f), FontStyles.Bold);
        PlaceCenter(title.rectTransform, new Vector2(0f, 115f), new Vector2(510f, 104f));

        _winScore = Txt(card.transform, "WScore", "Score  0", 30f, Color.white, FontStyles.Bold);
        PlaceCenter(_winScore.rectTransform, new Vector2(0f, 36f), new Vector2(460f, 42f));

        _winTime = Txt(card.transform, "WTime", "Time  0:00", 26f, new Color(0.75f, 0.87f, 1f));
        PlaceCenter(_winTime.rectTransform, new Vector2(0f, -12f), new Vector2(460f, 38f));

        _winBest = Txt(card.transform, "WBest", "Best  0", 23f, new Color(1f, 0.84f, 0.10f));
        PlaceCenter(_winBest.rectTransform, new Vector2(0f, -55f), new Vector2(460f, 34f));

        var retry = Btn(card.transform, "PLAY AGAIN", new Color(0.98f, 0.64f, 0.10f), () =>
        {
            SoundManager.Instance.PlayClick();
            GameManager.Instance.StartGame();
        });
        PlaceCenter(retry.GetComponent<RectTransform>(), new Vector2(-118f, -128f), new Vector2(210f, 58f));

        var menu = Btn(card.transform, "MENU", new Color(0.22f, 0.42f, 0.72f), () =>
        {
            SoundManager.Instance.PlayClick();
            GameManager.Instance.ReturnToMenu();
        });
        PlaceCenter(menu.GetComponent<RectTransform>(), new Vector2(118f, -128f), new Vector2(170f, 58f));

        return p;
    }

    // ── UI Helpers ──────────────────────────────────────────────────────
    private static GameObject FullPanel(Transform parent, string name, Color color)
    {
        var go = Panel(parent, name, color);
        FullStretch(go.GetComponent<RectTransform>());
        return go;
    }

    private static GameObject Panel(Transform parent, string name, Color color)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var img  = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    public static TextMeshProUGUI Txt(Transform parent, string name, string text,
        float size, Color color, FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp               = go.AddComponent<TextMeshProUGUI>();
        tmp.text              = text;
        tmp.fontSize          = size;
        tmp.color             = color;
        tmp.fontStyle         = style;
        tmp.alignment         = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    public static GameObject Btn(Transform parent, string label, Color color, UnityAction onClick)
    {
        var go = new GameObject(label + "_Btn");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();

        var img   = go.AddComponent<Image>();
        img.color = color;

        var btn   = go.AddComponent<Button>();
        var cols  = btn.colors;
        cols.normalColor      = color;
        cols.highlightedColor = new Color(
            Mathf.Min(color.r + 0.18f, 1f),
            Mathf.Min(color.g + 0.18f, 1f),
            Mathf.Min(color.b + 0.18f, 1f));
        cols.pressedColor = new Color(color.r * 0.65f, color.g * 0.65f, color.b * 0.65f);
        btn.colors        = cols;
        btn.onClick.AddListener(onClick);

        var txtGo = new GameObject("Txt");
        txtGo.transform.SetParent(go.transform, false);
        FullStretch(txtGo.AddComponent<RectTransform>());
        var tmp               = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text              = label;
        tmp.fontSize          = 23f;
        tmp.fontStyle         = FontStyles.Bold;
        tmp.color             = Color.white;
        tmp.alignment         = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;

        return go;
    }

    private static GameObject Container(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt              = go.AddComponent<RectTransform>();
        rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        return go;
    }

    private static void PlaceCenter(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
    }

    private static void PlaceAtAnchor(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
    }

    private static void FullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
