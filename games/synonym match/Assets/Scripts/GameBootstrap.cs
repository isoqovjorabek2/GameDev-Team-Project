using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// Bootstraps the entire game on scene load — no manual scene setup required.
public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (GameManager.Instance != null) return;

        SetupCamera();
        EnsureEventSystem();

        var root = new GameObject("[MemoryMatch]");
        Object.DontDestroyOnLoad(root);

        root.AddComponent<SoundManager>();
        root.AddComponent<GameManager>();

        var canvas = BuildCanvas(root.transform);

        root.AddComponent<UIManager>().Initialize(canvas);
        root.AddComponent<CardGridManager>().Initialize(canvas);
    }

    private static void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            cam = go.AddComponent<Camera>();
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.07f, 0.04f, 0.15f);
        cam.orthographic = true;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        go.AddComponent<InputSystemUIInputModule>();
#else
        go.AddComponent<StandaloneInputModule>();
#endif
    }

    private static Canvas BuildCanvas(Transform parent)
    {
        var go = new GameObject("Canvas");
        go.transform.SetParent(parent, false);

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }
}
