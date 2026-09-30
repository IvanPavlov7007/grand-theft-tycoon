using UnityEngine;

public interface IDirectionProvider
{
    Vector2 ProvidedDirection { get; }
}

public interface IActivatable
{
    public void Activate();
    public void Deactivate();
    public void SetActive(bool active);
}

public interface ISpeedProvider
{
    float Speed { get; }
    float Acceleration { get; }
}

public interface ISpeedCap
{
    float MaxSpeedOverride { get; set; }
}

public interface IMovement : IDirectionProvider, ISpeedProvider
{
    Vector2 Velocity => ProvidedDirection * Speed;
}