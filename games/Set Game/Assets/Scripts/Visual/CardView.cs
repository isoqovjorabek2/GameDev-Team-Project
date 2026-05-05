using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SetGame.Core;

namespace SetGame.Visual
{
    /// <summary>
    /// Represents a single card on the board. Handles selection, highlight, and animations.
    /// </summary>
    public class CardView : MonoBehaviour,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public CardData Data      { get; private set; }
        public int      BoardIndex{ get; set; }
        public bool     IsSelected{ get; private set; }
        public bool     IsHinted  { get; private set; }

        // Sub-components set by CardFactory
        public Image       CardBackground;
        public Image       GlowImage;
        public Image       SelectRing;
        public List<CardSymbolGraphic> Symbols = new();

        // Visual constants
        static readonly Color IDLE_BG       = new(0.09f, 0.09f, 0.18f, 1f);
        static readonly Color HOVER_BG      = new(0.12f, 0.12f, 0.24f, 1f);
        static readonly Color SELECTED_BG   = new(0.06f, 0.06f, 0.14f, 1f);
        static readonly Color GLOW_SELECTED = new(1f, 0.85f, 0.2f, 0.55f);
        static readonly Color GLOW_HINT     = new(0.2f, 0.9f, 1f, 0.55f);
        static readonly Color GLOW_NONE     = new(0f, 0f, 0f, 0f);

        bool _interactive = true;
        Coroutine _shakeCoroutine;
        Coroutine _glowPulse;
        Vector3   _baseScale;

        void Awake() => _baseScale = transform.localScale;

        public void Setup(CardData data, int boardIndex)
        {
            Data       = data;
            BoardIndex = boardIndex;
            IsSelected = false;
            IsHinted   = false;

            SetCardBackground(IDLE_BG);
            SetGlow(GLOW_NONE);
            SetSelectRing(false);
        }

        // ─── Interaction ──────────────────────────────────────────────────────────

        public void OnPointerClick(PointerEventData e)
        {
            if (!_interactive) return;
            if (IsSelected) Deselect();
            else            Select();
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if (!_interactive || IsSelected) return;
            StopScaleTween();
            _scaleTween = StartCoroutine(ScaleTo(1.04f, 0.12f));
            SetCardBackground(HOVER_BG);
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (!_interactive || IsSelected) return;
            StopScaleTween();
            _scaleTween = StartCoroutine(ScaleTo(1f, 0.12f));
            SetCardBackground(IDLE_BG);
        }

        // ─── Selection API ────────────────────────────────────────────────────────

        public void Select()
        {
            IsSelected = true;
            SetCardBackground(SELECTED_BG);
            SetSelectRing(true);
            StopGlowPulse();
            _glowPulse = StartCoroutine(PulseGlow(GLOW_SELECTED));
            StopScaleTween();
            _scaleTween = StartCoroutine(ScaleTo(1.07f, 0.15f));
            GameEvents.RaiseCardSelected(BoardIndex);
        }

        public void Deselect()
        {
            IsSelected = false;
            IsHinted   = false;
            SetCardBackground(IDLE_BG);
            SetSelectRing(false);
            StopGlowPulse();
            SetGlow(GLOW_NONE);
            StopScaleTween();
            _scaleTween = StartCoroutine(ScaleTo(1f, 0.12f));
            GameEvents.RaiseCardDeselected(BoardIndex);
        }

        public void ShowHint()
        {
            if (IsSelected) return;
            IsHinted = true;
            StopGlowPulse();
            _glowPulse = StartCoroutine(PulseGlow(GLOW_HINT));
        }

        public void ClearHint()
        {
            if (!IsHinted) return;
            IsHinted = false;
            StopGlowPulse();
            SetGlow(GLOW_NONE);
        }

        public void SetInteractive(bool on) => _interactive = on;

        // ─── Animations ───────────────────────────────────────────────────────────

        public IEnumerator DealIn(float delay)
        {
            transform.localScale = Vector3.zero;
            yield return new WaitForSeconds(delay);
            yield return ScaleTo(1f, 0.25f, AnimCurve.EaseOut);
        }

        public IEnumerator ValidSetAnimation()
        {
            _interactive = false;
            StopGlowPulse();
            SetGlow(new Color(0.3f, 1f, 0.4f, 0.9f));
            yield return ScaleTo(1.15f, 0.12f);
            yield return ScaleTo(0f, 0.2f, AnimCurve.EaseIn);
            Destroy(gameObject);
        }

        public IEnumerator InvalidSetAnimation()
        {
            _interactive = false;
            yield return ShakeAnim(0.35f, 12f);
            _interactive = true;
            Deselect();
        }

        IEnumerator ShakeAnim(float duration, float magnitude)
        {
            Vector3 orig = transform.localPosition;
            float   t    = 0;
            while (t < duration)
            {
                float s = Mathf.Sin(t * Mathf.PI * 10f) * magnitude * (1f - t / duration);
                transform.localPosition = orig + Vector3.right * s;
                t += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = orig;
        }

        Coroutine _scaleTween;
        void StopScaleTween() { if (_scaleTween != null) StopCoroutine(_scaleTween); }

        IEnumerator ScaleTo(float target, float dur, AnimCurve curve = AnimCurve.EaseInOut)
        {
            float from = transform.localScale.x / _baseScale.x;
            float t = 0;
            while (t < dur)
            {
                float p = Evaluate(t / dur, curve);
                float s = Mathf.Lerp(from, target, p);
                transform.localScale = _baseScale * s;
                t += Time.deltaTime;
                yield return null;
            }
            transform.localScale = _baseScale * target;
        }

        void StopGlowPulse() { if (_glowPulse != null) StopCoroutine(_glowPulse); }

        IEnumerator PulseGlow(Color baseColor)
        {
            while (true)
            {
                float t = (Mathf.Sin(Time.time * 3f) + 1f) * 0.5f;
                Color c = Color.Lerp(baseColor * 0.6f, baseColor, t);
                SetGlow(c);
                yield return null;
            }
        }

        // ─── Visual setters ───────────────────────────────────────────────────────

        void SetCardBackground(Color c) { if (CardBackground) CardBackground.color = c; }
        void SetGlow(Color c)           { if (GlowImage)      GlowImage.color      = c; }
        void SetSelectRing(bool on)     { if (SelectRing)     SelectRing.enabled   = on; }

        // ─── Curve helper ─────────────────────────────────────────────────────────

        enum AnimCurve { Linear, EaseIn, EaseOut, EaseInOut }

        static float Evaluate(float t, AnimCurve c) => c switch
        {
            AnimCurve.EaseIn    => t * t,
            AnimCurve.EaseOut   => 1 - (1 - t) * (1 - t),
            AnimCurve.EaseInOut => t < 0.5f ? 2*t*t : 1-2*(1-t)*(1-t),
            _                   => t
        };
    }
}
