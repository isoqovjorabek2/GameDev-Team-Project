using System.Collections.Generic;
using CircuitSolver.Data;
using UnityEngine;

namespace CircuitSolver.Managers
{
    /// <summary>
    /// Built-in fallback puzzles so the game runs even if no ScriptableObject
    /// assets live in Resources/Puzzles. Targets have been hand-verified
    /// against the MNA solver before being shipped.
    /// </summary>
    public static class DefaultPuzzleLibrary
    {
        public static List<CircuitPuzzle> BuildAll()
        {
            var list = new List<CircuitPuzzle>
            {
                P01_FirstCurrent(),
                P02_MysteryResistor(),
                P03_VoltageDivider(),
                P04_ParallelTwins(),
                P05_MirrorBranches(),
                P06_MixedBranch(),
                P07_DualBattery(),
                P08_Wheatstone(),
                P09_LadderNetwork(),
                P10_FullBridge()
            };
            return list;
        }

        static CircuitPuzzle MakePuzzle(int id, string title, DifficultyTier tier, string desc,
                                          System.Action<CircuitPuzzle> configure)
        {
            var p = ScriptableObject.CreateInstance<CircuitPuzzle>();
            p.puzzleId = id;
            p.title = title;
            p.description = desc;
            p.difficulty = tier;
            p.groundNodeId = 0;
            configure(p);
            return p;
        }

        // ---------------- 01 ----------------
        static CircuitPuzzle P01_FirstCurrent()
        {
            return MakePuzzle(1, "First Current", DifficultyTier.Intro,
                "A single resistor is missing. How many ohms keep the current at 3 A?",
                p =>
                {
                    p.components.Add(new CircuitComponent
                    { id = "BAT1", type = ComponentType.Battery, value = 9f,
                      nodeA = 1, nodeB = 0, position = new Vector2Int(-2, 0), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "R1", type = ComponentType.Resistor, value = 3f, isHidden = true,
                      nodeA = 1, nodeB = 0, position = new Vector2Int(2, 0), orientationDegrees = 90f });
                    p.target = new PuzzleTarget
                    { kind = TargetKind.CurrentThroughComponent, referenceId = "R1", expectedValue = 3f };
                    p.hintText = "Ohm's law: V = IR. Solve for R.";
                });
        }

        // ---------------- 02 ----------------
        static CircuitPuzzle P02_MysteryResistor()
        {
            return MakePuzzle(2, "Mystery Resistor", DifficultyTier.Intro,
                "Two resistors in series — find the hidden one that lands the current at 1 A.",
                p =>
                {
                    p.components.Add(new CircuitComponent
                    { id = "BAT1", type = ComponentType.Battery, value = 12f,
                      nodeA = 1, nodeB = 0, position = new Vector2Int(-3, 0), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "R1", type = ComponentType.Resistor, value = 4f,
                      nodeA = 1, nodeB = 2, position = new Vector2Int(0, 1) });
                    p.components.Add(new CircuitComponent
                    { id = "R2", type = ComponentType.Resistor, value = 8f, isHidden = true,
                      nodeA = 2, nodeB = 0, position = new Vector2Int(3, 0), orientationDegrees = 90f });
                    p.target = new PuzzleTarget
                    { kind = TargetKind.CurrentThroughComponent, referenceId = "R1", expectedValue = 1f };
                    p.hintText = "In a series circuit, resistances simply add up.";
                });
        }

        // ---------------- 03 ----------------
        static CircuitPuzzle P03_VoltageDivider()
        {
            return MakePuzzle(3, "Voltage Divider", DifficultyTier.Easy,
                "Set R2 so the mid-node sits at 8 V.",
                p =>
                {
                    p.components.Add(new CircuitComponent
                    { id = "BAT1", type = ComponentType.Battery, value = 12f,
                      nodeA = 1, nodeB = 0, position = new Vector2Int(-3, 0), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "R1", type = ComponentType.Resistor, value = 100f,
                      nodeA = 1, nodeB = 2, position = new Vector2Int(0, 1) });
                    p.components.Add(new CircuitComponent
                    { id = "R2", type = ComponentType.Resistor, value = 200f, isHidden = true,
                      nodeA = 2, nodeB = 0, position = new Vector2Int(3, 0), orientationDegrees = 90f });
                    p.target = new PuzzleTarget
                    { kind = TargetKind.VoltageAtNode, referenceId = "2", expectedValue = 8f };
                    p.hintText = "Voltage divider: V_out = V_in · R2/(R1+R2).";
                });
        }

