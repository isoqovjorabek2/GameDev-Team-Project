using UnityEngine;

[System.Flags]
public enum MoveFlags
{
    None = 0,
    Promotion = 1,
    CastleKingside = 2,
    CastleQueenside = 4,
    EnPassant = 8,
    DoublePawnPush = 16
}

public struct Move
{
    public Vector2Int From;
    public Vector2Int To;
    public PieceKind PromotionTo;
    public MoveFlags Flags;

    public Move(Vector2Int from, Vector2Int to, MoveFlags flags = MoveFlags.None, PieceKind promotionTo = PieceKind.None)
    {
        From = from;
        To = to;
        Flags = flags;
        PromotionTo = promotionTo;
    }
}
