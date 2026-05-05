using System.Linq;
using CircuitSolver.Core;
using CircuitSolver.Data;
using CircuitSolver.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CircuitSolver.Managers
{
    /// <summary>
    /// Single entry point. Runs automatically after every scene load
    /// (via RuntimeInitializeOnLoadMethod) and spawns:
    ///   - GameManager & PuzzleManager
    ///   - EventSystem + Canvas
    ///   - Four screens (MainMenu / PuzzleSelect / Gameplay / Result)
    ///   - Default puzzle library if none was loaded from ScriptableObjects
    /// This means the project runs zero-config: press Play in SampleScene.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<GameBootstrapper>() != null) return;
            var go = new GameObject("[CircuitSolver]");
            go.AddComponent<GameBootstrapper>();
            DontDestroyOnLoad(go);
        }

        void Awake()
        {
            EnsureEventSystem();
            var canvasGo = CreateCanvas();
            var canvasRt = (RectTransform)canvasGo.transform;

            // Ensure EventSystem is fully initialized before proceeding
            if (EventSystem.current == null)
            {
                Debug.LogWarning("EventSystem not properly initialized after EnsureEventSystem");
            }

            var managersGo = new GameObject("Managers");
            managersGo.transform.SetParent(transform, false);
            var gm = managersGo.AddComponent<GameManager>();
            var pm = managersGo.AddComponent<PuzzleManager>();
            var ui = managersGo.AddComponent<UIManager>();

            // Load puzzles from Resources if present; else generate defaults.
            var loaded = Resources.LoadAll<CircuitPuzzle>("Puzzles");
            if (loaded == null || loaded.Length == 0)
                loaded = DefaultPuzzleLibrary.BuildAll().ToArray();
            gm.SetPuzzles(loaded);

            var main = CreateScreen<MainMenuScreen>(canvasRt, "MainMenu");
            main.Build(canvasRt);

            var sel = CreateScreen<PuzzleSelectScreen>(canvasRt, "PuzzleSelect");
            sel.Build(canvasRt);

            var play = CreateScreen<GameplayHUDScreen>(canvasRt, "Gameplay");
            play.Build(canvasRt);

            var res = CreateScreen<ResultScreen>(canvasRt, "Result");
            res.Build(canvasRt);

            ui.mainMenu = main;
            ui.puzzleSelect = sel;
            ui.gameplay = play;
            ui.result = res;

            gm.GoTo(ScreenId.MainMenu);
        }

        void EnsureEventSystem()
        {
            // Remove any preexisting EventSystem + modules so the scene
            // never inherits a broken / unconfigured input module from the
            // default Unity setup.
            foreach (var stale in FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
                Destroy(stale.gameObject);

            var es = new GameObject("EventSystem", typeof(EventSystem));
            es.transform.SetParent(transform, false);

            // Our own router reads the mouse directly and dispatches
            // pointer events — no Input System actions or legacy
            // StandaloneInputModule required.
            es.AddComponent<CodeOnlyUIRouter>();
        }

        GameObject CreateCanvas()
        {
            var go = new GameObject("Canvas",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return go;
        }

        T CreateScreen<T>(RectTransform parent, string name) where T : MonoBehaviour
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UIFactory.Stretch((RectTransform)go.transform, 0, 0, 0, 0);
            return go.AddComponent<T>();
        }
    }
}
