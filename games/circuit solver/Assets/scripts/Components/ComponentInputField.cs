using System;
using CircuitSolver.Core;
using CircuitSolver.Data;
using CircuitSolver.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CircuitSolver.Components
{
    /// <summary>
    /// Floating numeric input that appears above a hidden "?" component.
    /// Accepts a decimal number, displays the right unit suffix, and
    /// pushes the typed value into PuzzleManager on Enter / blur.
    /// </summary>
    public class ComponentInputField : MonoBehaviour
    {
        public event Action<string, float> OnValueSubmitted;

        TMP_InputField _input;
        TMP_Text _suffix;
        CircuitComponent _target;

        public void Build(CircuitComponent target)
        {
            _target = target;
            var rt = (RectTransform)transform;
            rt.sizeDelta = new Vector2(150, 48);

            var bg = gameObject.AddComponent<Image>();
            bg.sprite = SpriteFactory.Rounded();
            bg.type = Image.Type.Sliced;
            bg.color = Theme.PanelNavy;

            var border = gameObject.AddComponent<Outline>();
            border.effectColor = Theme.AccentGreen;
            border.effectDistance = new Vector2(2, -2);

            _input = UIFactory.AddInputField(rt, "Input", "?", 22);
            UIFactory.Anchor(_input.GetComponent<RectTransform>(),
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var inputRt = (RectTransform)_input.transform;
            inputRt.anchorMin = Vector2.zero;
            inputRt.anchorMax = Vector2.one;
            inputRt.offsetMin = new Vector2(6, 6);
            inputRt.offsetMax = new Vector2(-42, -6);

            _suffix = UIFactory.AddText(rt, "Suffix", target.UnitSuffix(), 18,
                Theme.AccentGreen, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Anchor(_suffix.rectTransform,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(1, 0.5f), new Vector2(-8, 0), new Vector2(32, 28));

            if (target.value != 0f) _input.text = target.value.ToString("0.##");

            _input.onSubmit.AddListener(HandleSubmit);
            _input.onEndEdit.AddListener(HandleSubmit);
        }

        void HandleSubmit(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return;
            if (!float.TryParse(raw, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float v))
            {
                _input.text = "";
                return;
            }
            OnValueSubmitted?.Invoke(_target.id, v);
        }

        public void Focus()
        {
            if (_input == null) return;
            _input.Select();
            _input.ActivateInputField();
        }
    }
}
