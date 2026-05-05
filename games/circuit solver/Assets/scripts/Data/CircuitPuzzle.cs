using System.Collections.Generic;
using UnityEngine;

namespace CircuitSolver.Data
{
    /// <summary>
    /// ScriptableObject describing a complete circuit puzzle: components,
    /// their electrical connectivity (nodes), and the target behavior the
    /// player must achieve.
    /// </summary>
    [CreateAssetMenu(fileName = "Puzzle", menuName = "Circuit Solver/Circuit Puzzle")]
    public class CircuitPuzzle : ScriptableObject
    {
        [Header("Meta")]
        public int puzzleId = 1;
        public string title = "Project 01";
        [TextArea(2, 4)] public string description = "Find the missing value.";
        public DifficultyTier difficulty = DifficultyTier.Intro;

        [Header("Topology")]
        [Tooltip("All components. Node ids are plain integers; node 0 is ground by convention.")]
        public List<CircuitComponent> components = new List<CircuitComponent>();

        [Tooltip("Optional explicit node descriptors (positions, labels). Builder fills these if empty.")]
        public List<CircuitNode> nodes = new List<CircuitNode>();

        [Tooltip("Node id used as ground (0V reference). Must exist among component terminals.")]
        public int groundNodeId = 0;

        [Header("Goal")]
        public PuzzleTarget target = new PuzzleTarget();

        [Header("Hints")]
        [TextArea(2, 4)] public string hintText = "";

        public CircuitPuzzle CloneForPlay()
        {
            var copy = CreateInstance<CircuitPuzzle>();
            copy.puzzleId = puzzleId;
            copy.title = title;
            copy.description = description;
            copy.difficulty = difficulty;
            copy.groundNodeId = groundNodeId;
            copy.target = new PuzzleTarget
            {
                kind = target.kind,
                referenceId = target.referenceId,
                expectedValue = target.expectedValue,
                tolerancePercent = target.tolerancePercent
            };
            copy.hintText = hintText;
            copy.components = new List<CircuitComponent>(components.Count);
            foreach (var c in components) copy.components.Add(c.Clone());
            copy.nodes = new List<CircuitNode>(nodes);
            return copy;
        }
    }
}
