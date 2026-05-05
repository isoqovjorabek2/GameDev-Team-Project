
using UnityEngine;

public class GeoPoint 
{
    public Vector2 position;

    public GeoPoint(Vector2 pos)
    {
      position = pos;
    }
    public static implicit operator GeoPoint(Vector2 v)
    {
        return new GeoPoint(v);
    }
}
