using UnityEngine;

public class PoliceStateViewController : MonoBehaviour
{
    PoliceUnit policeUnit;
    public event System.Action<PoliceUnitState> OnStateChanged;
    PoliceUnitState lastState;

    public void Initialize(PoliceUnit policeUnit)
    {
        this.policeUnit = policeUnit;
        lastState = currentState();
    }

    public PoliceUnitState currentState()
    {
        return policeUnit.State;
    }

    private void LateUpdate()
    {
        if(lastState != currentState())
        {
            lastState = currentState();
            OnStateChanged?.Invoke(lastState);
        }
    }
}