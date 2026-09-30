using UnityEngine;
using UnityEngine.UI;

public class ScrollWithSliderTest : MonoBehaviour
{
    public ScrollRect Rect;
    public Slider ScrollSlider;

    private void Update()
    {
        Rect.verticalNormalizedPosition = ScrollSlider.value;
    }
}
