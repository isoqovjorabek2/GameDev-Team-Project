public class ChessAI
{
    readonly int searchDepth;

    public ChessAI(AISettings settings)
    {
        if (settings != null && settings.depth > 0)
            searchDepth = settings.depth;
        else
            searchDepth = 4;
    }

    public ChessAI(int depth)
    {
        searchDepth = depth > 0 ? depth : 4;
    }

    public Move GetBestMove(ChessBoard board) => Minimax.FindBestMove(board, searchDepth);
}
