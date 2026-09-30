using UnityEngine;
using Pixelplacement;

public class CustomUIAnimation : MonoBehaviour
{
    void Start()
    {
        //using tween make UI pop scale with overshooting
        Tween.LocalScale(transform, Vector3.one * 0.5f, Vector3.one, 0.1f, 0f, Tween.EaseOutBack);
    }
}
