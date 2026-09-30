using System.Collections;
using UnityEngine;

public class CarStashProcess : IProcess
{
    CityEntity EnteredCar;
    CarStashWarehouse Warehouse;
    CarStashSession Session;

    public CarStashProcess(CityEntity enteredCar, CarStashWarehouse warehouse)
    {
        this.EnteredCar = enteredCar;
        Warehouse = warehouse;
    }

    public IEnumerator Run()
    {
        if (G.VehicleController.IsInPrimaryVehicle)
        {
            Session = CarStashSession.CreatePickToDeploySession(Warehouse,EnteredCar.Position);
        }
        else
        {
            Session = CarStashSession.CreatePickToStoreSession(Warehouse,EnteredCar);
        }

        if(Session == null)
        {
            Debug.LogError("Failed to create CarStashSession for car stash process.");
            yield break;
        }

        bool closed = false;
        

        while (!closed)
        {
            StashSlot chosenSlot = null;

            var displayer = Session.GetDisplayer(x =>
            {
                chosenSlot = x;
            });
            var menu = G.FixedContextMenuManager.CreateContextMenu(displayer.generateOffers());

            menu.Closed += _ =>
            {
                closed = true;
            };
            yield return new WaitUntil(() => chosenSlot != null || closed);
            if (chosenSlot != null)
            {
                Session = Session.MakeChoice(chosenSlot);
            }
            if (Session == null)
            {
                closed = true;
            }
        }

    }


}