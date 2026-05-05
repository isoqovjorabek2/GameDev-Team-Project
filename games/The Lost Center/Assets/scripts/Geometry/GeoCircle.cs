using UnityEngine;

public class GeoCircle 
{
    public GeoPoint center;
    public float radius;

    public GeoCircle(GeoPoint c, float r)
    {
        center = c;
        radius = r;
    }
}
