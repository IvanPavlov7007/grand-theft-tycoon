using System;
using UnityEngine;

//TODO in the future, make the Ownership system as suggested by GPT
// - ownership based on containment graph

[Obsolete]
/// <summary>
/// Unfinished system to track player possessions based on locatable events. Please apply suggestion above.
/// </summary>
public class PlayerPossessionTracker : GlobalSingletonBehaviour<PlayerPossessionTracker>
{
    protected override PlayerPossessionTracker GlobalInstance { get => G.PlayerPossessionTracker; set => G.PlayerPossessionTracker = value; }

    private void OnEnable()
    {
        GameEvents.Instance.OnLocatableRegistered += onLocatableCreated;
        GameEvents.Instance.OnLocatableDestroyed += onLocatableDestroyed;
        GameEvents.Instance.OnLocatableLocationChanged += onLocatableLocationChanged;
        // Never put GameEvents.Instance.OnLocatableStateChanged
        // here, because that would cause infinite loop
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnLocatableRegistered -= onLocatableCreated;
        GameEvents.Instance.OnLocatableDestroyed -= onLocatableDestroyed;
        GameEvents.Instance.OnLocatableLocationChanged -= onLocatableLocationChanged;
    }

    private void onLocatableLocationChanged(LocatableLocationChangedEventData data)
    {
        
    }

    private void onLocatableDestroyed(LocatableDestroyedEventData data)
    {
    }

    private void onLocatableCreated(LocatableCreatedEventData data)
    {
        Debug.Assert(data.Locatable != null, "LocatableCreatedEventData has null Locatable.");
        Debug.Assert(data.Location != null, "LocatableCreatedEventData has null Location.");
        if (isPlayerOwner(data.Location))
        {
            //G.PlayerManager.AddPossession(data.Locatable as IPossession);
        }
    }

    private bool isPlayerOwner(ILocation location)
    {
        throw new NotImplementedException();
        //return G.Player.Owns(location as IPossession);
    }
}