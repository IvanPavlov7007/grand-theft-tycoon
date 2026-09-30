using System;
using UnityEngine;

[Serializable]
public class SpoilerRuntimeConfig : IRuntimeConfig
{
    public string Name;
    public Sprite Sprite;
    public Color Color;
    public float Size;
    public float BasePrice;
}