using UnityEngine;

public class Player : OwnableBase
{
    public float Money { get; private set; }

    internal void ChangeMoney(float newAmount)
    {
        Debug.Assert(newAmount >= 0, "Player money cannot be negative.");
        Money = newAmount;
    }

    public bool Owns(IOwnable ownable)
    {
        return ownable != null && ReferenceEquals(ownable.Owner, this);
    }
}

