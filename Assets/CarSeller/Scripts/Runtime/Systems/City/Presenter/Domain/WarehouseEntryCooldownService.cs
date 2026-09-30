using System.Collections.Generic;
using UnityEngine;

//Most of the time a car only exits one warehouse,
//but it’s still valid that during weird flows you could arm multiple:
//•	teleports
//•	scene reload ordering
//•	multiple exit systems firing
//Using a set means the service stays correct without extra assumptions.

public sealed class WarehouseEntryCooldownService
{
    readonly Dictionary<Car, HashSet<Warehouse>> _skipNextEntryForCarAndWarehouse =
        new Dictionary<Car, HashSet<Warehouse>>();

    public bool CanEnterWarehouse(Car car, Warehouse warehouse)
    {
        Debug.Log($"Checking if car {car?.Name} can enter warehouse {warehouse?.Name}.");
        if (car == null || warehouse == null)
            return false;

        if (!_skipNextEntryForCarAndWarehouse.TryGetValue(car, out var warehouses))
            return true;

        if (!warehouses.Remove(warehouse))
            return true;

        // cleanup
        if (warehouses.Count == 0)
            _skipNextEntryForCarAndWarehouse.Remove(car);
        Debug.Log($"Allowing entry of warehouse {warehouse.Name} for car {car.Name} after skipping one entry.");
        return false;
    }

    public void NotifyExitedWarehouse(Car car, Warehouse warehouse)
    {
        if (car == null || warehouse == null)
            return;

        if (!_skipNextEntryForCarAndWarehouse.TryGetValue(car, out var warehouses))
        {
            warehouses = new HashSet<Warehouse>();
            _skipNextEntryForCarAndWarehouse.Add(car, warehouses);
        }

        warehouses.Add(warehouse);

        Debug.Log($"Notified exit of warehouse {warehouse.Name} for car {car.Name}. Will skip next entry.");
    }

    public void Clear(Car car, Warehouse warehouse)
    {
        if (car == null || warehouse == null)
            return;

        if (!_skipNextEntryForCarAndWarehouse.TryGetValue(car, out var warehouses))
            return;

        warehouses.Remove(warehouse);

        if (warehouses.Count == 0)
            _skipNextEntryForCarAndWarehouse.Remove(car);
    }

    public void ClearCar(Car car)
    {
        if (car == null)
            return;

        _skipNextEntryForCarAndWarehouse.Remove(car);
    }

    public void Reset() => _skipNextEntryForCarAndWarehouse.Clear();
}