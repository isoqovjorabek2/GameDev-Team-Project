using UnityEngine;

public class ConstructionPoint 
{
    public int id;
    public GeoPoint geoPoint;
    
    public bool isDerived; // Indicates if the construction point is derived from another point
    private int v;
    private Vector2 position;

    public ConstructionPoint(int v, Vector2 position)
    {
        this.v = v;
        this.position = position;
    }

    public ConstructionPoint(int id, GeoPoint geoPoint, bool isDerived = false)
    {
        this.id = id;
        this.geoPoint = geoPoint;
        this.isDerived = isDerived;
    }
}
