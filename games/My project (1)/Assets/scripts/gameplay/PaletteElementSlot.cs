using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Palette table cell: begin drag spawns an atom and continues dragging it.</summary>
[RequireComponent(typeof(RectTransform))]
public class PaletteElementSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] ElementId elementId = ElementId.H;
    [SerializeField] AtomPalette palette;

    AtomView _spawned;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (palette == null)
            return;

        _spawned = palette.SpawnAtom(elementId, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        _spawned?.PaletteDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_spawned == null)
            return;

        _spawned.PaletteEndDrag(eventData);
        _spawned = null;
    }
}
