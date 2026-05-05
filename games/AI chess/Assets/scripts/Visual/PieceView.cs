using UnityEngine;

public class PieceView : MonoBehaviour
{
    // public ChessPiece pieceData; // Reference to the logical piece data 
    public PieceData pieceData; // Reference to the logical piece data`  
    public SpriteRenderer Sprite; //Sprite for each piece

    public static BoardManager Instance;
    Vector2Int targetPosition;

    public void Initialize(BoardManager board, Vector2Int square, bool isWhite)
    {
        targetPosition = square;
        if (board != null)
        {
            transform.position = board.PieceToWorld(square);
            board.RegisterPieceView(square, this);
        }
    }

    public void Update()
    {
        if (Instance == null) return;
        Vector3 targetWorldPos = Instance.PieceToWorld(targetPosition);
        transform.position = Vector3.Lerp(transform.position, targetWorldPos, Time.deltaTime * 10f);
    }
    
    public void SetTargetPosition(Vector2Int newPos)
    {
        targetPosition = newPos;
    }   

    public void Highlight(bool isHighlighting)
    {
        if (Sprite != null)
        {
            Sprite.color = isHighlighting ? Color.yellow : Color.white;
        }
    }

    public void Capture()
    {
        // Play capture animation or effect here
        // For now, we just destroy the piece
        Destroy(gameObject);
    }

    // Additional methods for promoting pawns, etc. can be added here

    // .....

}
