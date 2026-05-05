using System;
using System.Collections.Generic;
using UnityEngine;

namespace CircuitSolver.Data
{
    /// <summary>
    /// Optional node description used by the renderer. The canonical
    /// electrical graph is derived from CircuitComponent.nodeA/nodeB.
    /// </summary>
    [Serializable]
    public class CircuitNode
    {
        public int id;
        public Vector2 position;
        public bool isGround;
        public string label = "";

        [NonSerialized] public List<int> connectedComponentIndices = new List<int>();
    }
}
