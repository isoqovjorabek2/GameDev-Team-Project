using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    public SegmentDigit LeftDigit;
    public SegmentDigit RightDigit;
    public SegmentDigit ResultDigit;
    public EquationOperator Op;
    public EquationOperator Eq;
    public TextMesh StatusText;

    void Awake() { Instance = this; }

    public void OnMatchMoved() { Validate(); }

    public void Validate()
    {
        if (LeftDigit == null || RightDigit == null || ResultDigit == null || Op == null || Eq == null)
            return;

        int l = LeftDigit.GetCurrentDigit();
        int r = RightDigit.GetCurrentDigit();
        int res = ResultDigit.GetCurrentDigit();
        OpType op = Op.GetCurrentOp();
        OpType eq = Eq.GetCurrentOp();

        string opStr = op == OpType.Plus ? "+" : op == OpType.Minus ? "-" : "?";
        string lStr = l < 0 ? "?" : l.ToString();
        string rStr = r < 0 ? "?" : r.ToString();
        string resStr = res < 0 ? "?" : res.ToString();
        string eqStr = eq == OpType.Equals ? "=" : "?";

        if (l < 0 || r < 0 || res < 0 || eq != OpType.Equals || (op != OpType.Plus && op != OpType.Minus))
        {
            SetStatus($"{lStr} {opStr} {rStr} {eqStr} {resStr}", new Color(0.7f, 0.7f, 0.75f));
            return;
        }

        int computed = (op == OpType.Plus) ? l + r : l - r;
        if (computed == res)
            SetStatus($"Solved!  {l} {opStr} {r} = {res}", new Color(0.3f, 0.9f, 0.4f));
        else
            SetStatus($"{l} {opStr} {r} = {res} ?", new Color(1f, 0.55f, 0.25f));
    }

    void SetStatus(string msg, Color color)
    {
        if (StatusText == null) return;
        StatusText.text = msg;
        StatusText.color = color;
    }
}
