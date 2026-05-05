using System.Collections.Generic;

namespace CircuitSolver.Core
{
    public enum SolveStatus
    {
        Ok = 0,
        IncompleteCircuit = 1,
        ShortCircuit = 2,
        OpenCircuit = 3,
        SingularSystem = 4,
        InvalidInput = 5
    }

    /// <summary>
    /// Result of a CircuitSolver run. Node voltages are indexed by node id
    /// (ground = 0 V). Component currents are keyed by component id; sign
    /// convention: positive current flows from nodeA -> nodeB.
    /// </summary>
    public class CircuitSolution
    {
        public SolveStatus status = SolveStatus.Ok;
        public string message = "";

        public Dictionary<int, double> nodeVoltages = new Dictionary<int, double>();
        public Dictionary<string, double> componentCurrents = new Dictionary<string, double>();
        public Dictionary<string, double> componentVoltages = new Dictionary<string, double>();

        public double totalResistanceHint;
        public double totalCurrentHint;
        public double totalPowerHint;

        public bool IsSuccess => status == SolveStatus.Ok;
    }
}
