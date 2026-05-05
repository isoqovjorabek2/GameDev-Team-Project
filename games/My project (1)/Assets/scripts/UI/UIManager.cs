using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages all in-game UI: progress HUD, compound discovery popup, and win screen.
///
/// Scene setup required:
///   • Assign progressText  → TMP label showing "Compounds: X / Y"
///   • Assign discoveryPopup → Panel GameObject (add CanvasGroup + RectTransform)
///     - discoveryPopupRect  → the popup's own RectTransform
///     - discoveryFormulaText / discoveryNameText → child TMP labels
///   • Assign winPanel       → full-screen Panel (add CanvasGroup + RectTransform)
///     - winPanelRect        → the panel's own RectTransform
///   • Wire Play Again button → UIManager.PlayAgain()
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] TextMeshProUGUI progressText;

    [Header("Discovery Popup")]
    [SerializeField] GameObject discoveryPopup;
    [SerializeField] RectTransform discoveryPopupRect;
    [SerializeField] TextMeshProUGUI discoveryFormulaText;
    [SerializeField] TextMeshProUGUI discoveryNameText;

    [Header("Win Screen")]
    [SerializeField] GameObject winPanel;
    [SerializeField] RectTransform winPanelRect;

    readonly Queue<(string formula, string name)> _popupQueue = new Queue<(string, string)>();
    bool _showingPopup;

    void OnEnable()
    {
        DiscoveryManager.OnCompoundDiscovered += HandleDiscovery;
        DiscoveryManager.OnWin += HandleWin;
    }

    void OnDisable()
    {
        DiscoveryManager.OnCompoundDiscovered -= HandleDiscovery;
        DiscoveryManager.OnWin -= HandleWin;
    }

    void Start()
    {
        if (discoveryPopup != null) discoveryPopup.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        int total = DiscoveryManager.Instance != null ? DiscoveryManager.Instance.CompoundsToWin : 3;
        SetProgressText(0, total);
    }

    void HandleDiscovery(string formula, string name, int discovered, int total)
    {
        SetProgressText(discovered, total);
        _popupQueue.Enqueue((formula, name));
        if (!_showingPopup)
            StartCoroutine(DrainPopupQueue());
    }

    void HandleWin() => StartCoroutine(ShowWinRoutine());

    void SetProgressText(int discovered, int total)
    {
        if (progressText != null)
            progressText.text = $"Compounds: {discovered} / {total}";
    }

    // ── Discovery Popup ───────────────────────────────────────────────────────

    IEnumerator DrainPopupQueue()
    {
        _showingPopup = true;
        while (_popupQueue.Count > 0)
        {
            var item = _popupQueue.Dequeue();
            yield return PopupRoutine(item.formula, item.name);
        }
        _showingPopup = false;
    }

    IEnumerator PopupRoutine(string formula, string name)
    {
        if (discoveryPopup == null) yield break;

        if (discoveryFormulaText != null) discoveryFormulaText.text = formula;
        if (discoveryNameText != null)    discoveryNameText.text    = name;

        var cg = GetOrAddCanvasGroup(discoveryPopup);
        cg.alpha = 0f;
        if (discoveryPopupRect != null) discoveryPopupRect.localScale = Vector3.one * 0.6f;
        discoveryPopup.SetActive(true);

        // Fade + scale in
        yield return TweenCG(cg, discoveryPopupRect, 0f, 1f, 0.6f, 1f, 0.25f);
        yield return new WaitForSeconds(1.5f);
        // Fade + scale out
        yield return TweenCG(cg, discoveryPopupRect, 1f, 0f, 1f, 0.8f, 0.2f);

        discoveryPopup.SetActive(false);
    }

    IEnumerator TweenCG(CanvasGroup cg, RectTransform rt, float aFrom, float aTo,
                        float sFrom, float sTo, float dur)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float p = Mathf.Clamp01(t);
            if (cg != null) cg.alpha = Mathf.Lerp(aFrom, aTo, p);
            if (rt != null) rt.localScale = Vector3.one * Mathf.Lerp(sFrom, sTo, p);
            yield return null;
        }
        if (cg != null) cg.alpha = aTo;
        if (rt != null) rt.localScale = Vector3.one * sTo;
    }

    // ── Win Screen ────────────────────────────────────────────────────────────

    IEnumerator ShowWinRoutine()
    {
        if (winPanel == null) yield break;

        var cg = GetOrAddCanvasGroup(winPanel);
        cg.alpha = 0f;
        if (winPanelRect != null) winPanelRect.localScale = Vector3.one * 0.75f;
        winPanel.SetActive(true);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.55f;
            float p = Mathf.Clamp01(t);
            cg.alpha = p;
            if (winPanelRect != null)
                winPanelRect.localScale = Vector3.one * Mathf.Lerp(0.75f, 1f, EaseOutBack(p));
            yield return null;
        }
        cg.alpha = 1f;
        if (winPanelRect != null) winPanelRect.localScale = Vector3.one;
    }

    // Called by "Play Again" button
    public void PlayAgain()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    static CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        var cg = go.GetComponent<CanvasGroup>();
        return cg != null ? cg : go.AddComponent<CanvasGroup>();
    }
}
