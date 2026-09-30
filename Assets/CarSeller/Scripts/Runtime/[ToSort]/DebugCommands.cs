using UnityEngine;
using UnityEngine.InputSystem;

public class DebugCommands : MonoBehaviour
{
#if UNITY_EDITOR || DEBUG

    public void OnSprint(InputValue inputValue)
    {
        G.GameManager.switchFastForward();
    }

    public void OnA(InputValue val)
    {
        if(!G.runIntialized)
            return;
        G.CarSpawnManager.SpawnCarAtPosition(G.VehicleController.CurrentVehicleEntity.Position);
    }

    public void OnD(InputValue val)
    {

    }

    public void OnL(InputValue val)
    {
        Debug.Log("Cheat visible: " + !G.City.AspectsSystem.visibleSystem.cheatVisible);
        G.City.AspectsSystem.visibleSystem.cheatVisible = !G.City.AspectsSystem.visibleSystem.cheatVisible;
    }

    public void OnS(InputValue val)
    {
        if(!G.runIntialized)
            return;
        G.BuyerManager.SpawnBuyerAtPosition(Buyer.Any(), G.VehicleController.CurrentVehicleEntity.Position, null,null);
    }


    static bool showAreas = false;
    public void OnW(InputValue val)
    {
        var vc = GameObject.FindAnyObjectByType<CityAreasVisualsController>();
        Debug.Assert(vc != null, "CityAreasVisualsController not found in scene");
        foreach (var area in G.Areas.Values)
        {
            vc.SetHighlighted(area.Id, !showAreas);
        }
        showAreas = !showAreas;
    }

    public void OnQ(InputValue val)
    {
        CarFlexibleJunctionPolicy.IgnoreRules = !CarFlexibleJunctionPolicy.IgnoreRules;
        //var pos = G.VehicleController.CurrentVehicleEntity.Position;
        //Car car = G.SimplifiedCarsManager.CreateCarHidden(new CarKind(CarType.Bike, CarRarity.Common));
        //CityEntitiesCreationHelper.MoveInExistingCar(car, pos);
    }


#endif
}