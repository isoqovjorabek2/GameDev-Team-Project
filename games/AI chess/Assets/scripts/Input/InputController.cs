using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] Camera boardCamera;

    Vector2Int? selectedSquare;

    void Awake()
    {
        if (boardCamera == null)
            boardCamera = Camera.main;
    }

    void Update()
    {
        // Ensure we have  game manager and board
        if (gameManager == null || gameManager.Board == null)
            return;
        // Ensure we have  camera
        if (boardCamera == null)
            return;

        // left mouse click 
        if (!Input.GetMouseButtonDown(0))
            return;
        // mouse position to board square
        var world = BoardManager.ScreenToBoardPlane(boardCamera, Input.mousePosition);
        // if it clicked outside , undone selction
        if (!BoardManager.WorldToBoardSquare(world, out var square))
        {
            ClearSelection();
            return;
        }

        HandleClick(square);
    }

    void ClearSelection()
    {
        selectedSquare = null;
        if (gameManager != null && gameManager.boardManager != null)
            gameManager.boardManager.ClearTileHighlights();
    }

    void HandleClick(Vector2Int square)
    {
        var board = gameManager.Board;
        var bm = gameManager.boardManager;
        var piece = board.Get(square);

        if (!selectedSquare.HasValue)
        {
            if (piece.IsEmpty || piece.IsWhite != board.WhiteToMove)
            {
                ClearSelection();
                return;
            }

            selectedSquare = square;
            bm.ClearTileHighlights();
            HighlightSquare(square, true);
            foreach (var m in gameManager.GetLegalMoves(square))
                HighlightSquare(m.To, true);
            return;
        }

        var from = selectedSquare.Value;

        if (square == from)
        {
            ClearSelection();
            return;
        }

        if (!piece.IsEmpty && piece.IsWhite == board.WhiteToMove)
        {
            selectedSquare = square;
            bm.ClearTileHighlights();
            HighlightSquare(square, true);
            foreach (var m in gameManager.GetLegalMoves(square))
                HighlightSquare(m.To, true);
            return;
        }

        gameManager.TryApplyMove(from, square);
        ClearSelection();
    }

    void HighlightSquare(Vector2Int sq, bool on)
    {
        var tile = gameManager.boardManager.GetTile(sq);
        if (tile != null)
            tile.Highlighter(on);
    }
}
