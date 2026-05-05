using System;
using UnityEngine;

public struct SquarePiece
{
    public PieceKind Kind;
    public bool IsWhite;

    public bool IsEmpty => Kind == PieceKind.None;

    public static SquarePiece Of(PieceKind kind, bool white) =>
        new SquarePiece { Kind = kind, IsWhite = white };
}

public class ChessBoard
{
    public const int Size = 8;

    readonly SquarePiece[] squares = new SquarePiece[64];

    public bool WhiteToMove { get; set; } = true;

    public bool CastleWhiteKingside { get; set; } = true;
    public bool CastleWhiteQueenside { get; set; } = true;
    public bool CastleBlackKingside { get; set; } = true;
    public bool CastleBlackQueenside { get; set; } = true;

    /// <summary>FEN-style en passant target: square the capturing pawn moves to (e.g. e3 after e2-e4). Null if none.</summary>
    public Vector2Int? EnPassantTarget { get; set; }

    public static int ToIndex(int file, int rank) => file + rank * Size;

    public static int ToIndex(Vector2Int sq) => sq.x + sq.y * Size;

    public static bool InBounds(Vector2Int p) =>
        p.x >= 0 && p.x < Size && p.y >= 0 && p.y < Size;

    public SquarePiece Get(Vector2Int p) => squares[ToIndex(p)];

    public void Set(Vector2Int p, SquarePiece piece) => squares[ToIndex(p)] = piece;

    public ChessBoard Clone()
    {
        var c = new ChessBoard();
        Array.Copy(squares, c.squares, squares.Length);
        c.WhiteToMove = WhiteToMove;
        c.CastleWhiteKingside = CastleWhiteKingside;
        c.CastleWhiteQueenside = CastleWhiteQueenside;
        c.CastleBlackKingside = CastleBlackKingside;
        c.CastleBlackQueenside = CastleBlackQueenside;
        c.EnPassantTarget = EnPassantTarget;
        return c;
    }

    public void ResetToStandardStartingPosition()
    {
        for (int i = 0; i < squares.Length; i++)
            squares[i] = default;

        WhiteToMove = true;
        CastleWhiteKingside = CastleWhiteQueenside = true;
        CastleBlackKingside = CastleBlackQueenside = true;
        EnPassantTarget = null;

        void Put(int f, int r, PieceKind k, bool w) =>
            squares[ToIndex(f, r)] = SquarePiece.Of(k, w);

        for (int f = 0; f < 8; f++)
        {
            Put(f, 1, PieceKind.Pawn, true);
            Put(f, 6, PieceKind.Pawn, false);
        }

        Put(0, 0, PieceKind.Rook, true);
        Put(7, 0, PieceKind.Rook, true);
        Put(1, 0, PieceKind.Knight, true);
        Put(6, 0, PieceKind.Knight, true);
        Put(2, 0, PieceKind.Bishop, true);
        Put(5, 0, PieceKind.Bishop, true);
        Put(3, 0, PieceKind.Queen, true);
        Put(4, 0, PieceKind.King, true);

        Put(0, 7, PieceKind.Rook, false);
        Put(7, 7, PieceKind.Rook, false);
        Put(1, 7, PieceKind.Knight, false);
        Put(6, 7, PieceKind.Knight, false);
        Put(2, 7, PieceKind.Bishop, false);
        Put(5, 7, PieceKind.Bishop, false);
        Put(3, 7, PieceKind.Queen, false);
        Put(4, 7, PieceKind.King, false);
    }

    public Vector2Int? FindKingSquare(bool white)
    {
        for (int r = 0; r < Size; r++)
        for (int f = 0; f < Size; f++)
        {
            var p = new Vector2Int(f, r);
            var s = Get(p);
            if (!s.IsEmpty && s.Kind == PieceKind.King && s.IsWhite == white)
                return p;
        }
        return null;
    }

    public void ApplyMove(Move m)
    {
        var moving = Get(m.From);
        EnPassantTarget = null;

        if ((m.Flags & MoveFlags.EnPassant) != 0)
        {
            int capRank = moving.IsWhite ? m.To.y - 1 : m.To.y + 1;
            var capSq = new Vector2Int(m.To.x, capRank);
            Set(capSq, default);
        }

        Set(m.From, default);

        var newKind = moving.Kind;
        if ((m.Flags & MoveFlags.Promotion) != 0 && m.PromotionTo != PieceKind.None)
            newKind = m.PromotionTo;

        Set(m.To, SquarePiece.Of(newKind, moving.IsWhite));

        if ((m.Flags & MoveFlags.CastleKingside) != 0)
        {
            if (moving.IsWhite)
            {
                Set(new Vector2Int(7, 0), default);
                Set(new Vector2Int(5, 0), SquarePiece.Of(PieceKind.Rook, true));
            }
            else
            {
                Set(new Vector2Int(7, 7), default);
                Set(new Vector2Int(5, 7), SquarePiece.Of(PieceKind.Rook, false));
            }
        }

        if ((m.Flags & MoveFlags.CastleQueenside) != 0)
        {
            if (moving.IsWhite)
            {
                Set(new Vector2Int(0, 0), default);
                Set(new Vector2Int(3, 0), SquarePiece.Of(PieceKind.Rook, true));
            }
            else
            {
                Set(new Vector2Int(0, 7), default);
                Set(new Vector2Int(3, 7), SquarePiece.Of(PieceKind.Rook, false));
            }
        }

        UpdateCastlingRightsAfterMove(m, moving);

        if ((m.Flags & MoveFlags.DoublePawnPush) != 0)
        {
            int midRank = moving.IsWhite ? m.From.y + 1 : m.From.y - 1;
            EnPassantTarget = new Vector2Int(m.From.x, midRank);
        }

        WhiteToMove = !WhiteToMove;
    }

    void UpdateCastlingRightsAfterMove(Move m, SquarePiece moving)
    {
        if (moving.Kind == PieceKind.King)
        {
            if (moving.IsWhite)
            {
                CastleWhiteKingside = false;
                CastleWhiteQueenside = false;
            }
            else
            {
                CastleBlackKingside = false;
                CastleBlackQueenside = false;
            }
        }

        if (moving.Kind == PieceKind.Rook)
        {
            if (m.From.x == 0 && m.From.y == 0) CastleWhiteQueenside = false;
            if (m.From.x == 7 && m.From.y == 0) CastleWhiteKingside = false;
            if (m.From.x == 0 && m.From.y == 7) CastleBlackQueenside = false;
            if (m.From.x == 7 && m.From.y == 7) CastleBlackKingside = false;
        }

        var to = m.To;
        if (to == new Vector2Int(0, 0)) CastleWhiteQueenside = false;
        if (to == new Vector2Int(7, 0)) CastleWhiteKingside = false;
        if (to == new Vector2Int(0, 7)) CastleBlackQueenside = false;
        if (to == new Vector2Int(7, 7)) CastleBlackKingside = false;
    }
}
