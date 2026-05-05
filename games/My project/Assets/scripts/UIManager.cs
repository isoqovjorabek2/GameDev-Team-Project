using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds and drives the entire Canvas UI at runtime — no prefabs required.
/// Add this component to any scene GameObject. It creates a Screen-Space-Overlay
/// Canvas and manages three panels: Lobby, HUD, and Game-Over.
///
/// Setup: Window → TextMeshPro → Import TMP Essential Resources (first time only).
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // ── Colour palette ────────────────────────────────────────────────────────
    static readonly Color C_BG      = new(0.06f, 0.07f, 0.14f, 0.97f);
    static readonly Color C_CARD    = new(0.10f, 0.11f, 0.22f, 1.00f);
    static readonly Color C_STRIP   = new(0.05f, 0.05f, 0.12f, 0.88f);
    static readonly Color C_ACCENT  = new(0.00f, 0.82f, 1.00f, 1.00f);
    static readonly Color C_CORRECT = new(0.13f, 0.90f, 0.47f, 1.00f);
    static readonly Color C_WRONG   = new(1.00f, 0.27f, 0.27f, 1.00f);
    static readonly Color C_BOOST   = new(1.00f, 0.82f, 0.00f, 1.00f);
    static readonly Color C_P1      = new(0.25f, 0.62f, 1.00f, 1.00f);
    static readonly Color C_P2      = new(1.00f, 0.42f, 0.18f, 1.00f);
    static readonly Color C_DIM     = new(0.45f, 0.45f, 0.55f, 1.00f);

    // ── Canvas ────────────────────────────────────────────────────────────────
    Canvas    _canvas;

    // ── Lobby ─────────────────────────────────────────────────────────────────
    GameObject       _lobbyPanel;
    Button           _hostBtn, _joinBtn, _svrBtn;
    TextMeshProUGUI  _statusText;

    // ── HUD ───────────────────────────────────────────────────────────────────
    GameObject       _hudPanel;
    TextMeshProUGUI  _p1Score, _p1Tag;
    TextMeshProUGUI  _p2Score, _p2Tag;
    Image            _p1BoostDot, _p2BoostDot;
    RectTransform    _progressBg, _p1Dot, _p2Dot;

    // ── Game Over ─────────────────────────────────────────────────────────────
    GameObject       _gameOverPanel;
    TextMeshProUGUI  _gameOverLabel, _winnerLabel;

    // ── Answer flash ──────────────────────────────────────────────────────────
    Image  _flashOverlay;
    float  _flashTimer;
    const float FLASH_DUR = 0.4f;

    // ── Runtime state ─────────────────────────────────────────────────────────
    character   _localChar;
    character   _remoteChar;
    bool        _gameStarted;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        BuildCanvas();
        BuildLobbyPanel();
        BuildHUDPanel();
        BuildGameOverPanel();
        BuildFlashOverlay();

        ShowLobby();
    }

    void Start()
    {
        GameNetworkManager.OnGameStarted += HandleGameStarted;
    }

    void OnDestroy()
    {
        GameNetworkManager.OnGameStarted -= HandleGameStarted;
    }

    void Update()
    {
        TickLobbyState();
        if (_hudPanel.activeSelf) TickHUD();
        TickFlash();
    }

    // ── Panel switching ───────────────────────────────────────────────────────
    public void ShowLobby()
    {
        _lobbyPanel.SetActive(true);
        _hudPanel.SetActive(false);
        _gameOverPanel.SetActive(false);
    }

    public void ShowHUD()
    {
        _lobbyPanel.SetActive(false);
        _hudPanel.SetActive(true);
        _gameOverPanel.SetActive(false);
        _localChar = null;
        _remoteChar = null;
    }

    public void ShowGameOver(string winnerText)
    {
        _gameOverPanel.SetActive(true);
        _winnerLabel.text = winnerText;
        StartCoroutine(AnimateGameOver());
    }

    // ── Lobby polling ─────────────────────────────────────────────────────────
    void TickLobbyState()
    {
        if (!_lobbyPanel.activeSelf) return;
        var nm = NetworkManager.Singleton;
        if (nm == null) return;

        bool connected = nm.IsClient || nm.IsServer;
        _hostBtn.gameObject.SetActive(!connected);
        _joinBtn.gameObject.SetActive(!connected);
        _svrBtn.gameObject.SetActive(!connected);

        if (!connected)
            _statusText.text = "";
        else if (nm.IsServer && !_gameStarted)
            _statusText.text = "Waiting for players…";
        else if (!nm.IsServer && !_gameStarted)
            _statusText.text = "Connecting…";
    }

    // ── HUD update ────────────────────────────────────────────────────────────
    void TickHUD()
    {
        if (_localChar == null || _remoteChar == null)
            FindCharacters();
        if (_localChar == null) return;

        // Scores
        _p1Score.text = _localChar.CorrectAnswers.ToString();
        _p2Score.text = _remoteChar != null ? _remoteChar.CorrectAnswers.ToString() : "—";

        // Boost glow
        bool p1Boost = _localChar.IsBoosting;
        bool p2Boost = _remoteChar != null && _remoteChar.IsBoosting;
        _p1BoostDot.color = p1Boost ? C_BOOST : C_DIM;
        _p2BoostDot.color = p2Boost ? C_BOOST : C_DIM;
        _p1Tag.color = p1Boost ? C_BOOST : Color.white;
        _p2Tag.color = p2Boost ? C_BOOST : Color.white;
        _p1Tag.text  = p1Boost ? "BOOST!" : "YOU";
        _p2Tag.text  = p2Boost ? "BOOST!" : "OPP";

        // Race bar markers
        if (_remoteChar != null)
        {
            float d1   = _localChar.ForwardDistance;
            float d2   = _remoteChar.ForwardDistance;
            float maxD = Mathf.Max(d1, d2, 5f);
            float bw   = _progressBg.rect.width;
            _p1Dot.anchoredPosition = new Vector2(d1 / maxD * bw, 0f);
            _p2Dot.anchoredPosition = new Vector2(d2 / maxD * bw, 0f);
        }
    }

    void FindCharacters()
    {
        var all = FindObjectsByType<character>(FindObjectsSortMode.None);
        foreach (var c in all)
        {
            if (c.IsOwner) _localChar  = c;
            else           _remoteChar = c;
        }
        if (_localChar != null)
            _localChar.OnAnswerGiven += FlashAnswer;
    }

    // ── Answer flash ──────────────────────────────────────────────────────────
    public void FlashAnswer(bool correct)
    {
        Color col = correct ? C_CORRECT : C_WRONG;
        col.a = 0.30f;
        _flashOverlay.color = col;
        _flashOverlay.gameObject.SetActive(true);
        _flashTimer = FLASH_DUR;

        AudioManager.Instance?.Play(correct ? AudioManager.SFX.Correct : AudioManager.SFX.Wrong);
    }

    void TickFlash()
    {
        if (_flashTimer <= 0f) return;
        _flashTimer -= Time.unscaledDeltaTime;
        if (_flashTimer <= 0f)
        {
            _flashOverlay.gameObject.SetActive(false);
            return;
        }
        Color c = _flashOverlay.color;
        c.a = (_flashTimer / FLASH_DUR) * 0.30f;
        _flashOverlay.color = c;
    }

    // ── Game over animation ───────────────────────────────────────────────────
    IEnumerator AnimateGameOver()
    {
        _gameOverLabel.transform.localScale = Vector3.one * 1.8f;
        _gameOverLabel.color = new Color(C_WRONG.r, C_WRONG.g, C_WRONG.b, 0f);
        float t = 0f;
        while (t < 0.45f)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / 0.45f);
            _gameOverLabel.transform.localScale = Vector3.one * Mathf.Lerp(1.8f, 1f, p);
            Color c = _gameOverLabel.color; c.a = p; _gameOverLabel.color = c;
            yield return null;
        }
        _gameOverLabel.transform.localScale = Vector3.one;
    }

    // ── Event handlers ────────────────────────────────────────────────────────
    void HandleGameStarted()
    {
        _gameStarted = true;
        ShowHUD();
        AudioManager.Instance?.Play(AudioManager.SFX.GameStart);
    }

    // =========================================================================
    // Canvas & panel construction
    // =========================================================================

    void BuildCanvas()
    {
        var go = new GameObject("UICanvas");
        go.transform.SetParent(transform, false);
        _canvas = go.AddComponent<Canvas>();
        _canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        go.AddComponent<GraphicRaycaster>();
    }

    // ── Lobby ─────────────────────────────────────────────────────────────────
    void BuildLobbyPanel()
    {
        _lobbyPanel = FullPanel("Lobby", C_BG);

        // Central card
        var card = Child("Card", _lobbyPanel);
        SetRect(card, 0.5f, 0.5f, 0.5f, 0.5f, 0, 0, 460, 520);
        Img(card, C_CARD);
        RoundedOutline(card, C_ACCENT);

        // Title
        var title = TMP("Title", card, "MATH\nRUNNER", 64, FontStyles.Bold, C_ACCENT);
        SetRect(title.gameObject, 0.5f, 1f, 0.5f, 1f, 0, -130, 400, 130);
        title.alignment = TextAlignmentOptions.Center;
        title.lineSpacing = -18f;

        // Separator
        var sep = Child("Sep", card);
        SetRect(sep, 0.5f, 1f, 0.5f, 1f, 0, -148, 380, 2);
        Img(sep, C_ACCENT * 0.6f);

        // Buttons
        float btnY = -178;
        _hostBtn = MenuBtn("HostBtn", card, "HOST GAME",  new Vector2(0, btnY),        () => NetworkManager.Singleton?.StartHost());
        _joinBtn = MenuBtn("JoinBtn", card, "JOIN GAME",  new Vector2(0, btnY - 74),   () => NetworkManager.Singleton?.StartClient());
        _svrBtn  = MenuBtn("SvrBtn",  card, "SERVER",     new Vector2(0, btnY - 148),  () => NetworkManager.Singleton?.StartServer());

        // Status
        _statusText = TMP("Status", card, "", 20, FontStyles.Normal, C_DIM);
        SetRect(_statusText.gameObject, 0.5f, 0f, 0.5f, 0f, 0, 28, 400, 30);
        _statusText.alignment = TextAlignmentOptions.Center;
    }

    // ── HUD ───────────────────────────────────────────────────────────────────
    void BuildHUDPanel()
    {
        _hudPanel = FullPanel("HUD", Color.clear);
        _hudPanel.SetActive(false);

        // Top dark strip
        var strip = Child("Strip", _hudPanel);
        var srt   = strip.GetComponent<RectTransform>();
        srt.anchorMin  = new Vector2(0, 1);
        srt.anchorMax  = new Vector2(1, 1);
        srt.offsetMin  = new Vector2(0, -90);
        srt.offsetMax  = Vector2.zero;
        Img(strip, C_STRIP);

        // ── Player 1 (left) ──────────────────────────────────────────────────
        var p1Root = Child("P1", strip);
        var p1rt   = p1Root.GetComponent<RectTransform>();
        p1rt.anchorMin = new Vector2(0, 0);
        p1rt.anchorMax = new Vector2(0, 1);
        p1rt.offsetMin = new Vector2(18, 6);
        p1rt.offsetMax = new Vector2(220, -6);

        _p1BoostDot = BoostDot("P1Dot", p1Root, C_P1);

        var p1Label = TMP("P1Lbl", p1Root, "YOU", 15, FontStyles.Bold, C_P1);
        SetAnchoredRect(p1Label.gameObject, 0, 0.5f, 0, 1f, 30, 0, 60, 0);

        _p1Tag  = TMP("P1Tag", p1Root, "YOU", 14, FontStyles.Normal, Color.white);
        SetAnchoredRect(_p1Tag.gameObject, 0, 0, 0, 0.5f, 30, 0, 120, 0);

        var scoreRow1 = Child("ScoreRow1", p1Root);
        SetAnchoredRect(scoreRow1, 0, 0, 1, 1, 160, 4, 0, -4);
        var checkLbl1 = TMP("Chk1", scoreRow1, "✓", 20, FontStyles.Bold, C_CORRECT);
        SetAnchoredRect(checkLbl1.gameObject, 0, 0, 0, 1, 0, 0, 26, 0);
        _p1Score = TMP("Num1", scoreRow1, "0", 26, FontStyles.Bold, Color.white);
        SetAnchoredRect(_p1Score.gameObject, 0, 0, 1, 1, 28, 0, 0, 0);

        // ── Player 2 (right) ─────────────────────────────────────────────────
        var p2Root = Child("P2", strip);
        var p2rt   = p2Root.GetComponent<RectTransform>();
        p2rt.anchorMin = new Vector2(1, 0);
        p2rt.anchorMax = new Vector2(1, 1);
        p2rt.offsetMin = new Vector2(-220, 6);
        p2rt.offsetMax = new Vector2(-18, -6);

        _p2BoostDot = BoostDot("P2Dot", p2Root, C_P2);

        var p2Label = TMP("P2Lbl", p2Root, "OPP", 15, FontStyles.Bold, C_P2);
        SetAnchoredRect(p2Label.gameObject, 1, 0.5f, 1, 1f, -90, 0, 60, 0);
        p2Label.alignment = TextAlignmentOptions.Right;

        _p2Tag = TMP("P2Tag", p2Root, "OPP", 14, FontStyles.Normal, Color.white);
        SetAnchoredRect(_p2Tag.gameObject, 1, 0, 1, 0.5f, -150, 0, 120, 0);
        _p2Tag.alignment = TextAlignmentOptions.Right;

        var scoreRow2 = Child("ScoreRow2", p2Root);
        SetAnchoredRect(scoreRow2, 0, 0, 1, 1, 4, 4, -164, -4);
        _p2Score = TMP("Num2", scoreRow2, "0", 26, FontStyles.Bold, Color.white);
        var rt2 = _p2Score.GetComponent<RectTransform>();
        rt2.anchorMin = new Vector2(0, 0); rt2.anchorMax = new Vector2(1, 1);
        rt2.offsetMin = new Vector2(0, 0); rt2.offsetMax = new Vector2(-28, 0);
        _p2Score.alignment = TextAlignmentOptions.Right;
        var checkLbl2 = TMP("Chk2", scoreRow2, "✓", 20, FontStyles.Bold, C_CORRECT);
        SetAnchoredRect(checkLbl2.gameObject, 1, 0, 1, 1, -26, 0, 26, 0);

        // ── Race progress bar (center) ────────────────────────────────────────
        var progBg = Child("ProgBg", strip);
        var prt    = progBg.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.28f, 0);
        prt.anchorMax = new Vector2(0.72f, 1);
        prt.offsetMin = new Vector2(0, 18);
        prt.offsetMax = new Vector2(0, -18);
        _progressBg = prt;
        Img(progBg, new Color(0.15f, 0.15f, 0.25f));

        // track line
        var track = Child("Track", progBg);
        var trt = track.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 0.45f); trt.anchorMax = new Vector2(1, 0.55f);
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        Img(track, C_ACCENT * 0.35f);

        // finish flag
        var flag = TMP("Flag", progBg, "🏁", 16, FontStyles.Normal, Color.white);
        var frt  = flag.GetComponent<RectTransform>();
        frt.anchorMin = frt.anchorMax = new Vector2(1, 0.5f);
        frt.pivot     = new Vector2(0.5f, 0.5f);
        frt.anchoredPosition = new Vector2(-4, 0);
        frt.sizeDelta = new Vector2(24, 24);

        // P1 & P2 dots
        _p1Dot = RaceDot("RDot1", progBg, C_P1);
        _p2Dot = RaceDot("RDot2", progBg, C_P2);
    }

    // ── Game Over ─────────────────────────────────────────────────────────────
    void BuildGameOverPanel()
    {
        _gameOverPanel = FullPanel("GameOver", new Color(0, 0, 0, 0.78f));
        _gameOverPanel.SetActive(false);

        // Card
        var card = Child("GOCard", _gameOverPanel);
        SetRect(card, 0.5f, 0.5f, 0.5f, 0.5f, 0, 0, 560, 380);
        Img(card, C_CARD);
        RoundedOutline(card, C_WRONG);

        _gameOverLabel = TMP("GOLabel", card, "GAME OVER", 80, FontStyles.Bold, C_WRONG);
        SetRect(_gameOverLabel.gameObject, 0.5f, 0.5f, 0.5f, 0.5f, 0, 60, 520, 110);
        _gameOverLabel.alignment = TextAlignmentOptions.Center;

        // separator
        var sep = Child("GOSep", card);
        SetRect(sep, 0.5f, 0.5f, 0.5f, 0.5f, 0, 14, 460, 2);
        Img(sep, C_WRONG * 0.5f);

        _winnerLabel = TMP("Winner", card, "", 34, FontStyles.Normal, C_ACCENT);
        SetRect(_winnerLabel.gameObject, 0.5f, 0.5f, 0.5f, 0.5f, 0, -30, 500, 50);
        _winnerLabel.alignment = TextAlignmentOptions.Center;

        MenuBtn("Disconnect", card, "DISCONNECT", new Vector2(0, -115),
            () => NetworkManager.Singleton?.Shutdown());
    }

    // ── Flash overlay ─────────────────────────────────────────────────────────
    void BuildFlashOverlay()
    {
        var go = Child("Flash", _canvas.gameObject);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        _flashOverlay = go.AddComponent<Image>();
        _flashOverlay.color = Color.clear;
        _flashOverlay.raycastTarget = false;
        go.SetActive(false);
    }

    // =========================================================================
    // UI factory helpers
    // =========================================================================

    GameObject FullPanel(string name, Color bg)
    {
        var go = Child(name, _canvas.gameObject);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        if (bg.a > 0f) { var i = go.AddComponent<Image>(); i.color = bg; i.raycastTarget = true; }
        return go;
    }

    static GameObject Child(string name, GameObject parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static Image Img(GameObject go, Color c)
    {
        var i = go.AddComponent<Image>();
        i.color = c;
        i.raycastTarget = false;
        return i;
    }

    static void RoundedOutline(GameObject parent, Color c)
    {
        var outline = Child("Outline", parent);
        var rt = outline.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-2, -2); rt.offsetMax = new Vector2(2, 2);
        var img = outline.AddComponent<Image>();
        img.color = new Color(c.r, c.g, c.b, 0.45f);
        img.raycastTarget = false;
        outline.transform.SetAsFirstSibling();
    }

    static TextMeshProUGUI TMP(string name, GameObject parent, string text,
        int size, FontStyles style, Color color)
    {
        var go  = Child(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text       = text;
        tmp.fontSize   = size;
        tmp.fontStyle  = style;
        tmp.color      = color;
        tmp.raycastTarget = false;
        tmp.overflowMode  = TextOverflowModes.Overflow;
        return tmp;
    }

    Button MenuBtn(string name, GameObject parent, string label, Vector2 pos, System.Action onClick)
    {
        var go = Child(name, parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(340, 60);

        var img = go.AddComponent<Image>();
        img.color = C_ACCENT;

        var btn    = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = C_ACCENT;
        colors.highlightedColor = Color.white;
        colors.pressedColor     = C_ACCENT * 0.65f;
        colors.selectedColor    = C_ACCENT;
        btn.colors = colors;
        btn.onClick.AddListener(() => onClick?.Invoke());

        var lbl = TMP("Lbl", go, label, 22, FontStyles.Bold, new Color(0.05f, 0.05f, 0.14f));
        var lrt = lbl.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.raycastTarget = false;

        return btn;
    }

    static Image BoostDot(string name, GameObject parent, Color color)
    {
        var go = Child(name, parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot     = new Vector2(0, 0.5f);
        rt.anchoredPosition = new Vector2(4, 0);
        rt.sizeDelta = new Vector2(14, 14);
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static RectTransform RaceDot(string name, GameObject parent, Color color)
    {
        var go = Child(name, parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(22, 22);
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return rt;
    }

    // ── RectTransform positioning ─────────────────────────────────────────────

    // Centred anchor at (ax, ay), sized (w, h), offset (ox, oy) from anchor.
    static void SetRect(GameObject go, float ax, float ay, float pivX, float pivY,
        float ox, float oy, float w, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
        rt.pivot     = new Vector2(pivX, pivY);
        rt.anchoredPosition = new Vector2(ox, oy);
        rt.sizeDelta        = new Vector2(w, h);
    }

    // Stretch anchors + offsets. offsetMin/Max are added to the anchor edges.
    static void SetAnchoredRect(GameObject go,
        float xMin, float yMin, float xMax, float yMax,
        float l, float b, float r, float t)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = new Vector2(l, b);
        rt.offsetMax = new Vector2(r, t);
    }
}
