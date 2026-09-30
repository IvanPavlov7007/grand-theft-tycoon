using System;
using UnityEngine;

// TODO sort out not used anymore
public static class GameRules
{
    public static readonly WarehouseCanStoreCar WarehouseCanStoreCar = new WarehouseCanStoreCar();
    public static readonly WarehouseCanStripCar WarehouseCanStripCar = new WarehouseCanStripCar();
    public static readonly CanRideOutCar CanRideOutCar = new CanRideOutCar();
    public static readonly CarCanBeExited carCanBeExited = new CarCanBeExited();
    public static readonly CarBelongsToPlayer CarBelongsToPlayer = new CarBelongsToPlayer();
    public static readonly CarCanBeDisassembled CarCanBeDisassembled = new CarCanBeDisassembled();
    public static readonly ModelControlledByPlayer ModelControlledByPlayer = new ModelControlledByPlayer();
    public static readonly CanBePurchased CanBePurchased = new CanBePurchased();
    public static readonly CarCanBeSoldToBuyer CarCanBeSoldToBuyer = new CarCanBeSoldToBuyer();
    public static readonly CarIsOfRequiredType CarIsOfRequiredType = new CarIsOfRequiredType();
    public static readonly EdgeIsSecondary EdgeIsSecondary = new EdgeIsSecondary();
    public static readonly CarCanBeSpawnedOnEdge CarCanBeSpawnedOnEdge = new CarCanBeSpawnedOnEdge();
    public static readonly BuyerTypeCanBeSpanwnedOnEdge BuyerTypeCanBeSpanwnedOnEdge = new BuyerTypeCanBeSpanwnedOnEdge();
}

public class CarBelongsToPlayer
{
    public bool Check(Car car)
    {
        if (car == null)
            return false;

        return G.VehicleController.GetPersonalVehiclesList().OwnsVehicle(car);
        //return G.Player.Owns(car);
    }
}

public class CarCanBeDisassembled
{
    public bool Check(Car car)
    {
        if (car == null)
            return false;

        foreach (var location in car.carParts.Keys)
        {
            if (location.Occupant != null)
            {
                return true;
            }
        }
        return false;
    }
}

public abstract class CarWarehouseRule
{
    public abstract bool Check(Car car, Warehouse warehouse);
}

public class CanRideOutCar : CarWarehouseRule
{
    public override bool Check(Car car, Warehouse warehouse)
    {
        if (car == null || warehouse == null)
            return false;

        var carWarehouse = CityLocatorHelper.GetWarehouse(car);
        return car.IsComplete() && carWarehouse == warehouse;
    }
}

public class WarehouseCanStoreCar : CarWarehouseRule
{
    public override bool Check(Car car, Warehouse warehouse)
    {
        if (car == null || warehouse == null)
            return false;

        //TODO check if wh have capacity and if car is storable
        return warehouse.AvailableCarParkingSpots > 0;
    }
}

public class WarehouseCanStripCar : CarWarehouseRule
{
    public override bool Check(Car car, Warehouse warehouse)
    {
        if (car == null || warehouse == null)
            return false;

        //TODO check if warehouse has capacity and car is strippable
        return true;
    }
}

public class WarehouseCanReceivePart
{
    public bool Check(Product part, Warehouse warehouse)
    {
        if (part == null || warehouse == null)
            return false;
        if (part is Car)
        {
            return warehouse.AvailableCarParkingSpots > 0;
        }

        //TODO check if warehouse has capacity for part
        return true;
    }
}

public class ModelControlledByPlayer
{
    public bool Check(ILocatable model)
    {
        var state = G.GameState;
        Debug.Assert(state != null, "GameState is null");
        Debug.Assert(model != null, "Model is null");

        return G.VehicleController.CurrentCar == model;
    }
}

public class CarCanBeExited
{
    public bool Check(Car car)
    {
        if(car == null)
            return false;
        bool isCurrentCar = G.VehicleController.CurrentCar == car;
        return isCurrentCar && !G.VehicleController.IsInPrimaryVehicle;
    }
}

public class CanBePurchased
{
    public bool Check(IPurchasable purchasable)
    {
        return G.Player.Money >= purchasable.UnitPrice && purchasable.IsAvailable;
    }

