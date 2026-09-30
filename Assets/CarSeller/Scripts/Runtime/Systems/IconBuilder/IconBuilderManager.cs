using UnityEngine;

public class IconBuilderManager : GlobalSingletonBehaviour<IconBuilderManager>, IProductViewBuilder<Sprite>
{
    protected override IconBuilderManager GlobalInstance { get => G.IconBuilderManager; set => G.IconBuilderManager = value; }

    [Header("Rendering")]
    public Camera iconRenderCamera;
    public Transform buildPosition;

    [Tooltip("Icon texture width in pixels.")]
    [SerializeField] private int width = 256;

    [Tooltip("Icon texture height in pixels.")]
    [SerializeField] private int height = 256;

    [Tooltip("Transparent background color for the icon render.")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0);

    [Tooltip("Extra padding around the object (% of the largest bound dimension).")]
    [Range(0f, 0.5f)]
    [SerializeField] private float padding = 0.08f;

    [Tooltip("Pixels-per-unit used when creating the resulting Sprite.")]
    [SerializeField] private float spritePixelsPerUnit = 100f;

    [Tooltip("If true and the camera is orthographic, the object will be auto-scaled to fit the view with padding.")]
    [SerializeField] private bool fitToView = true;

    WarehouseProductGameObjectBuilder gameObjectBuilder => G.monolithWarehouseProductViewBuilder;

    RenderTexture _rt;

    public Sprite BuildCar(Car car)
    {
        var go = gameObjectBuilder.BuildCar(car);
        return BuildIconFromGameObject(go);
    }

    public Sprite BuildCarFrame(CarFrame carFrame)
    {
        // Warehouse builder doesn’t support CarFrame views (by design).
        Debug.LogWarning($"IconBuilderManager: Warehouse builder does not support CarFrame. {carFrame?.Name}");
        return null;
    }

    public Sprite BuildEngine(Engine engine)
    {
        return engine.runtimeConfig.Sprite;
    }

    public Sprite BuildSpoiler(Spoiler spoiler)
    {
        return spoiler.runtimeConfig.Sprite;
    }

    public Sprite BuildWheel(Wheel wheel)
    {
        return wheel.runtimeConfig.FrontSideViewSprite;
    }

    // Core flow: stage -> frame -> render -> cleanup
    private Sprite BuildIconFromGameObject(GameObject go)
    {
        if (iconRenderCamera == null || buildPosition == null)
        {
            go.SetActive(false);
            SafeDestroy(go);
            Debug.LogError("IconBuilderManager: iconRenderCamera and buildPosition must be assigned.");
            return null;
        }

        if (go == null)
        {
            Debug.LogError("IconBuilderManager: Source GameObject is null.");
            return null;
        }

        try
        {
            Stage(go);
            CenterOnBounds(go);

            if (fitToView && iconRenderCamera.orthographic)
            {
                FitOrthographic(go, iconRenderCamera, padding);
            }

            return RenderSprite();
        }
        finally
        {
            go.SetActive(false);
            SafeDestroy(go);
        }
    }

    // Place as a child of buildPosition, reset local transform for predictable staging.
    private void Stage(GameObject go)
    {
        go.hideFlags = HideFlags.HideAndDontSave;
        var t = go.transform;
        t.SetParent(buildPosition, worldPositionStays: false);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        // Keep the scale provided by the warehouse builder; we may adjust later if fitting is enabled.
    }

    // Center the staged object so that the rendered bounds are centered at buildPosition.
    private void CenterOnBounds(GameObject go)
    {
        if (!TryGetBounds(go, out var bounds)) return;
        var delta = buildPosition.position - bounds.center;
        go.transform.position += delta;
    }

    // If the camera is orthographic, scale uniformly so the object fits into the camera view with padding.
    private void FitOrthographic(GameObject go, Camera cam, float pad)
    {
        if (!TryGetBounds(go, out var bounds)) return;

        // View size in world units
        float viewHeight = cam.orthographicSize * 2f;
        float viewWidth = viewHeight * cam.aspect;

        // Desired max size considering padding
        float targetWidth = viewWidth * (1f - pad);
        float targetHeight = viewHeight * (1f - pad);

        // Current size
        float sizeX = Mathf.Max(bounds.size.x, 1e-4f);
        float sizeY = Mathf.Max(bounds.size.y, 1e-4f);

        float scaleX = targetWidth / sizeX;
        float scaleY = targetHeight / sizeY;

        float uniformScale = Mathf.Min(scaleX, scaleY);

        // Apply on root uniformly
        go.transform.localScale *= uniformScale;

        // Re-center after scaling (bounds.center changed)
        CenterOnBounds(go);
    }

