using System.Collections.Generic;

public class PlayerManager
{
    private Player Player => World.Instance.Economy.Player;
    private Dictionary<IOwnable, HashSet<IOwnable>> ownerships => World.Instance.ownerships;

    public bool PlayerOwns(IOwnable item)
    {
        return Player.Owns(item);
    }

    public IReadOnlyCollection<IOwnable> GetPlayerOwnedItems()
    {
        if (ownerships.TryGetValue(Player, out var set))
            return set;
        return System.Array.Empty<IOwnable>();
    }

    public float SetPlayerMoney(float amount)
    {
        float oldMoney = Player.Money;
        Player.ChangeMoney(amount);
        GameEvents.Instance.OnPlayerMoneyChanged?.Invoke(new PlayerMoneyChangeEventData(Player, oldMoney, Player.Money));
        return Player.Money;
    }

    public float DeltaPlayerMoney(float delta) => SetPlayerMoney(Player.Money + delta);
    public float AddPlayerMoney(float amount) => SetPlayerMoney(Player.Money + amount);
    public float SubtractPlayerMoney(float amount) => SetPlayerMoney(Player.Money - amount);
}

