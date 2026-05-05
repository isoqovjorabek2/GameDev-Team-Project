using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configuration for an infinite, self-recycling path.
/// Create via <c>Assets → Create → Math Runner → Path Config</c>.
/// </summary>
[CreateAssetMenu(menuName = "Math Runner/Path Config", fileName = "PathConfig", order = 1)]
public class PathConfigSO : ScriptableObject
{
    [Header("Segments")]
    [Tooltip("All segment variants the path can spawn. Weights control frequency.")]
    public List<PathSegmentSO> segments = new List<PathSegmentSO>();

    [Tooltip("Optional: always use this as the very first segment (e.g. a safe starting pad).")]
    public PathSegmentSO startSegment;

    [Header("Streaming")]
    [Tooltip("How many segments to keep active in front of the player.")]
    [Min(1)] public int segmentsAhead = 6;

    [Tooltip("World distance behind the player a segment must reach before it is recycled.")]
    [Min(0f)] public float recycleBuffer = 10f;

    [Header("Math gates")]
    [Tooltip("Segment used when a math gate should appear. Its prefab must have a MathGateSegment component. Leave empty to disable gates.")]
    public PathSegmentSO gateSegment;

    [Tooltip("Number of regular segments spawned between each math gate.")]
    [Min(1)] public int gateInterval = 4;

    [Header("World placement")]
    [Tooltip("Where the first segment starts (its front edge).")]
    public Vector3 startPosition = Vector3.zero;

    [Tooltip("Path forward direction. Will be normalized.")]
    public Vector3 forward = Vector3.forward;

    [Header("Randomness")]
    [Tooltip("Deterministic seed. 0 = fresh random each run.")]
    public int seed = 0;
}
