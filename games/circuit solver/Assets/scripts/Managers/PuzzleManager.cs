using System;
using System.Collections.Generic;
using CircuitSolver.Core;
using CircuitSolver.Data;
using UnityEngine;

namespace CircuitSolver.Managers
{
    /// <summary>
    /// Runtime controller for the currently-loaded puzzle.
    /// Tracks player-entered values for each hidden component, runs the
    /// solver on demand, and validates the solution against the puzzle's
    /// declared target with ±tolerance.
    /// </summary>
    public class PuzzleManager : MonoBehaviour
    {
        public static PuzzleManager Instance { get; private set; }

        public event Action<CircuitPuzzle> OnPuzzleStarted;
        public event Action<CircuitSolution, bool> OnSolutionChecked;
        public event Action<string, float> OnComponentValueChanged;
        public event Action<string> OnValidationError;

        public CircuitPuzzle Puzzle { get; private set; }
        public CircuitSolution LastSolution { get; private set; }
        public bool LastCheckWasCorrect { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartPuzzle(CircuitPuzzle puzzle)
        {
            Puzzle = puzzle;
            LastSolution = null;
            LastCheckWasCorrect = false;
            OnPuzzleStarted?.Invoke(puzzle);
        }

        public List<CircuitComponent> GetHiddenComponents()
        {
            var list = new List<CircuitComponent>();
            if (Puzzle == null) return list;
            foreach (var c in Puzzle.components) if (c.isHidden) list.Add(c);
            return list;
        }

        public void SetHiddenValue(string componentId, float value)
        {
            if (Puzzle == null) return;
            foreach (var c in Puzzle.components)
            {
                if (c.id != componentId) continue;
                if (!c.isHidden) return;
                c.value = value;
                OnComponentValueChanged?.Invoke(componentId, value);
                return;
            }
        }

        public void ResetHiddenValues()
        {
            if (Puzzle == null) return;
            foreach (var c in Puzzle.components)
                if (c.isHidden) c.value = 0f;
        }

        /// <summary>
        /// Runs the solver on the current puzzle and validates the target.
        /// Returns true iff the target value is met within tolerance AND
        /// every hidden component has a non-zero value set by the player.
        /// </summary>
        public bool CheckSolution()
        {
            if (Puzzle == null) return false;

            foreach (var c in Puzzle.components)
            {
                if (c.isHidden && c.value == 0f)
                {
                    OnValidationError?.Invoke($"Fill in a value for {c.id} first.");
                    LastCheckWasCorrect = false;
                    OnSolutionChecked?.Invoke(null, false);
                    return false;
                }
                if (c.type == ComponentType.Resistor && c.value < 0)
                {
                    OnValidationError?.Invoke($"{c.id}: resistance must be positive.");
                    LastCheckWasCorrect = false;
                    OnSolutionChecked?.Invoke(null, false);
                    return false;
                }
            }

            var solution = Core.CircuitSolver.Solve(Puzzle.components, Puzzle.groundNodeId);
            LastSolution = solution;

            if (!solution.IsSuccess)
            {
                OnValidationError?.Invoke($"Solver: {solution.status} — {solution.message}");
                LastCheckWasCorrect = false;
                OnSolutionChecked?.Invoke(solution, false);
                return false;
            }

            bool ok = EvaluateTarget(solution, Puzzle.target, out string reason);
            LastCheckWasCorrect = ok;
            if (!ok && !string.IsNullOrEmpty(reason))
                OnValidationError?.Invoke(reason);
            OnSolutionChecked?.Invoke(solution, ok);
            return ok;
        }

        public CircuitSolution SolvePeek()
        {
            if (Puzzle == null) return null;
            return Core.CircuitSolver.Solve(Puzzle.components, Puzzle.groundNodeId);
        }

        static bool EvaluateTarget(CircuitSolution sol, PuzzleTarget target, out string reason)
        {
            reason = "";
            double actual = 0, expected = target.expectedValue;
            string what = "";
            switch (target.kind)
            {
                case TargetKind.CurrentThroughComponent:
                    if (!sol.componentCurrents.TryGetValue(target.referenceId, out actual))
                    {
                        reason = $"Target references unknown component '{target.referenceId}'.";
                        return false;
                    }
                    what = $"I({target.referenceId})";
                    break;
                case TargetKind.VoltageAcrossComponent:
                    if (!sol.componentVoltages.TryGetValue(target.referenceId, out actual))
                    {
                        reason = $"Target references unknown component '{target.referenceId}'.";
                        return false;
                    }
                    what = $"V({target.referenceId})";
                    break;
                case TargetKind.VoltageAtNode:
                    if (!int.TryParse(target.referenceId, out int nid) ||
                        !sol.nodeVoltages.TryGetValue(nid, out actual))
                    {
                        reason = $"Target references unknown node '{target.referenceId}'.";
                        return false;
                    }
                    what = $"V(node {target.referenceId})";
                    break;
            }

            // Compare magnitudes so nodeA/nodeB orientation of the target
            // component is not a gotcha. Tolerance is relative unless the
            // expected value is essentially zero, in which case we fall
            // back on an absolute cutoff.
            double absExpected = Math.Abs(expected);
            double absActual = Math.Abs(actual);
            double rel = absExpected < 1e-6
                ? absActual
                : Math.Abs(absActual - absExpected) / absExpected;
            bool ok = rel * 100.0 <= target.tolerancePercent;
            if (!ok)
                reason = $"{what} = {absActual:0.###} (target {absExpected:0.###}, ±{target.tolerancePercent}%).";
            return ok;
        }
    }
}
