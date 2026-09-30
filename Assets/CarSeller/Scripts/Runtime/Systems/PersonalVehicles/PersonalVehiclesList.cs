using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrimaryVehicleManager
{
    public PersonalVehicle PrimaryVehicle { get; private set; }
    public PersonalVehiclesList PersonalVehiclesList { get; private set; }

    public PrimaryVehicleManager(PersonalVehiclesList personalVehiclesList, int initialPrimaryVehicle)
    {
            PersonalVehiclesList = personalVehiclesList;
            SetPrimaryVehicle(initialPrimaryVehicle);
    }

    public CityEntity DeployPrimaryVehicle(CityPosition position)
    {
        if (PrimaryVehicle == null)
        {
            throw new UnityException("Attempted to deploy a primary vehicle when none is set.");
        }
        return PrimaryVehicle.Deploy(position);
    }

    public void HidePrimaryVehicle()
    {
        if (PrimaryVehicle == null)
        {
            Debug.LogWarning("Attempted to hide a primary vehicle when none is set.");
            return;
        }
        PrimaryVehicle.Hide();
    }

    public void SwapPrimaryVehicle(PersonalVehicle nextVechilce)
    {
        SetPrimaryVehicle(nextVechilce);
    }

    private void SetPrimaryVehicle(PersonalVehicle nextVechilce)
    {
        Debug.Assert(PersonalVehiclesList.OwnedVehicles.Contains(nextVechilce));
        PrimaryVehicle = nextVechilce;
    }

    void SetPrimaryVehicle(Car car)
    {
        var foundLocation = PersonalVehiclesList.OwnedVehicles.FindIndex(x => x.Car == car);
        if (foundLocation < 0 || foundLocation >= PersonalVehiclesList.OwnedVehicles.Count)
        {
            throw new UnityException($"Attempted to set primary vehicle with car {car}, but it was not found in the owned vehicles list. Owned vehicles count: {PersonalVehiclesList.OwnedVehicles.Count}");
        }
        SetPrimaryVehicle(foundLocation);
    }

    void SetPrimaryVehicle(int index)
    {
        if (index < 0 || index >= PersonalVehiclesList.OwnedVehicles.Count)
        {
            throw new UnityException($"Attempted to set primary vehicle with index {index}, but it is out of bounds. Owned vehicles count: {PersonalVehiclesList.OwnedVehicles.Count}");
        }
        PrimaryVehicle = PersonalVehiclesList.OwnedVehicles[index];
    }
}

public class PersonalVehiclesList
{
    public List<PersonalVehicle> OwnedVehicles { get; private set; }
    public PersonalVehiclesList(IReadOnlyList<PersonalVehicle> initialVehicles)
    {
        OwnedVehicles = new List<PersonalVehicle>(initialVehicles);
    }

    public void UpdateList (IReadOnlyList<PersonalVehicle> newVehicles)
    {
        OwnedVehicles = new List<PersonalVehicle>(newVehicles);
    }

    public bool OwnsVehicle(Car car)
    {
        return OwnedVehicles.Any(vehicle => vehicle.Car == car);
    }
}

public class PersonalVehicle
{
    public Car Car { get; private set; }
    public CityEntity CityEntity { get; private set; }
    public bool IsDeployed => CityEntity != null;
    private PersonalVehicle(){}

    [Obsolete]
    public static PersonalVehicle CreateNew(SimpleCarSpawnConfig spawnConfig)
    {
        var car = spawnConfig.GenerateCarHidden();
        return new PersonalVehicle
        {
            Car = car,
            CityEntity = null,
        };
    }

    public static PersonalVehicle CreateNew(string carIdentifier)
    {
        var car = G.SimplifiedCarsManager.CreateCarHidden(carIdentifier);
        return new PersonalVehicle
        {
            Car = car,
            CityEntity = null,
        };
    }

    public static PersonalVehicle CreateFromExistingCar(Car car)
    {
        return new PersonalVehicle
        {
            Car = car,
            CityEntity = null,
        };
    }

    public CityEntity Deploy(CityPosition position)
    {
        if (CityEntity != null)
        {
            Debug.LogWarning($"Vehicle {Car} is already deployed at {CityEntity.Position}");
            CityEntity.Position = position;
            return CityEntity;
        }
        CityEntity = CityEntitiesCreationHelper.MoveInExistingCar(Car, position);
        return CityEntity;
    }

    public void Hide()
    {
        if (CityEntity != null)
        {
            G.ProductLifetimeService.MoveProduct(Car, World.Instance.HiddenSpace.GetEmptyLocation());
        }
        CityEntity = null;
    }
}