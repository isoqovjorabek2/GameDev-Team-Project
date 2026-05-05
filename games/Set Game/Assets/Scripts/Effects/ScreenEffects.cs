using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SetGame.Core;

namespace SetGame.Effects
{
    /// <summary>
    /// Screen-space effects: score popups, screen flash, combo burst.
    /// Attach to a full-screen canvas layer above everything.
    /// </summary>
    public class ScreenEffects : MonoBehaviour
    {
        public static ScreenEffects Instance { get; private set; }

        public Canvas EffectCanvas;  // Set by Bootstrap — overlay canvas on top

        Camera _mainCamera;
        Vector3 _originalCameraPosition;
        Coroutine _shakeCoroutine;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            _mainCamera = Camera.main;
            if (_mainCamera != null)
                _originalCameraPosition = _mainCamera.transform.position;
        }

        void OnEnable()
        {
            GameEvents.OnValidSetFound   += OnSet;
            GameEvents.OnGameOver        += OnGameOver;
            GameEvents.OnHighScoreBeaten += OnHighScore;
        }

        void OnDisable()
        {
            GameEvents.OnValidSetFound   -= OnSet;
            GameEvents.OnGameOver        -= OnGameOver;
            GameEvents.OnHighScoreBeaten -= OnHighScore;
        }

        void OnSet(System.Collections.Generic.List<int> _)
        {
            var ss = ScoreSystem.Instance;
            if (ss == null) return;
            int combo = ss.Combo;
            if (combo >= 2)
            {
                SpawnFloatingText($"+{combo * 50} COMBO ×{combo}", new Color(1f, 0.85f, 0.2f), 52);
                // Add screen shake for combos
                if (combo >= 3)
                    ShakeScreen(0.15f, 0.25f);
                // Add particle burst for high combos
                if (combo >= 2)
                    SpawnComboBurst(combo);
            }
            else
                SpawnFloatingText($"+{100}", Color.white, 40);

            StartCoroutine(FlashScreen(new Color(1f, 1f, 1f, 0.06f), 0.15f));
        }

        void OnGameOver()
        {
            ShakeScreen(0.3f, 0.5f);
            StartCoroutine(FlashScreen(new Color(1f, 0.1f, 0.1f, 0.2f), 0.3f));
        }

        void OnHighScore(int score)
        {
            SpawnFloatingText("NEW BEST!", new Color(1f, 0.85f, 0.2f), 58);
        }

        // ─── Floating score text ──────────────────────────────────────────────────

        void SpawnFloatingText(string text, Color color, int size)
        {
            if (EffectCanvas == null) return;

            var go = new GameObject("FloatText", typeof(RectTransform));
            go.transform.SetParent(EffectCanvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 80);
            rt.anchoredPosition = new Vector2(0, 80);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = size;
            tmp.color     = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false;

            StartCoroutine(AnimateFloat(go, rt, color));
        }

        IEnumerator AnimateFloat(GameObject go, RectTransform rt, Color baseColor)
        {
            float dur = 1.2f;
            float t   = 0;
            Vector2 startPos = rt.anchoredPosition;
            var tmp = go.GetComponent<TextMeshProUGUI>();

            while (t < dur)
            {
                float p    = t / dur;
                float yOff = Mathf.Sin(p * Mathf.PI * 0.5f) * 120f;
                rt.anchoredPosition = startPos + Vector2.up * yOff;

                float alpha = p < 0.5f ? 1f : 1f - (p - 0.5f) * 2f;
                float scale = p < 0.1f ? p / 0.1f : 1f;
                tmp.color      = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
                go.transform.localScale = Vector3.one * scale;

                t += Time.deltaTime;
                yield return null;
            }

            Destroy(go);
        }

        // ─── Screen flash ─────────────────────────────────────────────────────────

        IEnumerator FlashScreen(Color flashColor, float dur)
        {
            if (EffectCanvas == null) yield break;

            var go = new GameObject("Flash", typeof(RectTransform));
            go.transform.SetParent(EffectCanvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.raycastTarget = false;
            img.color = flashColor;

            float t = 0;
            while (t < dur)
            {
                float alpha = 1f - t / dur;
                img.color = new Color(flashColor.r, flashColor.g, flashColor.b,
                                      flashColor.a * alpha);
                t += Time.deltaTime;
                yield return null;
            }

            Destroy(go);
        }

        // ─── Screen shake ───────────────────────────────────────────────────────────

        public void ShakeScreen(float intensity, float duration)
        {
            if (_shakeCoroutine != null)
                StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, duration));
        }

        IEnumerator ShakeCoroutine(float intensity, float duration)
        {
            if (_mainCamera == null) yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float x = UnityEngine.Random.Range(-1f, 1f) * intensity;
                float y = UnityEngine.Random.Range(-1f, 1f) * intensity;

                _mainCamera.transform.position = _originalCameraPosition + new Vector3(x, y, 0f);

                yield return null;
            }

            _mainCamera.transform.position = _originalCameraPosition;
        }

        // ─── Combo burst particles ─────────────────────────────────────────────────

        void SpawnComboBurst(int combo)
        {
            if (EffectCanvas == null) return;

            int particleCount = Mathf.Min(8 + combo * 2, 20);
            Color burstColor = combo >= 5 ? new Color(1f, 0.4f, 0.2f) :
                              combo >= 3 ? new Color(1f, 0.85f, 0.2f) :
                              new Color(0.67f, 0.33f, 0.97f);

            for (int i = 0; i < particleCount; i++)
            {
                var go = new GameObject("BurstParticle", typeof(RectTransform));
                go.transform.SetParent(EffectCanvas.transform, false);
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(20, 20);
                rt.anchoredPosition = Vector2.zero;

                var img = go.AddComponent<Image>();
                img.sprite = CreateBurstSprite();
                img.color = burstColor;
                img.raycastTarget = false;

                StartCoroutine(AnimateBurstParticle(go, rt, img, burstColor));
            }
        }

        IEnumerator AnimateBurstParticle(GameObject go, RectTransform rt, Image img, Color baseColor)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(100f, 200f);
            float duration = Random.Range(0.4f, 0.7f);
            float elapsed = 0f;

            Vector2 startPos = rt.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float easedT = 1f - (1f - t) * (1f - t); // Ease out

                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, easedT);
                rt.localScale = Vector3.one * (1f - t * 0.8f);

                float alpha = 1f - t;
                img.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(go);
        }

        Sprite CreateBurstSprite()
        {
            int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];

            float center = size / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float radius = size / 3f;

                    byte alpha = dist < radius ? (byte)255 : (byte)Mathf.Max(0, 255 - (dist - radius) * 4);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
