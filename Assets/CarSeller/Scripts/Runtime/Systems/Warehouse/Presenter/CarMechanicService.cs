using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMechanicService : RoutinedObject
{
    readonly Dictionary<Car, Coroutine> coroutinesOnCars = new Dictionary<Car, Coroutine>();

    public void DisassembleCar(Car car)
    {
        Debug.Assert(car != null, "Car cannot be null when disassembling.");

        Warehouse warehouse = CityLocatorHelper.GetWarehouse(car);

        if (coroutinesOnCars.ContainsKey(car))
        {
            return;
        }

        List<Product> parts = new List<Product>();
        var carPartLocations = car.carParts;
        foreach (var location in carPartLocations.Keys)
        {
            if (location.Occupant != null)
            {
                parts.Add(location.Occupant as Product);
            }
        }

        List<Action> partRemovals = new List<Action>();
        foreach (var part in parts)
        {
            partRemovals.Add(() =>
            {
                var location = G.ProductLifetimeService.GetProductLocation(part);
                if (location != null)
                {
                    G.ProductLifetimeService.MoveProduct(part, warehouse.GetEmptyLocation());
                }
            });
        }

        var coroutine = StartRoutine(carCoroutine(car, partRemovals, 0.1f));
        coroutinesOnCars.Add(car, coroutine);
    }

    public bool CanDisassembleCar(Car car)
    {
        return GameRules.CarCanBeDisassembled.Check(car);
    }

    IEnumerator carCoroutine(Car car, List<Action> actions, float period)
    {
        foreach (var action in actions)
        {
            action?.Invoke();
            yield return new WaitForSeconds(period);
        }
        if (coroutinesOnCars.ContainsKey(car))
        {
            coroutinesOnCars.Remove(car);
        }
    }

    public void RideCarFromWarehouse(Car car, Warehouse sceneWarehouseModel)
    {
        Debug.Assert(car != null, "Car cannot be null when riding from warehouse.");
        if (coroutinesOnCars.ContainsKey(car))
        {
            return;
        }

        List<Action> actions = new List<Action>
        {
            () =>
            {
                G.CityActionService.PutCarOutsideWarehouse(car, sceneWarehouseModel);
            },
            () => G.GameFlowController.EnterCity()
        };

        var coroutine = StartRoutine(carCoroutine(car, actions, 0.2f));
        coroutinesOnCars.Add(car, coroutine);
    }
}