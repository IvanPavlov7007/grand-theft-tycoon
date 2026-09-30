public class CollectablesManager : GlobalSingletonBehaviour<CollectablesManager>
{
    protected override CollectablesManager GlobalInstance { get => G.CollectablesManager; set => G.CollectablesManager = value; }

    private void OnEnable()
    {
        GameEvents.Instance.OnTargetReached += onCityTargetReached;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnTargetReached -= onCityTargetReached;
    }

    void onCityTargetReached(CityTargetReachedEventData data)
    {
        if (data.ReachedObject.Subject is Collectable collectable)
        {
            collect(collectable, data.ReachedObject);
            City.EntityLifetimeService.Destroy(collectable);
        }
    }


    //TODO make process possessions, actions and custom data as well in the future
    void collect(Collectable collectable, CityEntity cityEntity)
    {
        var transactionLocation = new TransactionFeedbackLocation(TransactionLocationType.WorldSpace, cityEntity.Position.WorldPosition);
        var rewardTransaction = new RewardTransaction(collectable.MoneyAmount, null);
        G.TransactionProcessor.Process(rewardTransaction, transactionLocation);
    }
}