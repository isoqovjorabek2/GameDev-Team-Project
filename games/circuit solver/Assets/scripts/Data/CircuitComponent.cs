using System;
using UnityEngine;

namespace CircuitSolver.Data
{
    /// <summary>
    /// Serializable component definition stored inside a CircuitPuzzle.
    /// Each component connects two electrical nodes (nodeA -> nodeB).
    /// For batteries/voltage sources, nodeA is the positive terminal and
    /// nodeB is the negative terminal.
    /// </summary>
    [Serializable]
    public class CircuitComponent
    {
        public string id = "";
        public ComponentType type = ComponentType.Resistor;

        [Tooltip("Resistance in Ohms (resistor) or EMF in Volts (battery/voltage source).")]
        public float value = 0f;

        [Tooltip("If true, value is hidden from the player and must be filled in.")]
        public bool isHidden = false;

        public string label = "";

        [Tooltip("Grid position (cell coordinates) for the renderer.")]
        public Vector2Int position = Vector2Int.zero;

        [Tooltip("Rotation in degrees, 0 = horizontal (A on left, B on right).")]
        public float orientationDegrees = 0f;

        [Tooltip("Electrical node id of terminal A. For batteries this is the + terminal.")]
        public int nodeA = 0;

        [Tooltip("Electrical node id of terminal B. For batteries this is the - terminal.")]
        public int nodeB = 1;

        public CircuitComponent Clone()
        {
            return new CircuitComponent
            {
                id = id,
                type = type,
                value = value,
                isHidden = isHidden,
                label = label,
                position = position,
                orientationDegrees = orientationDegrees,
                nodeA = nodeA,
                nodeB = nodeB
            };
        }

        public string UnitSuffix()
        {
            switch (type)
            {
                case ComponentType.Resistor: return "Ω";
                case ComponentType.Battery:
                case ComponentType.VoltageSource: return "V";
                default: return "";
            }
        }
    }
}
