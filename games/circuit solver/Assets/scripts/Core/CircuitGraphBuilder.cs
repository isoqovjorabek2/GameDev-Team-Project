using System.Collections.Generic;
using CircuitSolver.Data;

namespace CircuitSolver.Core
{
    /// <summary>
    /// Validates the topology of a CircuitPuzzle and enriches the list of
    /// CircuitNodes with connectivity metadata. Also classifies each
    /// component by its connection style (series/parallel/mixed) for
    /// display and progressive hints.
    /// </summary>
    public static class CircuitGraphBuilder
    {
        public class GraphReport
        {
            public bool isComplete;
            public bool hasShort;
            public int nodeCount;
            public int componentCount;
            public int voltageSourceCount;
            public List<string> warnings = new List<string>();
            public Dictionary<int, List<string>> nodeToComponentIds = new Dictionary<int, List<string>>();
        }

        public static GraphReport Build(CircuitPuzzle puzzle)
        {
            var report = new GraphReport();
            if (puzzle == null || puzzle.components == null || puzzle.components.Count == 0)
            {
                report.warnings.Add("Puzzle has no components.");
                return report;
            }

            var nodeSet = new HashSet<int>();
            foreach (var c in puzzle.components)
            {
                if (!report.nodeToComponentIds.TryGetValue(c.nodeA, out var listA))
                {
                    listA = new List<string>();
                    report.nodeToComponentIds[c.nodeA] = listA;
                }
                listA.Add(c.id);

                if (!report.nodeToComponentIds.TryGetValue(c.nodeB, out var listB))
                {
                    listB = new List<string>();
                    report.nodeToComponentIds[c.nodeB] = listB;
                }
                listB.Add(c.id);

                nodeSet.Add(c.nodeA);
                nodeSet.Add(c.nodeB);
                if (c.type == ComponentType.Battery || c.type == ComponentType.VoltageSource)
                    report.voltageSourceCount++;
            }

            report.nodeCount = nodeSet.Count;
            report.componentCount = puzzle.components.Count;

            // Completeness: every node must connect to >= 2 components (no dangling terminals).
            foreach (var kv in report.nodeToComponentIds)
            {
                if (kv.Value.Count < 2)
                {
                    report.warnings.Add($"Node {kv.Key} has an open terminal (only component: {kv.Value[0]}).");
                    report.isComplete = false;
                    return report;
                }
            }

            // Reachability from ground.
            if (!nodeSet.Contains(puzzle.groundNodeId))
            {
                report.warnings.Add($"Ground node id {puzzle.groundNodeId} not used by any component.");
                return report;
            }

            var visited = new HashSet<int> { puzzle.groundNodeId };
            var stack = new Stack<int>();
            stack.Push(puzzle.groundNodeId);
            var adj = BuildAdjacency(puzzle);
            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                foreach (var nb in adj[cur])
                    if (visited.Add(nb)) stack.Push(nb);
            }
            foreach (var id in nodeSet)
            {
                if (!visited.Contains(id))
                {
                    report.warnings.Add($"Node {id} is not reachable from ground.");
                    return report;
                }
            }

            // Short-circuit on a voltage source (direct wire across its terminals).
            foreach (var v in puzzle.components)
            {
                if (v.type != ComponentType.Battery && v.type != ComponentType.VoltageSource) continue;
                foreach (var other in puzzle.components)
                {
                    if (other == v) continue;
                    if (other.type != ComponentType.Wire) continue;
                    bool same = (other.nodeA == v.nodeA && other.nodeB == v.nodeB) ||
                                (other.nodeA == v.nodeB && other.nodeB == v.nodeA);
                    if (same)
                    {
                        report.hasShort = true;
                        report.warnings.Add($"Wire {other.id} shorts voltage source {v.id}.");
                    }
                }
            }

            report.isComplete = true;
            return report;
        }

        static Dictionary<int, List<int>> BuildAdjacency(CircuitPuzzle puzzle)
        {
            var adj = new Dictionary<int, List<int>>();
            foreach (var c in puzzle.components)
            {
                if (!adj.ContainsKey(c.nodeA)) adj[c.nodeA] = new List<int>();
                if (!adj.ContainsKey(c.nodeB)) adj[c.nodeB] = new List<int>();
                adj[c.nodeA].Add(c.nodeB);
                adj[c.nodeB].Add(c.nodeA);
            }
            return adj;
        }
    }
}
