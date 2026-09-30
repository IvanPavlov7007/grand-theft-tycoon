using UnityEngine;
using Coffee.UISoftMask;
using Coffee.UISoftMaskInternal;

public class SoftMaskTransparency : MonoBehaviour, ITransparencyController
{
    public float threshold = 0.05f;
    public float Alpha
    {
        get
        {
            if (!initialize())
            {
                throw new UnityException(name + " can find an Alpha-Setting target");
            }
            return softMask.softnessRange.average;
        }
        set
        {
            if (!initialize())
            {
                throw new UnityException(name + " can find an Alpha-Setting target");
            }
            MinMax01 range;
            if (value < threshold)
                range = new MinMax01(0f, 0f);
            else if (value > 1f - threshold)
                range = new MinMax01(1f, 1f);
            else
                range = clampedRange(value, threshold);
            softMask.softnessRange = range;

        }
    }

    static MinMax01 clampedRange(float alphaMedian, float threshold)
    {
        float min = Mathf.Clamp01(alphaMedian - threshold);
        float max = Mathf.Clamp01(alphaMedian + threshold);
        return new MinMax01(min, max);
    }

    SoftMask softMask;
    private bool initialize()
    {
        if (softMask != null)
            return true;
        softMask = GetComponent<SoftMask>();
        if (softMask == null)
            return false;
        return true;
    }

}

public static partial class Transparency
{
    public static SoftMaskTransparency GetController(SoftMask softMask)
    {
        SoftMaskTransparency result;
        if (!softMask.TryGetComponent<SoftMaskTransparency>(out result))
        {
            result = softMask.gameObject.AddComponent<SoftMaskTransparency>();
        }
        return result;
    }
}