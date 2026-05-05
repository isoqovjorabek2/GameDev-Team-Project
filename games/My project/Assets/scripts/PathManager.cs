using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and recycles path segments around a moving target (the local
/// player) to create an infinite runner track.
///
/// Design:
///  - Segments are described by <see cref="PathSegmentSO"/> and picked
///    from a weighted list defined in <see cref="PathConfigSO"/>.
///  - Segments are pooled per-SO and reused; no Instantiate after warmup.
///  - The path is one-dimensional along <c>config.forward</c>. Segment
///    "end distance" is tracked along that axis.
///  - Each client runs its own PathManager following its local player.
///    The visuals do not need to be network-synchronized unless gameplay
///    depends on them being byte-identical between clients.
/// </summary>
public class PathManager : MonoBehaviour
{
    public static PathManager Local { get; private set; }

    [SerializeField] private PathConfigSO config;
    [SerializeField] private Transform target;

    private struct ActiveSegment
    {
        public PathSegmentSO data;
        public GameObject instance;
        public float endDistance;
    }

    private readonly Queue<ActiveSegment> _active = new Queue<ActiveSegment>();
    private readonly Dictionary<PathSegmentSO, Queue<GameObject>> _pool =
        new Dictionary<PathSegmentSO, Queue<GameObject>>();

    private Vector3 _forward;
    private float _nextSpawnDistance;
    private float _totalWeight;
    private System.Random _rng;
    private bool _initialized;

    private int _segmentIndex;
    private int _segmentsSinceGate;

    void Awake()
    {
        
        Local = this;
    }

    void OnDestroy()
    {
        if (Local == this) Local = null;
    }

    void Start()
    {
        if (!Validate()) return;
        Initialize();
        for (int i = 0; i < config.segmentsAhead; i++)
        {
            SpawnNextSegment();
        }
    }

    void Update()
    {
        if (!_initialized || target == null) return;

        float targetDist = Vector3.Dot(target.position - config.startPosition, _forward);
        float recycleThreshold = targetDist - config.recycleBuffer;

        // Recycle any segment whose far edge is already behind the threshold.
        while (_active.Count > 0 && _active.Peek().endDistance < recycleThreshold)
        {
            var seg = _active.Dequeue();
            ReturnToPool(seg);
            SpawnNextSegment();
        }
    }

    /// <summary>Assign the transform the path should follow (usually the local player).</summary>
    public void SetTarget(Transform t)
    {
        target = t;
    }

    /// <summary>Reset the path back to its starting state (clears all active segments).</summary>
    public void ResetPath()
    {
        while (_active.Count > 0)
        {
            ReturnToPool(_active.Dequeue());
        }
        _nextSpawnDistance = 0f;
        _initialized = false;
        if (Validate())
        {
            Initialize();
            for (int i = 0; i < config.segmentsAhead; i++)
            {
                SpawnNextSegment();
            }
        }
    }

    private bool Validate()
    {
        if (config == null)
        {
            Debug.LogError("[PathManager] No PathConfigSO assigned.");
            return false;
        }
        if (config.segments == null || config.segments.Count == 0)
        {
            Debug.LogError("[PathManager] PathConfigSO has no segments.");
            return false;
        }
        return true;
    }

    private void Initialize()
    {
        _forward = config.forward.sqrMagnitude > 0.0001f
            ? config.forward.normalized
            : Vector3.forward;

        _rng = config.seed == 0
            ? new System.Random()
            : new System.Random(config.seed);

        _totalWeight = 0f;
        foreach (var s in config.segments)
        {
            if (s != null && s.prefab != null) _totalWeight += s.weight;
        }

        _nextSpawnDistance = 0f;
        _segmentIndex = 0;
        _segmentsSinceGate = 0;
        _initialized = true;
    }

    private void SpawnNextSegment()
    {
        bool isGate;
        PathSegmentSO data = ChooseNextSegment(out isGate);
        if (data == null || data.prefab == null) return;

        GameObject go = GetFromPool(data);
        go.transform.SetParent(transform, worldPositionStays: false);
        go.transform.position = config.startPosition + _forward * _nextSpawnDistance;
        go.transform.rotation = Quaternion.LookRotation(_forward, Vector3.up);
        go.SetActive(true);

        if (isGate)
        {
            var gate = go.GetComponent<MathGateSegment>();
            if (gate != null)
            {
                int baseSeed = config.seed == 0 ? 1 : config.seed;
                int gateSeed = unchecked(baseSeed ^ (_segmentIndex * 73856093));
                gate.Initialize(gateSeed);
            }
            _segmentsSinceGate = 0;
        }
        else
        {
            _segmentsSinceGate++;
        }

        _nextSpawnDistance += data.length;
        _segmentIndex++;

        _active.Enqueue(new ActiveSegment
        {
            data = data,
            instance = go,
            endDistance = _nextSpawnDistance
        });
    }

    private PathSegmentSO ChooseNextSegment(out bool isGate)
    {
        isGate = false;

        if (_active.Count == 0 && config.startSegment != null)
        {
            return config.startSegment;
        }

        if (config.gateSegment != null
            && config.gateSegment.prefab != null
            && _segmentsSinceGate >= config.gateInterval)
        {
            isGate = true;
            return config.gateSegment;
        }

        return PickSegment();
    }

    private PathSegmentSO PickSegment()
    {
        if (_totalWeight <= 0f) return null;

        double r = _rng.NextDouble() * _totalWeight;
        foreach (var s in config.segments)
        {
            if (s == null || s.prefab == null) continue;
            r -= s.weight;
            if (r <= 0.0) return s;
        }
        // Fallback: last valid segment.
        for (int i = config.segments.Count - 1; i >= 0; i--)
        {
            if (config.segments[i] != null && config.segments[i].prefab != null)
                return config.segments[i];
        }
        return null;
    }

    private GameObject GetFromPool(PathSegmentSO data)
    {
        if (_pool.TryGetValue(data, out var q) && q.Count > 0)
        {
            return q.Dequeue();
        }
        return Instantiate(data.prefab);
    }

    private void ReturnToPool(ActiveSegment seg)
    {
        if (seg.instance == null) return;
        seg.instance.SetActive(false);

        if (!_pool.TryGetValue(seg.data, out var q))
        {
            q = new Queue<GameObject>();
            _pool[seg.data] = q;
        }
        q.Enqueue(seg.instance);
    }
}
