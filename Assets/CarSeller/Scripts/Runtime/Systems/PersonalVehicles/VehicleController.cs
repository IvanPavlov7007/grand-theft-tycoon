using UnityEngine;

public sealed class VehicleController
{
    public VehicleControlState CurrentState { get; private set; }

    public Car PrimaryCar => PrimaryVehicleManager.PrimaryVehicle.Car;
    public CityEntity CurrentVehicleEntity => CurrentState.CurrentCityEntity;
    public Car CurrentCar => CurrentState.Car;
    internal PersonalVehicle CurrentPrimaryVehicle => PrimaryVehicleManager.PrimaryVehicle;

    public bool IsInPrimaryVehicle => CurrentState is PrimaryVehicleControlState;

    private PersonalVehiclesList PersonalVehicles;
    private PrimaryVehicleManager PrimaryVehicleManager;

    internal PersonalVehiclesList GetPersonalVehiclesList() => PersonalVehicles;

    public void Initialize(VehicleControllerConfig config, PersonalVehiclesList list)
    {
        Debug.Assert(list != null, "PersonalVehiclesList cannot be null");
        Debug.Assert(config != null, "VehicleControllerConfig cannot be null");

        PersonalVehicles = list;
        initializePrimaryVehicleManager(config.initialPrimeVehicleIndex);

        initializeStateAsPrimaryVehicleControlState(config);
    }

    void initializeStateAsPrimaryVehicleControlState(VehicleControllerConfig data)
    {
        var initialPosition = data.GetInitialCityPosition();
        var initialEntity = PrimaryVehicleManager.DeployPrimaryVehicle(initialPosition);
        ChangeState(new PrimaryVehicleControlState(initialEntity,this));
    }

    private void initializePrimaryVehicleManager(int initialPrimaryVehicleIndex)
    {
        PrimaryVehicleManager = new PrimaryVehicleManager(PersonalVehicles, initialPrimaryVehicleIndex);
    }

    public void DrivePrimaryVehicle(CityPosition cityPosition)
    {
        CurrentState.DrivePrimaryVehicle(cityPosition);
    }

    public void DriveWorldVehicle(CityEntity vehicleEntity)
    {
        CurrentState.EnterVehicle(vehicleEntity);
    }

    public void ExitWorldVehicle()
    {
        CurrentState.ExitVehicle();
    }

    public void SwapPrimaryVehicle(PersonalVehicle personalVehicle)
    {
        CurrentState.SwapPrimaryVehicle(personalVehicle);
    }


    void SetPrimaryVehicleState(CityEntity vehicleEntity)
    {
        ChangeState(new PrimaryVehicleControlState(vehicleEntity, this));
    }

    void SetWorldVehicleState(CityEntity vehicleEntity)
    {
        ChangeState(new WorldVehicleControlState(vehicleEntity, this));
    }

    void ChangeState(VehicleControlState newState)
    {
        var oldState = CurrentState;
        CurrentState = newState;

        GameEvents.Instance.onVehicleControlStateChanged?.
            Invoke(new VehicleControlStateChangedEventData(oldState, newState));
    }

    
    public abstract class VehicleControlState
    {
        public readonly CityEntity CurrentCityEntity;
        public Car Car => CurrentCityEntity.Subject as Car;
        public readonly VehicleController Context;

        internal VehicleControlState(CityEntity currentCityEntity, VehicleController context)
        {
            CurrentCityEntity = currentCityEntity;
            Context = context;
        }

        internal void DrivePrimaryVehicle(CityPosition cityPosition)
        {
            Context.PrimaryVehicleManager.HidePrimaryVehicle();
            var entity = Context.PrimaryVehicleManager.DeployPrimaryVehicle(cityPosition);
            Context.SetPrimaryVehicleState(entity);
        }

        abstract internal void SwapPrimaryVehicle(PersonalVehicle personalVehicle);

        abstract internal void EnterVehicle(CityEntity vehicleEntity);
        abstract internal void ExitVehicle();
    }

    public class PrimaryVehicleControlState : VehicleControlState
    {
        public PrimaryVehicleControlState(CityEntity currentCityEntity, VehicleController context) : base(currentCityEntity, context)
        {
        }

        internal override void EnterVehicle(CityEntity vehicleEntity)
        {
            Context.PrimaryVehicleManager.HidePrimaryVehicle();
            Context.SetWorldVehicleState(vehicleEntity);
        }

        internal override void ExitVehicle()
        {
            throw new UnityException($"Prohibited Exit try: Primary Vehicle {Car}");
        }

        internal override void SwapPrimaryVehicle(PersonalVehicle personalVehicle)
        {
            Context.PrimaryVehicleManager.HidePrimaryVehicle();
            Context.PrimaryVehicleManager.SwapPrimaryVehicle(personalVehicle);
            var entity = Context.PrimaryVehicleManager.DeployPrimaryVehicle(CurrentCityEntity.Position);
            Context.SetPrimaryVehicleState(entity);
        }
    }

    public class WorldVehicleControlState : VehicleControlState
    {
        public WorldVehicleControlState(CityEntity currentCityEntity, VehicleController context) : base(currentCityEntity, context)
        {
        }

        internal override void EnterVehicle(CityEntity vehicleEntity)
        {
            Context.SetWorldVehicleState(vehicleEntity);
        }

        internal override void ExitVehicle()
        {
            var currentPosition = CurrentCityEntity.Position;
            var deployEntity = Context.PrimaryVehicleManager.DeployPrimaryVehicle(currentPosition);
            Context.SetPrimaryVehicleState(deployEntity);
        }

        internal override void SwapPrimaryVehicle(PersonalVehicle personalVehicle)
        {
            Context.PrimaryVehicleManager.SwapPrimaryVehicle(personalVehicle);
            Context.SetWorldVehicleState(CurrentCityEntity);
        }
    }
}

