using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ElementRegistry", menuName = "Game/Element Registry", order = 0)]
public class ElementRegistry : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public ElementId id;
        public string symbol;
        public int maxBonds;
        public Color displayColor;
    }

    [SerializeField] Entry[] entries =
    {
        new Entry { id = ElementId.H, symbol = "H", maxBonds = 1, displayColor = new Color(0.95f, 0.95f, 1f) },
        new Entry { id = ElementId.O, symbol = "O", maxBonds = 2, displayColor = new Color(1f, 0.4f, 0.35f) },
        new Entry { id = ElementId.N, symbol = "N", maxBonds = 3, displayColor = new Color(0.45f, 0.55f, 1f) },
        new Entry { id = ElementId.C, symbol = "C", maxBonds = 4, displayColor = new Color(0.35f, 0.35f, 0.4f) }
    };

    public int GetMaxBonds(ElementId id)
    {
        foreach (var e in entries)
        {
            if (e.id == id)
                return Mathf.Max(1, e.maxBonds);
        }

        return ElementRules.DefaultMaxBonds(id);
    }

    public void ApplyToAtom(ElementId id, TextMeshProUGUI label, Graphic background)
    {
        foreach (var e in entries)
        {
            if (e.id != id)
                continue;

            if (label != null)
                label.text = e.symbol;
            if (background != null)
                background.color = e.displayColor;
            return;
        }

        if (label != null)
            label.text = id.ToString();
    }
}

/// <summary>Fallback when no ScriptableObject is assigned.</summary>
public static class ElementRules
{
    public static int DefaultMaxBonds(ElementId id)
    {
        return id switch
        {
            ElementId.H => 1,
            ElementId.O => 2,
            ElementId.N => 3,
            ElementId.C => 4,
            _ => 1
        };
    }
}
