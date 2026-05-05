using System;

namespace CircuitSolver.Data
{
    [Serializable]
    public class PuzzleTarget
    {
        public TargetKind kind = TargetKind.CurrentThroughComponent;

        /// <summary>Component id (for current/voltage across) or node id as int-as-string (for VoltageAtNode).</summary>
        public string referenceId = "";

        /// <summary>Expected value in SI units (Amps for current, Volts for voltage).</summary>
        public float expectedValue = 0f;

        /// <summary>Tolerance as percentage (5 = 5%).</summary>
        public float tolerancePercent = 5f;

        public string Describe()
        {
            switch (kind)
            {
                case TargetKind.CurrentThroughComponent:
                    return $"I through {referenceId} = {FormatAmps(expectedValue)}";
                case TargetKind.VoltageAcrossComponent:
                    return $"V across {referenceId} = {expectedValue:0.##} V";
                case TargetKind.VoltageAtNode:
                    return $"V at node {referenceId} = {expectedValue:0.##} V";
                default: return "";
            }
        }

        static string FormatAmps(float a)
        {
            float abs = System.Math.Abs(a);
            if (abs < 1e-3f) return $"{a * 1e6f:0.##} µA";
            if (abs < 1f) return $"{a * 1e3f:0.##} mA";
            return $"{a:0.###} A";
        }
    }
}
