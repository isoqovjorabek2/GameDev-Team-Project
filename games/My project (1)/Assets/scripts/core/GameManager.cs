using UnityEngine;

/// <summary>Orchestrates scene-level references.</summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] AtomPalette palette;
    [SerializeField] ElementRegistry elementRegistry;
    [SerializeField] DiscoveryManager discoveryManager;

    public AtomPalette Palette => palette;
    public ElementRegistry ElementRegistry => elementRegistry;
    public DiscoveryManager DiscoveryManager => discoveryManager;

    void Awake()
    {
        if (palette == null)
            palette = FindObjectOfType<AtomPalette>();
        if (discoveryManager == null)
            discoveryManager = FindObjectOfType<DiscoveryManager>();
    }
}