    private bool TryGetBounds(GameObject go, out Bounds bounds)
    {
        bounds = new Bounds();
        var renderers = go.GetComponentsInChildren<Renderer>(includeInactive: true);
        bool hasAny = false;

        foreach (var r in renderers)
        {
            // Skip non-visible renderers
            if (!r.enabled) continue;

            if (!hasAny)
            {
                bounds = r.bounds;
                hasAny = true;
            }
            else
            {
                bounds.Encapsulate(r.bounds);
            }
        }

        return hasAny;
    }

    private RenderTexture GetOrCreateRT()
    {
        if (_rt != null && (_rt.width != width || _rt.height != height))
        {
            _rt.Release();
            SafeDestroy(_rt);
            _rt = null;
        }

        if (_rt == null)
        {
            _rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 1,
                useMipMap = false,
                autoGenerateMips = false,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            _rt.Create();
        }
        return _rt;
    }

    private Sprite RenderSprite()
    {
        var cam = iconRenderCamera;
        var prevTarget = cam.targetTexture;
        var prevClearFlags = cam.clearFlags;
        var prevBg = cam.backgroundColor;

        var rt = GetOrCreateRT();
        cam.targetTexture = rt;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = backgroundColor;

        // Render immediately
        cam.Render();

        // Read back
        RenderTexture activePrev = RenderTexture.active;
        RenderTexture.active = rt;

        var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
        tex.Apply(false, false);

        // Cleanup camera state
        RenderTexture.active = activePrev;
        cam.targetTexture = prevTarget;
        cam.clearFlags = prevClearFlags;
        cam.backgroundColor = prevBg;

        // --- New: crop to minimal non-transparent rectangle ---
        RectInt cropRect;
        if (TryGetOpaqueBounds(tex, out cropRect))
        {
            int cropWidth = cropRect.width;
            int cropHeight = cropRect.height;

            var cropped = new Texture2D(cropWidth, cropHeight, TextureFormat.RGBA32, false, false);

            // Copy pixels row by row (faster than GetPixels per texel)
            for (int y = 0; y < cropHeight; y++)
            {
                var row = tex.GetPixels(cropRect.x, cropRect.y + y, cropWidth, 1);
                cropped.SetPixels(0, y, cropWidth, 1, row);
            }
            cropped.Apply(false, false);

            // Optionally destroy the original full-size texture to save memory
            SafeDestroy(tex);
            tex = cropped;
        }

        // Create sprite from (possibly) cropped texture
        var sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            spritePixelsPerUnit
        );
        sprite.name = "IconSprite";

        return sprite;
    }

    /// <summary>
    /// Finds the minimal rectangle that contains all pixels with alpha &gt; 0.
    /// Returns false if the entire texture is fully transparent.
    /// </summary>
    private static bool TryGetOpaqueBounds(Texture2D tex, out RectInt rect)
    {
        int w = tex.width;
        int h = tex.height;

        int minX = w;
        int minY = h;
        int maxX = -1;
        int maxY = -1;

        var pixels = tex.GetPixels32(); // single allocation

        for (int y = 0; y < h; y++)
        {
            int rowIndex = y * w;
            for (int x = 0; x < w; x++)
            {
                Color32 c = pixels[rowIndex + x];
                if (c.a == 0) continue;

                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }
        }

        if (maxX < 0 || maxY < 0)
        {
            rect = default;
            return false; // fully transparent
        }

        rect = new RectInt(
            minX,
            minY,
            (maxX - minX + 1),
            (maxY - minY + 1)
        );
        return true;
    }

    private static void SafeDestroy(Object obj)
    {
        if (obj == null) return;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Object.DestroyImmediate(obj);
            return;
        }
#endif
        Object.Destroy(obj);
    }
}