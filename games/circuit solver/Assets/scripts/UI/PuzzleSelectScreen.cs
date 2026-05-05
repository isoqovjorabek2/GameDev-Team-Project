using CircuitSolver.Core;
using CircuitSolver.Data;
using CircuitSolver.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitSolver.UI
{
    public class PuzzleSelectScreen : MonoBehaviour
    {
        RectTransform _grid;

        public void Build(RectTransform canvas)
        {
            var rt = (RectTransform)transform;
            UIFactory.Stretch(rt, 0, 0, 0, 0);

            var bg = UIFactory.AddImage(rt, "Background", Theme.BackgroundNavy);
            UIFactory.Stretch(bg.rectTransform, 0, 0, 0, 0);

            var title = UIFactory.AddText(rt, "Title", "SELECT PROJECT", 48,
                Theme.TextPrimary, TextAlignmentOptions.Left, FontStyles.Bold);
            title.characterSpacing = -2f;
            UIFactory.Anchor(title.rectTransform,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 1), new Vector2(80, -60), new Vector2(-160, 60));

            var sub = UIFactory.AddText(rt, "Sub", "Pick a circuit. Solve the unknown.", 20,
                Theme.TextMuted, TextAlignmentOptions.Left);
            UIFactory.Anchor(sub.rectTransform,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 1), new Vector2(80, -120), new Vector2(-160, 28));

            var backBtn = UIFactory.AddButton(rt, "Back", "BACK", Theme.PanelNavyLight, Theme.TextPrimary, 18);
            UIFactory.Anchor(backBtn.GetComponent<RectTransform>(),
                new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(1, 1), new Vector2(-80, -60), new Vector2(120, 48));
            backBtn.onClick.AddListener(() => GameManager.Instance.GoTo(ScreenId.MainMenu));

            var scroll = UIFactory.AddChild(rt, "Scroll");
            UIFactory.Stretch(scroll, 80, 160, 80, 60);
            var scrollImg = scroll.gameObject.AddComponent<Image>();
            scrollImg.color = new Color(0, 0, 0, 0);
            var sr = scroll.gameObject.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;

            var viewport = UIFactory.AddChild(scroll, "Viewport");
            UIFactory.Stretch(viewport, 0, 0, 0, 0);
            var vpImg = viewport.gameObject.AddComponent<Image>();
            vpImg.color = new Color(0, 0, 0, 0);
            viewport.gameObject.AddComponent<RectMask2D>();
            sr.viewport = viewport;

            var content = UIFactory.AddChild(viewport, "Content");
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.offsetMin = new Vector2(0, -800);
            content.offsetMax = new Vector2(0, 0);
            var grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(260, 200);
            grid.spacing = new Vector2(24, 24);
            grid.padding = new RectOffset(12, 12, 12, 12);
            grid.constraint = GridLayoutGroup.Constraint.Flexible;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = content;

            _grid = content;
        }

        public void Rebuild()
        {
            if (_grid == null) return;
            for (int i = _grid.childCount - 1; i >= 0; i--)
                Destroy(_grid.GetChild(i).gameObject);

            var gm = GameManager.Instance;
            if (gm == null) return;

            for (int i = 0; i < gm.Puzzles.Count; i++)
            {
                var puzzle = gm.Puzzles[i];
                var cardRt = UIFactory.AddChild(_grid, $"Card_{puzzle.puzzleId:00}");
                var card = cardRt.gameObject.AddComponent<PuzzleCardUI>();
                int idx = i;
                card.Build(puzzle, gm.IsCompleted(puzzle.puzzleId),
                    () => gm.LoadPuzzleByIndex(idx));
            }
        }
    }
}
