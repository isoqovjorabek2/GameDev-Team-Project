using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SetGame.Effects
{
    /// <summary>
    /// Animated floating particles for the background. Canvas-space, CPU-driven.
    /// </summary>
    public class ParticleBackground : MonoBehaviour
    {
        struct Particle
        {
            public RectTransform RT;
            public Image         Img;
            public Vector2       Velocity;
            public float         Life;
            public float         MaxLife;
            public float         RotSpeed;
            public float         Size;
        }

        [Header("Settings")]
        public int   MaxParticles = 60;
        public float SpawnRate    = 3f;
        public float SpeedMin     = 20f;
        public float SpeedMax     = 60f;
        public float SizeMin      = 4f;
        public float SizeMax      = 14f;
        public float LifeMin      = 8f;
        public float LifeMax      = 18f;

        static readonly Color[] COLORS =
        {
            new(1f,   0.27f, 0.34f, 0.18f),  // red
            new(0.18f,0.83f, 0.45f, 0.18f),  // green
            new(0.67f,0.33f, 0.97f, 0.18f),  // purple
            new(0.4f, 0.6f,  1f,   0.12f),   // blue
            new(1f,   0.85f, 0.2f, 0.12f),   // gold
        };

        readonly List<Particle> _particles = new();
        Canvas _canvas;
        float  _spawnTimer;
        Rect   _screenRect;

        void Start()
        {
            _canvas = GetComponentInParent<Canvas>();
            UpdateScreenRect();
            // Pre-spawn some particles
            for (int i = 0; i < MaxParticles / 2; i++) SpawnParticle(randomY: true);
        }

        void Update()
        {
            UpdateScreenRect();
            _spawnTimer += Time.deltaTime;

            if (_spawnTimer >= 1f / SpawnRate && _particles.Count < MaxParticles)
            {
                _spawnTimer = 0;
                SpawnParticle(randomY: false);
            }

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Life -= Time.deltaTime;

                if (p.Life <= 0 || p.RT == null)
                {
                    if (p.RT != null) Destroy(p.RT.gameObject);
                    _particles.RemoveAt(i);
                    continue;
                }

                // Move
                p.RT.anchoredPosition += p.Velocity * Time.deltaTime;
                // Rotate gently
                p.RT.Rotate(Vector3.forward, p.RotSpeed * Time.deltaTime);

                // Fade in/out
                float t     = p.Life / p.MaxLife;
                float alpha = t < 0.2f ? t / 0.2f : (t > 0.8f ? (t - 0.8f) / 0.2f : 1f);
                var   c     = p.Img.color;
                c.a         = p.Img.color.a > 0 ? p.Img.color.a : 0.15f;
                p.Img.color = new Color(c.r, c.g, c.b, GetBaseAlpha(p) * alpha);

                _particles[i] = p;
            }
        }

        float GetBaseAlpha(Particle p) => p.Img.color.a > 0 ? 0.2f : 0.0f;

        void SpawnParticle(bool randomY)
        {
            var go = new GameObject("Particle", typeof(RectTransform));
            go.transform.SetParent(transform, false);

            var rt = go.GetComponent<RectTransform>();
            float x = Random.Range(_screenRect.xMin, _screenRect.xMax);
            float y = randomY
                ? Random.Range(_screenRect.yMin, _screenRect.yMax)
                : _screenRect.yMin - SizeMax;

            rt.anchoredPosition = new Vector2(x, y);
            float size = Random.Range(SizeMin, SizeMax);
            rt.sizeDelta = new Vector2(size, size);

            var img = go.AddComponent<Image>();
            img.raycastTarget = false;

            // Alternate between circle (soft) and diamond shapes
            bool circle = Random.value > 0.4f;
            img.sprite = circle ? MakeCircleSprite() : MakeDiamondSprite();
            img.color  = COLORS[Random.Range(0, COLORS.Length)];

            float speed = Random.Range(SpeedMin, SpeedMax);
            float angle = Random.Range(-25f, 25f) * Mathf.Deg2Rad;
            var vel = new Vector2(Mathf.Sin(angle) * speed, speed);

            float life = Random.Range(LifeMin, LifeMax);

            _particles.Add(new Particle
            {
                RT       = rt,
                Img      = img,
                Velocity = vel,
                Life     = life,
                MaxLife  = life,
                RotSpeed = Random.Range(-30f, 30f),
                Size     = size
            });
        }

        void UpdateScreenRect()
        {
            if (_canvas == null) return;
            var r = _canvas.GetComponent<RectTransform>();
            if (r == null) return;
            float hw = r.rect.width  * 0.5f;
            float hh = r.rect.height * 0.5f;
            _screenRect = new Rect(-hw, -hh, r.rect.width, r.rect.height);
        }

        // ─── Sprite factories ─────────────────────────────────────────────────────

        static Sprite _circleSprite;
        static Sprite MakeCircleSprite()
        {
            if (_circleSprite != null) return _circleSprite;
            int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px  = new Color32[size * size];
            float r = size * 0.5f;
            for (int i = 0; i < size * size; i++)
            {
                float x = i % size - r + 0.5f;
                float y = i / size - r + 0.5f;
                float d = Mathf.Sqrt(x * x + y * y);
                byte  a = (byte)Mathf.Clamp01(1f - (d - r + 2f) / 2f) == 0 ? (byte)0 : (byte)255;
                a       = d < r - 1 ? (byte)255 : (byte)Mathf.Clamp01(r - d) == 0 ? (byte)0 : (byte)(Mathf.Clamp01(r - d) * 255);
                px[i]   = new Color32(255, 255, 255, a);
            }
            tex.SetPixels32(px);
            tex.Apply();
            _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _circleSprite;
        }

        static Sprite _diamondSprite;
        static Sprite MakeDiamondSprite()
        {
            if (_diamondSprite != null) return _diamondSprite;
            int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px  = new Color32[size * size];
            float r = size * 0.5f;
            for (int i = 0; i < size * size; i++)
            {
                float x = Mathf.Abs(i % size - r + 0.5f);
                float y = Mathf.Abs(i / size - r + 0.5f);
                byte  a = (x + y) < r ? (byte)255 : (byte)0;
                px[i]   = new Color32(255, 255, 255, a);
            }
            tex.SetPixels32(px);
            tex.Apply();
            _diamondSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _diamondSprite;
        }
    }
}
