using UnityEngine;

/// <summary>
/// Gate segment that shows a math question with two answers (left / right).
/// When the local owner's character enters a trigger zone, it reports the
/// answer to the server which applies a speed boost if correct.
///
/// Prefab layout (you build this once):
///   Root (MathGateSegment + any visuals)
///     └ Question label (TextMesh) — assigned to <see cref="questionLabel"/>
///     └ LeftZone  (Collider isTrigger=true + MathGateTrigger[side=Left])
///         └ Answer label (TextMesh) — assigned to <see cref="leftAnswerLabel"/>
///     └ RightZone (Collider isTrigger=true + MathGateTrigger[side=Right])
///         └ Answer label (TextMesh) — assigned to <see cref="rightAnswerLabel"/>
/// </summary>
public class MathGateSegment : MonoBehaviour
{
    [SerializeField] private MathQuestionSO questionConfig;

    [Header("Text labels (TextMesh components)")]
    [SerializeField] private TextMesh questionLabel;
    [SerializeField] private TextMesh leftAnswerLabel;
    [SerializeField] private TextMesh rightAnswerLabel;

    private bool _correctOnLeft;
    private bool _consumed;

    /// <summary>Called by <see cref="PathManager"/> right after the gate is (re)spawned.</summary>
    public void Initialize(int seed)
    {
        _consumed = false;

        if (questionConfig == null)
        {
            if (questionLabel != null) questionLabel.text = "?";
            if (leftAnswerLabel != null) leftAnswerLabel.text = "?";
            if (rightAnswerLabel != null) rightAnswerLabel.text = "?";
            return;
        }

        var q = MathQuestionGenerator.Generate(questionConfig, seed);

        // Decide which side gets the correct answer, deterministically from the same seed.
        var sideRng = new System.Random(unchecked(seed ^ 0x5F3759DF));
        _correctOnLeft = sideRng.Next(2) == 0;

        if (questionLabel != null) questionLabel.text = q.QuestionText;
        if (leftAnswerLabel != null) leftAnswerLabel.text = _correctOnLeft ? q.CorrectText : q.WrongText;
        if (rightAnswerLabel != null) rightAnswerLabel.text = _correctOnLeft ? q.WrongText : q.CorrectText;
    }

    /// <summary>Called by <see cref="MathGateTrigger"/> when something enters an answer zone.</summary>
    public void HandleEnter(MathGateTrigger.Side side, Collider other)
    {
        if (_consumed) return;

        var ch = other.GetComponentInParent<character>();
        if (ch == null || !ch.IsOwner) return;

        _consumed = true;
        bool correct = side == MathGateTrigger.Side.Left ? _correctOnLeft : !_correctOnLeft;
        ch.ReportGateAnswer(correct);
    }
}
