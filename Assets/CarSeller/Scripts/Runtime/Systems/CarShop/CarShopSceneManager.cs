using UnityEngine;

public class CarShopSceneManager : GlobalSingletonBehaviour<CarShopSceneManager>
{
    protected override CarShopSceneManager GlobalInstance { get => G.CarShopSceneManager; set => G.CarShopSceneManager = value; }

    static CarShopOfferProvider OfferProvider => G.Economy.CarShopOfferProvider;
    static Warehouse CurrentWarehouse => G.GameFlowController.CurrentWarehouse;

    private void Start()
    {
        updateOffers();
    }

    private void OnEnable()
    {
        GameEvents.Instance.OnTransactionComplete += onTransactionComplete;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnTransactionComplete -= onTransactionComplete;
    }


    public void ExitCarShop()
    {
        var warehouse = CurrentWarehouse;
        var state = G.GameState as FreeRoamGameState;
        if (state == null)
        {
            Debug.LogError("ExitCarShop: GameState is not FreeRoamGameState");
            return;
        }

        var car = findCurrentCar();

        //TODO maybe move this into CarMechanicService or make common for all states and exits
        //depends on whenver its Primary car or world car
        throw new System.NotImplementedException("Implement car exit animation and transition to city");
        G.WarehouseEntryCooldownService.NotifyExitedWarehouse(car,warehouse);
        G.CarMechanicService.RideCarFromWarehouse(car, warehouse);
    }

    Car findCurrentCar()
    {
        var warehouse = CurrentWarehouse;
        var cars = warehouse.GetCars();
        if (cars.Count == 0)
        {
            Debug.LogError("ExitCarShop: No cars in warehouse to exit with.");
            return null;
        }
        return cars[0];
    }

    void updateOffers()
    {
        var offers = OfferProvider.GetOffers(findCurrentCar(), 500f);
        G.CarShopUIManager.UpdateOffers(offers, onOfferClick);
    }

    void onOfferClick(CarShopOffer offer)
    {
        if (offer.CanAccept())
        {
            var transaction = offer.Accept();
            var result = G.TransactionProcessor.Process(transaction);
            if (result.Type != TransactionResultType.Success)
            {
                Debug.LogError($"Offer {offer} acceptance failed with result {result.Type}.");
            }
        }
        else
        {
            Debug.LogWarning($"Offer {offer} cannot be accepted.");
        }
    }


    void onTransactionComplete(TransactionEventData eventData)
    {
        Debug.Assert(eventData != null, "onTransactionComplete received null eventData.");
        Debug.Assert(eventData.Transaction != null, "onTransactionComplete received null Transaction.");
        if (eventData.Transaction is not ExchangeTransaction)
        {
            return;
        }
        if(eventData.Transaction.Result.Type != TransactionResultType.Success)
        {
            Debug.LogError("onTransactionComplete: Exchange transaction failed with result " + eventData.Transaction.Result.Type);
            return;
        }
        var exchangeData = eventData.Transaction as ExchangeTransaction;

        OfferProvider.SwapCar(exchangeData.FromPlayer, exchangeData.ToPlayer);
        updateOffers();
    }
}