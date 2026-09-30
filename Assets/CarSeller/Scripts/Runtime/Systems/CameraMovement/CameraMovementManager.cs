using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraMovementManager : GlobalSingletonBehaviour<CameraMovementManager>
{
    protected override CameraMovementManager GlobalInstance { get => G.CameraMovementManager; set => G.CameraMovementManager = value; }

    [Header("References")]
    public Transform cameraTarget;
    public GameObject camConfiner; // GameObject holding the 2D collider used by CinemachineConfiner2D
    public CinemachineCamera virtualCamera;

    [Header("Drag Settings")]
    [Tooltip("Enable/disable camera dragging.")]
    public bool enableDrag = true;
    [Tooltip("Multiplier to tune drag responsiveness.")]
    [Range(0.05f, 5f)]
    public float dragSensitivity = 1.0f;
    [Tooltip("Curve remapping pointer delta magnitude (in world units) to applied delta. Optional softening at small deltas.")]
    public AnimationCurve sensitivityCurve = AnimationCurve.Linear(0, 1, 1, 1);
    [Tooltip("When true, dragging is disabled while the pointer is over UI.")]
    public bool blockDragOverUI = true;

    [Header("Feel")]
    [Tooltip("Apply simple inertia/smoothing to camera target movement.")]
    public bool useInertia = true;
    [Range(0f, 1f)]
    [Tooltip("0 = instant, 1 = very slow. Lerp factor toward target position per frame when inertia is enabled.")]
    public float inertiaLerp = 0.2f;

    [Header("Follow Dragged MovingPoint")]
    [Tooltip("When true, camera target will automatically track a dragged MovingPoint.")]
    public bool followDraggedMovingPoint = true;
    [Tooltip("How strongly the camera centers on the dragged MovingPoint (0 = no follow, 1 = hard lock).")]
    [Range(0f, 1f)]
    public float followStrength = 0.25f;

    [Header("Zoom")]
    [Tooltip("Enable/disable zoom input (wheel/pinch).")]
    public bool enableZoom = true;
    [Tooltip("Min orthographic size (zoomed in). Used only when the camera is orthographic.")]
    public float minOrthoSize = 3.5f;
    [Tooltip("Max orthographic size (zoomed out). Used only when the camera is orthographic.")]
    public float maxOrthoSize = 14f;

    [Header("Perspective Zoom")]
    [Tooltip("Min field of view in degrees (zoomed in). Used only when the camera is perspective.")]
    public float minFieldOfView = 25f;
    [Tooltip("Max field of view in degrees (zoomed out). Used only when the camera is perspective.")]
    public float maxFieldOfView = 70f;

    [Tooltip("Mouse wheel zoom speed (zoom units per wheel step). Units = OrthoSize when orthographic, Degrees when perspective.")]
    public float wheelZoomSpeed = 1.25f;
    [Tooltip("Pinch zoom speed multiplier.")]
    public float pinchZoomSpeed = 0.02f;
    [Tooltip("Zoom smoothing. Higher = snappier.")]
    public float zoomSmoothing = 12f;

    [Header("Game Zoom Bias")]
    [Tooltip("Optional: automatically zoom out based on an externally provided 0..1 value (e.g., speed normalized).")]
    public bool enableGameZoomBias = false;
    [Range(0f, 1f)]
    [Tooltip("How much the game bias can push you toward max zoom out.")]
    public float gameZoomBiasStrength = 0.5f;

    Camera cam => Camera.main;

    // Drag state
    bool _isDragging;
    Vector2 _lastPointerScreenPos;
    Vector3 _pendingTargetPos; // desired target position when using inertia

    // Zoom state
    float _userZoomT; // 0..1 => lerp(min..max) depending on camera mode
    float _gameZoom01; // 0..1 externally set (speed etc)

    // Scripted camera control
    Transform _forcedFollowTarget;
    Vector3? _forcedWorldPosition;
    int _forceToken;
    float _forceBlend; // 0..1 for “how much override is applied”

    protected override void Awake()
    {
        base.Awake();
        if (!IsActiveSingleton) return;

        // If not assigned, try to find a vcam in scene (safe fallback).
        if (virtualCamera == null)
            virtualCamera = FindAnyObjectByType<CinemachineCamera>();

        // Initialize zoom from the authoritative driver (Cinemachine vcam if present; otherwise Camera.main).
        if (virtualCamera != null)
        {
            if (virtualCamera.Lens.Orthographic)
            {
                float s = Mathf.Clamp(virtualCamera.Lens.OrthographicSize, minOrthoSize, maxOrthoSize);
                _userZoomT = Mathf.InverseLerp(minOrthoSize, maxOrthoSize, s);
            }
            else
            {
                float f = Mathf.Clamp(virtualCamera.Lens.FieldOfView, minFieldOfView, maxFieldOfView);
                _userZoomT = Mathf.InverseLerp(minFieldOfView, maxFieldOfView, f);
            }
        }
        else if (cam != null)
        {
            if (cam.orthographic)
            {
                float s = Mathf.Clamp(cam.orthographicSize, minOrthoSize, maxOrthoSize);
                _userZoomT = Mathf.InverseLerp(minOrthoSize, maxOrthoSize, s);
            }
            else
            {
                float f = Mathf.Clamp(cam.fieldOfView, minFieldOfView, maxFieldOfView);
                _userZoomT = Mathf.InverseLerp(minFieldOfView, maxFieldOfView, f);
            }
        }

        if (cameraTarget != null)
            _pendingTargetPos = cameraTarget.position;
    }

    void Update()
    {
        if (cam == null || cameraTarget == null) return;

        var cursor = G.GameCursor;
        Interactable dragged = cursor != null ? cursor.draggedInteractable : null;

        if (followDraggedMovingPoint && dragged != null)
        {
            var movingPoint = dragged.GetComponent<MovingPoint>();
            if (movingPoint != null)
            {
                FollowMovingPoint(movingPoint);
            }
        }

        ApplyScriptedPosition();

        // Dragging the camera with pointer is disabled while GameCursor is interacting.
        if (enableDrag)
        {
            HandlePointerPan(cursor);
        }

        if (enableZoom)
        {
            HandleZoomInput();
        }

        ApplyZoom();
    }

    bool IsOrthographicMode()
    {
        if (virtualCamera != null) return virtualCamera.Lens.Orthographic;
        return cam != null && cam.orthographic;
    }

    void GetZoomLimits(out float min, out float max)
    {
        if (IsOrthographicMode())
        {
            min = minOrthoSize;
            max = maxOrthoSize;
            return;
        }

        min = minFieldOfView;
        max = maxFieldOfView;
    }

    void HandleZoomInput()
    {
        // Block zoom when pointer is over UI.
        if (blockDragOverUI)
        {
            Vector2 pointerPos;
            if (Touchscreen.current?.primaryTouch != null)
                pointerPos = Touchscreen.current.primaryTouch.position.ReadValue();
            else if (Mouse.current != null)
                pointerPos = Mouse.current.position.ReadValue();
            else
                pointerPos = default;

            if (IsPointerOverUI(pointerPos)) return;
        }

        GetZoomLimits(out float min, out float max);
        float range = Mathf.Max(0.01f, max - min);

        if (Mouse.current != null)
        {
            float wheel = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(wheel) > 0.01f)
            {
                float deltaT = -wheel * (wheelZoomSpeed / range) * 0.1f;
                _userZoomT = Mathf.Clamp01(_userZoomT + deltaT);
            }
        }

        if (Touchscreen.current != null)
        {
            var t0 = Touchscreen.current.touches.Count > 0 ? Touchscreen.current.touches[0] : null;
            var t1 = Touchscreen.current.touches.Count > 1 ? Touchscreen.current.touches[1] : null;

            if (t0 != null && t1 != null && t0.press.isPressed && t1.press.isPressed)
            {
                Vector2 p0 = t0.position.ReadValue();
                Vector2 p1 = t1.position.ReadValue();

                Vector2 p0Prev = p0 - t0.delta.ReadValue();
                Vector2 p1Prev = p1 - t1.delta.ReadValue();

                float prevDist = Vector2.Distance(p0Prev, p1Prev);
                float currDist = Vector2.Distance(p0, p1);

                float d = currDist - prevDist;
                if (Mathf.Abs(d) > 0.01f)
                {
                    float deltaT = -d * pinchZoomSpeed;
                    _userZoomT = Mathf.Clamp01(_userZoomT + deltaT);
                }
            }
        }
    }

    void ApplyZoom()
    {
        // Compute biased zoom.
        float biasedT = _userZoomT;
        if (enableGameZoomBias)
        {
            float push = Mathf.Clamp01(_gameZoom01) * gameZoomBiasStrength;
            biasedT = Mathf.Lerp(_userZoomT, 1f, push);
        }

        GetZoomLimits(out float min, out float max);

        float targetValue = Mathf.Lerp(min, max, biasedT);
        targetValue = Mathf.Clamp(targetValue, min, max);

        float smooth = 1f - Mathf.Exp(-zoomSmoothing * Time.unscaledDeltaTime);

        // Apply to Cinemachine vcam lens (authoritative in Cinemachine setups).
        if (virtualCamera != null)
        {
            if (virtualCamera.Lens.Orthographic)
            {
                float current = virtualCamera.Lens.OrthographicSize;
                virtualCamera.Lens.OrthographicSize = Mathf.Lerp(current, targetValue, smooth);
            }
            else
            {
                float current = virtualCamera.Lens.FieldOfView;
                virtualCamera.Lens.FieldOfView = Mathf.Lerp(current, targetValue, smooth);
            }

            return;
        }

        // Fallback if you ever run without Cinemachine.
        if (cam != null)
        {
            if (cam.orthographic)
            {
                float current = cam.orthographicSize;
                cam.orthographicSize = Mathf.Lerp(current, targetValue, smooth);
            }
            else
            {
                float current = cam.fieldOfView;
                cam.fieldOfView = Mathf.Lerp(current, targetValue, smooth);
            }
        }
    }

    public void Teleport(Vector2 worldPosition)
    {
        if (cameraTarget == null) return;

        var desired = ClampToConfinerShrunk(new Vector3(worldPosition.x, worldPosition.y, cameraTarget.position.z));
        cameraTarget.position = desired;
        _pendingTargetPos = desired;
        // Critical for Cinemachine: prevents damping/interpolation from the previous frame.
        if (virtualCamera != null)
            virtualCamera.PreviousStateIsValid = false;
    }

    public void SetGameZoom01(float zoom01)
    {
        _gameZoom01 = Mathf.Clamp01(zoom01);
    }

    float GetCurrentFieldOfView()
    {
        if (virtualCamera != null) return virtualCamera.Lens.FieldOfView;
        if (cam != null) return cam.fieldOfView;
        return 60f;
    }

    float GetCurrentOrthoSize()
    {
        // NOTE: despite the name, this returns the camera half-height (in world units)
        // on the cameraTarget plane. For orthographic cameras it's orthographicSize;
        // for perspective cameras it's computed from FOV and distance.
        if (IsOrthographicMode())
        {
            if (virtualCamera != null && virtualCamera.Lens.Orthographic)
                return virtualCamera.Lens.OrthographicSize;

            if (cam != null && cam.orthographic)
                return cam.orthographicSize;

            return 5f;
        }

        if (cam == null || cameraTarget == null) return 5f;

        float distance = Mathf.Abs(cameraTarget.position.z - cam.transform.position.z);
        float fov = Mathf.Clamp(GetCurrentFieldOfView(), 1f, 179f);
        float halfHeight = Mathf.Tan(0.5f * fov * Mathf.Deg2Rad) * distance;
        return Mathf.Max(0.0001f, halfHeight);
    }

    Vector2 GetCameraHalfExtentsWorld()
    {
        // Important: use the size that actually drives the picture.
        float halfHeight = GetCurrentOrthoSize();
        float halfWidth = halfHeight * (cam != null ? cam.aspect : 1f);
        return new Vector2(halfWidth, halfHeight);
    }

    void ApplyScriptedPosition()
    {
        if (_forcedFollowTarget == null && _forcedWorldPosition == null)
        {
            _forceBlend = Mathf.MoveTowards(_forceBlend, 0f, Time.unscaledDeltaTime * 6f);
            return;
        }

        _forceBlend = Mathf.MoveTowards(_forceBlend, 1f, Time.unscaledDeltaTime * 6f);

        Vector3 desired;
        if (_forcedFollowTarget != null) desired = _forcedFollowTarget.position;
        else desired = _forcedWorldPosition.Value;

        desired = ClampToConfinerShrunk(desired);

        _pendingTargetPos = Vector3.Lerp(_pendingTargetPos, desired, _forceBlend);

        if (useInertia)
        {
            cameraTarget.position = Vector3.Lerp(
                cameraTarget.position,
                _pendingTargetPos,
                1f - Mathf.Pow(1f - inertiaLerp, Time.unscaledDeltaTime * 60f));
        }
        else
        {
            cameraTarget.position = _pendingTargetPos;
        }
    }

    public int AcquireFocus(Vector3 worldPosition)
    {
        _forceToken++;
        _forcedFollowTarget = null;
        _forcedWorldPosition = worldPosition;
        return _forceToken;
    }

    public int AcquireFollow(Transform target)
    {
        _forceToken++;
        _forcedFollowTarget = target;
        _forcedWorldPosition = null;
        return _forceToken;
    }

    public void Release(int token)
    {
        if (token != _forceToken) return;
        _forcedFollowTarget = null;
        _forcedWorldPosition = null;
    }

    // Accurate world conversion for orthographic: project onto the plane of the camera target (its z)
    Vector3 ScreenToWorldOnTargetPlane(Vector2 screenPos)
    {
        float zDistanceFromCamera = cameraTarget.position.z - cam.transform.position.z;
        return cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDistanceFromCamera));
    }

    Vector3 ClampToConfinerShrunk(Vector3 desired)
    {
        if (camConfiner == null) return desired;

        var col2d = camConfiner.GetComponent<Collider2D>();
        if (col2d == null)
        {
            var col3d = camConfiner.GetComponent<Collider>();
            if (col3d == null) return desired;

            Bounds b3 = col3d.bounds;
            Vector2 halfExtents = GetCameraHalfExtentsWorld();
            float minX = b3.min.x + halfExtents.x;
            float maxX = b3.max.x - halfExtents.x;
            float minY = b3.min.y + halfExtents.y;
            float maxY = b3.max.y - halfExtents.y;
            desired.x = Mathf.Clamp(desired.x, minX, maxX);
            desired.y = Mathf.Clamp(desired.y, minY, maxY);
            return desired;
        }

        Bounds b = col2d.bounds;
        Vector2 half = GetCameraHalfExtentsWorld();

        float minX2 = b.min.x + half.x;
        float maxX2 = b.max.x - half.x;
        float minY2 = b.min.y + half.y;
        float maxY2 = b.max.y - half.y;

        if (minX2 > maxX2) desired.x = b.center.x;
        else desired.x = Mathf.Clamp(desired.x, minX2, maxX2);

        if (minY2 > maxY2) desired.y = b.center.y;
        else desired.y = Mathf.Clamp(desired.y, minY2, maxY2);

        return desired;
    }

    // Basic UI check without depending on GameCursor internals
    bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;
        var eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
        {
            position = screenPosition
        };
        var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, results);
        bool over = results.Count > 0;
        results.Clear();
        return over;
    }

    void HandlePointerPan(GameCursor cursor)
    {
        // Choose input source: prefer touch ONLY when an actual touch is active; otherwise mouse.
        bool pointerPressed;
        bool pressedThisFrame;
        bool releasedThisFrame;
        Vector2 pointerPos;

        if (cam == null) return;

        var touch = Touchscreen.current?.primaryTouch;
        bool touchActive = touch != null && (touch.press.isPressed || touch.press.wasPressedThisFrame || touch.press.wasReleasedThisFrame);

        if (touchActive)
        {
            pointerPos = touch.position.ReadValue();
            pointerPressed = touch.press.isPressed;
            pressedThisFrame = touch.press.wasPressedThisFrame;
            releasedThisFrame = touch.press.wasReleasedThisFrame ||
                                touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled;
        }
        else if (Mouse.current != null)
        {
            pointerPos = Mouse.current.position.ReadValue();
            pointerPressed = Mouse.current.leftButton.isPressed;
            pressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
            releasedThisFrame = Mouse.current.leftButton.wasReleasedThisFrame;
        }
        else
        {
            return; // no pointer available
        }

        // Block when GameCursor is interacting (press/drag/hold or UI handling)
        bool cursorBlocking = cursor != null && cursor.IsInteracting;

        // Optionally block when pointer is over UI (when we don’t have cursor or want extra protection)
        bool overUI = false;
        if (blockDragOverUI)
        {
            overUI = IsPointerOverUI(pointerPos);
        }

        // Begin drag only if:
        // - pointer pressed this frame
        // - not interacting via GameCursor
        // - not over UI (if enabled)
        if (pressedThisFrame && !cursorBlocking && !overUI)
        {
            _isDragging = true;
            _lastPointerScreenPos = pointerPos;
            _pendingTargetPos = cameraTarget.position;
        }

        // End drag
        if (releasedThisFrame)
        {
            _isDragging = false;
        }

        // Apply drag movement
        if (_isDragging && pointerPressed)
        {
            // Compute world delta from screen movement on the camera-target plane
            Vector3 prev = ScreenToWorldOnTargetPlane(_lastPointerScreenPos);
            Vector3 curr = ScreenToWorldOnTargetPlane(pointerPos);
            Vector3 worldDelta = curr - prev;

            // Optional sensitivity curve (remap magnitude)
            float mag = worldDelta.magnitude;
            if (mag > 0.0001f)
            {
                float scale = sensitivityCurve != null ? sensitivityCurve.Evaluate(Mathf.Clamp01(mag)) : 1f;
                worldDelta = worldDelta.normalized * (mag * scale);
            }

            // Move follow target opposite to pointer delta (natural pan)
            Vector3 desired = _pendingTargetPos - worldDelta * dragSensitivity;

            // Clamp desired to confiner with camera half-extents considered
            desired = ClampToConfinerShrunk(desired);

            // If scripted control is active, reduce manual pan impact.
            float manualStrength = 1f - Mathf.Clamp01(_forceBlend);
            desired = Vector3.Lerp(_pendingTargetPos, desired, manualStrength);

            // Commit desired (either instantly or via inertia)
            if (useInertia)
            {
                _pendingTargetPos = desired;
                cameraTarget.position = Vector3.Lerp(cameraTarget.position, _pendingTargetPos, 1f - Mathf.Pow(1f - inertiaLerp, Time.unscaledDeltaTime * 60f));
            }
            else
            {
                _pendingTargetPos = desired;
                cameraTarget.position = desired;
            }

            _lastPointerScreenPos = pointerPos;
        }
        else if (useInertia)
        {
            // Continue easing toward pending target when not actively dragging
            cameraTarget.position = Vector3.Lerp(cameraTarget.position, _pendingTargetPos, 1f - Mathf.Pow(1f - inertiaLerp, Time.unscaledDeltaTime * 60f));
        }
    }

    void FollowMovingPoint(MovingPoint movingPoint)
    {
        // Target the MovingPoint's world position.
        Vector3 mpPos = movingPoint.transform.position;

        // Blend from current (or pending) toward moving point by followStrength.
        Vector3 desired = Vector3.Lerp(cameraTarget.position, mpPos, followStrength);

        // Respect confiner.
        desired = ClampToConfinerShrunk(desired);

        _pendingTargetPos = desired;

        if (useInertia)
        {
            cameraTarget.position = Vector3.Lerp(
                cameraTarget.position,
                _pendingTargetPos,
                1f - Mathf.Pow(1f - inertiaLerp, Time.unscaledDeltaTime * 60f));
        }
        else
        {
            cameraTarget.position = desired;
        }
    }
}