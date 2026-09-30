using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewStateChangerUI : MonoBehaviour
{
    [SerializeField, Range(0.2f, 1f)]
    private float disabledBrightnessMultiplier = 0.6f;

    private CityViewObjectController cityViewObjectController;
    private readonly Dictionary<Image, Color> originalColors = new Dictionary<Image, Color>();
    private Image[] imageRenderers;

    private void Awake()
    {
        CacheRenderersAndColors();
    }

    public void Initialize(CityViewObjectController controller)
    {
        cityViewObjectController = controller;
        cityViewObjectController.OnVisualStateChanged += onVisualStateChanged;
        onVisualStateChanged(cityViewObjectController.CurrentVisualState);
    }

    private void OnDisable()
    {
        if (cityViewObjectController != null)
        {
            cityViewObjectController.OnVisualStateChanged -= onVisualStateChanged;
        }
    }

    private void OnTransformChildrenChanged()
    {
        // Re-cache and re-apply when hierarchy changes at runtime.
        var current = cityViewObjectController != null ? cityViewObjectController.CurrentVisualState : ViewObjectVisualState.Normal;
        CacheRenderersAndColors();
        onVisualStateChanged(current);
    }

    private void onVisualStateChanged(ViewObjectVisualState state)
    {
        if (imageRenderers == null || imageRenderers.Length == 0)
            CacheRenderersAndColors();

        if (state == ViewObjectVisualState.Disabled)
        {
            ApplyDisabledColors();
        }
        else
        {
            RestoreOriginalColors();
        }
    }

    private void CacheRenderersAndColors()
    {
        imageRenderers = GetComponentsInChildren<Image>(true);
        originalColors.Clear();
        for (int i = 0; i < imageRenderers.Length; i++)
        {
            var sr = imageRenderers[i];
            if (sr == null) continue;
            originalColors[sr] = sr.color;
        }
    }

    private void RestoreOriginalColors()
    {
        foreach (var kvp in originalColors)
        {
            if (kvp.Key == null) continue;
            kvp.Key.color = kvp.Value;
        }
    }

    private void ApplyDisabledColors()
    {
        for (int i = 0; i < imageRenderers.Length; i++)
        {
            var sr = imageRenderers[i];
            if (sr == null) continue;

            // Ensure we have the original saved (covers late-added renderers).
            if (!originalColors.ContainsKey(sr))
                originalColors[sr] = sr.color;

            var orig = originalColors[sr];
            sr.color = ToGrayscale(orig, disabledBrightnessMultiplier);
        }
    }

    private static Color ToGrayscale(Color c, float brightnessMultiplier)
    {
        // Rec. 709 luminance coefficients
        float gray = (0.2126f * c.r) + (0.7152f * c.g) + (0.0722f * c.b);
        gray *= brightnessMultiplier;
        return new Color(gray, gray, gray, c.a);
    }
}