using UnityEngine;

[CreateAssetMenu(fileName = "PieceData", menuName = "Chess/PieceData")]
public class PieceData : ScriptableObject
{
    public enum PieceType { Pawn, Knight, Bishop, Rook, Queen, King }
    public enum PieceColor { White, Black }
    public PieceType pieceType;
    public PieceColor pieceColor;
    public GameObject piecePrefab;
     public Vector2Int position;
}