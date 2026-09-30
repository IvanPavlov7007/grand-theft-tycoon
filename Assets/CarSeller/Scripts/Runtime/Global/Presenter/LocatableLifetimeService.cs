using System;
using System.Collections.Generic;
using UnityEngine;

//Don't use
[Obsolete]
public class LocatableLifetimeService
{

    Dictionary<ILocatable, ILocation> allLocations => World.Instance.allLocations;

    public void RegisterLocatable(ILocatable locatable, ILocation location, bool notify)
    {
        Debug.Assert(locatable != null, "Locatable cannot be null when registering a location.");
        Debug.Assert(location != null, "Location cannot be null when registering a locatable.");
        Debug.Assert(!allLocations.ContainsKey(locatable), $"Locatable {locatable} is already registered in allLocations when trying to register to location {location}.");
        if (location.Occupant == null)
        {
            if (location.Attach(locatable))
            {
                Debug.LogWarning($"Locatable {locatable} registered to location {location} inside RegisterLocatable.");
            }
            else
            {
                Debug.LogError($"Failed to attach locatable {locatable} to location {location} inside RegisterLocatable.");
            }
        }
        else if(location.Occupant != locatable)
        {
            Debug.LogError($"Location {location} is already occupied by {location.Occupant} when trying to register locatable {locatable}.");
        }

        allLocations[locatable] = location;

        if (notify)
        {
            GameEvents.Instance.OnLocatableRegistered?.Invoke(new LocatableCreatedEventData(locatable, location));
        }
    }

    public void DestroyLocatable(ILocatable locatable)
    {
        Debug.Assert(locatable != null, "Locatable cannot be null when destroying.");
        var location = allLocations[locatable];
        if (location != null)
        {
            location.Detach();
        }
        GameEvents.Instance.OnLocatableDestroyed?.Invoke(new LocatableDestroyedEventData(locatable));
    }
}