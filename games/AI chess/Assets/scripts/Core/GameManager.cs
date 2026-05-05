using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BoardManager boardManager;

    [SerializeField] AISettings aiSettings;

    public ChessBoard Board { get; private set; }

    ChessAI blackAI;
    bool aiThinking;
    bool gameOver;
    Coroutine aiRoutine;

    public bool CanHumanInteract => Board != null && Board.WhiteToMove && !aiThinking && !gameOver;

    void Start()
    {
        Board = new ChessBoard();
        Board.ResetToStandardStartingPosition();

        gameOver = false;
        blackAI = new ChessAI(aiSettings);

        if (boardManager == null)
        {
            Debug.LogError("GameManager: BoardManager not assigned.");
            return;
        }

        boardManager.CreateChessBoard();
        boardManager.SpawnPiecesFromBoard(Board);

        ChessGameSanity.RunOnce();
    }

    public List<Move> GetLegalMoves(Vector2Int from) => RulesEngine.LegalMovesFrom(Board, from);

    public List<Move> GetAllLegalMoves() => RulesEngine.LegalMoves(Board);

    public bool TryApplyMove(Vector2Int from, Vector2Int to)
    {
        if (boardManager == null || Board == null || !CanHumanInteract)
            return false;

        if (!TryResolveMove(from, to, out Move chosen))
            return false;

        ApplyChosenMove(chosen);
        MaybeScheduleBlack();
        return true;
    }

    bool TryResolveMove(Vector2Int from, Vector2Int to, out Move chosen)
    {
        chosen = default;
        var candidates = RulesEngine.LegalMovesFrom(Board, from);
        bool found = false;

        foreach (var m in candidates)
        {
            if (m.To != to) continue;
            if ((m.Flags & MoveFlags.Promotion) != 0 && m.PromotionTo == PieceKind.Queen)
            {
                chosen = m;
                found = true;
                break;
            }
        }

        if (!found)
        {
            foreach (var m in candidates)
            {
                if (m.To != to) continue;
                chosen = m;
                found = true;
                break;
            }
        }

        return found;
    }

    void ApplyChosenMove(Move chosen)
    {
        Board.ApplyMove(chosen);
        boardManager.SpawnPiecesFromBoard(Board);
        LogCheckmateOrStalemateIfOver();
    }

    /// <summary>After a move, the side to move is the opponent. Detect check, then checkmate/stalemate.</summary>
    void LogCheckmateOrStalemateIfOver()
    {
        if (Board == null)
            return;

        var legal = RulesEngine.LegalMoves(Board);
        bool sideToMove = Board.WhiteToMove;
        bool inCheck = RulesEngine.IsInCheck(Board, sideToMove);

        if (legal.Count == 0)
        {
            if (inCheck)
            {
                gameOver = true;
                if (sideToMove)
                    Debug.Log("Checkmate — Black wins.");
                else
                    Debug.Log("Checkmate — White wins.");
            }
            else
            {
                gameOver = true;
                Debug.Log("Stalemate — draw.");
            }

            return;
        }

        if (inCheck)
        {
            if (sideToMove)
                Debug.Log("Check — White king is in check (not checkmate).");
            else
                Debug.Log("Check — Black king is in check (not checkmate).");
        }
    }

    void MaybeScheduleBlack()
    {
        if (Board == null || Board.WhiteToMove || gameOver)
            return;

        if (RulesEngine.LegalMoves(Board).Count == 0)
            return;

        if (aiRoutine != null)
            StopCoroutine(aiRoutine);

        aiThinking = true;
        aiRoutine = StartCoroutine(PlayBlackMoveRoutine());
    }

    IEnumerator PlayBlackMoveRoutine()
    {
        yield return null;

        var legal = RulesEngine.LegalMoves(Board);
        if (legal.Count == 0)
        {
            aiThinking = false;
            aiRoutine = null;
            yield break;
        }

        Move best = blackAI.GetBestMove(Board);
        bool ok = false;
        foreach (var m in legal)
        {
            if (m.From == best.From && m.To == best.To && m.Flags == best.Flags && m.PromotionTo == best.PromotionTo)
            {
                best = m;
                ok = true;
                break;
            }
        }

        if (!ok)
            best = legal[0];

        ApplyChosenMove(best);

        aiThinking = false;
        aiRoutine = null;
    }
}
