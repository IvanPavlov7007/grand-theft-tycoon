using UnityEngine;

public sealed class SellingCityContextMenuProfile : ICityContextMenuProfile
{
    public Widget GenerateContent(CityEntity model, GameState gameState)
    {
        return null;
    }
    private UIElement generateSellingWarehouseContent(SellingGameState sellingState, Warehouse warehouse)
    {
        var elementsList = CTX_Menu_Tools.WarehouseBaseInfoElements(warehouse);

        return new UIElement
        {
            Type = UIElementType.Container,
            Children = elementsList
        };
    }

    private UIElement generateSellingCarContent(SellingGameState sellingState, Car car)
    {
        var elementsList = CTX_Menu_Tools.CarBaseInfoElements(car);
        elementsList.Add(CTX_Menu_Tools.Hint("This is the car you are currently selling. Bring it to the buyer."));
        elementsList.Add(CTX_Menu_Tools.CancelButton());

        return new UIElement
        {
            Type = UIElementType.Container,
            Children = elementsList
        };
    }
}

public sealed class SellingCityTriggerProfile : ICityTriggerProfile
{
    public TriggerAction GenerateTriggerAction(TriggerContext ctx)
    {
        var sellingState = ctx.GameState as SellingGameState;
        if (sellingState == null)
        {
            Debug.LogError($"SellingCityTriggerProfile used with non-selling state {ctx.GameState?.GetType().Name}");
            return new TriggerAction(false, null);
        }

        Debug.Assert(sellingState.SellingCar != null, "SellingCityTriggerProfile: SellingCar is null");
        Debug.Assert(sellingState.Buyer != null, "SellingCityTriggerProfile: Buyer is null");

        bool canProceed =
            ctx.Trigger.Subject as Buyer == sellingState.Buyer &&
            ctx.TriggerCause.Subject as Car == sellingState.SellingCar;

        System.Action action = null;
        if (canProceed)
        {
            action = () =>
            {
                GameEvents.Instance.OnPlayerSucceed(
                    new PlayerActionEventData(PlayerOutcome.Succeeded, null));
            };
        }

        return new TriggerAction(canProceed, action);
    }
}