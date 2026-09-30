using UnityEngine;

public static class CameraHelper
{
    public static void SetCurrentPositionAtCar()
    {
        
        if(!G.runIntialized)
            return;
        var cityEntity = G.VehicleController.CurrentVehicleEntity;
        Debug.Assert(G.CameraMovementManager != null);
        G.CameraMovementManager?.Teleport(cityEntity.Position.WorldPosition);
    }

    private static Vector2 getCityLocationPosition(CityEntity cityEntity)
    {
        if(cityEntity != null)
        {
            return cityEntity.Position.WorldPosition;
        }
        Debug.LogError("CityLocation is null in ViewEnsure");
        return Vector2.zero;
    }
}