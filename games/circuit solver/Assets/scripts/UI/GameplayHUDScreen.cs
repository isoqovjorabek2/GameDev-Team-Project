using System.Collections.Generic;
using CircuitSolver.Components;
using CircuitSolver.Core;
using CircuitSolver.Data;
using CircuitSolver.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitSolver.UI
{
    public class GameplayHUDScreen : MonoBehaviour
    {
        CircuitRenderer _renderer;
        HighlightSystem _highlight;
        TMP_Text _titleText;
        TMP_Text _targetText;
        TMP_Text _feedbackText;
        TMP_Text _workText;
        RectTransform _canvasArea;

        public void Build(RectTransform canvas)
        {
            var rt = (RectTransform)transform;
            UIFactory.Stretch(rt, 0, 0, 0, 0);

            var bg = UIFactory.AddImage(rt, "Background", Theme.BackgroundNavy);
            UIFactory.Stretch(bg.rectTransform, 0, 0, 0, 0);

            // Top bar
            var top = UIFactory.AddImage(rt, "TopBar", Theme.PanelNavy);
            top.rectTransform.anchorMin = new Vector2(0, 1);
            top.rectTransform.anchorMax = new Vector2(1, 1);
            top.rectTransform.pivot = new Vector2(0.5f, 1);
            top.rectTransform.offsetMin = new Vector2(0, -96);
            top.rectTransform.offsetMax = new Vector2(0, 0);

            var backBtn = UIFactory.AddButton(top.transform, "Back", "<", Theme.PanelNavyLight, Theme.TextPrimary, 22);
            UIFactory.Anchor(backBtn.GetComponent<RectTransform>(),
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(56, 56));
            backBtn.onClick.AddListener(() => GameManager.Instance.GoTo(ScreenId.PuzzleSelect));

            _titleText = UIFactory.AddText(top.transform, "Title", "", 22, Theme.TextPrimary,
                TextAlignmentOptions.Left, FontStyles.Bold);
            UIFactory.Anchor(_titleText.rectTransform,
                new Vector2(0, 0.55f), new Vector2(0.5f, 1),
                new Vector2(0, 1), new Vector2(96, -16), new Vector2(0, 0));
            UIFactory.Stretch(_titleText.rectTransform, 96, 16, 0, 40);

            _targetText = UIFactory.AddText(top.transform, "Target", "", 16, Theme.AccentGreen,
                TextAlignmentOptions.Left, FontStyles.Bold);
            UIFactory.Anchor(_targetText.rectTransform,
                new Vector2(0, 0), new Vector2(0.5f, 0.55f),
                new Vector2(0, 0), new Vector2(96, 14), new Vector2(0, 0));
            UIFactory.Stretch(_targetText.rectTransform, 96, 40, 0, 10);

            _workText = UIFactory.AddText(top.transform, "Work", "", 14, Theme.TextMuted,
                TextAlignmentOptions.Right);
            UIFactory.Anchor(_workText.rectTransform,
                new Vector2(0.5f, 0), new Vector2(1, 1),
                new Vector2(1, 0.5f), new Vector2(-24, 0), new Vector2(0, 0));
            UIFactory.Stretch(_workText.rectTransform, 0, 16, 24, 16);

            // Bottom bar
            var bottom = UIFactory.AddImage(rt, "BottomBar", Theme.PanelNavy);
            bottom.rectTransform.anchorMin = new Vector2(0, 0);
            bottom.rectTransform.anchorMax = new Vector2(1, 0);
            bottom.rectTransform.pivot = new Vector2(0.5f, 0);
            bottom.rectTransform.offsetMin = new Vector2(0, 0);
            bottom.rectTransform.offsetMax = new Vector2(0, 104);

            _feedbackText = UIFactory.AddText(bottom.transform, "Feedback", "Tap a ? to enter a value.",
                16, Theme.TextMuted, TextAlignmentOptions.Left);
            UIFactory.Anchor(_feedbackText.rectTransform,
                new Vector2(0, 0.5f), new Vector2(0.55f, 0.5f),
                new Vector2(0, 0.5f), new Vector2(32, 0), new Vector2(0, 40));
            UIFactory.Stretch(_feedbackText.rectTransform, 32, 12, 12, 12);

            var checkBtn = UIFactory.AddButton(bottom.transform, "Check", "CHECK ANSWER",
                Theme.AccentGreen, Theme.BackgroundNavy, 18);
            UIFactory.Anchor(checkBtn.GetComponent<RectTransform>(),
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(1, 0.5f), new Vector2(-32, 0), new Vector2(200, 56));
            checkBtn.onClick.AddListener(OnCheckPressed);

            var hintBtn = UIFactory.AddButton(bottom.transform, "Hint", "HINT",
                Theme.WarningYellow, Theme.BackgroundNavy, 16);
            UIFactory.Anchor(hintBtn.GetComponent<RectTransform>(),
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(1, 0.5f), new Vector2(-248, 0), new Vector2(110, 56));
            hintBtn.onClick.AddListener(OnHintPressed);

            var resetBtn = UIFactory.AddButton(bottom.transform, "Reset", "RESET",
                Theme.PanelNavyLight, Theme.TextPrimary, 16);
            UIFactory.Anchor(resetBtn.GetComponent<RectTransform>(),
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(1, 0.5f), new Vector2(-374, 0), new Vector2(110, 56));
            resetBtn.onClick.AddListener(OnResetPressed);

            // Center circuit canvas
            _canvasArea = UIFactory.AddChild(rt, "CircuitCanvas");
            _canvasArea.anchorMin = new Vector2(0, 0);
            _canvasArea.anchorMax = new Vector2(1, 1);
            _canvasArea.offsetMin = new Vector2(32, 120);
            _canvasArea.offsetMax = new Vector2(-32, -112);

            var rendererGo = new GameObject("Renderer", typeof(RectTransform));
            _renderer = rendererGo.AddComponent<CircuitRenderer>();
            _renderer.Build(_canvasArea);

            _highlight = gameObject.AddComponent<HighlightSystem>();
        }

        void OnEnable()
        {
            if (PuzzleManager.Instance != null)
                PuzzleManager.Instance.OnValidationError += ShowFeedbackError;
        }

        void OnDisable()
        {
            if (PuzzleManager.Instance != null)
                PuzzleManager.Instance.OnValidationError -= ShowFeedbackError;
        }

        public void Rebuild()
        {
            var puzzle = GameManager.Instance.ActivePuzzle;
            if (puzzle == null) return;
            PuzzleManager.Instance.StartPuzzle(puzzle);

            _titleText.text = $"<size=12><color=#00FF87>PROJECT {puzzle.puzzleId:00}</color></size>  {puzzle.title.ToUpperInvariant()}";
            _targetText.text = $"Target: {puzzle.target.Describe()}";
            _feedbackText.text = string.IsNullOrEmpty(puzzle.description)
                ? "Tap a ? to enter a value, then check your answer."
                : puzzle.description;

            UpdateWorkReadout();
            _renderer.Render(puzzle);
        }

        void OnCheckPressed()
        {
            bool ok = PuzzleManager.Instance.CheckSolution();
            var hidden = new List<ComponentSprite>();
            foreach (var h in PuzzleManager.Instance.GetHiddenComponents())
                if (_renderer.Sprites.TryGetValue(h.id, out var s)) hidden.Add(s);

            if (ok)
            {
                _highlight.FlashCorrect(hidden);
                _feedbackText.text = "<color=#00FF87>Circuit correct. Nicely reasoned.</color>";
                GameManager.Instance.MarkCompleted(PuzzleManager.Instance.Puzzle.puzzleId);
                Invoke(nameof(GoToResult), 0.9f);
            }
            else
            {
                _highlight.FlashWrong(hidden);
            }
            UpdateWorkReadout();
        }

        void OnHintPressed()
        {
            var sol = PuzzleManager.Instance.SolvePeek();
            var target = PuzzleManager.Instance.Puzzle.target;
            if (sol == null || !sol.IsSuccess)
            {
                _feedbackText.text = "<color=#FFD166>Can't peek yet — circuit incomplete.</color>";
                return;
            }
            double iTotal = sol.totalCurrentHint;
            double rTotal = sol.totalResistanceHint;
            _feedbackText.text = $"<color=#FFD166>Hint:</color> total I ≈ {iTotal:0.###} A, " +
                                  $"R_eq ≈ {(double.IsInfinity(rTotal) ? "—" : rTotal.ToString("0.##"))} Ω. " +
                                  $"Work backwards from the target.";
        }

        void OnResetPressed()
        {
            PuzzleManager.Instance.ResetHiddenValues();
            foreach (var c in PuzzleManager.Instance.Puzzle.components)
                if (c.isHidden && _renderer.Sprites.TryGetValue(c.id, out var s))
                    s.SetValueDisplay(0f);
            _feedbackText.text = "Reset. Give it another go.";
        }

        void ShowFeedbackError(string msg)
        {
            _feedbackText.text = $"<color=#FF3B5C>{msg}</color>";
        }

        void UpdateWorkReadout()
        {
            var sol = PuzzleManager.Instance.SolvePeek();
            if (sol == null || !sol.IsSuccess) { _workText.text = ""; return; }
            string txt = $"<b>I_total</b> ≈ {sol.totalCurrentHint:0.###} A    " +
                         $"<b>P</b> ≈ {sol.totalPowerHint:0.##} W";
            if (!double.IsInfinity(sol.totalResistanceHint))
                txt += $"    <b>R_eq</b> ≈ {sol.totalResistanceHint:0.##} Ω";
            _workText.text = txt;
        }

        void GoToResult() => GameManager.Instance.GoTo(ScreenId.Result);
    }
}
