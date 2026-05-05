using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardGridManager : MonoBehaviour
{
    public static CardGridManager Instance { get; private set; }

    public bool IsLocked { get; private set; }

    private Canvas           _canvas;
    private GameObject       _gridRoot;
    private CardController   _first;
    private CardController   _second;
    private readonly List<CardController> _cards = new();

    // ── Card type library: (symbol, face background color) ─────────────
    private static readonly (string sym, Color col)[] CardTypes =
    {
        ("★", new Color(0.98f, 0.82f, 0.10f)),   // Gold star
        ("♥", new Color(0.94f, 0.27f, 0.35f)),   // Crimson heart
        ("◆", new Color(0.22f, 0.60f, 0.96f)),   // Sky-blue diamond
        ("▲", new Color(0.18f, 0.82f, 0.42f)),   // Emerald triangle
        ("●", new Color(1.00f, 0.50f, 0.12f)),   // Orange circle
        ("■", new Color(0.60f, 0.22f, 0.90f)),   // Purple square
        ("♠", new Color(0.35f, 0.88f, 0.85f)),   // Cyan spade
        ("♦", new Color(0.95f, 0.32f, 0.68f)),   // Pink diamond
        ("♣", new Color(0.46f, 0.78f, 0.22f)),   // Lime club
        ("▼", new Color(0.88f, 0.60f, 0.18f)),   // Amber triangle
        ("◉", new Color(0.80f, 0.12f, 0.28f)),   // Deep-red bullseye
        ("▶", new Color(0.14f, 0.50f, 0.85f)),   // Steel-blue arrow
        ("◀", new Color(0.72f, 0.35f, 0.62f)),   // Mauve arrow
        ("◇", new Color(0.35f, 0.88f, 0.72f)),   // Mint diamond
        ("◎", new Color(0.85f, 0.48f, 0.90f)),   // Lavender target
    };

    private void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
    }

    public void Initialize(Canvas canvas)
    {
        _canvas = canvas;
        GameManager.Instance.OnStateChanged += OnStateChanged;
    }

    // ── State Handling ──────────────────────────────────────────────────
    private void OnStateChanged(GameManager.State state)
    {
        if (state == GameManager.State.Playing)
        {
            DestroyGrid();
            BuildGrid();
        }
        else if (_gridRoot != null)
        {
            _gridRoot.SetActive(false);
        }
    }

    // ── Click Handling ──────────────────────────────────────────────────
    public void OnCardClicked(CardController card)
    {
        if (_first == null)
        {
            _first = card;
            StartCoroutine(card.FlipUp());
        }
        else if (_second == null && card != _first)
        {
            _second = card;
            StartCoroutine(ResolveMatch());
        }
    }

    private IEnumerator ResolveMatch()
    {
        IsLocked = true;
        yield return _second.FlipUp();
        yield return new WaitForSeconds(0.22f);

        if (_first.PairId == _second.PairId)
        {
            // ── Match ──
            yield return StartCoroutine(_first.PlayMatchEffect());
            yield return StartCoroutine(_second.PlayMatchEffect());
            SoundManager.Instance.PlayMatch();
            GameManager.Instance.RecordMatch();

            int combo = GameManager.Instance.Combo;
            UIManager.Instance.ShowComboPopup(combo);
            if (combo >= 2) SoundManager.Instance.PlayCombo();
        }
        else
        {
            // ── Miss ──
            yield return StartCoroutine(_first.PlayMissEffect());
            yield return StartCoroutine(_second.PlayMissEffect());
            SoundManager.Instance.PlayMiss();
            GameManager.Instance.RecordMiss();

            // Only flip back if lives remain (GameOver coroutine has a 1.3s delay
            // so CurrentState is still Playing even after the last miss)
            if (GameManager.Instance.Lives > 0)
            {
                yield return new WaitForSeconds(0.35f);
                StartCoroutine(_first.FlipDown());
                StartCoroutine(_second.FlipDown());
                yield return new WaitForSeconds(0.33f);
            }
        }

        _first  = null;
        _second = null;
        IsLocked = false;
    }

    // ── Grid Building ───────────────────────────────────────────────────
    private void BuildGrid()
    {
        var (cols, rows) = GameManager.Instance.GridSize();
        int totalCards   = cols * rows;
        int pairs        = totalCards / 2;

        // Root (full canvas area minus HUD)
        _gridRoot = new GameObject("CardGrid");
        _gridRoot.transform.SetParent(_canvas.transform, false);
        _gridRoot.transform.SetSiblingIndex(1); // behind HUD/menus

        var rootRt = _gridRoot.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = new Vector2(20f,  20f);
        rootRt.offsetMax = new Vector2(-20f, -85f); // -85 = HUD height

        // GridLayoutGroup container (centred inside root)
        var gridGo = new GameObject("Grid");
        gridGo.transform.SetParent(_gridRoot.transform, false);
        var gridRt = gridGo.AddComponent<RectTransform>();
        gridRt.anchorMin = gridRt.anchorMax = new Vector2(0.5f, 0.5f);

        var glg = gridGo.AddComponent<GridLayoutGroup>();
        glg.childAlignment = TextAnchor.MiddleCenter;
        glg.constraint     = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = cols;

        // Compute card size to fill the available area
        float availW   = 1880f;
        float availH   = 880f;
        float cellW    = Mathf.Min((availW - (cols - 1) * 12f) / cols, 220f);
        float cellH    = Mathf.Min((availH - (rows - 1) * 12f) / rows, 240f);
        float cellSize = Mathf.Min(cellW, cellH);
        glg.cellSize   = new Vector2(cellSize, cellSize * 1.12f);
        glg.spacing    = new Vector2(12f, 12f);

        float totalW = cols * (glg.cellSize.x + glg.spacing.x) - glg.spacing.x;
        float totalH = rows * (glg.cellSize.y + glg.spacing.y) - glg.spacing.y;
        gridRt.anchoredPosition = new Vector2(0f, -5f);
        gridRt.sizeDelta        = new Vector2(totalW, totalH);

        // Shuffle pair indices
        var pairIds = new List<int>();
        for (int i = 0; i < pairs; i++) { pairIds.Add(i); pairIds.Add(i); }
        Shuffle(pairIds);

        _cards.Clear();
        for (int i = 0; i < totalCards; i++)
            _cards.Add(SpawnCard(gridGo.transform, pairIds[i], glg.cellSize));

        _gridRoot.SetActive(true);
    }

    private CardController SpawnCard(Transform parent, int pairId, Vector2 size)
    {
        var go = new GameObject($"Card_{pairId}");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = size;

        // Glow ring (rendered behind card bg via sibling order)
        var glowGo = new GameObject("Glow");
        glowGo.transform.SetParent(go.transform, false);
        var glowRt          = glowGo.AddComponent<RectTransform>();
        glowRt.anchorMin    = Vector2.zero;
        glowRt.anchorMax    = Vector2.one;
        glowRt.offsetMin    = new Vector2(-8f, -8f);
        glowRt.offsetMax    = new Vector2( 8f,  8f);
        var glowImg         = glowGo.AddComponent<Image>();
        glowImg.color       = new Color(0.15f, 0.92f, 0.48f, 0f);
        glowGo.SetActive(false);

        // Card background
        var bg    = go.AddComponent<Image>();
        bg.color  = new Color(0.18f, 0.11f, 0.35f);

        // Symbol label
        var symGo = new GameObject("Symbol");
        symGo.transform.SetParent(go.transform, false);
        var symRt        = symGo.AddComponent<RectTransform>();
        symRt.anchorMin  = Vector2.zero;
        symRt.anchorMax  = Vector2.one;
        symRt.offsetMin  = Vector2.zero;
        symRt.offsetMax  = Vector2.zero;
        var lbl          = symGo.AddComponent<TextMeshProUGUI>();
        lbl.alignment    = TextAlignmentOptions.Center;
        lbl.enableWordWrapping = false;
        lbl.fontSize     = size.x * 0.42f;

        var (sym, col) = CardTypes[pairId % CardTypes.Length];
        var ctrl       = go.AddComponent<CardController>();
        ctrl.Setup(pairId, sym, col, bg, lbl, glowImg);
        return ctrl;
    }

    private void DestroyGrid()
    {
        if (_gridRoot != null) Destroy(_gridRoot);
        _cards.Clear();
        _first   = null;
        _second  = null;
        IsLocked = false;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
