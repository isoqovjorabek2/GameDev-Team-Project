using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// Spawns colourful falling confetti pieces when Burst() is called.
public class ConfettiSystem : MonoBehaviour
{
    private static readonly Color[] Palette =
    {
        new Color(0.98f, 0.27f, 0.35f),   // Red
        new Color(1.00f, 0.55f, 0.10f),   // Orange
        new Color(0.98f, 0.84f, 0.10f),   // Yellow
        new Color(0.25f, 0.85f, 0.42f),   // Green
        new Color(0.10f, 0.76f, 0.87f),   // Cyan
        new Color(0.25f, 0.45f, 1.00f),   // Blue
        new Color(0.60f, 0.22f, 0.92f),   // Purple
        new Color(0.96f, 0.30f, 0.70f),   // Pink
    };

    /// <summary>Trigger the confetti burst.</summary>
    public void Burst() => StartCoroutine(DoBurst());

    private IEnumerator DoBurst()
    {
        // Five waves, 22 pieces each, 180 ms apart
        for (int wave = 0; wave < 5; wave++)
        {
            for (int i = 0; i < 22; i++) SpawnPiece();
            yield return new WaitForSeconds(0.18f);
        }
    }

    private void SpawnPiece()
    {
        var go = new GameObject("Piece");
        go.transform.SetParent(transform, false);

        float w  = Random.Range(8f, 20f);
        float h  = Random.Range(5f, 13f);
        var   rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(w, h);
        rt.anchorMin        = new Vector2(Random.Range(0.03f, 0.97f), 1.04f);
        rt.anchorMax        = rt.anchorMin;
        rt.anchoredPosition = Vector2.zero;

        var img   = go.AddComponent<Image>();
        img.color = Palette[Random.Range(0, Palette.Length)];

        StartCoroutine(Animate(rt, img));
    }

    private static IEnumerator Animate(RectTransform rt, Image img)
    {
        float dur       = Random.Range(2.6f, 4.8f);
        float fallSpeed = Random.Range(280f, 580f);
        float swayFreq  = Random.Range(1.5f, 4.5f);
        float swayAmp   = Random.Range(28f,  85f);
        float spinSpeed = Random.Range(110f, 380f);

        Vector2 start = rt.anchoredPosition;
        Color   baseC = img.color;
        float   t     = 0f;

        while (t < dur)
        {
            rt.anchoredPosition = start + new Vector2(
                Mathf.Sin(t * swayFreq) * swayAmp,
                -t * fallSpeed);
            rt.localEulerAngles = new Vector3(0f, 0f, t * spinSpeed);

            // Fade out in final 30 % of life
            float alpha = t / dur < 0.70f ? 1f : Mathf.Lerp(1f, 0f, (t / dur - 0.70f) / 0.30f);
            img.color   = new Color(baseC.r, baseC.g, baseC.b, alpha);

            t += Time.deltaTime;
            yield return null;
        }

        Destroy(rt.gameObject);
    }
}
