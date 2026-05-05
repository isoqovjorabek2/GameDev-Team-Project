using System;
using UnityEngine;
[RequireComponent(typeof(LineRenderer))]
public class CircleView : MonoBehaviour
{
    public ConstructionCircle data; // The construction circle this view represents
    private LineRenderer lineRenderer;
    public int segments = 100; // Number of segments to approximate the circle
    public float lineWidth = 0.1f; // Width of the circle line

    public void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
       
        // --- Set line renderer properties for better visibility--- //
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        
        // ---- Set the line renderer to use world space coordinates and a simple material --- //
        lineRenderer.useWorldSpace = true;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        lineRenderer.loop = true; // Close the circle
    }
   
   public void Initialize(ConstructionCircle constructionCircle)
    {
        data = constructionCircle;
        UpdateCirclePositions();
    }

    public void LateUpdate()
    {
        if(data != null) 
        UpdateCirclePositions(); // Continuously update the circle positions to match the construction circle's geographic circle
    }

    void UpdateCirclePositions()
    {
        data.UpdateCircle(); // Ensure the construction circle's GeoCircle is up to date
        lineRenderer.positionCount = segments; // Ensure we have the correct number of points
        
        Vector2 center = data.geoCircle.center.position; // Get the center position of the circle
        float radius = data.geoCircle.radius; // Get the radius of the circle

        for(int i=0; i<segments; i++)
        {
            float angle = i*Mathf.PI*2/segments; // Calculate the angle for this segment
            float x = center.x + Mathf.Cos(angle)*radius; // Calculate the x position of the segment point
            float y = center.y + Mathf.Sin(angle)*radius; // Calculate the y position of
            lineRenderer.SetPosition(i, new Vector3(x,y,0)); // Set the position of the segment point in the line renderer
        }
    }

}
