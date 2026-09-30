using System;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class CarSpriteStaticRenderer : MonoBehaviour
{
    [TitleGroup("Cameras")]
    public Camera topCamera;

    [TitleGroup("Cameras")]
    public Camera sideCamera;

    [TitleGroup("Staging")]
    [Tooltip("Where the prefab will be instantiated for rendering. If null, this GameObject transform is used.")]
    public Transform buildPosition;

    [TitleGroup("Input")]
    [AssetsOnly]
    [Tooltip("Optional single prefab to render.")]
    public GameObject prefabToRender;

    [TitleGroup("Input")]
    [AssetsOnly]
    [Tooltip("Optional batch list. If not empty, Render Batch will render these (base name = prefab name).")]
    [ListDrawerSettings(Expanded = true)]
    public GameObject[] prefabsToRender;

    [TitleGroup("Output")]
    [Tooltip("If enabled and rendering a single prefab, base name is taken from prefab name.")]
    [SerializeField] private bool usePrefabNameAsBase = true;

    [TitleGroup("Output")]
    [Tooltip("Used when usePrefabNameAsBase is disabled, or when no prefab is assigned.")]
    [SerializeField] private string baseFileName = "Car";

    [TitleGroup("Output")]
    [Tooltip("Folder under Assets/Resources to save into. Example: Cars/Icons")]
    [SerializeField] private string resourcesSubFolder = "Cars/Icons";

    [TitleGroup("Output")]
    [SerializeField] private string topSuffix = "top";

    [TitleGroup("Output")]
    [SerializeField] private string sideSuffix = "side";

    [TitleGroup("Render Settings")]
    [SerializeField] private int width = 512;

    [TitleGroup("Render Settings")]
    [SerializeField] private int height = 512;

    [TitleGroup("Render Settings")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0);

    [TitleGroup("Render Settings")]
    [Range(0f, 0.5f)]
    [SerializeField] private float padding = 0.08f;

    [TitleGroup("Render Settings")]
    [Tooltip("If true, the object will be fit to the cameras. Orthographic: uniform scale. Perspective: per-camera depth fit while keeping the object centered.")]
    [SerializeField] private bool fitToView = true;

    [TitleGroup("Render Settings")]
    [Tooltip("If enabled, trims the output PNG to the minimal non-transparent rectangle.")]
    [SerializeField] private bool cropTransparent = false;

#if UNITY_EDITOR
    [TitleGroup("Unity Import (Editor)")]
    [Tooltip("If enabled, imports saved PNGs as Sprite assets (useful for UI/icons).")]
    [SerializeField] private bool importAsSprite = true;

    [TitleGroup("Unity Import (Editor)")]
    [Tooltip("When overwriting an existing asset, keep its current import settings (sprite mode, PPU, etc).")]
    [SerializeField] private bool preserveImporterSettings = true;

    [TitleGroup("Unity Import (Editor)")]
    [Tooltip("Used only when creating a NEW sprite asset (or converting a non-sprite to sprite).")]
    [SerializeField] private float defaultSpritePixelsPerUnit = 100f;
#endif

    RenderTexture _rt;

    [TitleGroup("Actions")]
    [Button(ButtonSizes.Large)]
    public void RenderSingleToPng()
    {
        var prefab = prefabToRender;
        if (prefab == null)
        {
            Debug.LogError($"{nameof(CarSpriteStaticRenderer)}: No prefab assigned.");
            return;
        }

        var baseName = GetBaseNameForSingle(prefab);
        RenderPrefabToPng(prefab, baseName);
    }

    [TitleGroup("Actions")]
    [Button(ButtonSizes.Large)]
    public void RenderBatchToPng()
    {
        if (prefabsToRender == null || prefabsToRender.Length == 0)
        {
            Debug.LogError($"{nameof(CarSpriteStaticRenderer)}: Batch list is empty.");
            return;
        }

        for (int i = 0; i < prefabsToRender.Length; i++)
        {
            var prefab = prefabsToRender[i];
            if (prefab == null) continue;

            RenderPrefabToPng(prefab, SanitizeFileName(prefab.name));
        }
    }

    void RenderPrefabToPng(GameObject prefab, string baseName)
    {
        if (topCamera == null || sideCamera == null)
        {
            Debug.LogError($"{nameof(CarSpriteStaticRenderer)}: Both cameras must be assigned.");
            return;
        }

        if (width <= 0 || height <= 0)
        {
            Debug.LogError($"{nameof(CarSpriteStaticRenderer)}: Invalid texture size {width}x{height}.");
            return;
        }

        var parent = buildPosition != null ? buildPosition : transform;

        GameObject go = null;
        try
        {
            go = Instantiate(prefab);
            Stage(go, parent);
            CenterOnBounds(go, parent);

            bool bothOrtho = topCamera.orthographic && sideCamera.orthographic;
            if (fitToView && bothOrtho)
            {
                FitOrthographicToBoth(go, parent, padding, topCamera, sideCamera);
            }

            var baseline = TransformState.Capture(go.transform);

            var outDirProject = GetOutputDirectoryProjectPath();
            EnsureDirectoryExists(outDirProject);

            var topPath = CombineUnityPath(outDirProject, $"{baseName}_{SanitizeFileName(topSuffix)}.png");
            var sidePath = CombineUnityPath(outDirProject, $"{baseName}_{SanitizeFileName(sideSuffix)}.png");

            RenderAndSaveCamera(go, topCamera, topPath, baseline);
            RenderAndSaveCamera(go, sideCamera, sidePath, baseline);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif

            Debug.Log($"{nameof(CarSpriteStaticRenderer)}: Saved\n- {topPath}\n- {sidePath}");
        }
        finally
        {
            if (go != null)
            {
                go.SetActive(false);
                SafeDestroy(go);
            }
        }
    }

    void RenderAndSaveCamera(GameObject go, Camera cam, string assetPath, TransformState baseline)
    {
        baseline.ApplyTo(go.transform);

        if (fitToView)
        {
            if (cam.orthographic)
            {
                FitOrthographicToCamera(go, buildPosition != null ? buildPosition : transform, cam, padding);
            }
            else
            {
                FitPerspectiveToCamera(go, cam, padding);
            }
        }

        var prevTarget = cam.targetTexture;
        var prevClearFlags = cam.clearFlags;
        var prevBg = cam.backgroundColor;

        var rt = GetOrCreateRT();

        try
        {
            cam.targetTexture = rt;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = backgroundColor;

            cam.Render();

            RenderTexture activePrev = RenderTexture.active;
            RenderTexture.active = rt;

            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
            tex.Apply(false, false);

            RenderTexture.active = activePrev;

            if (cropTransparent && TryGetOpaqueBounds(tex, out var cropRect))
            {
                var cropped = CropTexture(tex, cropRect);
                SafeDestroy(tex);
                tex = cropped;
            }

            var png = tex.EncodeToPNG();
            SafeDestroy(tex);

            var absPath = AssetPathToAbsolutePath(assetPath);

            bool existedBeforeWrite = File.Exists(absPath);
            File.WriteAllBytes(absPath, png);

#if UNITY_EDITOR
            PostprocessWrittenPng(assetPath, existedBeforeWrite);
#endif
        }
        finally
        {
            cam.targetTexture = prevTarget;
            cam.clearFlags = prevClearFlags;
            cam.backgroundColor = prevBg;
        }
    }

#if UNITY_EDITOR
    void PostprocessWrittenPng(string assetPath, bool existedBeforeWrite)
    {
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        if (!importAsSprite)
        {
            return;
        }

        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        if (preserveImporterSettings && existedBeforeWrite)
        {
            // Do not touch importer settings. Overwriting the file + reimport keeps Sprite Mode, PPU, etc.
            return;
        }

        // New asset (or user disabled preserving): apply defaults once.
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
        }

        // Only set "safe defaults" that establish a usable sprite;
        // user can change after first import and it will be preserved on subsequent overwrites.
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = Mathf.Max(1f, defaultSpritePixelsPerUnit);
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;

        importer.SaveAndReimport();
    }
