using UnityEngine;

public class ConstructionLine 
{
    public int id;
    public ConstructionPoint start;
    public ConstructionPoint end;

    public GeoLine geoLine;

    public ConstructionLine(int id, ConstructionPoint start, ConstructionPoint end)
    {
        this.id = id;
        this.start = start;
        this.end = end;

        geoLine = new GeoLine(start.geoPoint.position, end.geoPoint.position); // Create a GeoLine based on the positions of the start and end construction points
    }

    
}
