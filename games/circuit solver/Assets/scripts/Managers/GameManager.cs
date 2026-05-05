using System;
using System.Collections.Generic;
using CircuitSolver.Data;
using UnityEngine;

namespace CircuitSolver.Managers
{
    public enum ScreenId
    {
        MainMenu,
        PuzzleSelect,
        Gameplay,
        Result
    }

    /// <summary>
    /// Top-level singleton that owns the current puzzle list and drives the
    /// screen state machine. UI screens subscribe to OnScreenChanged and
    /// OnPuzzleLoaded events.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<ScreenId> OnScreenChanged;
        public event Action<CircuitPuzzle> OnPuzzleLoaded;

        public List<CircuitPuzzle> Puzzles { get; private set; } = new List<CircuitPuzzle>();
        public CircuitPuzzle ActivePuzzle { get; private set; }
        public ScreenId CurrentScreen { get; private set; } = ScreenId.MainMenu;
        public HashSet<int> CompletedPuzzleIds { get; private set; } = new HashSet<int>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetPuzzles(IEnumerable<CircuitPuzzle> puzzles)
        {
            Puzzles = new List<CircuitPuzzle>(puzzles);
            Puzzles.Sort((a, b) => a.puzzleId.CompareTo(b.puzzleId));
        }

        public void GoTo(ScreenId screen)
        {
            CurrentScreen = screen;
            OnScreenChanged?.Invoke(screen);
        }

        public void LoadPuzzle(CircuitPuzzle puzzle)
        {
            ActivePuzzle = puzzle.CloneForPlay();
            OnPuzzleLoaded?.Invoke(ActivePuzzle);
            GoTo(ScreenId.Gameplay);
        }

        public void LoadPuzzleByIndex(int index)
        {
            if (index < 0 || index >= Puzzles.Count) return;
            LoadPuzzle(Puzzles[index]);
        }

        public void MarkCompleted(int puzzleId) => CompletedPuzzleIds.Add(puzzleId);

        public bool IsCompleted(int puzzleId) => CompletedPuzzleIds.Contains(puzzleId);
    }
}
