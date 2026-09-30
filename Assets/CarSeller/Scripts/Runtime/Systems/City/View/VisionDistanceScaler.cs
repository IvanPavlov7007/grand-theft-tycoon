using UnityEngine;

/// <summary>
/// Scales and optionally hides a city entity view based on distance to the nearest active vision center.
/// Requires `VisionDistanceScaleAspect` on the underlying `CityEntity`.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CityViewObjectController))]
public sealed class VisionDistanceScaler : MonoBehaviour
{
    private VisibleDistanceScalerAspect scalerAspect;

    private Vector3 baseLocalScale;

    private Renderer[] renderers;
    private CanvasGroup[] canvasGroups;

    private bool _hasInit;

    private void Awake()
    {
        baseLocalScale = transform.localScale;

        renderers = GetComponentsInChildren<Renderer>(true);
        canvasGroups = GetComponentsInChildren<CanvasGroup>(true);
    }

    public void Initialize(VisibleDistanceScalerAspect scalerAspect)
    {
        this.scalerAspect = scalerAspect;
    }

    private void LateUpdate()
    {
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

        transform.localScale = baseLocalScale * Mathf.Max(0f, scalerEvaluation.Scale);
        ApplyAlpha(scalerEvaluation.Alpha);
    }

    private void OnNoVision()
    {
        if(VisionLogic.VisibleWhenNoCenter)
        {
            ApplyVisibility(visible: true);
            transform.localScale = baseLocalScale;
            ApplyAlpha(1f);
        }
        else
        {
            ApplyVisibility(visible: false);
        }
    }

    private void ApplyVisibility(bool visible)
    {
        if (renderers != null && renderers.Length > 0)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) renderers[i].enabled = visible;
            }
        }

        if (canvasGroups != null && canvasGroups.Length > 0)
        {
            for (int i = 0; i < canvasGroups.Length; i++)
            {
                var cg = canvasGroups[i];
                if (cg == null) continue;
                if (!visible) cg.alpha = 0f;
                cg.interactable = visible;
                cg.blocksRaycasts = visible;
            }
        }

        var cols2d = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < cols2d.Length; i++)
        {
            if (cols2d[i] != null) cols2d[i].enabled = visible;
        }

        var cols3d = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols3d.Length; i++)
        {
            if (cols3d[i] != null) cols3d[i].enabled = visible;
        }
    }

    private void ApplyAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        if (canvasGroups != null && canvasGroups.Length > 0)
        {
            for (int i = 0; i < canvasGroups.Length; i++)
            {
                var cg = canvasGroups[i];
                if (cg != null) cg.alpha = alpha;
            }
            return;
        }

        var srs = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < srs.Length; i++)
        {
            var sr = srs[i];
            if (sr == null) continue;
            var c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        var gs = GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
        for (int i = 0; i < gs.Length; i++)
        {
            var g = gs[i];
            if (g == null) continue;
            var c = g.color;
            c.a = alpha;
            g.color = c;
        }
    }
}
