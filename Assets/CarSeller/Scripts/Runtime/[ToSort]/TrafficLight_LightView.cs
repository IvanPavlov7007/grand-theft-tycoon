using UnityEngine;

public class TrafficLight_LightView : MonoBehaviour
{
    [SerializeField] SpriteRenderer _spriteRenderer;

    internal void SetColor(Color color)
    {
        _spriteRenderer.color = color;
    }
}
