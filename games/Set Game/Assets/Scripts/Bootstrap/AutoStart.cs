using UnityEngine;
using SetGame.Bootstrap;

/// <summary>
/// Auto-creates the Bootstrap root so the game runs from ANY scene with zero manual setup.
/// Just open Unity, open any scene, and press Play.
/// </summary>
public class AutoStart
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Launch()
    {
        // Don't duplicate if a Bootstrap already exists in the scene
        if (Object.FindAnyObjectByType<GameBootstrap>() != null) return;

        // Create bootstrap with proper initialization
        var go = new GameObject("Bootstrap");
        var bootstrap = go.AddComponent<GameBootstrap>();
        // Call Awake manually to ensure proper initialization order
        var awakeMethod = typeof(GameBootstrap).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        awakeMethod?.Invoke(bootstrap, null);
        Object.DontDestroyOnLoad(go);
    }
}
