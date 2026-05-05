using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Root of one connected molecule. All <see cref="AtomView"/> nodes in a component share one graph root.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class MoleculeGraph : MonoBehaviour
{
    /// <summary>Fired whenever a bond forms; passes the surviving merged graph.</summary>
    public static event Action<MoleculeGraph> OnMoleculeChanged;

    readonly HashSet<AtomView> _atoms = new HashSet<AtomView>();

    public IReadOnlyCollection<AtomView> Atoms => _atoms;

    public void Register(AtomView atom)
    {
        if (atom != null) _atoms.Add(atom);
    }

    public void Unregister(AtomView atom)
    {
        if (atom != null) _atoms.Remove(atom);
    }

    /// <summary>Merges <paramref name="other"/> into this graph, reparenting atoms and destroying the other root.</summary>
    public void MergeFrom(MoleculeGraph other)
    {
        if (other == null || other == this) return;

        var toMove = other._atoms.Count > 0
            ? other._atoms.ToArray()
            : other.GetComponentsInChildren<AtomView>(true);

        foreach (var atom in toMove)
        {
            if (atom == null) continue;
            atom.transform.SetParent(transform, worldPositionStays: true);
            _atoms.Add(atom);
        }

        other._atoms.Clear();
        Destroy(other.gameObject);
    }

    /// <summary>Attempts a single bond between two atoms, animates the snap, and notifies listeners.</summary>
    public static bool TryBond(AtomView dragger, AtomView target, float neighborRingRadius)
    {
        if (dragger == null || target == null || dragger == target) return false;
        if (!dragger.CanBondWith(target)) return false;

        var graphA = dragger.GetComponentInParent<MoleculeGraph>();
        var graphB = target.GetComponentInParent<MoleculeGraph>();
        if (graphA == null || graphB == null) return false;

        if (graphA != graphB)
            graphA.MergeFrom(graphB);

        dragger.AddBond(target);
        target.AddBond(dragger);

        // Animate dragger snapping into orbit around target
        var targetRt = target.RectTransform;
        int slot = Mathf.Max(0, target.NeighborCount - 1);
        float step = Mathf.PI * 2f / Mathf.Max(1, target.MaxBonds);
        float angle = slot * step;
        var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * neighborRingRadius;
        dragger.MoveTo(targetRt.anchoredPosition + offset, 0.2f);

        dragger.PlayBondAnimation();
        target.PlayBondAnimation();

        OnMoleculeChanged?.Invoke(graphA);

        return true;
    }
}
