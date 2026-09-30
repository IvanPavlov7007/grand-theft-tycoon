using System.Collections.Generic;
using UnityEngine;

public class TriggerCausable : MonoBehaviour
{
    public List<Triggerable> CurrentTriggerables { get; private set; } = new List<Triggerable>();

    private void OnDisable()
    {
        ClearTriggerables();
    }

    public void AddTriggerable(Triggerable triggerable)
    {
        if (!CurrentTriggerables.Contains(triggerable))
        {
            CurrentTriggerables.Add(triggerable);
        }
    }

    public void RemoveTriggerable(Triggerable triggerable)
    {
        if (CurrentTriggerables.Contains(triggerable))
        {
            CurrentTriggerables.Remove(triggerable);
        }
    }

    public void ClearTriggerables()
    {
        CurrentTriggerables.Clear();
    }
}