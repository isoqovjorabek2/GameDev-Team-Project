using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardController : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    // ── Card Identity ───────────────────────────────────────────────────
    public int    PairId    { get; private set; }
    public string Symbol    { get; private set; }
    public Color  FaceColor { get; private set; }

    // ── Card State ──────────────────────────────────────────────────────
    public bool IsFlipped   { get; private set; }
    public bool IsMatched   { get; private set; }
    public bool IsAnimating { get; private set; }

    // ── Visual References ───────────────────────────────────────────────
    private Image             _bg;
    private TextMeshProUGUI   _label;
    private Image             _glow;

    // ── Constants ───────────────────────────────────────────────────────
    private static readonly Color BackColor      = new Color(0.18f, 0.11f, 0.35f);
    private static readonly Color BackLabelColor = new Color(0.50f, 0.40f, 0.68f);
    private const            float FlipHalf      = 0.15f;

    // ── Setup ───────────────────────────────────────────────────────────
    public void Setup(int pairId, string symbol, Color faceColor,
                      Image bg, TextMeshProUGUI label, Image glow)
    {
        PairId    = pairId;
        Symbol    = symbol;
        FaceColor = faceColor;
        _bg       = bg;
        _label    = label;
        _glow     = glow;

        ShowBack();
    }

    // ── Pointer Handlers ────────────────────────────────────────────────
    public void OnPointerClick(PointerEventData _)
    {
        if (IsFlipped || IsMatched || IsAnimating) return;
        if (CardGridManager.Instance.IsLocked) return;
        CardGridManager.Instance.OnCardClicked(this);
    }

    public void OnPointerEnter(PointerEventData _)
    {
        if (IsFlipped || IsMatched || IsAnimating) return;
        StartCoroutine(ScaleTo(1.09f, 0.08f));
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (!IsMatched) StartCoroutine(ScaleTo(1.00f, 0.08f));
    }

    // ── Animation API ────────────────────────────────────────────────────
    public IEnumerator FlipUp()
    {
        IsAnimating = true;
        SoundManager.Instance.PlayFlip();
        yield return Squish(() => { IsFlipped = true; ShowFace(); });
        IsAnimating = false;
    }

    public IEnumerator FlipDown()
    {
        IsAnimating = true;
        yield return Squish(() => { IsFlipped = false; ShowBack(); });
        IsAnimating = false;
    }

    public IEnumerator PlayMatchEffect()
    {
        IsMatched = true;

        // Green glow burst
        _glow.color = new Color(0.15f, 0.92f, 0.48f, 0.65f);
        _glow.gameObject.SetActive(true);

        yield return ScaleTo(1.18f, 0.10f);
        yield return ScaleTo(1.00f, 0.12f);
        yield return new WaitForSeconds(0.12f);

        _glow.gameObject.SetActive(false);

        // Settled matched color: brighter tint of face color
        _bg.color = new Color(
            Mathf.Clamp01(FaceColor.r * 0.7f + 0.15f),
            Mathf.Clamp01(FaceColor.g * 0.7f + 0.15f),
            Mathf.Clamp01(FaceColor.b * 0.7f + 0.15f));
    }

    public IEnumerator PlayMissEffect()
    {
        // Red flash
        var orig = _bg.color;
        _bg.color = new Color(0.88f, 0.20f, 0.26f);
        yield return Shake(0.28f, 10f);
        _bg.color = orig;
    }

    // ── Internals ────────────────────────────────────────────────────────
    private void ShowFace()
    {
        _bg.color    = FaceColor;
        _label.text  = Symbol;
        _label.color = Color.white;
    }

    private void ShowBack()
    {
        _bg.color    = BackColor;
        _label.text  = "?";
        _label.color = BackLabelColor;
    }

    private IEnumerator Squish(System.Action midAction)
    {
        Vector3 orig = transform.localScale;

        // Squish to flat
        yield return LerpScaleX(orig, 0.04f, FlipHalf);

        midAction?.Invoke();

        // Unsquish back
        yield return LerpScaleX(new Vector3(0.04f, orig.y, orig.z), orig.x, FlipHalf);
        transform.localScale = orig;
    }

    private IEnumerator LerpScaleX(Vector3 from, float toX, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            float x = Mathf.Lerp(from.x, toX, t / dur);
            transform.localScale = new Vector3(x, from.y, from.z);
            t += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ScaleTo(float target, float dur)
    {
        Vector3 start = transform.localScale;
        Vector3 end   = new Vector3(target, target, 1f);
        float   t     = 0f;
        while (t < dur)
        {
            transform.localScale = Vector3.Lerp(start, end, t / dur);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localScale = end;
    }

    private IEnumerator Shake(float dur, float magnitude)
    {
        Vector3 origin = transform.localPosition;
        float   t      = 0f;
        while (t < dur)
        {
            float x = Mathf.Sin(t * 85f) * magnitude * (1f - t / dur);
            transform.localPosition = origin + new Vector3(x, 0f, 0f);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = origin;
    }
}