        // ---------------- 04 ----------------
        static CircuitPuzzle P04_ParallelTwins()
        {
            return MakePuzzle(4, "Parallel Twins", DifficultyTier.Easy,
                "Two resistors share the rails. Find R2 so the battery delivers 3 A.",
                p =>
                {
                    p.components.Add(new CircuitComponent
                    { id = "BAT1", type = ComponentType.Battery, value = 6f,
                      nodeA = 1, nodeB = 0, position = new Vector2Int(-3, 0), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "R1", type = ComponentType.Resistor, value = 3f,
                      nodeA = 1, nodeB = 0, position = new Vector2Int(0, 0), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "R2", type = ComponentType.Resistor, value = 6f, isHidden = true,
                      nodeA = 1, nodeB = 0, position = new Vector2Int(3, 0), orientationDegrees = 90f });
                    p.target = new PuzzleTarget
                    { kind = TargetKind.CurrentThroughComponent, referenceId = "BAT1", expectedValue = 3f };
                    p.hintText = "Each branch sees the full 6 V. Pick R2 so the total current adds to 3 A.";
                });
        }

        // ---------------- 05 ----------------
        static CircuitPuzzle P05_MirrorBranches()
        {
            return MakePuzzle(5, "Mirror Branches", DifficultyTier.Medium,
                "Two hidden resistors across 10 V. Any pair that delivers 3 A works.",
                p =>
                {
                    p.components.Add(new CircuitComponent
                    { id = "BAT1", type = ComponentType.Battery, value = 10f,
                      nodeA = 1, nodeB = 0, position = new Vector2Int(-3, 0), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "R1", type = ComponentType.Resistor, value = 5f, isHidden = true,
                      nodeA = 1, nodeB = 0, position = new Vector2Int(0, 0), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "R2", type = ComponentType.Resistor, value = 10f, isHidden = true,
                      nodeA = 1, nodeB = 0, position = new Vector2Int(3, 0), orientationDegrees = 90f });
                    p.target = new PuzzleTarget
                    { kind = TargetKind.CurrentThroughComponent, referenceId = "BAT1", expectedValue = 3f };
                    p.hintText = "I_total = V · (1/R1 + 1/R2). Pick two values whose conductances sum to 0.3 S.";
                });
        }

        // ---------------- 06 ----------------
        static CircuitPuzzle P06_MixedBranch()
        {
            return MakePuzzle(6, "Mixed Branch", DifficultyTier.Medium,
                "R3 sits parallel to R2. Tune it so the main current reads 1.5 A.",
                p =>
                {
                    p.components.Add(new CircuitComponent
                    { id = "BAT1", type = ComponentType.Battery, value = 12f,
                      nodeA = 1, nodeB = 0, position = new Vector2Int(-4, 0), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "R1", type = ComponentType.Resistor, value = 6f,
                      nodeA = 1, nodeB = 2, position = new Vector2Int(-1, 1) });
                    p.components.Add(new CircuitComponent
                    { id = "R2", type = ComponentType.Resistor, value = 4f,
                      nodeA = 2, nodeB = 0, position = new Vector2Int(2, 0), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "R3", type = ComponentType.Resistor, value = 4f, isHidden = true,
                      nodeA = 2, nodeB = 0, position = new Vector2Int(5, 0), orientationDegrees = 90f });
                    p.target = new PuzzleTarget
                    { kind = TargetKind.CurrentThroughComponent, referenceId = "R1", expectedValue = 1.5f };
                    p.hintText = "Parallel conductances add. R1 sees R2‖R3 in series with itself.";
                });
        }

        // ---------------- 07 ----------------
        static CircuitPuzzle P07_DualBattery()
        {
            return MakePuzzle(7, "Dual Battery", DifficultyTier.Medium,
                "Two batteries push in series through one resistor. Find the missing EMF for I = 2 A.",
                p =>
                {
                    p.components.Add(new CircuitComponent
                    { id = "BAT1", type = ComponentType.Battery, value = 9f,
                      nodeA = 1, nodeB = 0, position = new Vector2Int(-3, 0), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "BAT2", type = ComponentType.Battery, value = 3f, isHidden = true,
                      nodeA = 2, nodeB = 1, position = new Vector2Int(0, 2), orientationDegrees = 0f });
                    p.components.Add(new CircuitComponent
                    { id = "R1", type = ComponentType.Resistor, value = 6f,
                      nodeA = 2, nodeB = 0, position = new Vector2Int(3, 0), orientationDegrees = 90f });
                    p.target = new PuzzleTarget
                    { kind = TargetKind.CurrentThroughComponent, referenceId = "R1", expectedValue = 2f };
                    p.hintText = "KVL: EMFs add when their +/- terminals chain together.";
                });
        }

