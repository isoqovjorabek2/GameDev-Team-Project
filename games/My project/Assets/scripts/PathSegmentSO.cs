using UnityEngine;

/// <summary>
/// Defines a single type of path segment usable by <see cref="PathManager"/>.
/// Create via <c>Assets → Create → Math Runner → Path Segment</c>.
/// </summary>
[CreateAssetMenu(menuName = "Math Runner/Path Segment", fileName = "PathSegment", order = 0)]
public class PathSegmentSO : ScriptableObject
{
    [Tooltip("Prefab representing this segment. Its pivot should be at the segment's start edge.")]
    public GameObject prefab;

    [Tooltip("Length of the segment along the path's forward axis (in world units).")]
    [Min(0.01f)] public float length = 10f;

    [Tooltip("Relative spawn weight. Higher = more likely to be chosen.")]
    [Min(0f)] public float weight = 1f;
}
