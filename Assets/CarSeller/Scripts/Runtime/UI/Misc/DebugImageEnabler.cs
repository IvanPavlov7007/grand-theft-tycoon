using UnityEngine;
using UnityEngine.UI;

public class DebugImageEnabler : MonoBehaviour
{
#if UNITY_EDITOR
    Image debugImage;
    public bool enableDebugImage = true;

    private void Awake()
    {
        debugImage = GetComponent<Image>();
        if(debugImage != null && enableDebugImage)
        {
            // Set the debug image color to white
            debugImage.color = Color.white;
        }
    }
#endif
}
