using UnityEngine;
using System.Collections.Generic;

public static class GeometricConstraints
{
    public static List<Vector2> FindLineLineIntersection(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End)
    {
        List<Vector2> intersections = new List<Vector2>();

        float x1 = line1Start.x, y1 = line1Start.y;
        float x2 = line1End.x, y2 = line1End.y;
        float x3 = line2Start.x, y3 = line2Start.y;
        float x4 = line2End.x, y4 = line2End.y;

        float denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);

        if (Mathf.Abs(denom) < 0.0001f)
        {
            return intersections;
        }

        float ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / denom;
        float ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / denom;

        if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
        {
            Vector2 intersection = new Vector2(x1 + ua * (x2 - x1), y1 + ua * (y2 - y1));
            intersections.Add(intersection);
        }

        return intersections;
    }

    public static List<Vector2> FindLineCircleIntersection(Vector2 lineStart, Vector2 lineEnd, Vector2 circleCenter, float circleRadius)
    {
        List<Vector2> intersections = new List<Vector2>();

        Vector2 d = lineEnd - lineStart;
        Vector2 f = lineStart - circleCenter;

        float a = Vector2.Dot(d, d);
        float b = 2 * Vector2.Dot(f, d);
        float c = Vector2.Dot(f, f) - circleRadius * circleRadius;

        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            return intersections;
        }

        discriminant = Mathf.Sqrt(discriminant);

        float t1 = (-b - discriminant) / (2 * a);
        float t2 = (-b + discriminant) / (2 * a);

        if (t1 >= 0 && t1 <= 1)
        {
            intersections.Add(lineStart + t1 * d);
        }

        if (t2 >= 0 && t2 <= 1)
        {
            intersections.Add(lineStart + t2 * d);
        }

        return intersections;
    }

    public static List<Vector2> FindCircleCircleIntersection(Vector2 circle1Center, float circle1Radius, Vector2 circle2Center, float circle2Radius)
    {
        List<Vector2> intersections = new List<Vector2>();

        float d = Vector2.Distance(circle1Center, circle2Center);

        if (d > circle1Radius + circle2Radius || d < Mathf.Abs(circle1Radius - circle2Radius) || d == 0)
        {
            return intersections;
        }

        float a = (circle1Radius * circle1Radius - circle2Radius * circle2Radius + d * d) / (2 * d);
        float h = Mathf.Sqrt(Mathf.Max(0, circle1Radius * circle1Radius - a * a));

        Vector2 p2 = circle1Center + a * (circle2Center - circle1Center) / d;

        float x3 = p2.x + h * (circle2Center.y - circle1Center.y) / d;
        float y3 = p2.y - h * (circle2Center.x - circle1Center.x) / d;

        float x4 = p2.x - h * (circle2Center.y - circle1Center.y) / d;
        float y4 = p2.y + h * (circle2Center.x - circle1Center.x) / d;

        intersections.Add(new Vector2(x3, y3));

        if (h > 0.0001f)
        {
            intersections.Add(new Vector2(x4, y4));
        }

        return intersections;
    }

    public static List<Vector2> FindAllIntersections(List<ConstructionLine> lines, List<ConstructionCircle> circles)
    {
        List<Vector2> allIntersections = new List<Vector2>();

        for (int i = 0; i < lines.Count; i++)
        {
            for (int j = i + 1; j < lines.Count; j++)
            {
                var intersections = FindLineLineIntersection(
                    lines[i].geoLine.start, lines[i].geoLine.end,
                    lines[j].geoLine.start, lines[j].geoLine.end
                );
                allIntersections.AddRange(intersections);
            }
        }

        foreach (var line in lines)
        {
            foreach (var circle in circles)
            {
                var intersections = FindLineCircleIntersection(
                    line.geoLine.start, line.geoLine.end,
                    circle.geoCircle.center.position, circle.geoCircle.radius
                );
                allIntersections.AddRange(intersections);
            }
        }

        for (int i = 0; i < circles.Count; i++)
        {
            for (int j = i + 1; j < circles.Count; j++)
            {
                var intersections = FindCircleCircleIntersection(
                    circles[i].geoCircle.center.position, circles[i].geoCircle.radius,
                    circles[j].geoCircle.center.position, circles[j].geoCircle.radius
                );
                allIntersections.AddRange(intersections);
            }
        }

        return allIntersections;
    }

    public static Vector2 FindClosestIntersection(Vector2 position, List<Vector2> intersections, float maxDistance = 0.5f)
    {
        Vector2 closest = Vector2.zero;
        float minDist = maxDistance;

        foreach (var intersection in intersections)
        {
            float dist = Vector2.Distance(position, intersection);
            if (dist < minDist)
            {
                minDist = dist;
                closest = intersection;
            }
        }

        return minDist < maxDistance ? closest : Vector2.zero;
    }
}