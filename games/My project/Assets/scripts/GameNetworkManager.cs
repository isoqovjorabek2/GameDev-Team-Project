using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Host/Client bootstrap for the Math Runner prototype.
///
/// Flow:
///  1. User clicks Host or Client.
///  2. Server tracks connected clients (host counts as one of them).
///  3. When exactly <see cref="requiredPlayers"/> clients are connected,
///     the server spawns a player prefab for every client (side-by-side
///     on the X axis) and fires <see cref="OnGameStarted"/>.
///  4. Clients that try to connect after the game has started are
///     rejected.
///
/// Attach this to a scene GameObject together with a NetworkManager +
/// UnityTransport, and assign the player prefab in the inspector.
/// </summary>
public class GameNetworkManager : MonoBehaviour
{
    [Header("Player spawning")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField, Min(1)] private int requiredPlayers = 2;
    [SerializeField] private Vector3 spawnCenter = Vector3.zero;
    [SerializeField] private float spawnSpacing = 1.5f;

    /// <summary>Server-only: fired once both required clients are in and players have been spawned.</summary>
    public static event System.Action OnGameStarted;

    /// <summary>Fired on the server when lobby connection count changes.</summary>
    public static event System.Action<string> OnStatusChanged;

    private readonly List<ulong> _connectedClients = new List<ulong>();
    private bool _gameStarted;

    void Start()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null)
        {
            Debug.LogError("[GameNetworkManager] No NetworkManager in scene.");
            return;
        }

        nm.ConnectionApprovalCallback += HandleConnectionApproval;
        nm.OnServerStarted += HandleServerStarted;
        nm.OnClientConnectedCallback += HandleClientConnected;
        nm.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    void OnDestroy()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null) return;
        nm.ConnectionApprovalCallback -= HandleConnectionApproval;
        nm.OnServerStarted -= HandleServerStarted;
        nm.OnClientConnectedCallback -= HandleClientConnected;
        nm.OnClientDisconnectCallback -= HandleClientDisconnected;
    }

    // Reject late joiners once the game has started or the lobby is full.
    private void HandleConnectionApproval(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        bool lobbyFull = _connectedClients.Count >= requiredPlayers;
        bool accept = !_gameStarted && !lobbyFull;

        response.Approved = accept;
        response.CreatePlayerObject = false; // we spawn manually
        response.Pending = false;
        if (!accept)
        {
            response.Reason = _gameStarted ? "Game already started." : "Lobby is full.";
        }
    }

    private void HandleServerStarted()
    {
        _connectedClients.Clear();
        _gameStarted = false;
    }

    private void HandleClientConnected(ulong clientId)
    {
        var nm = NetworkManager.Singleton;
        if (!nm.IsServer) return;

        if (!_connectedClients.Contains(clientId))
        {
            _connectedClients.Add(clientId);
        }

        Debug.Log($"[GameNetworkManager] Client {clientId} connected " +
                  $"({_connectedClients.Count}/{requiredPlayers}).");

        OnStatusChanged?.Invoke($"Players connected: {_connectedClients.Count} / {requiredPlayers}");

        if (!_gameStarted && _connectedClients.Count >= requiredPlayers)
        {
            StartGame();
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        _connectedClients.Remove(clientId);

        if (!_gameStarted)
        {
            Debug.Log($"[GameNetworkManager] Client {clientId} disconnected " +
                      $"({_connectedClients.Count}/{requiredPlayers}).");
        }
    }

    private void StartGame()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[GameNetworkManager] Player prefab is not assigned.");
            return;
        }

        _gameStarted = true;
        Debug.Log("[GameNetworkManager] All players connected, starting game.");

        for (int i = 0; i < _connectedClients.Count; i++)
        {
            SpawnPlayer(_connectedClients[i], i, _connectedClients.Count);
        }

        OnGameStarted?.Invoke();
    }

    private void SpawnPlayer(ulong clientId, int index, int total)
    {
        Vector3 pos = GetSpawnPosition(index, total);
        GameObject go = Instantiate(playerPrefab, pos, Quaternion.identity);
        var netObj = go.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("[GameNetworkManager] Player prefab is missing a NetworkObject component.");
            Destroy(go);
            return;
        }

        netObj.SpawnAsPlayerObject(clientId, true);
    }

    private Vector3 GetSpawnPosition(int index, int total)
    {
        // Evenly distribute players on the X axis around spawnCenter.
        float offset = (index - (total - 1) * 0.5f) * spawnSpacing;
        return spawnCenter + new Vector3(offset, 0f, 0f);
    }

}
