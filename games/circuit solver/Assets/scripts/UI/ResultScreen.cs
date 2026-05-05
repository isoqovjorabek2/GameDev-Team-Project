using System.Text;
using CircuitSolver.Core;
using CircuitSolver.Data;
using CircuitSolver.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitSolver.UI
{
    public class ResultScreen : MonoBehaviour
    {
        TMP_Text _title;
        TMP_Text _summary;
        TMP_Text _valuesList;

        public void Build(RectTransform canvas)
        {
            var rt = (RectTransform)transform;
            UIFactory.Stretch(rt, 0, 0, 0, 0);

            var bg = UIFactory.AddImage(rt, "Background", Theme.BackgroundNavy);
            UIFactory.Stretch(bg.rectTransform, 0, 0, 0, 0);

            var card = UIFactory.AddImage(rt, "Card", Theme.PanelNavy, SpriteFactory.Rounded());
            card.type = Image.Type.Sliced;
            UIFactory.Anchor(card.rectTransform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640, 480));

            var badge = UIFactory.AddImage(card.transform, "Badge", Theme.AccentGreen, SpriteFactory.Rounded());
            badge.type = Image.Type.Sliced;
            UIFactory.Anchor(badge.rectTransform,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0.5f, 1), new Vector2(0, -36), new Vector2(160, 32));
            var badgeText = UIFactory.AddText(badge.transform, "Txt", "SOLVED", 16,
                Theme.BackgroundNavy, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Stretch(badgeText.rectTransform, 4, 2, 4, 2);

            _title = UIFactory.AddText(card.transform, "Title", "", 44, Theme.TextPrimary,
                TextAlignmentOptions.Center, FontStyles.Bold);
            _title.characterSpacing = -2f;
            UIFactory.Anchor(_title.rectTransform,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), new Vector2(0, -92), new Vector2(0, 64));
            UIFactory.Stretch(_title.rectTransform, 24, 92, 24, 0);

            _summary = UIFactory.AddText(card.transform, "Summary", "", 20, Theme.AccentGreen,
                TextAlignmentOptions.Center);
            UIFactory.Anchor(_summary.rectTransform,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), new Vector2(0, -156), new Vector2(0, 40));

            _valuesList = UIFactory.AddText(card.transform, "Values", "", 16, Theme.TextPrimary,
                TextAlignmentOptions.TopLeft);
            UIFactory.Anchor(_valuesList.rectTransform,
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UIFactory.Stretch(_valuesList.rectTransform, 48, 200, 48, 96);

            var nextBtn = UIFactory.AddButton(card.transform, "Next", "NEXT PROJECT",
                Theme.AccentGreen, Theme.BackgroundNavy, 18);
            UIFactory.Anchor(nextBtn.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0.5f, 0), new Vector2(0, 32), new Vector2(240, 56));
            nextBtn.onClick.AddListener(GoNext);

            var backBtn = UIFactory.AddButton(card.transform, "Back", "Project Select",
                Theme.PanelNavyLight, Theme.TextPrimary, 16);
            UIFactory.Anchor(backBtn.GetComponent<RectTransform>(),
                new Vector2(0, 0), new Vector2(0, 0),
                new Vector2(0, 0), new Vector2(24, 32), new Vector2(170, 48));
            backBtn.onClick.AddListener(() => GameManager.Instance.GoTo(ScreenId.PuzzleSelect));
        }

        public void Rebuild()
        {
            var puzzle = PuzzleManager.Instance.Puzzle;
            var sol = PuzzleManager.Instance.LastSolution;
            if (puzzle == null) return;

            _title.text = puzzle.title.ToUpperInvariant();
            _summary.text = $"Target met: {puzzle.target.Describe()}";

            var sb = new StringBuilder();
            sb.AppendLine("<b>Calculated values</b>\n");
            if (sol != null && sol.IsSuccess)
            {
                foreach (var c in puzzle.components)
                {
                    string val = c.type == ComponentType.Resistor
                        ? $"{c.value:0.##} Ω"
                        : $"{c.value:0.##} V";
                    double i = sol.componentCurrents.TryGetValue(c.id, out var a) ? a : 0;
                    double v = sol.componentVoltages.TryGetValue(c.id, out var b) ? b : 0;
                    sb.AppendLine($"<color=#00FF87>{c.id,-6}</color> {val,-12}  I={FormatAmps(i),-9}  V={v:0.##} V" +
                                   (c.isHidden ? "   <i>(was hidden)</i>" : ""));
                }
                sb.AppendLine();
                sb.AppendLine($"<color=#8FA0B8>R_eq ≈ {(double.IsInfinity(sol.totalResistanceHint) ? "∞" : sol.totalResistanceHint.ToString("0.##"))} Ω   " +
                                $"P ≈ {sol.totalPowerHint:0.##} W</color>");
            }
            _valuesList.text = sb.ToString();
        }

        void GoNext()
        {
            var gm = GameManager.Instance;
            if (PuzzleManager.Instance.Puzzle == null) { gm.GoTo(ScreenId.PuzzleSelect); return; }
            int currentId = PuzzleManager.Instance.Puzzle.puzzleId;
            for (int i = 0; i < gm.Puzzles.Count; i++)
            {
                if (gm.Puzzles[i].puzzleId == currentId && i + 1 < gm.Puzzles.Count)
                {
                    gm.LoadPuzzleByIndex(i + 1);
                    return;
                }
            }
            gm.GoTo(ScreenId.PuzzleSelect);
        }

        static string FormatAmps(double a)
        {
            double abs = System.Math.Abs(a);
            if (abs < 1e-3) return $"{a * 1e6:0.##} µA";
            if (abs < 1) return $"{a * 1e3:0.##} mA";
            return $"{a:0.###} A";
        }
    }
}
