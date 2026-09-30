using System;
using System.Linq;
using UnityEngine;

public class GlobalUIMethods : MonoBehaviour
{
    public void OpenCity()
    {
        G.GameFlowController.EnterCity();
    }

    [Obsolete]
    private void hackCreatePlayerFigure()
    {
        Warehouse warehouse = G.GameFlowController.CurrentWarehouse;
        if(warehouse == null)
            warehouse = (Warehouse)
            World.Instance.City.Entities.Keys.First(x => x.GetType() == typeof(Warehouse));
        var figure = new PlayerFigure();
        CityEntitiesCreationHelper.CreatePlayerFigure(figure,
            CityLocatorHelper.GetCityEntity(warehouse).Position);
        //G.GameFlowController.TryControlPlayerFigure(figure, out _);

    }

    [Obsolete]
    /// <summary>
    /// Don't use this method in production code!
    /// </summary>
    private void hackPickRandomCar()
    {
            var car = CityLocatorHelper.GetCityEntity(
            CityLocatorHelper.GetClosestCar(G.GameFlowController.CurrentWarehouse, out _));
        G.VehicleController.DriveWorldVehicle(car);
    }

    public void StartGame()
    {
        G.GameFlowController.EnterWarehouse((Warehouse)
                    World.Instance.City.Entities.Keys.First(x => x.GetType() == typeof(Warehouse)));
    }

    public void ResetGame()
    {
        G.GameManager.ResetGame();
    }

    public void Resume()
    {
        G.GameManager.Pause(false);
        Debug.Log("Resuming game");
    }

    public void Pause()
    {
        G.GameManager.Pause(true);
        Debug.Log("Pausing game");
    }

    public void OpenShop()
    {
        G.ProcessRunner.Run(new PersonalVehicleShopProcess(G.VehicleShop));
    }
}