using System.Collections.Generic;
using UnityEngine;

public enum OpSlot { HTop, HMiddle, HBottom, Vertical }
public enum OpType { None, Plus, Minus, Equals, Invalid }

public class EquationOperator : MonoBehaviour
{
    public Dictionary<OpSlot, MatchSlot> Slots = new();

    public OpType GetCurrentOp()
    {
        var on = new HashSet<OpSlot>();
        foreach (var kv in Slots)
            if (kv.Value != null && kv.Value.IsOccupied) on.Add(kv.Key);

        if (on.Count == 0) return OpType.None;

        if (on.Count == 2 && on.Contains(OpSlot.HMiddle) && on.Contains(OpSlot.Vertical))
            return OpType.Plus;
        if (on.Count == 1 && on.Contains(OpSlot.HMiddle))
            return OpType.Minus;
        if (on.Count == 2 && on.Contains(OpSlot.HTop) && on.Contains(OpSlot.HBottom))
            return OpType.Equals;

        return OpType.Invalid;
    }
}
