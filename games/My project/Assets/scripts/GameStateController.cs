using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Scene-level networked game state. Must live on its own GameObject with a
/// <c>NetworkObject</c> component (NetworkManager and NetworkObject cannot
/// share a GameObject).
///
/// Responsibilities:
///  - Track whether the game is over (replicated to all clients).
///  - On the server, monitor forward-distance gap between players and end
///    the game when it exceeds <see cref="maxBoostGap"/> boost-units.
/// </summary>
public class GameStateController : NetworkBehaviour
{
    public static GameStateController Instance { get; private set; }

    [Header("Game over condition")]
    [Tooltip("World distance that represents ONE boost unit.")]
    [SerializeField, Min(0.01f)] private float boostUnitDistance = 8f;

    [Tooltip("Game ends when |leader.distance - tail.distance| exceeds this many boost units.")]
    [SerializeField, Min(1)] private int maxBoostGap = 5;

    private readonly NetworkVariable<bool> _isGameOver = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<int> _winnerClientId = new NetworkVariable<int>(
        -1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public bool IsGameOver => _isGameOver.Value;
    public int WinnerClientId => _winnerClientId.Value;
    public float GameOverDistance => boostUnitDistance * maxBoostGap;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnDestroy()
    {
        if (Instance == this) Instance = null;
        base.OnDestroy();
    }

    void Update()
    {
        if (!IsServer || _isGameOver.Value) return;

        var nm = NetworkManager.Singleton;
        if (nm == null) return;

        float minDist = float.MaxValue;
        float maxDist = float.MinValue;
        ulong leader = 0;
        ulong tail = 0;
        int sampled = 0;

        foreach (var id in nm.ConnectedClientsIds)
        {
            if (!nm.ConnectedClients.TryGetValue(id, out var client)) continue;
            var po = client.PlayerObject;
            if (po == null) continue;

            var ch = po.GetComponent<character>();
            if (ch == null) continue;

            float d = ch.ForwardDistance;
            if (d < minDist) { minDist = d; tail = id; }
            if (d > maxDist) { maxDist = d; leader = id; }
            sampled++;
        }

        if (sampled < 2) return;

        if (maxDist - minDist > GameOverDistance)
        {
            _winnerClientId.Value = (int)leader;
            _isGameOver.Value = true;
            Debug.Log($"[GameStateController] Game over. Winner client {leader}, loser client {tail}.");
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _isGameOver.OnValueChanged += (_, isOver) =>
        {
            if (!isOver) return;
            string winner = _winnerClientId.Value >= 0
                ? $"Winner: Client {_winnerClientId.Value}"
                : "";
            UIManager.Instance?.ShowGameOver(winner);
            AudioManager.Instance?.Play(AudioManager.SFX.GameOver);
        };
    }
}
