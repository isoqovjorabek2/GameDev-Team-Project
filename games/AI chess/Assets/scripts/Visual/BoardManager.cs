using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject TilePrefab;
    public Transform TileParent;
    public Transform PieceParent;

    [Tooltip("Optional legacy list; spawning uses prefab arrays below.")]
    public List<PieceData> pieceDatas;

    [Tooltip("Order: Pawn, Knight, Bishop, Rook, Queen, King (index = PieceKind enum − 1)")]
    public GameObject[] whitePiecePrefabs = new GameObject[6];
    [Tooltip("Order: Pawn, Knight, Bishop, Rook, Queen, King")]
    public GameObject[] blackPiecePrefabs = new GameObject[6];

    public BoardTheme boardTheme;
    string[] BoardLetters = { "a", "b", "c", "d", "e", "f", "g", "h" };
    string[] BoardDigits = { "1", "2", "3", "4", "5", "6", "7", "8" };

    Dictionary<Vector2Int, TileView> tiles = new Dictionary<Vector2Int, TileView>();
    readonly Dictionary<Vector2Int, PieceView> pieceViews = new Dictionary<Vector2Int, PieceView>();

    void Awake()
    {
        PieceView.Instance = this;
    }

    public const float PieceZOffset = -0.1f;

    static readonly Color DefaultLightSquare = new Color(0.941f, 0.851f, 0.710f, 1f);
    static readonly Color DefaultDarkSquare = new Color(0.710f, 0.533f, 0.388f, 1f);

    static Color ResolveDarkSquareColor(BoardTheme theme)
    {
        Color c = theme != null ? theme.darkSquare : DefaultDarkSquare;
        if (c.r + c.g + c.b < 0.08f)
            return DefaultDarkSquare;
        return c;
    }

    static Color ResolveLightSquareColor(BoardTheme theme)
    {
        Color c = theme != null ? theme.lightSquare : DefaultLightSquare;
        return c;
    }

    public Vector3 BoardtoWorld(Vector2Int pos)
    {
        return new Vector3(pos.x, pos.y, 0);
    }

    public Vector3 PieceToWorld(Vector2Int pos)
    {
        return new Vector3(pos.x, pos.y, PieceZOffset);
    }

    public void CreateChessBoard()
    {
        Debug.Log("CreateChessBoard called");

        if (TilePrefab == null)
        {
            Debug.LogError("TilePrefab is NULL! Assign it in the Inspector.");
            return;
        }

        if (TileParent == null)
        {
            Debug.LogError("TileParent is NULL! Assign it in the Inspector.");
            return;
        }

        tiles.Clear();
    
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GameObject tileObj = Instantiate(TilePrefab, TileParent);
                tileObj.name = BoardLetters[i] + BoardDigits[j];

                tileObj.transform.position = BoardtoWorld(new Vector2Int(i, j));

                TileView tile = tileObj.GetComponent<TileView>();
                if (tile == null)
                {
                    Debug.LogError($"TilePrefab missing TileView component at {i},{j}");
                    continue;
                }

                tile.boardPosition = new Vector2Int(i, j);

                if (!tiles.ContainsKey(tile.boardPosition))
                    tiles.Add(tile.boardPosition, tile);
                else
                    Debug.LogWarning($"Duplicate tile at {i},{j}");

                SpriteRenderer rend = tileObj.GetComponentInChildren<SpriteRenderer>();

                if (rend != null)
                {
                    bool isDark = (i + j) % 2 == 0;
                    Color lightCol = ResolveLightSquareColor(boardTheme);
                    Color darkCol = ResolveDarkSquareColor(boardTheme);
                    Color c = isDark ? darkCol : lightCol;
                    rend.color = c;
                    if (tile != null)
                        tile.SetSquareVisual(rend, c);
                }
            }
        }

        Debug.Log("Chessboard created with " + tiles.Count + " tiles");
    }

    public void SpawnPiecesFromBoard(ChessBoard board)
    {
        if (PieceParent == null)
        {
            Debug.LogError("PieceParent is NULL! Assign it in the Inspector.");
            return;
        }

        foreach (Transform child in PieceParent)
            Destroy(child.gameObject);

        pieceViews.Clear();

        for (int r = 0; r < ChessBoard.Size; r++)
        for (int f = 0; f < ChessBoard.Size; f++)
        {
            var sq = new Vector2Int(f, r);
            var pc = board.Get(sq);
            if (pc.IsEmpty) continue;

            var prefab = GetPrefab(pc.Kind, pc.IsWhite);
            if (prefab == null)
            {
                Debug.LogWarning($"Missing piece prefab for {pc.Kind} {(pc.IsWhite ? "white" : "black")} at {sq}");
                continue;
            }

            var world = PieceToWorld(sq);
            var go = Instantiate(prefab, world, Quaternion.identity, PieceParent);
            go.name = $"{pc.Kind}_{(pc.IsWhite ? "W" : "B")}_{sq.x},{sq.y}";

            var view = go.GetComponent<PieceView>();
            if (view != null)
                view.Initialize(this, sq, pc.IsWhite);
            else
                go.transform.position = world;
        }
    }

    GameObject GetPrefab(PieceKind kind, bool white)
    {
        int i = (int)kind - 1;
        if (i < 0 || i >= 6) return null;
        var arr = white ? whitePiecePrefabs : blackPiecePrefabs;
        if (arr == null || i >= arr.Length) return null;
        return arr[i];
    }

    public TileView GetTile(Vector2Int pos)
    {
        if (tiles.TryGetValue(pos, out var tile))
            return tile;

        Debug.LogWarning("No tile found at " + pos);
        return null;
    }

    public void ClearTileHighlights()
    {
        foreach (var tile in tiles.Values)
        {
            if (tile != null)
                tile.Highlighter(false);
        }
    }

    public static bool WorldToBoardSquare(Vector3 worldPosition, out Vector2Int square)
    {
        int file = Mathf.RoundToInt(worldPosition.x);
        int rank = Mathf.RoundToInt(worldPosition.y);
        if (file >= 0 && file < ChessBoard.Size && rank >= 0 && rank < ChessBoard.Size)
        {
            square = new Vector2Int(file, rank);
            return true;
        }

        square = default;
        return false;
    }

    public static Vector3 ScreenToBoardPlane(Camera cam, Vector3 screenPosition)
    {
        screenPosition.z = Mathf.Abs(cam.transform.position.z);
        return cam.ScreenToWorldPoint(screenPosition);
    }

    public bool TryGetPieceView(Vector2Int square, out PieceView view) =>
        pieceViews.TryGetValue(square, out view);

    public void RegisterPieceView(Vector2Int square, PieceView view)
    {
        pieceViews[square] = view;
    }

    public void RelocatePieceView(Vector2Int from, Vector2Int to, PieceView view)
    {
        if (pieceViews.TryGetValue(from, out var existing) && existing == view)
            pieceViews.Remove(from);
        pieceViews[to] = view;
    }
}