        // ---------------- 08 ----------------
        static CircuitPuzzle P08_Wheatstone()
        {
            return MakePuzzle(8, "Balanced Pair", DifficultyTier.Hard,
                "Two branches, one unknown. Make node 3's voltage match node 2.",
                p =>
                {
                    p.components.Add(new CircuitComponent
                    { id = "BAT1", type = ComponentType.Battery, value = 10f,
                      nodeA = 1, nodeB = 0, position = new Vector2Int(-4, 0), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "R1", type = ComponentType.Resistor, value = 2f,
                      nodeA = 1, nodeB = 2, position = new Vector2Int(-1, 2) });
                    p.components.Add(new CircuitComponent
                    { id = "R2", type = ComponentType.Resistor, value = 3f,
                      nodeA = 2, nodeB = 0, position = new Vector2Int(2, 2), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "R3", type = ComponentType.Resistor, value = 4f,
                      nodeA = 1, nodeB = 3, position = new Vector2Int(-1, -2) });
                    p.components.Add(new CircuitComponent
                    { id = "R4", type = ComponentType.Resistor, value = 6f, isHidden = true,
                      nodeA = 3, nodeB = 0, position = new Vector2Int(2, -2), orientationDegrees = 90f });
                    p.target = new PuzzleTarget
                    { kind = TargetKind.VoltageAtNode, referenceId = "3", expectedValue = 6f };
                    p.hintText = "Use the divider formula on the right branch and match node 2's voltage.";
                });
        }

        // ---------------- 09 ----------------
        static CircuitPuzzle P09_LadderNetwork()
        {
            return MakePuzzle(9, "Ladder Network", DifficultyTier.Hard,
                "A three-resistor ladder sits in parallel with R4. Find R4 so the source sees 2 A.",
                p =>
                {
                    p.components.Add(new CircuitComponent
                    { id = "BAT1", type = ComponentType.Battery, value = 12f,
                      nodeA = 1, nodeB = 0, position = new Vector2Int(-4, 0), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "R1", type = ComponentType.Resistor, value = 2f,
                      nodeA = 1, nodeB = 2, position = new Vector2Int(-1, 2) });
                    p.components.Add(new CircuitComponent
                    { id = "R2", type = ComponentType.Resistor, value = 4f,
                      nodeA = 2, nodeB = 3, position = new Vector2Int(2, 2) });
                    p.components.Add(new CircuitComponent
                    { id = "R3", type = ComponentType.Resistor, value = 4f,
                      nodeA = 3, nodeB = 0, position = new Vector2Int(5, 0), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "R4", type = ComponentType.Resistor, value = 8f, isHidden = true,
                      nodeA = 2, nodeB = 0, position = new Vector2Int(2, -2), orientationDegrees = 90f });
                    p.target = new PuzzleTarget
                    { kind = TargetKind.CurrentThroughComponent, referenceId = "R1", expectedValue = 2f };
                    p.hintText = "From R2 onwards you see R2+R3 in parallel with R4 — reduce step by step.";
                });
        }

        // ---------------- 10 ----------------
        static CircuitPuzzle P10_FullBridge()
        {
            return MakePuzzle(10, "Full Wheatstone Bridge", DifficultyTier.Expert,
                "Balance the bridge so no current flows through R5.",
                p =>
                {
                    p.components.Add(new CircuitComponent
                    { id = "BAT1", type = ComponentType.Battery, value = 12f,
                      nodeA = 1, nodeB = 0, position = new Vector2Int(-5, 0), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "R1", type = ComponentType.Resistor, value = 4f,
                      nodeA = 1, nodeB = 2, position = new Vector2Int(-2, 2) });
                    p.components.Add(new CircuitComponent
                    { id = "R2", type = ComponentType.Resistor, value = 2f,
                      nodeA = 2, nodeB = 0, position = new Vector2Int(2, 2), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "R3", type = ComponentType.Resistor, value = 2f,
                      nodeA = 1, nodeB = 3, position = new Vector2Int(-2, -2) });
                    p.components.Add(new CircuitComponent
                    { id = "R4", type = ComponentType.Resistor, value = 1f, isHidden = true,
                      nodeA = 3, nodeB = 0, position = new Vector2Int(2, -2), orientationDegrees = 90f });
                    p.components.Add(new CircuitComponent
                    { id = "R5", type = ComponentType.Resistor, value = 5f,
                      nodeA = 2, nodeB = 3, position = new Vector2Int(5, 0), orientationDegrees = 90f });
                    p.target = new PuzzleTarget
                    { kind = TargetKind.CurrentThroughComponent, referenceId = "R5", expectedValue = 0f,
                      tolerancePercent = 5f };
                    p.hintText = "Balance condition: R1/R2 = R3/R4. Solve for R4.";
                });
        }
    }
}
