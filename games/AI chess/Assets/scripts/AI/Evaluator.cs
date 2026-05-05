using UnityEngine;

/// <summary>Static evaluation from White's perspective: positive favors White.</summary>
public static class Evaluator
{
    static readonly int[] Material = { 0, 100, 320, 330, 500, 900, 0 }; // index = PieceKind

    // PST rows: y=0 bottom (White back rank in starting position), y increases toward Black.
    static readonly int[] PawnWhite =
    {
        0, 0, 0, 0, 0, 0, 0, 0,
        50, 50, 50, 50, 50, 50, 50, 50,
        10, 10, 20, 30, 30, 20, 10, 10,
        5, 5, 10, 25, 25, 10, 5, 5,
        0, 0, 0, 20, 20, 0, 0, 0,
        5, -5, -10, 0, 0, -10, -5, 5,
        5, 10, 10, -20, -20, 10, 10, 5,
        0, 0, 0, 0, 0, 0, 0, 0
    };
    
    static readonly int[] KnightWhite =
    {
        -50, -40, -30, -30, -30, -30, -40, -50,
        -40, -20, 0, 0, 0, 0, -20, -40,
        -30, 0, 10, 15, 15, 10, 0, -30,
        -30, 5, 15, 20, 20, 15, 5, -30,
        -30, 0, 15, 20, 20, 15, 0, -30,
        -30, 5, 10, 15, 15, 10, 5, -30,
        -40, -20, 0, 5, 5, 0, -20, -40,
        -50, -40, -30, -30, -30, -30, -40, -50
    };

    static readonly int[] BishopWhite =
    {
        -20, -10, -10, -10, -10, -10, -10, -20,
        -10, 0, 0, 0, 0, 0, 0, -10,
        -10, 0, 5, 10, 10, 5, 0, -10,
        -10, 5, 5, 10, 10, 5, 5, -10,
        -10, 0, 10, 10, 10, 10, 0, -10,
        -10, 10, 10, 10, 10, 10, 10, -10,
        -10, 5, 0, 0, 0, 0, 5, -10,
        -20, -10, -10, -10, -10, -10, -10, -20
    };

    static readonly int[] RookWhite =
    {
        0, 0, 0, 0, 0, 0, 0, 0,
        5, 10, 10, 10, 10, 10, 10, 5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        -5, 0, 0, 0, 0, 0, 0, -5,
        0, 0, 0, 5, 5, 0, 0, 0
    };

    static readonly int[] QueenWhite =
    {
        -20, -10, -10, -5, -5, -10, -10, -20,
        -10, 0, 0, 0, 0, 0, 0, -10,
        -10, 0, 5, 5, 5, 5, 0, -10,
        -5, 0, 5, 5, 5, 5, 0, -5,
        0, 0, 5, 5, 5, 5, 0, -5,
        -10, 5, 5, 5, 5, 5, 0, -10,
        -10, 0, 5, 0, 0, 0, 0, -10,
        -20, -10, -10, -5, -5, -10, -10, -20
    };

    static readonly int[] KingWhiteMg =
    {
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -20, -30, -30, -40, -40, -30, -30, -20,
        -10, -20, -20, -20, -20, -20, -20, -10,
        20, 20, 0, 0, 0, 0, 20, 20,
        20, 30, 10, 0, 0, 10, 30, 20
    };

    public static int Evaluate(ChessBoard board)
    {
        int score = 0;

        for (int r = 0; r < ChessBoard.Size; r++)
        for (int f = 0; f < ChessBoard.Size; f++)
        {
            var sq = new Vector2Int(f, r);
            var pc = board.Get(sq);
            if (pc.IsEmpty)
                continue;

            int mat = Material[(int)pc.Kind];
            int pst = Pst(pc.IsWhite, pc.Kind, sq);
            int term = mat + pst;
            score += pc.IsWhite ? term : -term;
        }

        return score;
    }
    // For PST lookup, flip Black squares vertically so we can use the same tables for both colors.
    static Vector2Int MirrorSquare(Vector2Int sq) => new Vector2Int(sq.x, ChessBoard.Size - 1 - sq.y);

    static int Pst(bool white, PieceKind kind, Vector2Int sq)
    {
        int idx = ChessBoard.ToIndex(white ? sq : MirrorSquare(sq));
        return kind switch
        {
            PieceKind.Pawn => PawnWhite[idx],
            PieceKind.Knight => KnightWhite[idx],
            PieceKind.Bishop => BishopWhite[idx],
            PieceKind.Rook => RookWhite[idx],
            PieceKind.Queen => QueenWhite[idx],
            PieceKind.King => KingWhiteMg[idx],
            _ => 0
        };
    }
}
