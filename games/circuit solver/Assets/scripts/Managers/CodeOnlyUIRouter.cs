using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CircuitSolver.Managers
{
    /// <summary>
    /// Bypass Unity's EventSystem input module entirely and drive UI
    /// pointer events from code. Reads the mouse (new Input System or
    /// legacy) in Update, performs a GraphicRaycast against the active
    /// Canvas, and dispatches PointerEnter/Exit/Down/Up/Click through
    /// ExecuteEvents so Buttons, TMP_InputFields, ScrollRects, etc.,
    /// all respond without any module configuration.
    /// </summary>
    public class CodeOnlyUIRouter : MonoBehaviour
    {
        EventSystem _eventSystem;
        GraphicRaycaster _raycaster;
        PointerEventData _ped;
        readonly List<RaycastResult> _hits = new List<RaycastResult>();
        GameObject _pressed;
        GameObject _hover;

        void Awake()
        {
            _eventSystem = EventSystem.current;
            if (_eventSystem == null)
            {
                var go = new GameObject("EventSystem", typeof(EventSystem));
                go.transform.SetParent(transform, false);
                _eventSystem = go.GetComponent<EventSystem>();
            }
            _ped = new PointerEventData(_eventSystem);
        }

        void Start()
        {
            // Find GraphicRaycaster in Start to ensure Canvas is fully initialized
            _raycaster = FindFirstObjectByType<GraphicRaycaster>();
            if (_raycaster == null)
            {
                Debug.LogWarning("GraphicRaycaster not found in Start. Will retry in Update.");
            }
        }

        void Update()
        {
            if (_raycaster == null)
            {
                _raycaster = FindFirstObjectByType<GraphicRaycaster>();
                if (_raycaster == null) return; // Early return if still not found
            }

            if (!TryReadMouse(out Vector2 pos, out bool down, out bool up, out float scroll))
                return;

            _ped.position = pos;
            _ped.scrollDelta = new Vector2(0, scroll);
            _ped.button = PointerEventData.InputButton.Left;

            _hits.Clear();
            _raycaster.Raycast(_ped, _hits);
            GameObject top = _hits.Count > 0 ? _hits[0].gameObject : null;

            if (_hits.Count > 0)
            {
                Debug.Log($"Raycast hit: {_hits[0].gameObject.name}");
            }

            if (top != _hover)
            {
                if (_hover != null)
                    ExecuteEvents.ExecuteHierarchy(_hover, _ped, ExecuteEvents.pointerExitHandler);
                if (top != null)
                    ExecuteEvents.ExecuteHierarchy(top, _ped, ExecuteEvents.pointerEnterHandler);
                _hover = top;
            }

            if (down && top != null)
            {
                _pressed = ExecuteEvents.ExecuteHierarchy(top, _ped, ExecuteEvents.pointerDownHandler);
                if (_pressed == null) _pressed = top;

                var sel = ExecuteEvents.GetEventHandler<ISelectHandler>(top);
                if (sel != null && _eventSystem.currentSelectedGameObject != sel)
                    _eventSystem.SetSelectedGameObject(sel, _ped);
                else if (sel == null && _eventSystem.currentSelectedGameObject != null)
                    _eventSystem.SetSelectedGameObject(null, _ped);
            }

            if (up)
            {
                if (_pressed != null)
                    ExecuteEvents.Execute(_pressed, _ped, ExecuteEvents.pointerUpHandler);
                if (top != null && top == _pressed)
                    ExecuteEvents.ExecuteHierarchy(top, _ped, ExecuteEvents.pointerClickHandler);
                // Fallback: some UI objects (e.g. our code-built Buttons) do not
                // propagate clickHandler through ExecuteHierarchy on the first
                // release, so invoke Button.onClick directly as a safety net.
                if (top != null && top == _pressed)
                {
                    var btn = top.GetComponentInParent<Button>();
                    if (btn != null && btn.IsInteractable())
                    {
                        Debug.Log($"Direct button invoke: {btn.gameObject.name}");
                        btn.onClick.Invoke();
                    }
                }
                _pressed = null;
            }

            if (Mathf.Abs(scroll) > 0.0001f && top != null)
                ExecuteEvents.ExecuteHierarchy(top, _ped, ExecuteEvents.scrollHandler);

            // Drive the currently-selected UI object's per-frame update.
            // TMP_InputField uses this hook to poll keyboard events from the
            // IMGUI Event queue, so keyboards work without any input module.
            var selected = _eventSystem.currentSelectedGameObject;
            if (selected != null)
                ExecuteEvents.Execute(selected, _ped, ExecuteEvents.updateSelectedHandler);
        }

        bool TryReadMouse(out Vector2 position, out bool leftDown, out bool leftUp, out float scroll)
        {
            position = Vector2.zero;
            leftDown = false;
            leftUp = false;
            scroll = 0f;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                position = Mouse.current.position.ReadValue();
                leftDown = Mouse.current.leftButton.wasPressedThisFrame;
                leftUp = Mouse.current.leftButton.wasReleasedThisFrame;
                scroll = Mouse.current.scroll.ReadValue().y / 120f;
                return true;
            }
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                position = Touchscreen.current.primaryTouch.position.ReadValue();
                leftDown = Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
                leftUp = Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;
                return true;
            }
            return false;
#else
            position = Input.mousePosition;
            leftDown = Input.GetMouseButtonDown(0);
            leftUp = Input.GetMouseButtonUp(0);
            scroll = Input.mouseScrollDelta.y;
            return true;
#endif
        }
    }
}
