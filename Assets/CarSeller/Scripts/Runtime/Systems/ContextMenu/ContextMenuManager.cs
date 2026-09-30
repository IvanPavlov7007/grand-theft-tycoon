using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Pool; // for LayoutRebuilder

public class ContextMenuManager : GlobalSingletonBehaviour<ContextMenuManager>
{
    protected override ContextMenuManager GlobalInstance { get => G.ContextMenuManager; set => G.ContextMenuManager = value; }

    public GameObject popUpMenuPrefab;
    private List<PopUpContextMenu> activeMenus = new List<PopUpContextMenu>();

    // ----- Layout / Positioning Constants -----
    [Header("Context Menu Layout")]
    [Tooltip("Padding in screen pixels between the menu and the target.")]
    [SerializeField] private float _menuTargetPadding = 8f;

    [Tooltip("Padding in screen pixels between the menu and screen edges.")]
    [SerializeField] private float _screenEdgePadding = 8f;

    [Tooltip("How fast the menu interpolates toward the target position (0-1).")]
    [Range(0.01f, 1f)]
    [SerializeField] private float _positionLerpFactor = 0.25f;

    [SerializeField] private float IdealViewHight = 500f;

    [Header("Context Menu Visuals")]
    [SerializeField]
    private Color UsedContextMenuColor = Color.white;

    public void closeMenu(PopUpContextMenu menu)
    {
        if (activeMenus.Contains(menu))
        {
            activeMenus.Remove(menu);
            G.BlockUIManager.Unblock(G.ContextMenuCanvas.Canvas);
            Destroy(menu.gameObject);
        }
    }

    // TODO make target ILocatable or other most general model-type in the project
    // If needed to find a view based on a model
    public void CreateContextMenu(GameObject target, UIElement content)
    {
        GameObject panel = Instantiate(popUpMenuPrefab, G.ContextMenuCanvas.Canvas.transform);
        RectTransform panelTransform = panel.GetComponent<RectTransform>();

        //Set panel color
        var image = panelTransform.GetComponent<Image>();
        if (image != null)
        {
            image.color = UsedContextMenuColor;
        }

        // Build content first so layout can size to contents
        RectTransform contentRT = getContentTransform(panel);
        SimpleUIBuilder.Instance.Build(content, contentRT);

        // Create menu controller
        var ctxMenu = createContextMenu(panel, target.transform, content.blockingInput);
        ctxMenu.Initialize(target.transform, contentRT, closeMenu);

        UpdateContextMenu(ctxMenu);
        G.GameCursor.CancelCurrentInteraction(invokeDragEnd: false);
    }

    private RectTransform getContentTransform(GameObject panel)
    {
        // Using a LayoutGroup child as Content (VerticalLayoutGroup present)
        return panel.GetComponentInChildren<LayoutGroup>().GetComponent<RectTransform>();
    }

    private void adjustViewRect(RectTransform view, RectTransform content)
    {
        if (view.sizeDelta.y > content.sizeDelta.y)
        {
            var size = view.sizeDelta;
            size.y = content.sizeDelta.y;
            view.sizeDelta = size;
        }
        else if(view.sizeDelta.y < content.sizeDelta.y)
        {
            var size = view.sizeDelta;
            size.y = Mathf.Min(IdealViewHight, content.sizeDelta.y);
            view.sizeDelta = size;
        }
    }

