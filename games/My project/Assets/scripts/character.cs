using System;
using Unity.Netcode;
using UnityEngine;

public class character : NetworkBehaviour
{
    /// <summary>Fires on the owning client when the player passes through a gate.</summary>
    public event Action<bool> OnAnswerGiven;

    /// <summary>Fires on the owning client whenever the boost state changes.</summary>
    public event Action<bool> OnBoostChanged;

    [Header("Visuals / refs")]
    public Animator animator;
    public Material material;

    [Header("Lane movement")]
    [Tooltip("Total number of lanes. 3 = left / center / right.")]
    [SerializeField, Min(1)] private int laneCount = 3;

    [Tooltip("Distance between adjacent lanes, in world units.")]
    [SerializeField, Min(0.01f)] private float laneWidth = 1.5f;

    [Tooltip("How quickly the character eases toward the current lane's X.")]
    [SerializeField, Min(0.01f)] private float laneLerpSpeed = 12f;

    [Header("Running")]
    [Tooltip("Base forward speed (units/second).")]
    [SerializeField, Min(0f)] private float baseSpeed = 5f;

    [Tooltip("Forward speed while boosted (units/second).")]
    [SerializeField, Min(0f)] private float boostSpeed = 9f;

    [Tooltip("How long one correct-answer boost lasts, in seconds.")]
    [SerializeField, Min(0.01f)] private float boostDuration = 2f;

    [Header("Camera (local owner)")]
    [Tooltip("Offset from the character where the main camera sits (world-space).")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 3.4f, -2.2f);

    [Tooltip("Look-at point is this many units above the character pivot. Higher = camera tilts up (better for math text ahead).")]
    [SerializeField, Min(0f)] private float cameraLookAtHeight = 2f;

    private Rigidbody[] _ragdollRigidbodies;
    private Camera _mainCamera;

    // Captured on spawn so movement is relative to the spawn pose.
    private float _baseX;
    private float _baseZ;

    // Server-only boost timer.
    private float _serverBoostRemaining;

    // Server-authoritative state replicated to every client.
    private readonly NetworkVariable<int> _currentLane = new NetworkVariable<int>(
        1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<float> _forwardDistance = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<bool> _isBoosting = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<int> _correctAnswers = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    /// <summary>Total forward distance this player has travelled (replicated).</summary>
    public float ForwardDistance => _forwardDistance.Value;

    /// <summary>Is this player currently speed-boosting?</summary>
    public bool IsBoosting => _isBoosting.Value;

    /// <summary>How many correct answers this player has given.</summary>
    public int CorrectAnswers => _correctAnswers.Value;

    void Awake()
    {
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
        DisableRagdoll();
    }

    public override void OnNetworkSpawn()
    {
        _baseX = transform.position.x;
        _baseZ = transform.position.z;

        if (IsServer)
        {
            _currentLane.Value = laneCount / 2;
        }

        if (IsOwner)
        {
            _mainCamera = Camera.main;
            if (PathManager.Local != null)
            {
                PathManager.Local.SetTarget(transform);
            }

            _isBoosting.OnValueChanged += (_, newVal) =>
            {
                OnBoostChanged?.Invoke(newVal);
                AudioManager.Instance?.Play(newVal ? AudioManager.SFX.Boost : AudioManager.SFX.BoostEnd);
            };
        }
    }

    void Update()
    {
        HandleOwnerInput();
        if (IsServer) ServerTick();
        ApplyMovement();
    }

    void LateUpdate()
    {
        if (!IsOwner || _mainCamera == null) return;
        CameraFollow();
    }

    // --- Input (owner only) -------------------------------------------------

    private void HandleOwnerInput()
    {
        if (!IsOwner) return;
        if (IsGameOver) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            RequestLaneChangeServerRpc(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            RequestLaneChangeServerRpc(1);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            EnableRagdollServerRpc();
        }
    }

    // --- Server tick --------------------------------------------------------

    private void ServerTick()
    {
        if (IsGameOver) return;

        if (_serverBoostRemaining > 0f)
        {
            _serverBoostRemaining -= Time.deltaTime;
            if (_serverBoostRemaining <= 0f)
            {
                _serverBoostRemaining = 0f;
                _isBoosting.Value = false;
            }
        }

        float speed = _serverBoostRemaining > 0f ? boostSpeed : baseSpeed;
        _forwardDistance.Value += speed * Time.deltaTime;
    }

    // --- Movement (all clients) --------------------------------------------

    private void ApplyMovement()
    {
        float targetX = _baseX + (_currentLane.Value - (laneCount - 1) * 0.5f) * laneWidth;

        Vector3 p = transform.position;
        p.x = Mathf.Lerp(p.x, targetX, Time.deltaTime * laneLerpSpeed);
        p.z = _baseZ + _forwardDistance.Value;
        transform.position = p;
    }

    private void CameraFollow()
    {
        Vector3 lookAt = transform.position + Vector3.up * cameraLookAtHeight;
        _mainCamera.transform.position = transform.position + cameraOffset;
        _mainCamera.transform.LookAt(lookAt);
    }

    // --- Lane change RPC ---------------------------------------------------

    [ServerRpc]
    private void RequestLaneChangeServerRpc(int delta, ServerRpcParams rpcParams = default)
    {
        if (IsGameOver) return;
        int newLane = Mathf.Clamp(_currentLane.Value + delta, 0, laneCount - 1);
        if (newLane != _currentLane.Value)
        {
            _currentLane.Value = newLane;
        }
    }

    // --- Gate answer --------------------------------------------------------

    /// <summary>Called by a local <see cref="MathGateSegment"/> when the owner picks a side.</summary>
    public void ReportGateAnswer(bool correct)
    {
        if (!IsOwner) return;
        OnAnswerGiven?.Invoke(correct);
        ReportGateAnswerServerRpc(correct);
    }

    [ServerRpc]
    private void ReportGateAnswerServerRpc(bool correct, ServerRpcParams rpcParams = default)
    {
        if (IsGameOver) return;
        if (!correct) return;

        _correctAnswers.Value++;
        _serverBoostRemaining = Mathf.Max(_serverBoostRemaining, boostDuration);
        _isBoosting.Value = true;
    }

    // --- Ragdoll ------------------------------------------------------------

    [ServerRpc]
    private void EnableRagdollServerRpc()
    {
        EnableRagdollClientRpc();
    }

    [ClientRpc]
    private void EnableRagdollClientRpc()
    {
        EnableRagdoll();
        if (animator != null) animator.enabled = false;
    }

    private void DisableRagdoll()
    {
        foreach (var rb in _ragdollRigidbodies)
        {
            rb.isKinematic = true;
        }
    }

    private void EnableRagdoll()
    {
        foreach (var rb in _ragdollRigidbodies)
        {
            rb.isKinematic = false;
        }
    }

    // --- Helpers ------------------------------------------------------------

    private bool IsGameOver =>
        GameStateController.Instance != null && GameStateController.Instance.IsGameOver;
}
