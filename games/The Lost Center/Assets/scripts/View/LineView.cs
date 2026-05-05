using UnityEngine;
using UnityEngine.Rendering;
[RequireComponent(typeof(LineRenderer))]
public class LineView : MonoBehaviour
{

    public ConstructionLine data; // The construction line this view represents
    private LineRenderer lineRenderer;
    public float renderExtent = 200f; // How much to extend the line beyond the start and end points for better visibility
    public float lineWidth = 0.1f; // Width of the line
    public void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2; // We need two points to draw a line

        // --- Set line renderer properties for better visibility--- //
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        
        // ---- Set the line renderer to use world space coordinates and a simple material --- //
        lineRenderer.useWorldSpace = true;
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
    }

    public void Initialize(ConstructionLine constructionLine)
    {
        data = constructionLine;
        UpdateLinePositions();
    }
    public void LateUpdate()
    {
        UpdateLinePositions(); // Continuously update the line positions to match the construction line's geographic line
    }

    public void UpdateLinePositions()
    {
        if (data == null) return;

        Vector2 start = data.geoLine.start;
        Vector2 end = data.geoLine.end;
        Vector2 direction = (end - start).normalized;

        // Extend the line in both directions for better visibility
        Vector2 extendedStart = start - direction * renderExtent;
        Vector2 extendedEnd = end + direction * renderExtent;

        lineRenderer.SetPosition(0, extendedStart);
        lineRenderer.SetPosition(1, extendedEnd);
    }



}
