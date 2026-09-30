using UnityEngine;

[CreateAssetMenu(fileName = "EngineBaseConfig", menuName = "Configs/Products/Engine/Engine Base Config")]
public class EngineBaseConfig : ScriptableObject, IBaseConfig
{
    public string Name = "Default Engine";
    public Sprite Sprite;
    public int Level = 1;
    public float BasePrice = 1000f;
}