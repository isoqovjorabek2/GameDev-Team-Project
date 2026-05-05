using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class AtomView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] ElementId elementId = ElementId.H;
    [SerializeField] ElementRegistry registry;
    [SerializeField] TextMeshProUGUI symbolLabel;
    [SerializeField] Graphic backgroundGraphic;
    [SerializeField] float neighborRingRadius = 88f;

    readonly List<AtomView> _neighbors = new List<AtomView>();

    RectTransform _rectTransform;
    CanvasGroup _canvasGroup;
    Canvas _rootCanvas;
    Vector2 _dragPointerOffset;
    Vector2 _restAnchoredPosition;
    Transform _restParent;
    int _restSiblingIndex;
    bool _dragging;

    public RectTransform RectTransform => _rectTransform;
    public ElementId ElementId => elementId;
    public int NeighborCount => _neighbors.Count;
    public int MaxBonds => registry != null ? registry.GetMaxBonds(elementId) : ElementRules.DefaultMaxBonds(elementId);

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _rootCanvas = GetComponentInParent<Canvas>();
        ApplyVisuals();
    }

    void Start()
    {
        var graph = GetComponentInParent<MoleculeGraph>();
        graph?.Register(this);
    }

    public void Initialize(ElementId id, ElementRegistry reg)
    {
        elementId = id;
        if (reg != null)
            registry = reg;
        ApplyVisuals();
        PlaySpawnAnimation();
    }

    void ApplyVisuals()
    {
        if (registry != null)
            registry.ApplyToAtom(elementId, symbolLabel, backgroundGraphic);
        else
        {
            if (symbolLabel != null)
                symbolLabel.text = elementId.ToString();
        }
    }

    // ── Bond Logic ───────────────────────────────────────────────────────────

    public bool CanBondWith(AtomView other)
    {
        if (other == null || other == this) return false;
        if (_neighbors.Contains(other)) return false;
        if (_neighbors.Count >= MaxBonds || other._neighbors.Count >= other.MaxBonds) return false;
        return true;
    }

    public void AddBond(AtomView other)
    {
        if (other != null && other != this && !_neighbors.Contains(other))
            _neighbors.Add(other);
    }

    public void RemoveBond(AtomView other)
    {
        _neighbors.Remove(other);
    }

    // ── Drag Handlers ────────────────────────────────────────────────────────

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isActiveAndEnabled) return;

        _dragging = true;
        _restAnchoredPosition = _rectTransform.anchoredPosition;
        _restParent = _rectTransform.parent;
        _restSiblingIndex = _rectTransform.GetSiblingIndex();

        GetComponentInParent<MoleculeGraph>()?.transform.SetAsLastSibling();

        if (_canvasGroup != null) _canvasGroup.blocksRaycasts = false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform.parent as RectTransform, eventData.position,
            _rootCanvas != null ? _rootCanvas.worldCamera : null, out var localPoint);
        _dragPointerOffset = _rectTransform.anchoredPosition - localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_dragging) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform.parent as RectTransform, eventData.position,
                _rootCanvas != null ? _rootCanvas.worldCamera : null, out var localPoint))
        {
            _rectTransform.anchoredPosition = localPoint + _dragPointerOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_dragging) return;
        _dragging = false;

        if (_canvasGroup != null) _canvasGroup.blocksRaycasts = false;
        var target = FindAtomViewUnderPointer(eventData, this);
        if (_canvasGroup != null) _canvasGroup.blocksRaycasts = true;

        if (target != null && MoleculeGraph.TryBond(this, target, neighborRingRadius))
            return;

        _rectTransform.SetParent(_restParent, false);
        _rectTransform.SetSiblingIndex(_restSiblingIndex);
        _rectTransform.anchoredPosition = _restAnchoredPosition;
    }

    public void PaletteBeginDrag(PointerEventData eventData)
    {
        if (_rootCanvas == null)
            _rootCanvas = GetComponentInParent<Canvas>();

        GetComponentInParent<MoleculeGraph>()?.transform.SetAsLastSibling();

        if (_canvasGroup != null) _canvasGroup.blocksRaycasts = false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform.parent as RectTransform, eventData.position,
            _rootCanvas != null ? _rootCanvas.worldCamera : null, out var localPoint);
        _dragPointerOffset = _rectTransform.anchoredPosition - localPoint;
        _dragging = true;
    }

    public void PaletteDrag(PointerEventData eventData) => OnDrag(eventData);

    public void PaletteEndDrag(PointerEventData eventData)
    {
        _dragging = false;

        if (_canvasGroup != null) _canvasGroup.blocksRaycasts = false;
        var target = FindAtomViewUnderPointer(eventData, this);
        if (_canvasGroup != null) _canvasGroup.blocksRaycasts = true;

        if (target != null && MoleculeGraph.TryBond(this, target, neighborRingRadius))
            return;
    }

    static AtomView FindAtomViewUnderPointer(PointerEventData eventData, AtomView exclude)
    {
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var hit in results)
        {
            var view = hit.gameObject.GetComponent<AtomView>()
                       ?? hit.gameObject.GetComponentInParent<AtomView>();
            if (view != null && view != exclude)
                return view;
        }
        return null;
    }

    // ── Animations ───────────────────────────────────────────────────────────

    public void PlaySpawnAnimation()
    {
        StopCoroutine(nameof(SpawnRoutine));
        StartCoroutine(SpawnRoutine());
    }

    public void PlayBondAnimation()
    {
        StartCoroutine(BondFlashRoutine());
    }

    public void PlayDiscoveryAnimation()
    {
        StartCoroutine(DiscoveryPulseRoutine());
    }

    /// <summary>Smoothly moves atom to target anchored position (used during bonding snap).</summary>
    public void MoveTo(Vector2 targetAnchoredPos, float duration)
    {
        StartCoroutine(MoveToRoutine(targetAnchoredPos, duration));
    }

    IEnumerator SpawnRoutine()
    {
        _rectTransform.localScale = Vector3.zero;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.25f;
            _rectTransform.localScale = Vector3.one * BounceEaseOut(Mathf.Clamp01(t));
            yield return null;
        }
        _rectTransform.localScale = Vector3.one;
    }

    IEnumerator BondFlashRoutine()
    {
        Color baseColor = backgroundGraphic != null ? backgroundGraphic.color : Color.white;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.3f;
            float p = Mathf.Clamp01(t);
            _rectTransform.localScale = Vector3.one * (1f + Mathf.Sin(p * Mathf.PI) * 0.25f);
            if (backgroundGraphic != null)
                backgroundGraphic.color = Color.Lerp(Color.white, baseColor, p);
            yield return null;
        }
        _rectTransform.localScale = Vector3.one;
        if (backgroundGraphic != null)
            backgroundGraphic.color = baseColor;
    }

    IEnumerator DiscoveryPulseRoutine()
    {
        Color baseColor = backgroundGraphic != null ? backgroundGraphic.color : Color.white;
        var gold = new Color(1f, 0.85f, 0.15f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.7f;
            float sin = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI);
            _rectTransform.localScale = Vector3.one * (1f + sin * 0.4f);
            if (backgroundGraphic != null)
                backgroundGraphic.color = Color.Lerp(baseColor, gold, sin);
            yield return null;
        }
        _rectTransform.localScale = Vector3.one;
        if (backgroundGraphic != null)
            backgroundGraphic.color = baseColor;
    }

    IEnumerator MoveToRoutine(Vector2 target, float duration)
    {
        Vector2 start = _rectTransform.anchoredPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            _rectTransform.anchoredPosition = Vector2.Lerp(start, target, EaseOutBack(Mathf.Clamp01(t)));
            yield return null;
        }
        _rectTransform.anchoredPosition = target;
    }

    // t in [0,1], returns values up to ~1.7 (bounce/overshoot effects)
    static float BounceEaseOut(float t)
    {
        const float n1 = 7.5625f, d1 = 2.75f;
        if (t < 1f / d1) return n1 * t * t;
        if (t < 2f / d1) { t -= 1.5f / d1;   return n1 * t * t + 0.75f; }
        if (t < 2.5f / d1) { t -= 2.25f / d1; return n1 * t * t + 0.9375f; }
        t -= 2.625f / d1;
        return n1 * t * t + 0.984375f;
    }

    static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
