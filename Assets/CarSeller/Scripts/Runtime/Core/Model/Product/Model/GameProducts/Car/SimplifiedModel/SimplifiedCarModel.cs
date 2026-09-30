using System;

[Serializable]
public enum CarColor
{
    Red,
    Green,
    Blue,
    Yellow, 
    Unique
}
[Serializable]
public enum CarType
{
    Sedan,
    Small,
    Bike,
    Truck,
    Super,
    Any
}
[Serializable]
public enum CarRarity
{
    Common,
    Uncommon,
    Epic
}

public interface ISimplifiedCarModel
{
    public CarKind Kind { get; }
}

public struct CarKind
{
    public CarType Type;
    public CarRarity Rarity;
    public CarKind(CarType type, CarRarity rarity)
    {
        Type = type;
        Rarity = rarity;
    }
}