using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Spawns subtle twinkling star-dots on the background panel.
public class AnimatedBackground : MonoBehaviour
{
    private struct Star
    {
        public Image  Img;
        public float  Speed;
        public float  Phase;
        public float  BaseAlpha;
    }

    private readonly List<Star> _stars = new();
    private const int Count = 70;

    private void Start()
    {
        var rt = GetComponent<RectTransform>();
        // Reference rect used for positioning (1920×1080 logical pixels)
        for (int i = 0; i < Count; i++) SpawnStar(rt);
        // Gentle background colour shift
        StartCoroutine(ShiftBackground());
    }

    private void SpawnStar(RectTransform parentRt)
    {
        var go = new GameObject("Star");
        go.transform.SetParent(transform, false);

        float sz   = Random.Range(2f, 6f);
        var   stRt = go.AddComponent<RectTransform>();
        stRt.anchorMin        = Vector2.zero;
        stRt.anchorMax        = Vector2.zero;
        stRt.sizeDelta        = new Vector2(sz, sz);
        stRt.anchoredPosition = new Vector2(
            Random.Range(0f, 1920f),
            Random.Range(0f, 1080f));

        float b   = Random.Range(0.45f, 0.85f);
        var   img = go.AddComponent<Image>();
        float a   = Random.Range(0.15f, 0.55f);
        img.color = new Color(b, b, b * 1.25f, a);

        _stars.Add(new Star
        {
            Img       = img,
            Speed     = Random.Range(0.4f, 1.6f),
            Phase     = Random.Range(0f, Mathf.PI * 2f),
            BaseAlpha = a,
        });
    }

    private void Update()
    {
        float t = Time.time;
        foreach (var s in _stars)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(t * s.Speed + s.Phase);
            var   c     = s.Img.color;
            s.Img.color = new Color(c.r, c.g, c.b, s.BaseAlpha * (0.3f + 0.7f * pulse));
        }
    }

    private System.Collections.IEnumerator ShiftBackground()
    {
        var img = GetComponent<Image>();
        while (true)
        {
            float t = Time.time * 0.07f;
            img.color = new Color(
                0.07f + Mathf.Sin(t * 0.9f) * 0.015f,
                0.04f + Mathf.Sin(t * 1.1f) * 0.010f,
                0.15f + Mathf.Sin(t * 0.7f) * 0.030f);
            yield return null;
        }
    }
}
