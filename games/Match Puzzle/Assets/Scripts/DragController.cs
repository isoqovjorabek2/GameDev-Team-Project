using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DragController : MonoBehaviour
{
    public float SnapDistance = 0.5f;

    Camera _cam;
    Match _dragging;
    MatchSlot _originalSlot;
    Vector3 _dragOffset;
    SpriteRenderer _dragSprite;
    int _origSortOrder;

    void Awake() { _cam = Camera.main; }

    void Update()
    {
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        bool down = MouseDown();
        bool up = MouseUp();
        Vector3 world = MouseWorld();

        if (_dragging == null && down)
        {
            var hit = Physics2D.OverlapPoint(world);
            if (hit != null)
            {
                var m = hit.GetComponent<Match>();
                if (m != null) BeginDrag(m, world);
            }
        }

        if (_dragging != null)
        {
            _dragging.transform.position = world + _dragOffset;
            if (up) EndDrag();
        }
    }

    void BeginDrag(Match m, Vector3 world)
    {
        _dragging = m;
        _originalSlot = m.CurrentSlot;
        if (m.CurrentSlot != null) m.CurrentSlot.OccupyingMatch = null;
        _dragOffset = m.transform.position - world;
        _dragSprite = m.GetComponent<SpriteRenderer>();
        if (_dragSprite != null)
        {
            _origSortOrder = _dragSprite.sortingOrder;
            _dragSprite.sortingOrder = 100;
        }
    }

    void EndDrag()
    {
        var slot = MatchSlot.FindNearestEmpty(_dragging.transform.position, SnapDistance, _dragging.transform.rotation);
        if (slot == null) slot = _originalSlot;
        if (slot != null)
        {
            _dragging.transform.position = slot.transform.position;
            _dragging.transform.rotation = slot.transform.rotation;
            slot.OccupyingMatch = _dragging;
            _dragging.CurrentSlot = slot;
        }
        if (_dragSprite != null) _dragSprite.sortingOrder = _origSortOrder;
        _dragging = null;
        _dragSprite = null;
        _originalSlot = null;

        if (PuzzleManager.Instance != null) PuzzleManager.Instance.OnMatchMoved();
    }

    Vector3 MouseWorld()
    {
        Vector2 sp = MouseScreen();
        var p = _cam.ScreenToWorldPoint(new Vector3(sp.x, sp.y, -_cam.transform.position.z));
        p.z = 0;
        return p;
    }

    Vector2 MouseScreen()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.position.ReadValue();
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.mousePosition;
#else
        return Vector2.zero;
#endif
    }

    bool MouseDown()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.leftButton.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetMouseButtonDown(0);
#else
        return false;
#endif
    }

    bool MouseUp()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null) return Mouse.current.leftButton.wasReleasedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetMouseButtonUp(0);
#else
        return false;
#endif
    }
}
