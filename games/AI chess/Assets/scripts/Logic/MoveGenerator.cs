using System.Collections.Generic;
using UnityEngine;

public static class MoveGenerator
{
    public static List<Move> GeneratePseudoLegalMoves(ChessBoard board)
    {
        var list = new List<Move>(64);
        bool side = board.WhiteToMove;

        for (int r = 0; r < ChessBoard.Size; r++)
        for (int f = 0; f < ChessBoard.Size; f++)
        {
            var sq = new Vector2Int(f, r);
            var pc = board.Get(sq);
            if (pc.IsEmpty || pc.IsWhite != side)
                continue;

            switch (pc.Kind)
            {
                case PieceKind.Pawn:
                    AddPawnMoves(board, sq, pc, list);
                    break;
                case PieceKind.Knight:
                    AddStepMoves(board, sq, pc, ChessDirections.KnightDeltas, list);
                    break;
                case PieceKind.Bishop:
                    AddRayMoves(board, sq, pc, ChessDirections.BishopDirs, list);
                    break;
                case PieceKind.Rook:
                    AddRayMoves(board, sq, pc, ChessDirections.RookDirs, list);
                    break;
                case PieceKind.Queen:
                    AddRayMoves(board, sq, pc, ChessDirections.RookDirs, list);
                    AddRayMoves(board, sq, pc, ChessDirections.BishopDirs, list);
                    break;
                case PieceKind.King:
                    AddStepMoves(board, sq, pc, ChessDirections.KingDeltas, list);
                    TryAddCastling(board, sq, pc, list);
                    break;
            }
        }

        return list;
    }
   // The following methods add moves for specific piece types. They are called by GeneratePseudoLegalMoves.
    static void AddPawnMoves(ChessBoard board, Vector2Int from, SquarePiece pc, List<Move> list)
    {
        int dir = pc.IsWhite ? 1 : -1;
        int startRank = pc.IsWhite ? 1 : 6;
        int promoRank = pc.IsWhite ? 7 : 0;

        var one = new Vector2Int(from.x, from.y + dir);
        if (ChessBoard.InBounds(one) && board.Get(one).IsEmpty)
        {
            if (one.y == promoRank)
                AddPromotions(from, one, list);
            else
            {
                list.Add(new Move(from, one));
                if (from.y == startRank)
                {
                    var two = new Vector2Int(from.x, from.y + 2 * dir);
                    if (ChessBoard.InBounds(two) && board.Get(two).IsEmpty)
                        list.Add(new Move(from, two, MoveFlags.DoublePawnPush));
                }
            }
        }

        for (int df = -1; df <= 1; df += 2)
        {
            var cap = new Vector2Int(from.x + df, from.y + dir);
            if (!ChessBoard.InBounds(cap)) continue;

            var target = board.Get(cap);
            if (!target.IsEmpty && target.IsWhite != pc.IsWhite)
            {
                if (cap.y == promoRank)
                    AddPromotions(from, cap, list, isCapture: true);
                else
                    list.Add(new Move(from, cap));
            }
            else if (target.IsEmpty && board.EnPassantTarget.HasValue && cap == board.EnPassantTarget.Value)
            {
                int capRank = pc.IsWhite ? cap.y - 1 : cap.y + 1;
                var behind = new Vector2Int(cap.x, capRank);
                var epPawn = board.Get(behind);
                if (!epPawn.IsEmpty && epPawn.Kind == PieceKind.Pawn && epPawn.IsWhite != pc.IsWhite)
                    list.Add(new Move(from, cap, MoveFlags.EnPassant));
            }
        }
    }

   
     // Adds promotion moves for a pawn moving from 'from' to 'to'. If 'isCapture' is true, these are capture promotions.
    static void AddPromotions(Vector2Int from, Vector2Int to, List<Move> list, bool isCapture = false)
    {
        var kinds = new[] { PieceKind.Queen, PieceKind.Rook, PieceKind.Bishop, PieceKind.Knight };
        foreach (var k in kinds)
            list.Add(new Move(from, to, MoveFlags.Promotion, k));
    }
       
