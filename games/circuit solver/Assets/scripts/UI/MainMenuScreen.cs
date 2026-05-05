using CircuitSolver.Core;
using CircuitSolver.Data;
using CircuitSolver.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CircuitSolver.UI
{
    public class MainMenuScreen : MonoBehaviour
    {
        public void Build(RectTransform canvas)
        {
            var rt = (RectTransform)transform;
            UIFactory.Stretch(rt, 0, 0, 0, 0);

            var bg = UIFactory.AddImage(rt, "Background", Theme.BackgroundNavy);
            UIFactory.Stretch(bg.rectTransform, 0, 0, 0, 0);

            // Decorative accent bar on the left
            var accent = UIFactory.AddImage(rt, "AccentBar", Theme.AccentGreen);
            UIFactory.Anchor(accent.rectTransform,
                new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(6, 0));

            var title = UIFactory.AddText(rt, "Title", "CIRCUIT\nSOLVER", 120,
                Theme.TextPrimary, TextAlignmentOptions.Left, FontStyles.Bold);
            UIFactory.Anchor(title.rectTransform,
                new Vector2(0, 0.5f), new Vector2(0.6f, 1f),
                new Vector2(0, 1), new Vector2(80, -60), new Vector2(0, 0));
            UIFactory.Stretch(title.rectTransform, 80, 60, 40, 0);
            title.characterSpacing = -5f;

            var subtitle = UIFactory.AddText(rt, "Subtitle",
                "A puzzle of wires, resistors, and laws.\nKirchhoff would be proud.",
                24, Theme.TextMuted, TextAlignmentOptions.TopLeft);
            UIFactory.Anchor(subtitle.rectTransform,
                new Vector2(0, 0.45f), new Vector2(0.6f, 0.55f),
                new Vector2(0, 1), new Vector2(80, 0), new Vector2(0, 0));
            UIFactory.Stretch(subtitle.rectTransform, 80, 0, 40, 0);

            var playBtn = UIFactory.AddButton(rt, "PlayBtn", "PLAY", Theme.AccentGreen, Theme.BackgroundNavy, 28);
            UIFactory.Anchor(playBtn.GetComponent<RectTransform>(),
                new Vector2(0, 0), new Vector2(0, 0),
                new Vector2(0, 0), new Vector2(80, 200), new Vector2(260, 72));
            playBtn.onClick.AddListener(() => GameManager.Instance.GoTo(ScreenId.PuzzleSelect));

            var quitBtn = UIFactory.AddButton(rt, "QuitBtn", "QUIT", Theme.PanelNavyLight, Theme.TextPrimary, 22);
            UIFactory.Anchor(quitBtn.GetComponent<RectTransform>(),
                new Vector2(0, 0), new Vector2(0, 0),
                new Vector2(0, 0), new Vector2(360, 200), new Vector2(160, 72));
            quitBtn.onClick.AddListener(Application.Quit);

            // Right-side "project card" that mimics a circuit board preview
            var card = UIFactory.AddImage(rt, "Card", Theme.PanelNavy, SpriteFactory.Rounded());
            card.type = Image.Type.Sliced;
            card.rectTransform.anchorMin = new Vector2(0.55f, 0.12f);
            card.rectTransform.anchorMax = new Vector2(0.98f, 0.92f);
            card.rectTransform.offsetMin = Vector2.zero;
            card.rectTransform.offsetMax = Vector2.zero;

            var cardAccent = UIFactory.AddImage(card.transform, "CardAccent", Theme.AccentGreen);
            UIFactory.Anchor(cardAccent.rectTransform,
                new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(0, 1), new Vector2(24, -24), new Vector2(48, 6));

            var cardTitle = UIFactory.AddText(card.transform, "CardTitle",
                "Project 00 · Tutorial", 22, Theme.AccentGreen,
                TextAlignmentOptions.TopLeft, FontStyles.Bold);
            UIFactory.Anchor(cardTitle.rectTransform,
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 1), new Vector2(24, -36), new Vector2(-48, 30));

            var cardName = UIFactory.AddText(card.transform, "CardName",
                "THE FIRST\nRESISTOR", 64, Theme.TextPrimary,
                TextAlignmentOptions.TopLeft, FontStyles.Bold);
            cardName.characterSpacing = -3f;
            UIFactory.Anchor(cardName.rectTransform,
                new Vector2(0, 0.4f), new Vector2(1, 0.95f),
                new Vector2(0, 1), new Vector2(24, -76), new Vector2(-48, 0));
            UIFactory.Stretch(cardName.rectTransform, 24, 76, 24, 0);

            // Fake circuit preview (three nodes + zigzag)
            var preview = UIFactory.AddImage(card.transform, "Preview", Theme.BoardCream, SpriteFactory.Rounded());
            preview.type = Image.Type.Sliced;
            UIFactory.Anchor(preview.rectTransform,
                new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.4f),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            var resistorIcon = UIFactory.AddImage(preview.transform, "Resistor", Theme.ResistorOrange, SpriteFactory.Resistor);
            resistorIcon.preserveAspect = true;
            UIFactory.Anchor(resistorIcon.rectTransform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220, 80));
        }
    }
}
