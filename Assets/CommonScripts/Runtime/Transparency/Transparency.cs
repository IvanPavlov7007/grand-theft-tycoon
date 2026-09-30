using UnityEngine;
using UnityEngine.UI;

public static partial class Transparency
{
    public static ParticleSystemTransparency GetController(ParticleSystem system)
    {
        ParticleSystemTransparency result;
        if (!system.TryGetComponent<ParticleSystemTransparency>(out result))
        {
            result = system.gameObject.AddComponent<ParticleSystemTransparency>();
        }
        return result;
    }

    public static CanvasGroupTransparency GetController(CanvasGroup canvasGroup)
    {
        CanvasGroupTransparency result;
        if (!canvasGroup.TryGetComponent<CanvasGroupTransparency>(out result))
        {
            result = canvasGroup.gameObject.AddComponent<CanvasGroupTransparency>();
        }
        return result;
    }

    public static GraphicTransparency GetController(Graphic graphic)
    {
        GraphicTransparency result;
        if (!graphic.TryGetComponent<GraphicTransparency>(out result))
        {
            result = graphic.gameObject.AddComponent<GraphicTransparency>();
        }
        return result;
    }
}