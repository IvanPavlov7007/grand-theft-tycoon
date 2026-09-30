using System;

public class Collectable : CityDestroyable
{
    public CollectableConfig Config { get; set; }
    public Collectable(CollectableConfig config)
    {
        Config = config;
    }

    public float MoneyAmount => Config.MoneyAmount;
    public IOwnable[] items { get; set; }
    public event Action OnCollectedAdditionalCallback;

}

[Serializable]
public class CollectableConfig
{
    public string InfoText { get; set; } = "Collectable Info";
    public string Name { get; set; } = "Collectable";
    public float MoneyAmount { get; set; }
}