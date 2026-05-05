using UnityEngine;

public class TileView : MonoBehaviour
{
    public Vector2Int boardPosition;

    [Tooltip("Optional; BoardManager assigns the square SpriteRenderer at runtime.")]
    public Renderer renderer;

    SpriteRenderer spriteRenderer;
    Color baseSquareColor;
    bool highlighted;

    [Tooltip("Clearly visible on both light and dark squares.")]
    public Color HighLightcolor = new Color(0.95f, 0.88f, 0.25f, 1f);

    void Awake()
    {
        CacheSpriteRenderer();
    }

    void CacheSpriteRenderer()
    {
        if (spriteRenderer != null) return;
        if (renderer is SpriteRenderer sr)
            spriteRenderer = sr;
        else
            spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
    }

    /// <summary>Call from BoardManager after the tile theme color is chosen so highlight restore works.</summary>
    public void SetSquareVisual(SpriteRenderer sr, Color baseColor)
    {
        if (sr == null) return;
        spriteRenderer = sr;
        baseSquareColor = baseColor;
        highlighted = false;
        spriteRenderer.color = baseColor;
    }

    public void Highlighter(bool on)
    {
        CacheSpriteRenderer();
        if (spriteRenderer == null) return;

        highlighted = on;
        spriteRenderer.color = on ? HighLightcolor : baseSquareColor;
    }
}
