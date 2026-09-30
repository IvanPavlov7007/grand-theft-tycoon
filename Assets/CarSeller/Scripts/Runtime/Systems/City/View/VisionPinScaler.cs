using UnityEngine;

/// <summary>
/// Applies the same vision-based scaling/hiding as `VisionDistanceScaler`, but to the UI pin.
/// Attach to `CityUIPin` GameObject.
/// </summary>
[DisallowMultipleComponent]
public sealed class VisionPinScaler : MonoBehaviour
{
    private VisibleDistanceScalerAspect scalerAspect;

    private CityUIPinPositioner positioner;
    private RectTransform rect;
    private Vector3 baseScale;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        positioner = GetComponent<CityUIPinPositioner>();
        rect = GetComponent<RectTransform>();
        if (rect != null) baseScale = rect.localScale;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Initialize(VisibleDistanceScalerAspect scalerAspect)
    {
        this.scalerAspect = scalerAspect;
    }

    private void LateUpdate()
    {
        if (scalerAspect == null || scalerAspect.VisibleAspect == null)
            return;

        if (!scalerAspect.VisibleAspect.Visible && scalerAspect.Config.HideBeyondMax)
        {
            ApplyVisibility(visible: false);
            return;
        }

        if (scalerAspect.VisibleAspect.NearestCenter == null)
        {
            OnNoVision();
            return;
        }

        ApplyVisibility(visible: true);

        var scalerEvaluation = scalerAspect.Evaluate();

        ApplyScale(scalerEvaluation.Scale);
    }

    private void OnNoVision()
    {
        if (VisionLogic.VisibleWhenNoCenter)
        {
            ApplyVisibility(visible: true);
            ApplyScale(1f);
        }
        else
        {
            ApplyVisibility(visible: false);
        }
    }

    private void ApplyVisibility(bool visible)
    {
        // Do NOT SetActive(false) here, otherwise LateUpdate stops and the pin can never become visible again.
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private void ApplyScale(float scaleMul)
    {
        if (rect == null) return;
        rect.localScale = baseScale * Mathf.Max(0f, scaleMul);
    }
}
