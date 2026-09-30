using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ICityPositionable
{
    CityPosition Position { get; set; }
}

/// <summary>
/// 
/// 1 to 1 relationship between a locatable entity and its position in the city
/// Should live as long as having an attached locatable
/// 
/// </summary>
public sealed class CityEntity : ILocation, ICityPositionable
{

    public CityPosition Position { get; set; }
    public ILocatable Subject;
    public IReadOnlyCollection<CityEntityAspect> Aspects => aspects;

    HashSet<CityEntityAspect> aspects = new HashSet<CityEntityAspect>();

    City City;

    public T[] GetAspects<T>() where T : CityEntityAspect
    {
        return aspects.OfType<T>().ToArray();
    }

    public T GetAspect<T>() where T : CityEntityAspect
    {
        return aspects.OfType<T>().FirstOrDefault();
    }

    public bool IsValid()
    {
        return Subject != null;
    }

    internal CityEntity(City city, ILocatable subject, CityPosition initialCityPosition, ICollection<CityEntityAspect> aspects)
    {
        City = city;
        Position = initialCityPosition;
        Attach(subject);
        city.AspectsService.TryAddAspects(this, aspects);
    }

    internal CityEntity(City city, CityPosition initialCityPosition, ICollection<CityEntityAspect> aspects)
    {
        City = city;
        Position = initialCityPosition;;
        city.AspectsService.TryAddAspects(this, aspects);
    }

    internal bool TryAddAspectInternal(CityEntityAspect aspect)
    {
        if (aspect == null) return false;
        return aspects.Add(aspect);
    }

    internal bool TryRemoveAspectInternal(CityEntityAspect aspect)
    {
        if (aspect == null) return false;
        return aspects.Remove(aspect);
    }

    // ILocation implementation
    // Don't use from outside except for CityEntityLifetimeService
    public ILocatable Occupant => Subject;
    public ILocationsHolder Holder => City;
    public bool Attach(ILocatable locatable)
    {
        Debug.Assert(locatable != null, "Locatable to attach cannot be null");
        if (Subject != null || locatable == null) return false;

        Subject = locatable;
        City.Entities[locatable] = this;

        return true;
    }
    public void Detach()
    {
        if (Subject == null) return;

        G.City.AspectsService.TryRemoveAllAspects(this);

        City.Entities.Remove(Subject);
        Subject = null;
        GameEvents.Instance.OnCityEntityDestroyed?.Invoke(new CityEntityDestroyedEventData(this));
    }

    public override string ToString()
    {
        var subjectStr = Subject != null ? Subject.ToString() : "NoSubject";
        return base.ToString() + ' ' + subjectStr;
    }
}