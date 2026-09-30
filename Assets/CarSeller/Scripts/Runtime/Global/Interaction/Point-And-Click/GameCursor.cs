using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GameCursor : GlobalSingletonBehaviour<GameCursor>
{
    
    public Interactable currentInteractable { get; private set; }
    public Interactable draggedInteractable { get; private set; }

    public LayerMask interactableLayersMask => interactableLayers;

    [SerializeField] private LayerMask interactableLayers;
    public bool use3dPhysics = false;

    [Header("Mouse Gesture Thresholds")]
    [SerializeField] private float mouseDragStartDistancePixels = 10f;
    [SerializeField] private float mouseHoldStartTime = 0.35f;
    [SerializeField] private float mouseHoldMoveTolerancePixels = 6f;
    [SerializeField] private float mouseClickMaxTime = 0.30f;

    [Header("Touch Gesture Thresholds")]
    [SerializeField] private float touchDragStartDistancePixels = 22f;
    [SerializeField] private float touchHoldStartTime = 0.50f;
    [SerializeField] private float touchHoldMoveTolerancePixels = 10f;
    [SerializeField] private float touchClickMaxTime = 0.50f;

    [Header("Behavior Flags")]
    [Tooltip("If true a long press (hold) will still emit a click on release.")]
    [SerializeField] private bool clickAfterHold = false;
    [Tooltip("Log gesture decisions for debugging.")]
    [SerializeField] private bool debugGestures = false;

    // Active thresholds for current press
    private float dragStartDistancePixels;
    private float holdStartTime;
    private float holdMoveTolerancePixels;
    private float clickMaxTime;

    private bool dragStarted;
    private Vector2 lastScreenPosition;
    private bool pointerWasPressed;
    private bool isPointerDown;

    private Vector2 pressScreenPosition;
    private float pressTimeUnscaled;
    private Interactable pressInteractable;
    private bool holdActive;
    private bool isTouchPress;

    // Public read-only flag to let camera movement know when interactions are happening
    public bool IsInteracting
    {
        get
        {
            // Interaction is considered active while a press is ongoing on an interactable,
            // or when a drag is in progress, or while a hold is active.
            // Also treat UI suppression path as interaction (press over UI).
            bool pressOnInteractable = isPointerDown && (pressInteractable != null || currentInteractable != null);
            bool dragging = draggedInteractable != null;
            bool holding = isPointerDown && holdActive;
            return pressOnInteractable || dragging || holding;
        }
    }

    protected override GameCursor GlobalInstance
    {
        get { return G.GameCursor;}
        set { G.GameCursor = value; }
    }

    private void Start()
    {
        //Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        HandlePointerInput();
        updateWorldPosition();
        processDrag();
        processHold();
    }

    private bool IsTouchControlScheme()
    {
        var locator = G.PlayerInputLocator;
        var playerInput = locator != null ? locator.PlayerInput : null;
        var scheme = playerInput != null ? playerInput.currentControlScheme : null;
        if (string.IsNullOrEmpty(scheme)) return false;
        return scheme.IndexOf("touch", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static int GetCanvasSortingOrder(Canvas canvas)
    {
        if (canvas == null) return int.MinValue;

        // For ScreenSpaceOverlay, sortingOrder is the meaningful value.
        // For other modes, sortingOrder is still used (plus sortingLayer), but this keeps it simple and deterministic.
        return canvas.sortingOrder;
    }

    private static int GetTopmostUISortingOrder(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return int.MinValue;

        var eventData = new PointerEventData(EventSystem.current) { position = screenPosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        int topOrder = int.MinValue;

        for (int i = 0; i < results.Count; i++)
        {
            var go = results[i].gameObject;
            if (go == null) continue;

            // Closest canvas in parents is the canvas controlling the draw order.
            var canvas = go.GetComponentInParent<Canvas>();
            if (canvas == null) continue;

            int order = GetCanvasSortingOrder(canvas);
            if (order > topOrder)
            {
                topOrder = order;
            }
        }

        return topOrder;
    }

    private bool ShouldSuppressWorldInputDueToUI(Vector2 screenPosition, Interactable hoveredInteractable)
    {
        int topUiOrder = GetTopmostUISortingOrder(screenPosition);
        if (topUiOrder == int.MinValue)
            return false; // no UI hit

        // If there is no hovered interactable, UI always wins.
        if (hoveredInteractable == null)
            return true;

        // Only suppress world input if UI is on top of (or equal to) the interactable.
        // If interactable has higher "sortingOrder", it should receive input even if pointer is over UI.
        return topUiOrder >= hoveredInteractable.sortingOrder;
    }

    private void HandlePointerInput()
    {
        bool pointerPressed;
        bool pointerPressedThisFrame = false;
        bool pointerReleasedThisFrame = false;
        Vector2 pointerPosition;

        bool schemeIsTouch = IsTouchControlScheme();

        if (schemeIsTouch && Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            pointerPosition = touch.position.ReadValue();
            pointerPressed = touch.press.isPressed;
            pointerPressedThisFrame = touch.press.wasPressedThisFrame;
            pointerReleasedThisFrame = touch.press.wasReleasedThisFrame ||
                                       touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled;
            isTouchPress = true;
        }
        else if (Mouse.current != null)
        {
            pointerPosition = Mouse.current.position.ReadValue();
            pointerPressed = Mouse.current.leftButton.isPressed;
            pointerPressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
            pointerReleasedThisFrame = Mouse.current.leftButton.wasReleasedThisFrame;
            isTouchPress = false;
        }
        else
        {
            return;
        }

        lastScreenPosition = pointerPosition;

        Vector2 worldPosition = ScreenToWorld(lastScreenPosition);

        // Determine hovered interactable first (needed to decide UI suppression based on sorting order).
        currentInteractable = updateCurrentInteractable(currentInteractable, worldPosition, interactableLayers);

        // If pointer is over UI whose canvas order is >= hovered interactable, suppress world interactions.
        if (ShouldSuppressWorldInputDueToUI(lastScreenPosition, currentInteractable))
        {
            if (currentInteractable != null)
            {
                currentInteractable.CursorExit();
                currentInteractable = null;
            }

            // If a press started over UI, we ignore it and reset our gesture state.
            if (pointerPressedThisFrame)
            {
                isPointerDown = false;
                pressInteractable = null;
                draggedInteractable = null;
                dragStarted = false;
                holdActive = false;
            }

            // Also treat release-over-UI as a hard cancel of any ongoing gesture.
            if (pointerReleasedThisFrame && isPointerDown)
            {
                isPointerDown = false;
                if (dragStarted && draggedInteractable != null)
                {
                    endDrag();
                }
                if (pressInteractable != null)
                {
                    // Cancel hold without emitting click/select end on UI
                    if (holdActive)
                    {
                        pressInteractable.CursorHoldEnd();
                        holdActive = false;
                    }
                    pressInteractable.CursorSelectEnd();
                }

                pressInteractable = null;
                draggedInteractable = null;
                dragStarted = false;
            }

            pointerWasPressed = pointerPressed;
            return;
        }

        // Press start
        if (pointerPressedThisFrame)
        {
            isPointerDown = true;
            pressScreenPosition = lastScreenPosition;
            pressTimeUnscaled = Time.unscaledTime;
            pressInteractable = currentInteractable;
            dragStarted = false;
            draggedInteractable = null;
            holdActive = false;

            // Apply thresholds based on scheme
            if (schemeIsTouch)
            {
                dragStartDistancePixels = touchDragStartDistancePixels;
                holdStartTime = touchHoldStartTime;
                holdMoveTolerancePixels = touchHoldMoveTolerancePixels;
                clickMaxTime = touchClickMaxTime;
            }
            else
            {
                dragStartDistancePixels = mouseDragStartDistancePixels;
                holdStartTime = mouseHoldStartTime;
                holdMoveTolerancePixels = mouseHoldMoveTolerancePixels;
                clickMaxTime = mouseClickMaxTime;
            }

            if (pressInteractable != null)
            {
                pressInteractable.CursorSelectStart();
            }

            if (debugGestures)
            {
                Debug.Log($"[GameCursor] Press start ({(schemeIsTouch ? "Touch" : "Mouse")}) on {pressInteractable?.name}");
            }
        }

        // Held frame
        if (isPointerDown && !pointerReleasedThisFrame && pressInteractable != null)
        {
            float heldTime = Time.unscaledTime - pressTimeUnscaled;
            float movedPixels = Vector2.Distance(lastScreenPosition, pressScreenPosition);

            if (!dragStarted && movedPixels >= dragStartDistancePixels)
            {
                if (holdActive)
                {
                    pressInteractable.CursorHoldEnd();
                    holdActive = false;
                }

                draggedInteractable = pressInteractable;
                dragStarted = true;
                draggedInteractable.DragStart();

                if (debugGestures)
                {
                    Debug.Log($"[GameCursor] DragStart (moved {movedPixels:0.0}px)");
                }
            }
            else if (!dragStarted)
            {
                if (!holdActive && heldTime >= holdStartTime && movedPixels <= holdMoveTolerancePixels)
                {
                    holdActive = true;
                    pressInteractable.CursorHoldStart();

                    if (debugGestures)
                    {
                        Debug.Log($"[GameCursor] HoldStart (time {heldTime:0.000}s)");
                    }
                }
                else if (holdActive && movedPixels > dragStartDistancePixels)
                {
                    pressInteractable.CursorHoldEnd();
                    holdActive = false;
                }
            }
        }

        // Release
        if (pointerReleasedThisFrame && pointerWasPressed)
        {
            isPointerDown = false;

            float totalHeldTime = Time.unscaledTime - pressTimeUnscaled;
            float movedPixels = Vector2.Distance(lastScreenPosition, pressScreenPosition);

            bool emittedClick = false;

            if (dragStarted && draggedInteractable != null)
            {
                endDrag();
            }
            else if (pressInteractable != null)
            {
                if (holdActive)
                {
                    pressInteractable.CursorHoldEnd();
                    holdActive = false;

                    if (clickAfterHold)
                    {
                        pressInteractable.CursorClick();
                        emittedClick = true;
                    }
                }
                else
                {
                    bool withinMovement = movedPixels <= holdMoveTolerancePixels;
                    bool withinTime = totalHeldTime <= clickMaxTime || schemeIsTouch;
                    if (withinMovement && withinTime)
                    {
                        pressInteractable.CursorClick();
                        emittedClick = true;
                    }
                }
            }

            if (pressInteractable != null)
            {
                pressInteractable.CursorSelectEnd();
            }

            if (debugGestures)
            {
                Debug.Log($"[GameCursor] Release on {pressInteractable?.name} | drag={dragStarted} holdEnded={!holdActive} click={emittedClick} time={totalHeldTime:0.000}s moved={movedPixels:0.0}px");
            }

            pressInteractable = null;
            draggedInteractable = null;
            dragStarted = false;
            holdActive = false;
        }

        pointerWasPressed = pointerPressed;
    }

    private void processDrag()
    {
        if (draggedInteractable != null)
        {
            draggedInteractable.Drag();
        }
    }

    private void processHold()
    {
        if (isPointerDown && holdActive && pressInteractable != null && !dragStarted)
        {
            pressInteractable.CursorHold();
        }
    }

    private void endDrag()
    {
        if (draggedInteractable == null) return;
        draggedInteractable.DragEnd();
        draggedInteractable = null;
        dragStarted = false;
    }

    private Vector2 updateWorldPosition()
    {
        Vector2 worldPostion = ScreenToWorld(lastScreenPosition);
        transform.position = worldPostion;
        return worldPostion;
    }

    private Vector2 ScreenToWorld(Vector2 screenPos)
    {
        var v3 = new Vector3(screenPos.x, screenPos.y, -Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(v3);
    }

    private Interactable updateCurrentInteractable(Interactable current, Vector2 worldPosition, LayerMask layerMask)
    {
        var hitInteracrable = InteractionPhysics.RaycastForInteractable(layerMask, worldPosition, use3dPhysics);

        if (hitInteracrable != current)
        {
            current?.CursorExit();
            hitInteracrable?.CursorEnter();
        }

        return hitInteracrable;
    }

    public void CancelCurrentInteraction(bool invokeDragEnd = true)
    {
        if (dragStarted && draggedInteractable != null)
        {
            if (invokeDragEnd)
            {
                endDrag(); // will call draggedInteractable.DragEnd()
            }
            else
            {
                // Silent cancel: do NOT call DragEnd again.
                draggedInteractable = null;
                dragStarted = false;
            }
        }

        // If there is an ongoing press/hold/select, end it
        if (pressInteractable != null)
        {
            if (holdActive)
            {
                pressInteractable.CursorHoldEnd();
                holdActive = false;
            }

            pressInteractable.CursorSelectEnd();
        }

        // Clear hover
        if (currentInteractable != null)
        {
            currentInteractable.CursorExit();
        }

        // Reset internal state
        currentInteractable = null;
        draggedInteractable = null;
        pressInteractable = null;

        isPointerDown = false;
        dragStarted = false;
        holdActive = false;
        pointerWasPressed = false;
    }
}
