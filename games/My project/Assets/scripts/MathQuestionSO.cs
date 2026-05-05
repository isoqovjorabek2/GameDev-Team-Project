using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configuration for how math questions are generated at gates.
/// Create via <c>Assets → Create → Math Runner → Math Questions</c>.
/// </summary>
[CreateAssetMenu(menuName = "Math Runner/Math Questions", fileName = "MathQuestions", order = 2)]
public class MathQuestionSO : ScriptableObject
{
    [Header("Allowed operators")]
    public bool allowAdd = true;
    public bool allowSub = true;
    public bool allowMul = false;

    [Header("Operand range")]
    [Min(0)] public int minOperand = 1;
    [Min(1)] public int maxOperand = 10;

    [Header("Wrong answer")]
    [Tooltip("The wrong answer differs from the correct one by up to +/- this much.")]
    [Min(1)] public int wrongAnswerMaxDelta = 3;
}

public readonly struct MathQuestion
{
    public readonly int A;
    public readonly int B;
    public readonly string OpSymbol;
    public readonly int Correct;
    public readonly int Wrong;

    public MathQuestion(int a, int b, string op, int correct, int wrong)
    {
        A = a; B = b; OpSymbol = op; Correct = correct; Wrong = wrong;
    }

    public string QuestionText => $"{A} {OpSymbol} {B} = ?";
    public string CorrectText => Correct.ToString();
    public string WrongText => Wrong.ToString();
}

public static class MathQuestionGenerator
{
    /// <summary>Generate a deterministic question for the given seed.</summary>
    public static MathQuestion Generate(MathQuestionSO cfg, int seed)
    {
        var rng = new System.Random(seed);

        var ops = new List<string>(3);
        if (cfg.allowAdd) ops.Add("+");
        if (cfg.allowSub) ops.Add("-");
        if (cfg.allowMul) ops.Add("x");
        if (ops.Count == 0) ops.Add("+");

        string op = ops[rng.Next(ops.Count)];
        int min = cfg.minOperand;
        int max = Mathf.Max(cfg.minOperand + 1, cfg.maxOperand);
        int a = rng.Next(min, max + 1);
        int b = rng.Next(min, max + 1);

        int correct = op switch
        {
            "+" => a + b,
            "-" => a - b,
            "x" => a * b,
            _ => a + b
        };

        int delta = 0;
        for (int i = 0; i < 10 && delta == 0; i++)
        {
            delta = rng.Next(-cfg.wrongAnswerMaxDelta, cfg.wrongAnswerMaxDelta + 1);
        }
        if (delta == 0) delta = 1;

        return new MathQuestion(a, b, op, correct, correct + delta);
    }
}
