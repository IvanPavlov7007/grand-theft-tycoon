using UnityEngine;

public class CameraViewSetter : MonoBehaviour
{
    private void Awake()
    {
        var Canvas = GetComponent<Canvas>();
        Camera cam = Camera.main;
        Canvas.worldCamera = cam;
    }
}