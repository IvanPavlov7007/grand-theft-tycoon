public abstract class GameState
{
}

public class NeutralGameState : GameState
{
}

public class SellingGameState : GameState
{
    public readonly Car SellingCar;
    public readonly Buyer Buyer;
    public readonly Warehouse SellingWarehouse;
    public SellingGameState(Car sellingCar , Buyer buyer, Warehouse sellingWarehouse)
    {
        SellingCar = sellingCar;
        Buyer = buyer;
        SellingWarehouse = sellingWarehouse;
    }
}

public class StealingGameState : GameState
{
    public readonly Car StealingCar;

    public StealingGameState(Car stealingCar)
    {
        StealingCar = stealingCar;
    }
}

public class FreeRoamGameState : GameState
{
}

public class MissionGameState : GameState
{
    public readonly MissionRuntime CurrentMission;
    public MissionGameState(MissionRuntime currentMission)
        
    {
        CurrentMission = currentMission;
    }
}