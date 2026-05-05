using System.Collections.Generic;
using UnityEngine;

public enum Segment { Top, TopLeft, TopRight, Middle, BottomLeft, BottomRight, Bottom }

public class SegmentDigit : MonoBehaviour
{
    public Dictionary<Segment, MatchSlot> Slots = new();

    static readonly Dictionary<int, HashSet<Segment>> Patterns = new()
    {
        { 0, new HashSet<Segment> { Segment.Top, Segment.TopLeft, Segment.TopRight, Segment.BottomLeft, Segment.BottomRight, Segment.Bottom } },
        { 1, new HashSet<Segment> { Segment.TopRight, Segment.BottomRight } },
        { 2, new HashSet<Segment> { Segment.Top, Segment.TopRight, Segment.Middle, Segment.BottomLeft, Segment.Bottom } },
        { 3, new HashSet<Segment> { Segment.Top, Segment.TopRight, Segment.Middle, Segment.BottomRight, Segment.Bottom } },
        { 4, new HashSet<Segment> { Segment.TopLeft, Segment.TopRight, Segment.Middle, Segment.BottomRight } },
        { 5, new HashSet<Segment> { Segment.Top, Segment.TopLeft, Segment.Middle, Segment.BottomRight, Segment.Bottom } },
        { 6, new HashSet<Segment> { Segment.Top, Segment.TopLeft, Segment.Middle, Segment.BottomLeft, Segment.BottomRight, Segment.Bottom } },
        { 7, new HashSet<Segment> { Segment.Top, Segment.TopRight, Segment.BottomRight } },
        { 8, new HashSet<Segment> { Segment.Top, Segment.TopLeft, Segment.TopRight, Segment.Middle, Segment.BottomLeft, Segment.BottomRight, Segment.Bottom } },
        { 9, new HashSet<Segment> { Segment.Top, Segment.TopLeft, Segment.TopRight, Segment.Middle, Segment.BottomRight, Segment.Bottom } },
    };

    public static HashSet<Segment> GetPattern(int digit)
    {
        return Patterns.TryGetValue(digit, out var p) ? p : new HashSet<Segment>();
    }

    public int GetCurrentDigit()
    {
        var on = new HashSet<Segment>();
        foreach (var kv in Slots)
            if (kv.Value != null && kv.Value.IsOccupied) on.Add(kv.Key);

        foreach (var kv in Patterns)
            if (kv.Value.SetEquals(on)) return kv.Key;

        return -1;
    }
}
