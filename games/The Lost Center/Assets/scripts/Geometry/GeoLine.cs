using UnityEngine;

public class GeoLine 
{
    public Vector2 start;
    public Vector2 end;

    public GeoLine(Vector2 start, Vector2 end)
    {
        this.start = start;
        this.end = end;
    }

    public Vector2 Direction() => (end - start).normalized; // Returns the normalized direction vector of the line

    
}
