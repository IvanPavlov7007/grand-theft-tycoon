using UnityEngine;

[CreateAssetMenu(fileName = "WheelBaseConfig", menuName = "Configs/Products/Wheel/Wheel Base Config")]
public class WheelBaseConfig : ScriptableObject, IBaseConfig
{
    public string Name = "Default Wheel";
    public WheelType WheelType;
    public Sprite FrontSideViewSprite;
    public Sprite BackSideViewSprite;
    public Sprite TopViewSprite;
    public Color Color = Color.white;
    public float SideViewSize = 1f;
    public float TopViewSize = 1f;
    public float BasePrice = 100f;
}