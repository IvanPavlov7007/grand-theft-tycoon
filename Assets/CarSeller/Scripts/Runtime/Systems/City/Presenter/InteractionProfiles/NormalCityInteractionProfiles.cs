using UnityEngine;

public class NormalCityContextMenuProfile : ICityContextMenuProfile
{
    public virtual Widget GenerateContent(CityEntity entity, GameState gameState)
    {
        if (gameState == null)
        {
            Debug.LogError($"NormalCityContextMenuProfile used with no state");
            return null;
        }

        //TODO: check by visibility and other aspects if the entity is displayed, but not discovered
        // and show "undiscovered" hint in that case instead of actual content

        bool discovered = entity.GetAspect<CityVisibleAspect>()?.Discovered ?? false;

        if (!discovered)
        {
            return new GenericInfoWidget("???", "There is something here. Come closer to find out what is it.");
        }

        if (entity.Subject is PersonalVehicleShop vehicleShop)
        {
            return new GenericInfoWidget(vehicleShop.DisplayName, "A place where you can buy personal vehicles. Drag your car here to see what they offer.");
        }

        if (entity.Subject is CarStashWarehouse carStashWarehouse)
        {
            return new GenericInfoWidget(CarStashWarehouse.DisplayName, "A place where you can stash stolen cars. Drag the car here to stash it for later.");
        }

        if (entity.Subject is MissionLauncher launcher)
        {
            //return CTX_Menu_Tools.MissionLauncherHint(launcher);
        }
        if (entity.Subject is PlayerFigure figure)
        {
            // "It's you"
            //return new UIElement
            //{
            //    Type = UIElementType.Container,
            //    Children = new List<UIElement>
            //    {
            //        CTX_Menu_Tools.Header("You"),
            //        CTX_Menu_Tools.Hint("It's you")
            //    }
            //};
        }

        if (entity.Subject is Car car)
        {
            if (GameRules.carCanBeExited.Check(car))
            {
                return CarInfoWidget.StolenCar(car);
            }

            if (G.VehicleController.CurrentCar == car)
                return CarInfoWidget.PrimaryCar(car);

            return CarInfoWidget.WorldCar(car);
        }

        if (entity.Subject is Buyer buyer)
        {
            return new BuyerInfoWidget(buyer);
        }

        Debug.LogWarning($"NormalCityContextMenuProfile: No context menu defined for model type {entity.Subject.GetType()}");
        return null;
    }
}

public class NormalCityTriggerProfile : ICityTriggerProfile
{
    public TriggerAction GenerateTriggerAction(TriggerContext ctx)
    {
        var gameState = ctx.GameState;
        Debug.Assert(gameState != null, "NormalCityTriggerProfile: gameState is not set");

        // Determine who is controlled right now (figure has priority if present).
        ILocatable actor = G.VehicleController.CurrentCar;
        if (actor == null)
            return new TriggerAction(false, null);

        // Controlled entity must be the trigger cause.
        if (ctx.TriggerCause.Subject != actor)
            return new TriggerAction(false, null);


        // PlayerFigure -> Car drag end: get into car
        if (ctx.Kind == TriggerContext.TriggerKind.DragEnd && ctx.Trigger.Subject is Car dragTargetCar)
        {
            return new TriggerAction(true, () =>
            {
                G.VehicleController.DriveWorldVehicle(ctx.Trigger);
                //CameraHelper.SetCurrentPositionAtCar();
            });
        }

        // Actor -> Warehouse drag end: enter warehouse
        if (ctx.Kind == TriggerContext.TriggerKind.DragEnd && ctx.Trigger.Subject is Warehouse warehouse)
        {
            // Car-specific cooldown gating (figure doesn't use it)
            if (actor is Car focusedCar)
            {
                if (!G.WarehouseEntryCooldownService.CanEnterWarehouse(focusedCar, warehouse))
                    return new TriggerAction(false, null);

                return new TriggerAction(true, () => { G.ProcessRunner.Run(new WarehouseEnterProcess(focusedCar, warehouse)); });
            }

            if (actor is PlayerFigure figure)
            {
                return new TriggerAction(true, () => { G.ProcessRunner.Run(new WarehouseEnterProcess(figure, warehouse)); });
            }
        }

        if (ctx.Kind == TriggerContext.TriggerKind.DragEnd && ctx.Trigger.Subject is CarStashWarehouse carStashWarehouse)
        {
            return new TriggerAction(true, () =>
            { G.ProcessRunner.Run(new CarStashProcess(ctx.TriggerCause, carStashWarehouse)); });
        }

        if (ctx.Kind == TriggerContext.TriggerKind.DragEnd && ctx.Trigger.Subject is PersonalVehicleShop vehicleShop)
        {
            return new TriggerAction(true, () =>
            { G.ProcessRunner.Run(new PersonalVehicleShopProcess(vehicleShop)); });
        }

        if (ctx.Kind == TriggerContext.TriggerKind.DragEnd && ctx.Trigger.Subject is Buyer buyer)
        {
            return new TriggerAction(true, () =>
            { G.ProcessRunner.Run(new BuyerProcess(buyer, ctx.TriggerCause.Subject as Car)); });
        }

        // Generic city target reached events (figure should also trigger them)
        if (ctx.Trigger is CityEntity cityEntity)
        {
            return new TriggerAction(true, () =>
            {
                switch (ctx.Kind)
                {
                    case TriggerContext.TriggerKind.Enter:
                        GameEvents.Instance.OnTargetReached?.Invoke(new CityTargetReachedEventData(cityEntity, ctx));
                        break;
                    case TriggerContext.TriggerKind.DragEnd:
                        GameEvents.Instance.OnTargetReachDragEnded?.Invoke(new CityTargetReachedEventData(cityEntity, ctx));
                        break;
                }
            });
        }

        return new TriggerAction(false, null);
    }
}