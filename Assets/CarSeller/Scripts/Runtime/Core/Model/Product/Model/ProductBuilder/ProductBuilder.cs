using System.Collections.Generic;

public interface IProductBuilder
{
    Car BuildCar(CarRuntimeConfig carRuntimeConfig);
    CarFrame BuildCarFrame(CarFrameRuntimeConfig carFrameRuntimeConfig);
    Product BuildEngine(EngineRuntimeConfig engineConfig);
    Product BuildWheel(WheelRuntimeConfig wheelConfig);
    Product BuildSpoiler(SpoilerRuntimeConfig spoilerConfig);
}

public class ProductBuilder : IProductBuilder
{
    public Car BuildCar(CarRuntimeConfig carRuntimeConfig)
    {
        //1 create car
        Car car = new Car(carRuntimeConfig);

        //2 initialize car frame
        var carFrame = BuildCarFrame(carRuntimeConfig.CarFrameRuntimeConfig);
        car.SetCarFrame(carFrame);

        //3 initialize products and slots
        var slotsDict = new Dictionary<Car.CarPartLocation, PartSlotRuntimeConfig>();
        foreach (var slotConfig in carRuntimeConfig.SlotConfigs)
        {
            //3.1 create product
            Product product = null;
            if (slotConfig.RuntimeConfig != null)
            {
                product = slotConfig.BuildOccupyingProduct(this);
            }
            //3.2 create slot location with product inside
            var slotLocation = new Car.CarPartLocation(car,slotConfig,product);
            slotsDict.Add(slotLocation, slotConfig);
        }
        car.addSlots(slotsDict);
        return car;
    }
    public CarFrame BuildCarFrame(CarFrameRuntimeConfig carFrameRuntimeConfig)
    {
        CarFrame carFrame = new CarFrame(carFrameRuntimeConfig);
        return carFrame;
    }

    public Product BuildEngine(EngineRuntimeConfig engineConfig)
    {
        Engine engine = new Engine(engineConfig);
        return engine;
    }

    public Product BuildWheel(WheelRuntimeConfig wheelConfig)
    {
        Wheel wheel = new Wheel(wheelConfig);
        return wheel;
    }

    public Product BuildSpoiler(SpoilerRuntimeConfig spoilerConfig)
    {
        Spoiler spoiler = new Spoiler(spoilerConfig);
        return spoiler;
    }
}