    private void adjustContainerHeight(RectTransform container)
    {
        if (container == null)
            return;

        // Force Unity to rebuild the layout before we measure
        LayoutRebuilder.ForceRebuildLayoutImmediate(container);

        float totalPreferredHeight = 0f;

        // Sum preferred heights of immediate children, including layout info
        foreach (RectTransform child in container)
        {
            if (child == null || !child.gameObject.activeInHierarchy)
                continue;

            float childHeight = 0f;

            // Query all ILayoutElement(s) on the child
            var layoutElements = ListPool<Component>.Get();
            try
            {
                child.GetComponents(typeof(ILayoutElement), layoutElements);

                float preferred = 0f;
                for (int i = 0; i < layoutElements.Count; i++)
                {
                    var layoutElement = layoutElements[i] as ILayoutElement;
                    if (layoutElement == null)
                        continue;

                    // Use preferred if set, otherwise min height
                    float elemPreferred = layoutElement.preferredHeight;
                    if (elemPreferred <= 0f)
                        elemPreferred = layoutElement.minHeight;

                    if (elemPreferred > preferred)
                        preferred = elemPreferred;
                }

                // Fallback if no layout elements or invalid values
                if (preferred <= 0f)
                    preferred = child.sizeDelta.y;

                childHeight = preferred;
            }
            finally
            {
                ListPool<Component>.Release(layoutElements);
            }

            totalPreferredHeight += childHeight;
        }

        // Respect any VerticalLayoutGroup padding + spacing
        var vGroup = container.GetComponent<VerticalLayoutGroup>();
        if (vGroup != null)
        {
            int activeChildCount = 0;
            foreach (RectTransform child in container)
            {
                if (child != null && child.gameObject.activeInHierarchy)
                    activeChildCount++;
            }

            float spacingTotal = Mathf.Max(0, activeChildCount - 1) * vGroup.spacing;
            float paddingTotal = vGroup.padding.top + vGroup.padding.bottom;
            totalPreferredHeight += spacingTotal + paddingTotal;
        }

        // Apply new height
        var sizeDelta = container.sizeDelta;
        sizeDelta.y = totalPreferredHeight;
        container.sizeDelta = sizeDelta;
    }

    private PopUpContextMenu createContextMenu(GameObject panel, Transform target, bool blocking)
    {
        var ctxMenu = panel.AddComponent<PopUpContextMenu>();
        activeMenus.Add(ctxMenu);
        if (blocking)
            G.BlockUIManager.Block(G.ContextMenuCanvas.Canvas, ctxMenu.Close);
        return ctxMenu;
    }

    private void LateUpdate()
    {
        for (int i = 0; i < activeMenus.Count; i++)
        {
            UpdateContextMenu(activeMenus[i]);
        }
    }

    private void UpdateContextMenu(PopUpContextMenu menu)
    {
        var canvas = G.ContextMenuCanvas.Canvas;
        var camera = Camera.main;
        
        Debug.Assert(menu != null, "UpdateContextMenu: menu is null");
        Debug.Assert(canvas != null, "UpdateContextMenu: canvas is null");
        Debug.Assert(camera != null, "UpdateContextMenu: camera is null");
        Debug.Assert(menu.TargetTransform != null, "UpdateContextMenu: menu.TargetTransform is null");
        if (canvas == null || camera == null || menu == null || menu.TargetTransform == null)
            return;

        // First, ensure size is correct based on content
        //adjustContainerHeight(menu.ContentTransform);
        adjustViewRect(menu.RectTransform, menu.ContentTransform);

        // Then, update its position relative to the target
        PositionMenuNextToTarget(menu, canvas, camera);
    }

    /// <summary>
    /// Positions the menu next to its target so:
    /// - It does not cover the target.
    /// - It remains fully within the canvas rect.
    /// - It prefers positions: above, below, right, left (in that order).
    /// </summary>
    private void PositionMenuNextToTarget(PopUpContextMenu menu, Canvas canvas, Camera camera)
    {
        RectTransform canvasRect = canvas.transform as RectTransform;
        RectTransform menuRect = menu.RectTransform;

        // Convert target world position to canvas local space (screen-space canvas assumed).
        Vector2 targetCanvasPos = WorldToCanvasPosition(canvas, camera, menu.TargetTransform.position);

        // Menu size in canvas local space
        Vector2 menuSize = GetRectSizeInCanvasSpace(menuRect);

        // Target "screen rect" approximation in canvas space:
        // If the target has a renderer, use its bounds; otherwise, just use a point with zero size.
        Rect targetRect = GetTargetRectInCanvasSpace(menu.TargetTransform, canvas, camera);

        // Candidate positions (center points) for the menu, in preference order
        Vector2[] candidates = BuildCandidatePositions(targetRect, menuSize);

        // Choose first candidate that keeps the menu fully inside the canvas rect
        Vector2 chosenPos = targetCanvasPos;
        bool found = false;
        foreach (var candidate in candidates)
        {
            if (IsMenuInsideCanvas(candidate, menuSize, canvasRect))
            {
                chosenPos = candidate;
                found = true;
                break;
            }
        }

        // Fallback: clamp chosen position inside canvas
        if (!found)
        {
            chosenPos = ClampMenuInsideCanvas(chosenPos, menuSize, canvasRect);
        }

        // Smoothly interpolate to the new anchored position for a small animation effect
        Vector2 current = menuRect.anchoredPosition;
        Vector2 target = Vector2.Lerp(current, chosenPos, _positionLerpFactor);

        menuRect.anchoredPosition = target;
    }

