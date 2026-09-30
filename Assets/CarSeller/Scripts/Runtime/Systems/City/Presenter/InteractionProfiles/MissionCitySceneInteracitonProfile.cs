public sealed class MissionCityContextMenuProfile : NormalCityContextMenuProfile
{
}

public sealed class MissionCityTriggerProfile : NormalCityTriggerProfile
{
    //public TriggerAction GenerateTriggerAction(TriggerContext ctx)
    //{
    //    var missionGameState = ctx.GameState as MissionGameState;
    //    if (missionGameState == null)
    //    {
    //        Debug.LogError($"{this} used with wrong {ctx.GameState?.GetType().Name}");
    //        return new TriggerAction(false, null);
    //    }
    //    if (ctx.TriggerCause.Subject != missionGameState.FocusedCar)
    //        return new TriggerAction(false, null);
    //    Car car = ctx.TriggerCause.Subject as Car;

    //    if (ctx.Trigger is CityEntity cityEntity)
    //    {
    //        return new TriggerAction
    //        (
    //            true,
    //            () =>
    //            {
    //                switch (ctx.Kind)
    //                {
    //                    case TriggerContext.TriggerKind.Enter:
    //                        GameEvents.Instance.OnTargetReached?.Invoke(new CityTargetReachedEventData(cityEntity, ctx));
    //                        break;
    //                    case TriggerContext.TriggerKind.DragEnd:
    //                        GameEvents.Instance.OnTargetReachDragEnded?.Invoke(new CityTargetReachedEventData(cityEntity, ctx));
    //                        break;
    //                    default:
    //                        break;
    //                }

    //            }
    //        );
    //    }
    //    return new TriggerAction(false, null);
    //}
}