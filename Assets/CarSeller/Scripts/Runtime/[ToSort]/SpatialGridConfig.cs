using UnityEngine;

[CreateAssetMenu(fileName = "SpatialGridConfig", menuName = "Configs/City/SpatialGridConfig", order = 1)]
public class SpatialGridConfig : ScriptableObject
{
    public int gridSizeX = 40;
    public int gridSizeY = 20;
    public float cellSize = 4f;
    public Vector2 originPosition = Vector2.zero;
}