    private Vector2 WorldToCanvasPosition(Canvas canvas, Camera camera, Vector3 worldPosition)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldPosition);
        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera,
            out localPoint);
        return localPoint;
    }

    private Vector2 GetRectSizeInCanvasSpace(RectTransform rect)
    {
        // For non-scaled, screen-space canvas, sizeDelta is fine.
        // If you later use different canvas scaling, you can extend this.
        return rect.sizeDelta;
    }

    private Rect GetTargetRectInCanvasSpace(Transform target, Canvas canvas, Camera camera)
    {
        var renderer = target.GetComponent<Renderer>();
        RectTransform canvasRect = canvas.transform as RectTransform;

        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;

            // 4 corners of world bounds projected into canvas space
            Vector3[] worldCorners = new Vector3[4];
            worldCorners[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.center.z);
            worldCorners[1] = new Vector3(bounds.min.x, bounds.max.y, bounds.center.z);
            worldCorners[2] = new Vector3(bounds.max.x, bounds.max.y, bounds.center.z);
            worldCorners[3] = new Vector3(bounds.max.x, bounds.min.y, bounds.center.z);

            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < 4; i++)
            {
                Vector2 local = WorldToCanvasPosition(canvas, camera, worldCorners[i]);
                min = Vector2.Min(min, local);
                max = Vector2.Max(max, local);
            }

            Rect r = new Rect(min, max - min);
            return r;
        }
        else
        {
            // No renderer, treat as a point
            Vector2 center = WorldToCanvasPosition(canvas, camera, target.position);
            float size = 1f; // small pseudo-size
            return new Rect(center - Vector2.one * (size * 0.5f), Vector2.one * size);
        }
    }

    private Vector2[] BuildCandidatePositions(Rect targetRect, Vector2 menuSize)
    {
        // Compute basic offsets to keep menu next to the target
        float verticalOffset = (targetRect.height * 0.5f) + (menuSize.y * 0.5f) + _menuTargetPadding;
        float horizontalOffset = (targetRect.width * 0.5f) + (menuSize.x * 0.5f) + _menuTargetPadding;

        Vector2 targetCenter = targetRect.center;

        // Order: above, below, right, left
        Vector2 above = targetCenter + new Vector2(0f, verticalOffset);
        Vector2 below = targetCenter - new Vector2(0f, verticalOffset);
        Vector2 right = targetCenter + new Vector2(horizontalOffset, 0f);
        Vector2 left = targetCenter - new Vector2(horizontalOffset, 0f);

        return new[]
        {
            above,  // Top preferred
            below,  // Then bottom
            right,  // Then right
            left    // Finally left
        };
    }

    private bool IsMenuInsideCanvas(Vector2 menuCenter, Vector2 menuSize, RectTransform canvasRect)
    {
        // Canvas rect in local space
        Rect canvasBounds = new Rect(
            canvasRect.rect.xMin + _screenEdgePadding,
            canvasRect.rect.yMin + _screenEdgePadding,
            canvasRect.rect.width - 2f * _screenEdgePadding,
            canvasRect.rect.height - 2f * _screenEdgePadding);

        float halfW = menuSize.x * 0.5f;
        float halfH = menuSize.y * 0.5f;

        float left = menuCenter.x - halfW;
        float right = menuCenter.x + halfW;
        float bottom = menuCenter.y - halfH;
        float top = menuCenter.y + halfH;

        return left >= canvasBounds.xMin &&
               right <= canvasBounds.xMax &&
               bottom >= canvasBounds.yMin &&
               top <= canvasBounds.yMax;
    }

    private Vector2 ClampMenuInsideCanvas(Vector2 menuCenter, Vector2 menuSize, RectTransform canvasRect)
    {
        Rect canvasBounds = new Rect(
            canvasRect.rect.xMin + _screenEdgePadding,
            canvasRect.rect.yMin + _screenEdgePadding,
            canvasRect.rect.width - 2f * _screenEdgePadding,
            canvasRect.rect.height - 2f * _screenEdgePadding);

        float halfW = menuSize.x * 0.5f;
        float halfH = menuSize.y * 0.5f;

        float x = Mathf.Clamp(menuCenter.x, canvasBounds.xMin + halfW, canvasBounds.xMax - halfW);
        float y = Mathf.Clamp(menuCenter.y, canvasBounds.yMin + halfH, canvasBounds.yMax - halfH);

        return new Vector2(x, y);
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
}