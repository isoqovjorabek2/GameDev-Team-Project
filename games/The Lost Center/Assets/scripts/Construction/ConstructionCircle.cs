using UnityEngine;
using UnityEngine.Rendering;

public class ConstructionCircle 
{
    public GeoCircle geoCircle;
    public ConstructionPoint centerPoint;
    public ConstructionPoint radiusPoint;

    public ConstructionCircle(ConstructionPoint center, ConstructionPoint radius)
    {
        centerPoint = center;
        radiusPoint = radius;
        float  radius1 =Vector2.Distance(centerPoint.geoPoint.position, radiusPoint.geoPoint.position);
        geoCircle = new GeoCircle(centerPoint.geoPoint.position, radius1);
    }

    public void UpdateCircle()
    {
        geoCircle.center = centerPoint.geoPoint.position;
        geoCircle.radius = Vector2.Distance(centerPoint.geoPoint.position, radiusPoint.geoPoint.position);  
    }
}
