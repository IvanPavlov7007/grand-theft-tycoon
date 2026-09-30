using UnityEngine;
using UnityEngine.EventSystems;

public class CityUIPinPositioner : MonoBehaviour
{
    public Camera cam { get; private set; }
    public Canvas canvas { get; private set; }
    public Transform origin { get; private set; }
    public Transform target { get; private set; }
    public RectTransform IconRectTransform => iconTransform;
    public RectTransform FrameRectTransform => frameTransform;

    public float screenEdgeMargin = 50f;
    public float targetOnScreenMargin = 50f;

    public bool IsDragging { get; private set; }


    CityUIPinDragHandler dragHandler;
    Quaternion userRotation = Quaternion.identity;

    [SerializeField]
    RectTransform iconTransform;
    [SerializeField]
    RectTransform frameTransform;
    [SerializeField]
    RectTransform circleTransform;

    RectTransform rectTransform;
    Quaternion initialIconRotation;


    public bool ConfinedToScreen { get; set; } = true;
    public GraphicsMode graphicsMode = GraphicsMode.Pin;
    public enum GraphicsMode
    {
        Hidden,
        Circle,
        Pin
    }

    public void SetGraphicsMode(GraphicsMode mode)
    {
        graphicsMode = mode;
        switch (mode)
        {
            case GraphicsMode.Hidden:
                frameTransform.gameObject.SetActive(false);
                circleTransform.gameObject.SetActive(false);
                break;
            case GraphicsMode.Circle:
                frameTransform.gameObject.SetActive(false);
                circleTransform.gameObject.SetActive(true);
                break;
            case GraphicsMode.Pin:
                frameTransform.gameObject.SetActive(true);
                circleTransform.gameObject.SetActive(false);
                break;
        }
    }


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialIconRotation = iconTransform.rotation;
        dragHandler = GetComponentInChildren<CityUIPinDragHandler>();

        if (dragHandler != null)
        {
            dragHandler.OnCustomDrag += OnDrag;
            dragHandler.OnCustomBeginDrag += OnBeginDrag;
            dragHandler.OnCustomEndDrag += OnEndDrag;
        }
    }

    public void Initialize(Camera cam, Canvas canvas, Transform origin, Transform target)
    {
        this.cam = cam;
        this.canvas = canvas;
        this.origin = origin;
        this.target = target;
    }

    private void LateUpdate()
    {
        RelocatePin();
        iconTransform.rotation = initialIconRotation;
    }

    private void RelocateOnScreen()
    {
        Vector2 distanceToTarget = target.position - origin.position;
        Vector2 directionToTarget = distanceToTarget.normalized;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 screenPos;

        screenPos = cam.WorldToScreenPoint(target.position);
        screenPos -= directionToTarget * targetOnScreenMargin;

        SetScreenPosisition(screenPos);
        SetUpRotation(userRotation * Vector2.up);
    }

    private void RelocateOnEdge()
    {
        Vector2 distanceToTarget = target.position - origin.position;
        Vector2 directionToTarget = distanceToTarget.normalized;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        Vector2 screenPos;
        Vector2 edgePoint = GetEdgePoint(directionToTarget, screenSize);
        screenPos = edgePoint - directionToTarget * screenEdgeMargin;
        userRotation = Quaternion.identity; // reset user rotation when target is offscreen

        SetScreenPosisition(screenPos);
        SetUpRotation(-directionToTarget);
    }

    private void SetScreenPosisition(Vector2 screenPosition)
    {
        Camera canvasCamera = canvas.worldCamera != null ? canvas.worldCamera : cam;

        Vector2 localPos;
        RectTransform canvasRect = canvas.transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvasCamera,
            out localPos);

        rectTransform.anchoredPosition = localPos;
    }

    private void SetUpRotation(Vector2 up)
    {
        rectTransform.up = up;
    }

    void RelocatePin()
    {
        Debug.Assert(cam != null, "Camera not set for CityUIPinPositioner");
        Debug.Assert(canvas != null, "Canvas not set for CityUIPinPositioner");

        // Is target within camera world rect?
        // Or if not confined to screen,
        // just treat as on-screen and let it be positioned
        // according to its actual screen position (which may be off-screen).
        if (!ConfinedToScreen || cameraWorldSizeRect(cam).Contains(target.position))
        {
            // Target on screen: start from its screen position
            RelocateOnScreen();
        }
        else
        {
            // Target off screen: clamp to screen edge
            RelocateOnEdge();
        }
    }

    public static Vector2 worldScreenHalfSize(Camera cam)
    {
        if (cam.orthographic)
        {
            float height = cam.orthographicSize;
            float width = height * cam.aspect;

            return new Vector2(width, height);
        }
        else
        {
            // For perspective, use field of view and distance
            float height = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * -cam.transform.position.z;
            float width = height * cam.aspect;
            return new Vector2(width, height);
        }
    }

    public static Rect cameraWorldSizeRect(Camera cam)
    {
        var halfSize = worldScreenHalfSize(cam);
        Rect rect = new Rect(Vector2.zero, halfSize * 2f);
        rect.center = cam.transform.position;
        return rect;
    }

    static Vector2 GetEdgePoint(Vector2 direction, Vector2 screenSize)
    {
        // operate in screen coordinates where (0,0) is bottom-left and (w,h) is top-right
        Vector2 screenCenter = screenSize * 0.5f;

        float tMax = float.MaxValue;

        // Left or Right edge
        if (!Mathf.Approximately(direction.x, 0f))
        {
            float tx1 = (0f - screenCenter.x) / direction.x;           // left edge (x = 0)
            float tx2 = (screenSize.x - screenCenter.x) / direction.x; // right edge (x = width)
            if (tx1 > 0f) tMax = Mathf.Min(tMax, tx1);
            if (tx2 > 0f) tMax = Mathf.Min(tMax, tx2);
        }

        // Top or Bottom edge
        if (!Mathf.Approximately(direction.y, 0f))
        {
            float ty1 = (0f - screenCenter.y) / direction.y;           // bottom edge (y = 0)
            float ty2 = (screenSize.y - screenCenter.y) / direction.y; // top edge (y = height)
            if (ty1 > 0f) tMax = Mathf.Min(tMax, ty1);
            if (ty2 > 0f) tMax = Mathf.Min(tMax, ty2);
        }

        Vector2 edgeOffsetFromCenter = direction * tMax;
        return screenCenter + edgeOffsetFromCenter;
    }

    Vector2 previousDir;
    Quaternion dragRotation;
    const float dragThreshold = 1f;

    public void OnBeginDrag(PointerEventData eventData)
    {
        previousDir = GetDirectionFromCenter(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentDir = GetDirectionFromCenter(eventData.position);

        // angle between previous and current drag direction (signed around Z)
        float deltaAngle = Vector2.SignedAngle(previousDir, currentDir);

        userRotation *= Quaternion.Euler(0f, 0f, deltaAngle);

        // accumulate drag rotation for drag threshold check
        dragRotation = Quaternion.Euler(0f, 0f, deltaAngle);
        if(Mathf.Abs(dragRotation.eulerAngles.z) > dragThreshold)
            IsDragging = true;

        previousDir = currentDir;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;
        dragRotation = Quaternion.identity;
    }

    Vector2 GetDirectionFromCenter(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            frameTransform,
            screenPos,
            cam,
            out Vector3 worldPoint);
        return (worldPoint - rectTransform.position).normalized;
    }

    internal void update()
    {
        LateUpdate();
    }
}