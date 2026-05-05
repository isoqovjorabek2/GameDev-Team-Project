using CircuitSolver.Core;
using CircuitSolver.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CircuitSolver.UI
{
    /// <summary>
    /// Helpers that build uGUI widgets in code so we don't have to author
    /// scene/prefab YAML by hand. All widgets use the shared Theme palette
    /// and the built-in TMP default font for a consistent look.
    /// </summary>
    public static class UIFactory
    {
        public static RectTransform AddChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static Image AddImage(Transform parent, string name, Color color, Sprite sprite = null)
        {
            var rt = AddChild(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            if (sprite != null) { img.sprite = sprite; img.type = Image.Type.Sliced; }
            return img;
        }

        public static TMP_Text AddText(Transform parent, string name, string content, int size, Color color,
                                        TextAlignmentOptions align = TextAlignmentOptions.Center,
                                        FontStyles style = FontStyles.Normal)
        {
            var rt = AddChild(parent, name);
            var txt = rt.gameObject.AddComponent<TextMeshProUGUI>();
            txt.text = content;
            txt.fontSize = size;
            txt.color = color;
            txt.alignment = align;
            txt.fontStyle = style;
            txt.raycastTarget = false;
            txt.textWrappingMode = TextWrappingModes.Normal;
            return txt;
        }

        public static Button AddButton(Transform parent, string name, string label,
                                        Color bg, Color fg, int fontSize = 20)
        {
            var rt = AddChild(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = SpriteFactory.Rounded();
            img.type = Image.Type.Sliced;
            img.color = bg;
            img.raycastTarget = true;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.interactable = true;
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1, 1, 1, 0.92f);
            cb.pressedColor = new Color(1, 1, 1, 0.8f);
            cb.selectedColor = Color.white;
            cb.disabledColor = new Color(1, 1, 1, 0.4f);
            cb.fadeDuration = 0.12f;
            btn.colors = cb;

            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;

            var txt = AddText(rt, "Label", label, fontSize, fg, TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(txt.rectTransform, 12, 6, 12, 6);
            return btn;
        }

        public static TMP_InputField AddInputField(Transform parent, string name, string placeholder,
                                                     int fontSize = 22)
        {
            var rt = AddChild(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = SpriteFactory.Rounded();
            img.type = Image.Type.Sliced;
            img.color = new Color(1, 1, 1, 0.95f);

            var field = rt.gameObject.AddComponent<TMP_InputField>();
            field.textViewport = rt;

            var textArea = AddChild(rt, "TextArea");
            Stretch(textArea, 10, 4, 10, 4);
            var ta = textArea.gameObject.AddComponent<RectMask2D>();
            ta.padding = Vector4.zero;
            field.textViewport = textArea;

            var placeholderTxt = AddText(textArea, "Placeholder", placeholder, fontSize,
                new Color(0.3f, 0.3f, 0.35f, 0.7f), TextAlignmentOptions.Left);
            placeholderTxt.fontStyle = FontStyles.Italic;
            Stretch(placeholderTxt.rectTransform, 4, 2, 4, 2);

            var textGo = AddChild(textArea, "Text");
            var t = textGo.gameObject.AddComponent<TextMeshProUGUI>();
            t.fontSize = fontSize;
            t.color = Theme.TextOnBoard;
            t.alignment = TextAlignmentOptions.Left;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Overflow;
            Stretch((RectTransform)textGo.transform, 4, 2, 4, 2);

            field.placeholder = placeholderTxt;
            field.textComponent = t;
            field.contentType = TMP_InputField.ContentType.DecimalNumber;
            field.lineType = TMP_InputField.LineType.SingleLine;
            return field;
        }

        public static void Stretch(RectTransform rt, float l, float t, float r, float b)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(l, b);
            rt.offsetMax = new Vector2(-r, -t);
        }

        public static void Anchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax,
                                    Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }

        public static EventTrigger AddClickable(GameObject go, System.Action onClick)
        {
            var et = go.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener(_ => onClick?.Invoke());
            et.triggers.Add(entry);
            return et;
        }
    }
}
