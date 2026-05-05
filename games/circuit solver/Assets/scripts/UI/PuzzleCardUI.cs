using System;
using CircuitSolver.Core;
using CircuitSolver.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitSolver.UI
{
    public class PuzzleCardUI : MonoBehaviour
    {
        Button _button;
        TMP_Text _title;
        TMP_Text _subtitle;
        Image _statusBadge;

        public void Build(CircuitPuzzle puzzle, bool completed, Action onClick)
        {
            var rt = (RectTransform)transform;

            var img = gameObject.AddComponent<Image>();
            img.sprite = SpriteFactory.Rounded();
            img.type = Image.Type.Sliced;
            img.color = Theme.PanelNavy;
            _button = gameObject.AddComponent<Button>();
            _button.onClick.AddListener(() => onClick?.Invoke());

            var accent = UIFactory.AddImage(rt, "Accent", completed ? Theme.AccentGreen : Theme.WireOrange);
            UIFactory.Anchor(accent.rectTransform,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 4));
            accent.rectTransform.anchorMin = new Vector2(0, 0);
            accent.rectTransform.anchorMax = new Vector2(1, 0);
            accent.rectTransform.offsetMin = new Vector2(0, 0);
            accent.rectTransform.offsetMax = new Vector2(0, 4);

            _title = UIFactory.AddText(rt, "Title", $"Project {puzzle.puzzleId:00}",
                16, Theme.AccentGreen, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            UIFactory.Anchor(_title.rectTransform,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 1), new Vector2(16, -14), new Vector2(-32, 24));

            _subtitle = UIFactory.AddText(rt, "Name", puzzle.title, 26, Theme.TextPrimary,
                TextAlignmentOptions.TopLeft, FontStyles.Bold);
            _subtitle.characterSpacing = -1f;
            UIFactory.Anchor(_subtitle.rectTransform,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0, 1), new Vector2(16, -40), new Vector2(-32, 80));
            UIFactory.Stretch(_subtitle.rectTransform, 16, 40, 16, 60);

            var meta = UIFactory.AddText(rt, "Meta",
                $"{puzzle.difficulty}  ·  {puzzle.target.Describe()}", 14,
                Theme.TextMuted, TextAlignmentOptions.BottomLeft);
            UIFactory.Anchor(meta.rectTransform,
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 0), new Vector2(16, 16), new Vector2(-32, 32));

            _statusBadge = UIFactory.AddImage(rt, "Badge",
                completed ? Theme.AccentGreen : Theme.PanelNavyLight, SpriteFactory.Rounded());
            _statusBadge.type = Image.Type.Sliced;
            UIFactory.Anchor(_statusBadge.rectTransform,
                new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(1, 1), new Vector2(-14, -14), new Vector2(58, 24));

            var badgeText = UIFactory.AddText(_statusBadge.transform, "BadgeTxt",
                completed ? "DONE" : "OPEN", 12,
                completed ? Theme.BackgroundNavy : Theme.TextMuted,
                TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Stretch(badgeText.rectTransform, 6, 2, 6, 2);
        }
    }
}
