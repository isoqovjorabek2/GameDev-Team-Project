using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks compound discovery and fires the win event when enough unique compounds are found.
/// Compounds currently defined: H₂, H₂O, NH₃, CH₄.
/// </summary>
public class DiscoveryManager : MonoBehaviour
{
    [Serializable]
    public class CompoundDefinition
    {
        public string compoundName;
        public string formula;
        public ElementId[] elements;
        public bool requireFullValence = true;
    }

    [SerializeField] int compoundsToWin = 3;

    [SerializeField] CompoundDefinition[] compounds = new CompoundDefinition[]
    {
        new CompoundDefinition
        {
            compoundName = "Hydrogen",
            formula = "H₂",
            elements = new[] { ElementId.H, ElementId.H },
            requireFullValence = true
        },
        new CompoundDefinition
        {
            compoundName = "Water",
            formula = "H₂O",
            elements = new[] { ElementId.H, ElementId.H, ElementId.O },
            requireFullValence = true
        },
        new CompoundDefinition
        {
            compoundName = "Ammonia",
            formula = "NH₃",
            elements = new[] { ElementId.N, ElementId.H, ElementId.H, ElementId.H },
            requireFullValence = true
        },
        new CompoundDefinition
        {
            compoundName = "Methane",
            formula = "CH₄",
            elements = new[] { ElementId.C, ElementId.H, ElementId.H, ElementId.H, ElementId.H },
            requireFullValence = true
        }
    };

    // formula, name, discoveredCount, totalToWin
    public static event Action<string, string, int, int> OnCompoundDiscovered;
    public static event Action OnWin;

    public static DiscoveryManager Instance { get; private set; }

    public int DiscoveredCount => _discovered.Count;
    public int CompoundsToWin => compoundsToWin;

    readonly HashSet<string> _discovered = new HashSet<string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void OnEnable()  => MoleculeGraph.OnMoleculeChanged += HandleMoleculeChanged;
    void OnDisable() => MoleculeGraph.OnMoleculeChanged -= HandleMoleculeChanged;

    void HandleMoleculeChanged(MoleculeGraph graph)
    {
        if (graph == null) return;

        var compound = FindMatch(graph);
        if (compound == null || _discovered.Contains(compound.formula)) return;

        _discovered.Add(compound.formula);

        foreach (var atom in graph.Atoms)
            atom.PlayDiscoveryAnimation();

        int count = _discovered.Count;
        OnCompoundDiscovered?.Invoke(compound.formula, compound.compoundName, count, compoundsToWin);

        if (count >= compoundsToWin)
            OnWin?.Invoke();
    }

    CompoundDefinition FindMatch(MoleculeGraph graph)
    {
        // Tally elements and check valence
        var graphCounts = new Dictionary<ElementId, int>();
        bool allFull = true;

        foreach (var atom in graph.Atoms)
        {
            graphCounts.TryGetValue(atom.ElementId, out int c);
            graphCounts[atom.ElementId] = c + 1;
            if (atom.NeighborCount < atom.MaxBonds)
                allFull = false;
        }

        foreach (var def in compounds)
        {
            if (def.requireFullValence && !allFull) continue;

            var defCounts = new Dictionary<ElementId, int>();
            foreach (var el in def.elements)
            {
                defCounts.TryGetValue(el, out int c);
                defCounts[el] = c + 1;
            }

            if (defCounts.Count != graphCounts.Count) continue;

            bool match = true;
            foreach (var kvp in defCounts)
            {
                if (!graphCounts.TryGetValue(kvp.Key, out int count) || count != kvp.Value)
                { match = false; break; }
            }

            if (match) return def;
        }

        return null;
    }

    public void ResetDiscoveries() => _discovered.Clear();
}
