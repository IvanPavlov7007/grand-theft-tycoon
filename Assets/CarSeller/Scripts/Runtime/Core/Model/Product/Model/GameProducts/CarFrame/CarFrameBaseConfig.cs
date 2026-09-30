using UnityEngine;
[CreateAssetMenu(fileName = "CarFrameBaseConfig", menuName = "Configs/Products/Frame/FrameBaseConfig")]
public class CarFrameBaseConfig : ScriptableObject, IBaseConfig
{
    public string Name = "Default Frame";
    public GameObject Prefab;
    public Color FrameColor = Color.white;
    public Color WindshieldColor = Color.white;
    public Sprite Icon;
    public float BasePrice = 0f;
}