    static void AddStepMoves(ChessBoard board, Vector2Int from, SquarePiece pc, Vector2Int[] deltas, List<Move> list)
    {
        foreach (var d in deltas)
        {
            var to = from + d;
            if (!ChessBoard.InBounds(to)) continue;
            var t = board.Get(to);
            if (t.IsEmpty || t.IsWhite != pc.IsWhite)
                list.Add(new Move(from, to));
        }
    }
    // Adds sliding moves in the given directions until blocked.
    static void AddRayMoves(ChessBoard board, Vector2Int from, SquarePiece pc, Vector2Int[] dirs, List<Move> list)
    {
        foreach (var d in dirs)
        {
            var cur = from + d;
            while (ChessBoard.InBounds(cur))
            {
                var t = board.Get(cur);
                if (t.IsEmpty)
                {
                    list.Add(new Move(from, cur));
                    cur += d;
                    continue;
                }
                if (t.IsWhite != pc.IsWhite)
                    list.Add(new Move(from, cur));
                break;
            }
        }
    }
    
    // Checks if castling is legal and adds the appropriate move if so.
    static void TryAddCastling(ChessBoard board, Vector2Int from, SquarePiece pc, List<Move> list)
    {
        if (pc.Kind != PieceKind.King) return;

        if (pc.IsWhite && from == new Vector2Int(4, 0))
        {
            if (board.CastleWhiteKingside &&
                board.Get(new Vector2Int(5, 0)).IsEmpty &&
                board.Get(new Vector2Int(6, 0)).IsEmpty &&
                board.Get(new Vector2Int(7, 0)).Kind == PieceKind.Rook &&
                board.Get(new Vector2Int(7, 0)).IsWhite)
                list.Add(new Move(from, new Vector2Int(6, 0), MoveFlags.CastleKingside));

            if (board.CastleWhiteQueenside &&
                board.Get(new Vector2Int(1, 0)).IsEmpty &&
                board.Get(new Vector2Int(2, 0)).IsEmpty &&
                board.Get(new Vector2Int(3, 0)).IsEmpty &&
                board.Get(new Vector2Int(0, 0)).Kind == PieceKind.Rook &&
                board.Get(new Vector2Int(0, 0)).IsWhite)
                list.Add(new Move(from, new Vector2Int(2, 0), MoveFlags.CastleQueenside));
        }
        else if (!pc.IsWhite && from == new Vector2Int(4, 7))
        {
            if (board.CastleBlackKingside &&
                board.Get(new Vector2Int(5, 7)).IsEmpty &&
                board.Get(new Vector2Int(6, 7)).IsEmpty &&
                board.Get(new Vector2Int(7, 7)).Kind == PieceKind.Rook &&
                !board.Get(new Vector2Int(7, 7)).IsWhite)
                list.Add(new Move(from, new Vector2Int(6, 7), MoveFlags.CastleKingside));

            if (board.CastleBlackQueenside &&
                board.Get(new Vector2Int(1, 7)).IsEmpty &&
                board.Get(new Vector2Int(2, 7)).IsEmpty &&
                board.Get(new Vector2Int(3, 7)).IsEmpty &&
                board.Get(new Vector2Int(0, 7)).Kind == PieceKind.Rook &&
                !board.Get(new Vector2Int(0, 7)).IsWhite)
                list.Add(new Move(from, new Vector2Int(2, 7), MoveFlags.CastleQueenside));
        }
    }

    /// <summary>Kept for compatibility; generates pseudo-legal moves for the whole side to move.</summary>
    public static List<Move> GenerateMoves(ChessBoard board, ChessPiece piece)
    {
        return GeneratePseudoLegalMoves(board);
    }
}
