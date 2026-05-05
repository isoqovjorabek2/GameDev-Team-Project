using UnityEngine;

public static class ChessGameSanity
{
    public static void RunOnce()
    {
        var b = new ChessBoard();
        b.ResetToStandardStartingPosition();
        var startWhite = RulesEngine.LegalMoves(b);
        if (startWhite.Count != 20)
            Debug.LogError($"ChessGameSanity: white start moves expected 20, got {startWhite.Count}.");

        Move e4Move = default;
        bool foundE4 = false;
        foreach (var m in startWhite)
        {
            if (m.From == new Vector2Int(4, 1) && m.To == new Vector2Int(4, 3) &&
                (m.Flags & MoveFlags.DoublePawnPush) != 0)
            {
                e4Move = m;
                foundE4 = true;
                break;
            }
        }

        if (!foundE4)
            Debug.LogError("ChessGameSanity: e2-e4 not found in legal moves.");
        else
        {
            var afterE4 = b.Clone();
            afterE4.ApplyMove(e4Move);
            var blackReplies = RulesEngine.LegalMoves(afterE4);
            if (blackReplies.Count != 20)
                Debug.LogError($"ChessGameSanity: after 1.e4 black moves expected 20, got {blackReplies.Count}.");
        }

        var pinBoard = new ChessBoard();
        pinBoard.Set(new Vector2Int(4, 0), SquarePiece.Of(PieceKind.King, true));
        pinBoard.Set(new Vector2Int(4, 1), SquarePiece.Of(PieceKind.Queen, true));
        pinBoard.Set(new Vector2Int(4, 7), SquarePiece.Of(PieceKind.Rook, false));
        pinBoard.WhiteToMove = true;
        pinBoard.CastleWhiteKingside = false;
        pinBoard.CastleWhiteQueenside = false;
        pinBoard.CastleBlackKingside = false;
        pinBoard.CastleBlackQueenside = false;
        pinBoard.EnPassantTarget = null;

        var pinLegals = RulesEngine.LegalMoves(pinBoard);
        foreach (var m in pinLegals)
        {
            if (m.From == new Vector2Int(4, 1) && m.To.x != 4)
            {
                Debug.LogError("ChessGameSanity: pinned queen should not leave the e-file.");
                break;
            }
        }
    }
}
