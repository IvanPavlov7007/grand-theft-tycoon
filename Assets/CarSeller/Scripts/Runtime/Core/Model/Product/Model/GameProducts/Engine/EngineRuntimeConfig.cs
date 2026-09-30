using System;
using UnityEngine;

[Serializable]
public class EngineRuntimeConfig : IRuntimeConfig
{
    public string Name;
    public Sprite Sprite;
    public int Level;
    public float BasePrice;
}