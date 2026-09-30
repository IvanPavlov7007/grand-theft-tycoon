using UnityEngine;

public class PoliceLightsViewController : MonoBehaviour
{
    PoliceStateViewController policeStateViewController;
    PoliceLightsVisuals lightsVisuals;
    public void Initialize(PoliceStateViewController policeStateViewController, PoliceLightsVisuals lightsVisuals)
    {
        this.policeStateViewController = policeStateViewController;
        this.lightsVisuals = lightsVisuals;
        policeStateViewController.OnStateChanged += handleStateChanged;
        handleStateChanged(policeStateViewController.currentState());
    }
    private void handleStateChanged(PoliceUnitState newState)
    {
        switch (newState)
        {
            case PoliceUnitState.Chase:
            case PoliceUnitState.Backup:
            case PoliceUnitState.TryRelocate:
                lightsVisuals.TurnOn();
            break;
            default:
                lightsVisuals.TurnOff();
            break;
        }
    }
}