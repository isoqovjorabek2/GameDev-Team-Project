using UnityEngine;
using System.Collections.Generic;

public static class ConstructionUtils
{
    public static ConstructionPoint FindClosestPoint(Vector2 pos, List<ConstructionPoint> points, float maxDistance = 0.3f)
    {
        ConstructionPoint best = null;
        float minDist = maxDistance;

        foreach (var p in points)
        {
            float d = Vector2.Distance(p.geoPoint.position, pos);
            if (d < minDist)
            {
                minDist = d;
                best = p;
            }
        }

        return best;
    }

    public static Vector2 SnapToGrid(Vector2 position, float gridSize = 0.5f)
    {
        float x = Mathf.Round(position.x / gridSize) * gridSize;
        float y = Mathf.Round(position.y / gridSize) * gridSize;
        return new Vector2(x, y);
    }

    public static bool IsValidConstruction(ConstructionPoint point)
    {
        return point != null && point.geoPoint != null;
    }

    public static bool IsValidConstruction(ConstructionLine line)
    {
        return line != null && line.start != null && line.end != null &&
               line.start != line.end;
    }

    public static bool IsValidConstruction(ConstructionCircle circle)
    {
        return circle != null && circle.centerPoint != null && circle.radiusPoint != null &&
               circle.centerPoint != circle.radiusPoint;
    }
}