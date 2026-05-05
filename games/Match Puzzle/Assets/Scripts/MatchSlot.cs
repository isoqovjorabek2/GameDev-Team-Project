using System.Collections.Generic;
using UnityEngine;

public class MatchSlot : MonoBehaviour
{
    public static readonly List<MatchSlot> All = new();

    public SpriteRenderer Ghost;
    Match _occupying;

    public Match OccupyingMatch
    {
        get => _occupying;
        set
        {
            _occupying = value;
            if (Ghost != null) Ghost.enabled = (_occupying == null);
        }
    }

    public bool IsOccupied => _occupying != null;

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public static MatchSlot FindNearestEmpty(Vector3 worldPos, float maxDist, Quaternion preferredRot)
    {
        MatchSlot best = null;
        float bestDist = maxDist;
        foreach (var s in All)
        {
            if (s == null || s.IsOccupied) continue;
            if (Quaternion.Angle(s.transform.rotation, preferredRot) > 45f) continue;
            float d = Vector3.Distance(worldPos, s.transform.position);
            if (d < bestDist) { bestDist = d; best = s; }
        }
        if (best != null) return best;

        bestDist = maxDist;
        foreach (var s in All)
        {
            if (s == null || s.IsOccupied) continue;
            float d = Vector3.Distance(worldPos, s.transform.position);
            if (d < bestDist) { bestDist = d; best = s; }
        }
        return best;
    }
}
