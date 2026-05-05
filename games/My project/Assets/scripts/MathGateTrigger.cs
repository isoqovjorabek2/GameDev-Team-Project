using UnityEngine;

/// <summary>
/// Attach to the two trigger child colliders of a <see cref="MathGateSegment"/>
/// (one for the left answer zone, one for the right). Forwards trigger
/// events up to the gate.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MathGateTrigger : MonoBehaviour
{
    public enum Side { Left, Right }

    [SerializeField] private Side side;
    [SerializeField] private MathGateSegment gate;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (gate == null)
        {
            gate = GetComponentInParent<MathGateSegment>();
            if (gate == null) return;
        }
        gate.HandleEnter(side, other);
    }
}
