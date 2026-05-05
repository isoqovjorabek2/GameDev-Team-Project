public abstract class ChessPiece
{
    public bool IsWhite { get; set; }
    public int Position { get; set; } // Or Vector2Int

   // public abstract List<Move> GetPossibleMoves(ChessBoard board);
}