using CircuitSolver.Core;
using CircuitSolver.Data;
using CircuitSolver.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitSolver.Components
{
    /// <summary>
    /// Renders a single electrical component as a small UI card with an
    /// icon, id label and value/unit label. Hidden components show a "?"
    /// instead of the value and host a ComponentInputField on click.
    /// </summary>
    public class ComponentSprite : MonoBehaviour
    {
        public CircuitComponent Data { get; private set; }
        public RectTransform TerminalA { get; private set; }
        public RectTransform TerminalB { get; private set; }

        Image _icon;
        TMP_Text _idLabel;
        TMP_Text _valueLabel;
        Image _glow;

        public void Build(CircuitComponent data)
        {
            Data = data;
            var rt = (RectTransform)transform;
            rt.sizeDelta = new Vector2(140, 90);

            _glow = UIFactory.AddImage(rt, "Glow", new Color(0, 0, 0, 0), SpriteFactory.Rounded());
            _glow.type = Image.Type.Sliced;
            UIFactory.Stretch(_glow.rectTransform, -6, -6, -6, -6);
            _glow.raycastTarget = false;

            var hit = gameObject.AddComponent<Image>();
            hit.color = new Color(0, 0, 0, 0);
            hit.raycastTarget = true;

            Color iconColor;
            Sprite iconSprite;
            switch (data.type)
            {
                case ComponentType.Resistor:
                    iconColor = Theme.ResistorOrange;
                    iconSprite = SpriteFactory.Resistor;
                    break;
                case ComponentType.Battery:
                    iconColor = Theme.BatteryGreen;
                    iconSprite = SpriteFactory.Battery;
                    break;
                case ComponentType.VoltageSource:
                    iconColor = Theme.BatteryGreen;
                    iconSprite = SpriteFactory.VoltageSourceCircle;
                    break;
                default:
                    iconColor = Theme.TextOnBoard;
                    iconSprite = SpriteFactory.Solid;
                    break;
            }

            _icon = UIFactory.AddImage(rt, "Icon", iconColor, iconSprite);
            _icon.preserveAspect = true;
            _icon.raycastTarget = false;
            UIFactory.Anchor(_icon.rectTransform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120, 48));

            _icon.rectTransform.localEulerAngles = new Vector3(0, 0, data.orientationDegrees);

            _idLabel = UIFactory.AddText(rt, "Id", data.id, 14, Theme.TextOnBoard,
                TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Anchor(_idLabel.rectTransform,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0.5f, 1), new Vector2(0, -2), new Vector2(120, 20));

            _valueLabel = UIFactory.AddText(rt, "Value", ValueText(), 18, Theme.TextOnBoard,
                TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Anchor(_valueLabel.rectTransform,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0.5f, 0), new Vector2(0, 4), new Vector2(140, 22));

            TerminalA = CreateTerminal("TerminalA", new Vector2(0f, 0.5f));
            TerminalB = CreateTerminal("TerminalB", new Vector2(1f, 0.5f));
        }

        RectTransform CreateTerminal(string name, Vector2 anchor)
        {
            var rt = UIFactory.AddChild(transform, name);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(12, 12);
            rt.anchoredPosition = Vector2.zero;
            return rt;
        }

        string ValueText()
        {
            if (Data.isHidden) return "?";
            switch (Data.type)
            {
                case ComponentType.Resistor:
                    return Data.value >= 1000
                        ? $"{Data.value / 1000f:0.##}kΩ"
                        : $"{Data.value:0.##}Ω";
                case ComponentType.Battery:
                case ComponentType.VoltageSource:
                    return $"{Data.value:0.##}V";
                default:
                    return "";
            }
        }

        public void SetValueDisplay(float value)
        {
            Data.value = value;
            _valueLabel.text = ValueText();
        }

        public void Pulse(Color color)
        {
            _glow.color = color;
            StopAllCoroutines();
            StartCoroutine(FadeGlow());
        }

        System.Collections.IEnumerator FadeGlow()
        {
            Color start = _glow.color;
            float t = 0;
            while (t < 0.8f)
            {
                t += Time.deltaTime;
                _glow.color = new Color(start.r, start.g, start.b, Mathf.Lerp(0.75f, 0f, t / 0.8f));
                yield return null;
            }
            _glow.color = new Color(0, 0, 0, 0);
        }

        public void Shake()
        {
            StopAllCoroutines();
            StartCoroutine(ShakeRoutine());
        }

        System.Collections.IEnumerator ShakeRoutine()
        {
            Vector3 origin = transform.localPosition;
            float t = 0, dur = 0.35f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float x = Mathf.Sin(t * 60f) * Mathf.Lerp(8f, 0f, t / dur);
                transform.localPosition = origin + new Vector3(x, 0, 0);
                yield return null;
            }
            transform.localPosition = origin;
        }
    }
}