#endif

    void Stage(GameObject go, Transform parent)
    {
        go.hideFlags = HideFlags.HideAndDontSave;

        var t = go.transform;
        t.SetParent(parent, worldPositionStays: false);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
    }

    void CenterOnBounds(GameObject go, Transform parent)
    {
        if (!TryGetBounds(go, out var bounds)) return;
        var delta = parent.position - bounds.center;
        go.transform.position += delta;
    }

    void FitOrthographicToBoth(GameObject go, Transform parent, float pad, Camera camA, Camera camB)
    {
        float scaleA = GetFitOrthographicScale(go, camA, pad);
        float scaleB = GetFitOrthographicScale(go, camB, pad);

        float uniform = Mathf.Min(scaleA, scaleB);
        if (uniform <= 0f || float.IsNaN(uniform) || float.IsInfinity(uniform)) return;

        go.transform.localScale *= uniform;
        CenterOnBounds(go, parent);
    }

    void FitOrthographicToCamera(GameObject go, Transform parent, Camera cam, float pad)
    {
        float uniform = GetFitOrthographicScale(go, cam, pad);
        if (uniform <= 0f || float.IsNaN(uniform) || float.IsInfinity(uniform)) return;

        go.transform.localScale *= uniform;
        CenterOnBounds(go, parent);
    }

    static float GetFitOrthographicScale(GameObject go, Camera cam, float pad)
    {
        if (cam == null || !cam.orthographic) return 1f;
        if (!TryGetBoundsStatic(go, out var bounds)) return 1f;

        float viewHeight = cam.orthographicSize * 2f;
        float viewWidth = viewHeight * cam.aspect;

        float targetWidth = viewWidth * (1f - pad);
        float targetHeight = viewHeight * (1f - pad);

        float sizeX = Mathf.Max(bounds.size.x, 1e-4f);
        float sizeY = Mathf.Max(bounds.size.y, 1e-4f);

        float scaleX = targetWidth / sizeX;
        float scaleY = targetHeight / sizeY;

        return Mathf.Min(scaleX, scaleY);
    }

    static void FitPerspectiveToCamera(GameObject go, Camera cam, float pad)
    {
        if (cam == null || cam.orthographic) return;
        if (!TryGetBoundsStatic(go, out var bounds)) return;

        var centerLocal = cam.transform.InverseTransformPoint(bounds.center);
        var targetCenterWorld = cam.transform.TransformPoint(new Vector3(0f, 0f, centerLocal.z));
        go.transform.position += (targetCenterWorld - bounds.center);

        if (!TryGetBoundsStatic(go, out bounds)) return;

        float vTan = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f);
        float hTan = vTan * cam.aspect;

        float usable = Mathf.Clamp01(1f - pad);
        vTan *= usable;
        hTan *= usable;

        if (vTan <= 1e-6f || hTan <= 1e-6f) return;

        var ext = bounds.extents;
        var c = bounds.center;

        float maxDeltaZ = 0f;
        float minZ = float.PositiveInfinity;

        for (int xi = -1; xi <= 1; xi += 2)
        for (int yi = -1; yi <= 1; yi += 2)
        for (int zi = -1; zi <= 1; zi += 2)
        {
            var cornerWorld = c + Vector3.Scale(ext, new Vector3(xi, yi, zi));
            var p = cam.transform.InverseTransformPoint(cornerWorld);

            float z = Mathf.Max(p.z, 1e-4f);
            minZ = Mathf.Min(minZ, z);

            float needZFromX = Mathf.Abs(p.x) / hTan;
            float needZFromY = Mathf.Abs(p.y) / vTan;

            float needZ = Mathf.Max(needZFromX, needZFromY);
            float deltaZ = needZ - z;

            if (deltaZ > maxDeltaZ) maxDeltaZ = deltaZ;
        }

        float nearMargin = 0.01f;
        float needNearDelta = (cam.nearClipPlane + nearMargin) - minZ;
        if (needNearDelta > maxDeltaZ) maxDeltaZ = needNearDelta;

        if (maxDeltaZ > 0f && !float.IsNaN(maxDeltaZ) && !float.IsInfinity(maxDeltaZ))
        {
            go.transform.position += cam.transform.forward * maxDeltaZ;
        }
    }

    bool TryGetBounds(GameObject go, out Bounds bounds) => TryGetBoundsStatic(go, out bounds);

    static bool TryGetBoundsStatic(GameObject go, out Bounds bounds)
    {
        bounds = new Bounds();
        var renderers = go.GetComponentsInChildren<Renderer>(includeInactive: true);

        bool hasAny = false;
        foreach (var r in renderers)
        {
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

    RenderTexture GetOrCreateRT()
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

    string GetBaseNameForSingle(GameObject prefab)
    {
        if (usePrefabNameAsBase && prefab != null)
        {
            return SanitizeFileName(prefab.name);
        }

        if (!string.IsNullOrWhiteSpace(baseFileName))
        {
            return SanitizeFileName(baseFileName);
        }

        return prefab != null ? SanitizeFileName(prefab.name) : "render";
    }

    string GetOutputDirectoryProjectPath()
    {
        var sub = (resourcesSubFolder ?? string.Empty).Replace("\\", "/").Trim('/');
        return string.IsNullOrEmpty(sub)
            ? "Assets/Resources"
            : CombineUnityPath("Assets/Resources", sub);
    }

    static void EnsureDirectoryExists(string projectDir)
    {
        var abs = AssetPathToAbsolutePath(projectDir);
        if (!Directory.Exists(abs))
        {
            Directory.CreateDirectory(abs);
        }
    }

    static string AssetPathToAbsolutePath(string assetPath)
    {
        var path = assetPath.Replace("\\", "/");
        if (!path.StartsWith("Assets/", StringComparison.Ordinal) && path != "Assets")
        {
            throw new ArgumentException($"Expected an Assets-relative path, got: {assetPath}");
        }

        var projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
        return Path.Combine(projectRoot, path);
    }

    static string CombineUnityPath(params string[] parts)
    {
        return string.Join("/", parts).Replace("\\", "/").Replace("//", "/");
    }

    static string SanitizeFileName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "render";

        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        return name.Trim();
    }

    static Texture2D CropTexture(Texture2D tex, RectInt cropRect)
    {
        int cropWidth = cropRect.width;
        int cropHeight = cropRect.height;

        var cropped = new Texture2D(cropWidth, cropHeight, TextureFormat.RGBA32, false, false);

        for (int y = 0; y < cropHeight; y++)
        {
            var row = tex.GetPixels(cropRect.x, cropRect.y + y, cropWidth, 1);
            cropped.SetPixels(0, y, cropWidth, 1, row);
        }

        cropped.Apply(false, false);
        return cropped;
    }

    static bool TryGetOpaqueBounds(Texture2D tex, out RectInt rect)
    {
        int w = tex.width;
        int h = tex.height;

        int minX = w;
        int minY = h;
        int maxX = -1;
        int maxY = -1;

        var pixels = tex.GetPixels32();

        for (int y = 0; y < h; y++)
        {
            int rowIndex = y * w;
            for (int x = 0; x < w; x++)
            {
                var c = pixels[rowIndex + x];
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
            return false;
        }

        rect = new RectInt(minX, minY, (maxX - minX + 1), (maxY - minY + 1));
        return true;
    }

    static void SafeDestroy(UnityEngine.Object obj)
    {
        if (obj == null) return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DestroyImmediate(obj);
            return;
        }
#endif
        Destroy(obj);
    }

    readonly struct TransformState
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 LocalScale;

        TransformState(Vector3 position, Quaternion rotation, Vector3 localScale)
        {
            Position = position;
            Rotation = rotation;
            LocalScale = localScale;
        }

        public static TransformState Capture(Transform t) => new TransformState(t.position, t.rotation, t.localScale);

        public void ApplyTo(Transform t)
        {
            t.position = Position;
            t.rotation = Rotation;
            t.localScale = LocalScale;
        }
    }
}
