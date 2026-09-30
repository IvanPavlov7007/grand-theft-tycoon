using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarPark : TypeRestrictedStorage
{
    public int OverallLotsCount { get; private set; }

    public List<CarParkingLot> FreeLots;
    public List<CarParkingLot> OccupiedLots;

    public CarPark(Warehouse warehouse, int overallLotsCount) : base(warehouse,typeof(Car)) 
    { 
        OverallLotsCount = overallLotsCount;
        FreeLots = new List<CarParkingLot>(overallLotsCount);
        OccupiedLots = new List<CarParkingLot>();
    }

    public override int FreeLocationsCount => FreeLots.Count;

    public override ILocation GetEmptyLocation()
    {
        if(FreeLots.Count == 0)
            return null;
        return FreeLots[0];
    }

    public override ILocation[] GetLocations()
    {
        return OccupiedLots.Concat(FreeLots).ToArray();
    }

    public class CarParkingLot : ILocation
    {
        public CarPark ParkingLots { get; private set; }
        public Car Car { get; private set; }
        public ILocationsHolder Holder => ParkingLots;
        public ILocatable Occupant => Car;
        public CarParkingLot(CarPark parkingLots, Car car)
        {
            Car = car;
            ParkingLots = parkingLots;
        }
        public bool Attach(ILocatable locatable)
        {
            Debug.Assert(Car == null, "CarParkingLot: Attempt to attach a car to an occupied parking lot");
            var car = locatable as Car;
            if(Car != null || car == null)
                return false;
            Car = car;
            ParkingLots.FreeLots.Remove(this);
            ParkingLots.OccupiedLots.Add(this);
            return true;
        }
        public void Detach()
        {
            Car = null;
            ParkingLots.OccupiedLots.Remove(this);
            if (ParkingLots.FreeLots.Contains(this))
            {
                Debug.LogWarning("CarParkingLot: Attempt to add an already existing free parking lot");
            }
            else
            {
                ParkingLots.FreeLots.Add(this);
            }
        }
    }
}