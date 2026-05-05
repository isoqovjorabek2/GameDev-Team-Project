using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Spawns molecules (single atom + root) into the play area, driven by <see cref="PaletteElementSlot"/>.</summary>
public class AtomPalette : MonoBehaviour
{
    [SerializeField] RectTransform playArea;
    [SerializeField] Canvas rootCanvas;
    [SerializeField] AtomView atomPrefab;
    [SerializeField] ElementRegistry registry;

    public AtomView SpawnAtom(ElementId elementId, PointerEventData eventData)
    {
        if (playArea == null || atomPrefab == null)
            return null;

        var canvas = rootCanvas != null ? rootCanvas : playArea.GetComponentInParent<Canvas>();
        var cam = canvas != null ? canvas.worldCamera : null;

        var rootGo = new GameObject("Molecule", typeof(RectTransform), typeof(MoleculeGraph));
        var rootRt = rootGo.GetComponent<RectTransform>();
        rootRt.SetParent(playArea, false);
        rootRt.localScale = Vector3.one;
        rootRt.anchorMin = rootRt.anchorMax = new Vector2(0.5f, 0.5f);
        rootRt.pivot = new Vector2(0.5f, 0.5f);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(playArea, eventData.position, cam, out var localRoot))
            rootRt.anchoredPosition = localRoot;

        var atomGo = Instantiate(atomPrefab.gameObject, rootRt);
        var atom = atomGo.GetComponent<AtomView>();
        if (atom == null)
        {
            Destroy(rootGo);
            return null;
        }

        var atomRt = atomGo.GetComponent<RectTransform>();
        atomRt.anchorMin = atomRt.anchorMax = new Vector2(0.5f, 0.5f);
        atomRt.pivot = new Vector2(0.5f, 0.5f);
        atomRt.anchoredPosition = Vector2.zero;
        atomRt.localScale = Vector3.one;

        atom.Initialize(elementId, registry);
        rootGo.GetComponent<MoleculeGraph>().Register(atom);
        atom.PaletteBeginDrag(eventData);
        return atom;
    }
}
