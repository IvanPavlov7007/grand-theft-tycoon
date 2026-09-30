using UnityEngine;

public class  PlayerFigure : CityDestroyable
{
    
}

public class PlayerFigureSpeedProvider : MonoBehaviour, ISpeedProvider
{
    PlayerFigure playerFigure;
    public float Speed => 0.5f;
    public float Acceleration => 1f;
    public void Initialize(PlayerFigure playerFigure)
    {
        this.playerFigure = playerFigure;
    }
}