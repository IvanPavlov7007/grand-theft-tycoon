using UnityEngine;

public interface ModelProvider
{
    public CityEntity CityEntity { get;}
    public ILocatable Locatable { get; }
    public GameObject ViewGameObject { get; }
}