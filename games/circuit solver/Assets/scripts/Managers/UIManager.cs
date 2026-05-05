using CircuitSolver.UI;
using UnityEngine;

namespace CircuitSolver.Managers
{
    /// <summary>
    /// Holds references to the four main screens and toggles their visibility
    /// whenever GameManager raises OnScreenChanged.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        public MainMenuScreen mainMenu;
        public PuzzleSelectScreen puzzleSelect;
        public GameplayHUDScreen gameplay;
        public ResultScreen result;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnScreenChanged += HandleScreenChanged;
            HandleScreenChanged(ScreenId.MainMenu);
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnScreenChanged -= HandleScreenChanged;
        }

        void HandleScreenChanged(ScreenId id)
        {
            if (mainMenu) mainMenu.gameObject.SetActive(id == ScreenId.MainMenu);
            if (puzzleSelect) puzzleSelect.gameObject.SetActive(id == ScreenId.PuzzleSelect);
            if (gameplay) gameplay.gameObject.SetActive(id == ScreenId.Gameplay);
            if (result) result.gameObject.SetActive(id == ScreenId.Result);

            if (id == ScreenId.PuzzleSelect && puzzleSelect) puzzleSelect.Rebuild();
            if (id == ScreenId.Gameplay && gameplay) gameplay.Rebuild();
            if (id == ScreenId.Result && result) result.Rebuild();
        }
    }
}
