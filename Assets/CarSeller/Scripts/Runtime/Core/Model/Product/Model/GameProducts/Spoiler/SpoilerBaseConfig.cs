using UnityEngine;

[CreateAssetMenu(fileName = "SpoilerBaseConfig", menuName = "Configs/Products/Spoiler/Spoiler Base Config")]
public class SpoilerBaseConfig : ScriptableObject, IBaseConfig
{
    public string Name;
    public Sprite Sprite;
    public Color Color = Color.white;
    public float Size = 1f;
    public float BasePrice = 100f;
}