using System;
using System.Collections.Generic;
using CircuitSolver.Data;
using UnityEngine;

namespace CircuitSolver.Core
{
    /// <summary>
    /// Modified Nodal Analysis (MNA) solver supporting resistors and
    /// independent voltage sources. Sign convention for reported
    /// component currents: positive = current flowing from nodeA to nodeB.
    ///
    /// Kirchhoff's Current Law is applied at every non-ground node;
    /// Kirchhoff's Voltage Law is enforced implicitly for every voltage
    /// source via the B/C incidence rows/cols of the MNA matrix.
    /// </summary>
    public static class CircuitSolver
    {
        const double ShortCircuitResistance = 1e-9;

        public static CircuitSolution Solve(IList<CircuitComponent> components, int groundNodeId)
        {
            var result = new CircuitSolution();
            if (components == null || components.Count == 0)
            {
                result.status = SolveStatus.InvalidInput;
                result.message = "No components to solve.";
                return result;
            }

            // Collect all node ids and map non-ground nodes -> matrix rows.
            var allNodes = new HashSet<int>();
            foreach (var c in components)
            {
                if (c == null) continue;
                allNodes.Add(c.nodeA);
                allNodes.Add(c.nodeB);
            }
            if (!allNodes.Contains(groundNodeId))
            {
                result.status = SolveStatus.InvalidInput;
                result.message = $"Ground node {groundNodeId} not present in any component.";
                return result;
            }

            var nodeIndex = new Dictionary<int, int>();
            int nIdx = 0;
            foreach (var id in allNodes)
            {
                if (id == groundNodeId) continue;
                nodeIndex[id] = nIdx++;
            }
            int n = nIdx;

            // Connectivity check: every node must reach ground through components.
            if (!AllNodesReachGround(components, groundNodeId, allNodes, out var orphan))
            {
                result.status = SolveStatus.OpenCircuit;
                result.message = $"Node {orphan} is floating (no path to ground).";
                return result;
            }

            // Voltage sources (batteries + ideal sources) become extra unknowns.
            var voltageSources = new List<CircuitComponent>();
            foreach (var c in components)
            {
                if (c.type == ComponentType.Battery || c.type == ComponentType.VoltageSource)
                    voltageSources.Add(c);
            }
            int m = voltageSources.Count;
            int size = n + m;

            if (size == 0)
            {
                // Pure-ground single node trivially solved.
                result.nodeVoltages[groundNodeId] = 0;
                return result;
            }

            var A = new double[size, size];
            var z = new double[size];

            // Stamp resistors (and wire shorts, if any).
            foreach (var c in components)
            {
                if (c.type == ComponentType.Resistor)
                {
                    double r = c.value;
                    if (r <= 0) r = ShortCircuitResistance;
                    double g = 1.0 / r;
                    StampConductance(A, nodeIndex, groundNodeId, c.nodeA, c.nodeB, g);
                }
                else if (c.type == ComponentType.Wire)
                {
                    StampConductance(A, nodeIndex, groundNodeId, c.nodeA, c.nodeB, 1.0 / ShortCircuitResistance);
                }
            }

            // Stamp voltage sources: B (top-right), C (bottom-left), E (rhs).
            for (int k = 0; k < m; k++)
            {
                var vs = voltageSources[k];
                int row = n + k;

                if (vs.nodeA != groundNodeId)
                {
                    int ia = nodeIndex[vs.nodeA];
                    A[ia, row] += 1.0;
                    A[row, ia] += 1.0;
                }
                if (vs.nodeB != groundNodeId)
                {
                    int ib = nodeIndex[vs.nodeB];
                    A[ib, row] -= 1.0;
                    A[row, ib] -= 1.0;
                }
                z[row] = vs.value;
            }

            if (!GaussianSolver.Solve(A, z, out var x))
            {
                result.status = SolveStatus.SingularSystem;
                result.message = "Circuit is degenerate (possible short or ill-posed loop).";
                return result;
            }

            // Extract node voltages.
            result.nodeVoltages[groundNodeId] = 0;
            foreach (var kv in nodeIndex)
                result.nodeVoltages[kv.Key] = x[kv.Value];

            // Compute currents per component (signed: nodeA -> nodeB).
            for (int k = 0; k < m; k++)
            {
                var vs = voltageSources[k];
                double iMna = x[n + k];
                // In our stamp, i_k flows INTO nodeA through the source from
                // nodeB; so current A->B (internal) equals i_k. For a battery
                // powering a resistive load we expect this to be negative
                // (conventional current exits the + terminal externally).
                result.componentCurrents[vs.id] = iMna;
                result.componentVoltages[vs.id] = result.nodeVoltages[vs.nodeA] - result.nodeVoltages[vs.nodeB];
            }

            foreach (var c in components)
            {
                if (c.type == ComponentType.Resistor || c.type == ComponentType.Wire)
                {
                    double va = result.nodeVoltages[c.nodeA];
                    double vb = result.nodeVoltages[c.nodeB];
                    double r = c.value;
                    if (c.type == ComponentType.Wire || r <= 0) r = ShortCircuitResistance;
                    double i = (va - vb) / r;
                    result.componentCurrents[c.id] = i;
                    result.componentVoltages[c.id] = va - vb;
                }
            }

            // Convenience summary values (useful for UI "intermediate work").
            double vsTotal = 0, iTotal = 0, pTotal = 0;
            foreach (var vs in voltageSources)
            {
                vsTotal += Math.Abs(vs.value);
                double i = Math.Abs(result.componentCurrents[vs.id]);
                iTotal = Math.Max(iTotal, i);
                pTotal += Math.Abs(vs.value * result.componentCurrents[vs.id]);
            }
            result.totalCurrentHint = iTotal;
            result.totalPowerHint = pTotal;
            result.totalResistanceHint = iTotal > 1e-12 ? vsTotal / iTotal : double.PositiveInfinity;

            // Short-circuit heuristic: enormous current + tiny resistance.
            if (double.IsNaN(x[0]) || iTotal > 1e6)
            {
                result.status = SolveStatus.ShortCircuit;
                result.message = "Very large current detected: likely a short circuit.";
            }

            return result;
        }

        static void StampConductance(double[,] A, Dictionary<int, int> idx, int ground,
                                      int nodeA, int nodeB, double g)
        {
            bool aGnd = nodeA == ground;
            bool bGnd = nodeB == ground;
            if (aGnd && bGnd) return;

            if (!aGnd)
            {
                int ia = idx[nodeA];
                A[ia, ia] += g;
            }
            if (!bGnd)
            {
                int ib = idx[nodeB];
                A[ib, ib] += g;
            }
            if (!aGnd && !bGnd)
            {
                int ia = idx[nodeA];
                int ib = idx[nodeB];
                A[ia, ib] -= g;
                A[ib, ia] -= g;
            }
        }

        static bool AllNodesReachGround(IList<CircuitComponent> components, int ground,
                                          HashSet<int> allNodes, out int orphan)
        {
            var adj = new Dictionary<int, List<int>>();
            foreach (var id in allNodes) adj[id] = new List<int>();
            foreach (var c in components)
            {
                adj[c.nodeA].Add(c.nodeB);
                adj[c.nodeB].Add(c.nodeA);
            }

            var visited = new HashSet<int> { ground };
            var queue = new Queue<int>();
            queue.Enqueue(ground);
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                foreach (var n in adj[cur])
                    if (visited.Add(n)) queue.Enqueue(n);
            }

            foreach (var id in allNodes)
            {
                if (!visited.Contains(id))
                {
                    orphan = id;
                    return false;
                }
            }
            orphan = -1;
            return true;
        }
    }
}
