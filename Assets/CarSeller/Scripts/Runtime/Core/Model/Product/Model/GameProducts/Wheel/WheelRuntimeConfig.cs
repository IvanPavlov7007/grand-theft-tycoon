using System;
using UnityEngine;

[Serializable]
public sealed class WheelRuntimeConfig : IRuntimeConfig
{
    public string Name;
    public WheelType WheelType;
    public Sprite FrontSideViewSprite;
    public Sprite BackSideViewSprite;
    public Sprite TopViewSprite;
    public Color Color;
    public float SideViewSize;
    public float TopViewSize;
    public float BasePrice;
}

[Serializable]
public enum WheelType
{
    Sport,
    OffRoad,
    Super
}