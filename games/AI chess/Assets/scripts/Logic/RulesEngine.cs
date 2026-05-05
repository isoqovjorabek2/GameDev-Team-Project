using System.Collections.Generic;
using UnityEngine;

public static class RulesEngine
{
    // Checks if a square is attacked by the opponent
    public static bool IsSquareAttacked(ChessBoard board, Vector2Int sq, bool attackedByWhite)
    {
        int pawnDir = attackedByWhite ? 1 : -1;
    
        var pawnFrom1 = new Vector2Int(sq.x - 1, sq.y - pawnDir);
        var pawnFrom2 = new Vector2Int(sq.x + 1, sq.y - pawnDir);
        // Check for pawns
        foreach (var pf in new[] { pawnFrom1, pawnFrom2 })
        {
            if (!ChessBoard.InBounds(pf)) continue;
            var p = board.Get(pf);
            if (!p.IsEmpty && p.Kind == PieceKind.Pawn && p.IsWhite == attackedByWhite)
                return true;
        }
          // Check for knights and kings
        foreach (var d in ChessDirections.KnightDeltas)
        {
            var c = sq + d;
            if (!ChessBoard.InBounds(c)) continue;
            var p = board.Get(c);
            if (!p.IsEmpty && p.Kind == PieceKind.Knight && p.IsWhite == attackedByWhite)
                return true;
        }
        // Check for kings
        foreach (var d in ChessDirections.KingDeltas)
        {
            var c = sq + d;
            if (!ChessBoard.InBounds(c)) continue;
            var p = board.Get(c);
            if (!p.IsEmpty && p.Kind == PieceKind.King && p.IsWhite == attackedByWhite)
                return true;
        }
        // check for rooks
        foreach (var d in ChessDirections.RookDirs)
        {
            var cur = sq + d;
            while (ChessBoard.InBounds(cur))
            {
                var p = board.Get(cur);
                if (!p.IsEmpty)
                {
                    if (p.IsWhite == attackedByWhite &&
                        (p.Kind == PieceKind.Rook || p.Kind == PieceKind.Queen))
                        return true;
                    break;
                }
                cur += d;
            }
        }
       // check for bishops
        foreach (var d in ChessDirections.BishopDirs)
        {
            var cur = sq + d;
            while (ChessBoard.InBounds(cur))
            {
                var p = board.Get(cur);
                if (!p.IsEmpty)
                {
                    if (p.IsWhite == attackedByWhite &&
                        (p.Kind == PieceKind.Bishop || p.Kind == PieceKind.Queen))
                        return true;
                    break;
                }
                cur += d;
            }
        }

        return false;
    }
     // Checks if the king of the given color is in check
    public static bool IsInCheck(ChessBoard board, bool whiteKing)
    {
        var k = board.FindKingSquare(whiteKing);
        if (!k.HasValue) return false;
        return IsSquareAttacked(board, k.Value, !whiteKing);
    }

    public static List<Move> LegalMoves(ChessBoard board)
    {
        var pseudo = MoveGenerator.GeneratePseudoLegalMoves(board);
        var legal = new List<Move>(pseudo.Count);
        bool side = board.WhiteToMove;

        foreach (var m in pseudo)
        {
            if ((m.Flags & MoveFlags.CastleKingside) != 0 || (m.Flags & MoveFlags.CastleQueenside) != 0)
            {
                if (!IsCastlingLegal(board, m, side))
                    continue;
            }

            var copy = board.Clone();
            copy.ApplyMove(m);
            if (!IsInCheck(copy, side))
                legal.Add(m);
        }

        return legal;
    }
    
    // Checks if castling is legal for the given move
    static bool IsCastlingLegal(ChessBoard board, Move m, bool whiteKing)
    {
        if (IsInCheck(board, whiteKing))
            return false;

        if ((m.Flags & MoveFlags.CastleKingside) != 0)
        {
            int r = whiteKing ? 0 : 7;
            var f1 = new Vector2Int(5, r);
            var g1 = new Vector2Int(6, r);
            return !IsSquareAttacked(board, f1, !whiteKing) &&
                   !IsSquareAttacked(board, g1, !whiteKing);
        }

        if ((m.Flags & MoveFlags.CastleQueenside) != 0)
        {
            int r = whiteKing ? 0 : 7;
            var d1 = new Vector2Int(3, r);
            var c1 = new Vector2Int(2, r);
            return !IsSquareAttacked(board, d1, !whiteKing) &&
                   !IsSquareAttacked(board, c1, !whiteKing);
        }

        return true;
    }
        // Gets legal moves from a specific square
    public static List<Move> LegalMovesFrom(ChessBoard board, Vector2Int from)
    {
        var all = LegalMoves(board);
        var filtered = new List<Move>();
        foreach (var m in all)
        {
            if (m.From == from)
                filtered.Add(m);
        }
        return filtered;
    }
}
