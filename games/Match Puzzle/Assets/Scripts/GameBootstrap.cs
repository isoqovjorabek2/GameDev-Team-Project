using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("Initial Puzzle (edit & press Play)")]
    public int LeftDigitValue = 8;
    public int RightDigitValue = 8;
    public int ResultDigitValue = 0;
    public OpType OperatorValue = OpType.Plus;

    [Header("Layout")]
    public float ItemSpacing = 1.9f;
    public Color MatchColor = new Color(1f, 0.6f, 0.25f);
    public Color BackgroundColor = new Color(0.07f, 0.09f, 0.13f);

    static Sprite _rectSprite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        if (Object.FindAnyObjectByType<GameBootstrap>() != null) return;
        var go = new GameObject("GameBootstrap");
        go.AddComponent<GameBootstrap>();
    }

    void Start()
    {
        SetupCamera();
        EnsureDragController();
        BuildPuzzle();
    }

    void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 4f;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BackgroundColor;
    }

    void EnsureDragController()
    {
        if (Object.FindAnyObjectByType<DragController>() != null) return;
        var go = new GameObject("DragController");
        go.AddComponent<DragController>();
    }

    void BuildPuzzle()
    {
        var pmGo = new GameObject("PuzzleManager");
        var pm = pmGo.AddComponent<PuzzleManager>();

        var dLeft  = BuildDigit("Digit_Left",   new Vector3(-2 * ItemSpacing, 0, 0));
        var op     = BuildOperator("Op",        new Vector3(-1 * ItemSpacing, 0, 0));
        var dRight = BuildDigit("Digit_Right",  new Vector3( 0, 0, 0));
        var eq     = BuildOperator("Eq",        new Vector3( 1 * ItemSpacing, 0, 0));
        var dRes   = BuildDigit("Digit_Result", new Vector3( 2 * ItemSpacing, 0, 0));

        pm.LeftDigit = dLeft;
        pm.RightDigit = dRight;
        pm.ResultDigit = dRes;
        pm.Op = op;
        pm.Eq = eq;

        FillDigit(dLeft, LeftDigitValue);
        FillDigit(dRight, RightDigitValue);
        FillDigit(dRes, ResultDigitValue);
        FillOperator(op, OperatorValue);
        FillOperator(eq, OpType.Equals);

        pm.StatusText = BuildStatusText(new Vector3(0, 2.6f, 0));

        BuildHintText(new Vector3(0, -2.8f, 0),
            "Drag matches to rearrange.  Make the equation true.");

        pm.Validate();
    }

    SegmentDigit BuildDigit(string name, Vector3 worldPos)
    {
        var go = new GameObject(name);
        go.transform.position = worldPos;
        var d = go.AddComponent<SegmentDigit>();

        float halfH = 0.7f;     // distance from center to top/bottom segment
        float halfW = 0.45f;    // distance from center to left/right verticals
        float quarterH = 0.35f; // y of top-left / top-right slot centers

        var data = new (Segment seg, Vector3 local, bool horizontal)[]
        {
            (Segment.Top,         new Vector3(0,         halfH, 0), true),
            (Segment.TopLeft,     new Vector3(-halfW,    quarterH, 0), false),
            (Segment.TopRight,    new Vector3( halfW,    quarterH, 0), false),
            (Segment.Middle,      new Vector3(0,         0, 0), true),
            (Segment.BottomLeft,  new Vector3(-halfW,   -quarterH, 0), false),
            (Segment.BottomRight, new Vector3( halfW,   -quarterH, 0), false),
            (Segment.Bottom,      new Vector3(0,        -halfH, 0), true),
        };

        foreach (var item in data)
            d.Slots[item.seg] = CreateSlot(go.transform, item.local, item.horizontal);

        return d;
    }

    EquationOperator BuildOperator(string name, Vector3 worldPos)
    {
        var go = new GameObject(name);
        go.transform.position = worldPos;
        var op = go.AddComponent<EquationOperator>();

        op.Slots[OpSlot.HTop]     = CreateSlot(go.transform, new Vector3(0,  0.3f, 0), true);
        op.Slots[OpSlot.HMiddle]  = CreateSlot(go.transform, new Vector3(0,  0,    0), true);
        op.Slots[OpSlot.HBottom]  = CreateSlot(go.transform, new Vector3(0, -0.3f, 0), true);
        op.Slots[OpSlot.Vertical] = CreateSlot(go.transform, new Vector3(0,  0,    0), false);

        return op;
    }

    MatchSlot CreateSlot(Transform parent, Vector3 localPos, bool horizontal)
    {
        var go = new GameObject("Slot");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = horizontal ? Quaternion.identity : Quaternion.Euler(0, 0, 90);
        go.transform.localScale = Vector3.one;
        var slot = go.AddComponent<MatchSlot>();

        // Ghost outline for empty slot
        var ghost = new GameObject("Ghost");
        ghost.transform.SetParent(go.transform, false);
        ghost.transform.localPosition = Vector3.zero;
        ghost.transform.localRotation = Quaternion.identity;
        ghost.transform.localScale = new Vector3(0.62f, 0.09f, 1);
        var sr = ghost.AddComponent<SpriteRenderer>();
        sr.sprite = GetRectSprite();
        sr.color = new Color(1f, 1f, 1f, 0.07f);
        sr.sortingOrder = 0;
        slot.Ghost = sr;

        return slot;
    }

    void FillDigit(SegmentDigit digit, int value)
    {
        var pattern = SegmentDigit.GetPattern(value);
        foreach (var kv in digit.Slots)
            if (pattern.Contains(kv.Key))
                CreateMatchInSlot(kv.Value);
    }

    void FillOperator(EquationOperator op, OpType type)
    {
        switch (type)
        {
            case OpType.Plus:
                CreateMatchInSlot(op.Slots[OpSlot.HMiddle]);
                CreateMatchInSlot(op.Slots[OpSlot.Vertical]);
                break;
            case OpType.Minus:
                CreateMatchInSlot(op.Slots[OpSlot.HMiddle]);
                break;
            case OpType.Equals:
                CreateMatchInSlot(op.Slots[OpSlot.HTop]);
                CreateMatchInSlot(op.Slots[OpSlot.HBottom]);
                break;
        }
    }

    void CreateMatchInSlot(MatchSlot slot)
    {
        var go = new GameObject("Match");
        go.transform.position = slot.transform.position;
        go.transform.rotation = slot.transform.rotation;
        go.transform.localScale = new Vector3(0.7f, 0.13f, 1);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetRectSprite();
        sr.color = MatchColor;
        sr.sortingOrder = 2;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.05f, 2.2f); // slightly larger than visual for easy clicking

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        var m = go.AddComponent<Match>();
        slot.OccupyingMatch = m;
        m.CurrentSlot = slot;
    }

    Sprite GetRectSprite()
    {
        if (_rectSprite != null) return _rectSprite;
        var tex = new Texture2D(2, 2);
        var white = Color.white;
        tex.SetPixel(0, 0, white); tex.SetPixel(1, 0, white);
        tex.SetPixel(0, 1, white); tex.SetPixel(1, 1, white);
        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();
        _rectSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
        return _rectSprite;
    }

    TextMesh BuildStatusText(Vector3 pos)
    {
        var go = new GameObject("Status");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one;
        var tm = go.AddComponent<TextMesh>();
        tm.text = "...";
        tm.fontSize = 64;
        tm.characterSize = 0.08f;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = new Color(0.7f, 0.7f, 0.75f);

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font != null)
        {
            tm.font = font;
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = font.material;
            mr.sortingOrder = 10;
        }
        return tm;
    }

    void BuildHintText(Vector3 pos, string text)
    {
        var go = new GameObject("Hint");
        go.transform.position = pos;
        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 36;
        tm.characterSize = 0.08f;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = new Color(0.55f, 0.6f, 0.7f);

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font != null)
        {
            tm.font = font;
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = font.material;
            mr.sortingOrder = 10;
        }
    }
}
