using System;
using System.Collections.Generic;

public static class Minimax
{
    public const int MateScore = 100000;

    static readonly int[] OrderMaterial = { 0, 100, 320, 330, 500, 900, 100000 };
    // pawn=1 < knight=3.2 < bishop=3.3 < rook=5 < queen=9 < king; index = PieceKind

    /// <summary>Best move for the side to move, using alpha-beta at fixed depth.</summary>
    public static Move FindBestMove(ChessBoard board, int searchDepth)
    {
        var moves = RulesEngine.LegalMoves(board);
        if (moves.Count == 0)
            return default;

        if (searchDepth < 1)
            searchDepth = 1;

        OrderMoves(moves, board);

        bool whiteRoot = board.WhiteToMove;
        Move best = moves[0];
        int bestScore = whiteRoot ? int.MinValue / 4 : int.MaxValue / 4;

        foreach (var m in moves)
        {
            var copy = board.Clone();
            copy.ApplyMove(m);
            int score = AlphaBeta(copy, searchDepth - 1, int.MinValue / 4, int.MaxValue / 4);

            if (whiteRoot)
            {
                if (score > bestScore)
                {
                    bestScore = score;
                    best = m;
                }
            }
            else
            {
                if (score < bestScore)
                {
                    bestScore = score;
                    best = m;
                }
            }
        }

        return best;
    }

    public static int AlphaBeta(ChessBoard board, int depth, int alpha, int beta)
    {
        var moves = RulesEngine.LegalMoves(board);
        if (moves.Count == 0)
        {
            if (RulesEngine.IsInCheck(board, board.WhiteToMove))
                return TerminalMate(board.WhiteToMove);
            return 0;
        }

        if (depth <= 0)
            return Evaluator.Evaluate(board);

        OrderMoves(moves, board);

        if (board.WhiteToMove)
        {
            int maxEval = int.MinValue / 4;
            foreach (var m in moves)
            {
                var copy = board.Clone();
                copy.ApplyMove(m);
                int e = AlphaBeta(copy, depth - 1, alpha, beta);
                maxEval = Math.Max(maxEval, e);
                alpha = Math.Max(alpha, e);
                if (beta <= alpha)
                    break;
            }
            return maxEval;
        }

        int minEval = int.MaxValue / 4;
        foreach (var m in moves)
        {
            var copy = board.Clone();
            copy.ApplyMove(m);
            int e = AlphaBeta(copy, depth - 1, alpha, beta);
            minEval = Math.Min(minEval, e);
            beta = Math.Min(beta, e);
            if (beta <= alpha)
                break;
        }
        return minEval;
    }
    
    static int TerminalMate(bool whiteWasMatedSideToMove)
    {
        // Side to move is in checkmate → losing side; eval is white-centric.
        return whiteWasMatedSideToMove ? -MateScore : MateScore;
    }

    static void OrderMoves(List<Move> moves, ChessBoard board)
    {
        moves.Sort((a, b) => MoveOrderScore(board, b).CompareTo(MoveOrderScore(board, a)));
    }

    static int MoveOrderScore(ChessBoard board, Move m)
    {
        var moving = board.Get(m.From);
        int attacker = OrderMaterial[(int)moving.Kind];
        int score = 0;

        if ((m.Flags & MoveFlags.EnPassant) != 0)
            score += 100 * OrderMaterial[(int)PieceKind.Pawn] - attacker;
        else
        {
            var cap = board.Get(m.To);
            if (!cap.IsEmpty)
                score += 100 * OrderMaterial[(int)cap.Kind] - attacker;
        }

        if ((m.Flags & MoveFlags.Promotion) != 0 && m.PromotionTo == PieceKind.Queen)
            score += 800;

        return score;
    }
}
