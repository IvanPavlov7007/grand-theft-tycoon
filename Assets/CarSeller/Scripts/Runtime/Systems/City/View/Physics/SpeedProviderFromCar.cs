using UnityEngine;

public class SpeedProviderFromCar : MonoBehaviour, ISpeedProvider
{
    Car car;

    public float Speed => car.runtimeConfig.Speed;

    public float Acceleration => car.runtimeConfig.Acceleration;

    public void Initialize(Car car)
    {
        this.car = car;
    }
}