using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Controls the main menu panel shown at game start.
///
/// Scene setup required:
///   • Attach this script to a GameObject (e.g. "MenuController") in the scene.
///   • menuPanel     → full-screen panel with a dark semi-transparent background Image
///                     (make sure the Image has Raycast Target = true so it blocks input)
///   • menuCanvasGroup → CanvasGroup on that same panel (for fade animation)
///   • subtitleText  → TMP label auto-filled with "Discover N compounds to win!"
///   • Wire Play button  → MenuController.PlayGame()
///   • Wire Quit button  → MenuController.QuitGame()
/// </summary>
public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject menuPanel;
    [SerializeField] CanvasGroup menuCanvasGroup;
    [SerializeField] TextMeshProUGUI subtitleText;

    void Start()
    {
        int toWin = DiscoveryManager.Instance != null ? DiscoveryManager.Instance.CompoundsToWin : 3;
        if (subtitleText != null)
            subtitleText.text = $"Discover {toWin} compounds to win!";

        if (menuPanel != null) menuPanel.SetActive(true);
        if (menuCanvasGroup != null) menuCanvasGroup.alpha = 1f;
    }

    // Called by the Play button
    public void PlayGame()
    {
        StartCoroutine(HideMenuRoutine());
    }

    // Called by the Quit button
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator HideMenuRoutine()
    {
        yield return FadeCanvasGroup(menuCanvasGroup, 1f, 0f, 0.35f);
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;
        float t = 0f;
        cg.alpha = from;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t));
            yield return null;
        }
        cg.alpha = to;
    }
}