    public string GetUnavailabilityReason(IPurchasable purchasable)
    {
        if (G.Player.Money < purchasable.UnitPrice)
            return "Not enough money";
        if (!purchasable.IsAvailable)
            return "Not available";
        return "";
    }
}

public class CarIsOfRequiredType
{
    public bool Check(Car car, CarType requiredType)
    {
        Debug.Assert(car != null, "Car is null");

        if(requiredType == CarType.Any)
            return true;

        return car.Kind.Type == requiredType;
    }
}

public enum CanCantBeSoldReason
{
    None,
    CarBelongsToPlayer,
    CarNotOfRequiredType
}

public class CarCanBeSoldToBuyer
{
    public CanCantBeSoldReason Check(Car car, Buyer buyer)
    {
        Debug.Assert(car != null, "Car is null");
        Debug.Assert(buyer != null, "Buyer is null");

        if(GameRules.CarBelongsToPlayer.Check(car))
            return CanCantBeSoldReason.CarBelongsToPlayer;
        if(!GameRules.CarIsOfRequiredType.Check(car,buyer.RequiredCarType))
            return CanCantBeSoldReason.CarNotOfRequiredType;
        return CanCantBeSoldReason.None;
    }
}

public class EdgeIsSecondary
{
    public bool Check(RoadEdge edge)
    {
        Debug.Assert(edge != null, "Edge is null");
        return edge.HasTag("Secondary");
    }
}

public class CarCanBeSpawnedOnEdge
{
    public bool Check(Car car, RoadEdge edge)
    {
        Debug.Assert(car != null, "Car is null");
        return !GameRules.EdgeIsSecondary.Check(edge) || car.HasModifier<CanNarrowStreet>();
    }
}

public class BuyerTypeCanBeSpanwnedOnEdge
{
    public bool Check(CarType buyerType, RoadEdge edge)
    {
        Debug.Assert(edge != null, "Edge is null");

        return true;
        //return !GameRules.EdgeIsSecondary.Check(edge) || buyerType == CarType.Bike || buyerType == CarType.Any;
    } 
}

//////////////////////////////////////////////////////////////////////////////////////

[Serializable]
public struct ColorPalette
{
    public Color WhiteColor;
    public Color LimeGreenColor;
    public Color LightBlueColor;
    public Color GreyedOutColor;
    public Color RedColor;
    public Color YellowColor;
}

public class SellPriceCalculator : SellPriceWrapper
{
    public override float CalculateMultiplier(CarRarity rarity, CarType requiredType)
    {
        CarRarityConfig carRarityConfig = G.Balancing.CarRarityConfigs.Find(x => x.GetRarity() == rarity);
        CarTypeConfig requiredTypeConfig = G.Balancing.CarTypeConfigs.Find(x => x.GetType() == requiredType);
        float multiplier = carRarityConfig.ValueMultiplier * requiredTypeConfig.SpecificValue;
        return multiplier;
    }

    public override float CalculateUnitSellPrice(Car car, Buyer buyer)
    {
        Debug.Assert(car != null, "Car is null");
        Debug.Assert(buyer != null, "Buyer is null");

        CarType requiredType = buyer.RequiredCarType;
        CarType carType = car.Kind.Type;
        CarRarity carRarity = car.Kind.Rarity;

        CarTypeConfig carTypeConfig = G.Balancing.CarTypeConfigs.Find(x => x.GetType() == carType);
        CarTypeConfig requiredTypeConfig = G.Balancing.CarTypeConfigs.Find(x => x.GetType() == requiredType);
        CarRarityConfig carRarityConfig = G.Balancing.CarRarityConfigs.Find(x => x.GetRarity() == carRarity);

        float price = carTypeConfig.BaseValue * carRarityConfig.ValueMultiplier * requiredTypeConfig.SpecificValue;
        return price;

    }
}

public abstract class SellPriceWrapper
{

    public abstract float CalculateMultiplier(CarRarity rarity, CarType requiredType);
    public abstract float CalculateUnitSellPrice(Car car, Buyer buyer);
    public static float CalculateAbsolutePrice(float unitPrice)
    {
        return G.Balancing.GlobalConfig.DisplayValueMultiplier * unitPrice;
    }
}