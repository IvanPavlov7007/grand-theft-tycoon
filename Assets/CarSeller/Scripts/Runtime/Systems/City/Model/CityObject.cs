/// <summary>
/// Base class for objects that only exist within the city environment.
/// Used by Profiles through ContentProvider
/// </summary>
//public class CityObject : ILocatable, IDestroyable
//{
//    public string Name { get; private set; }
//    public string InfoText { get; set; }

//    public City.CityLocation Location { get; private set; }

//    public City.CityMarker CityMarker { get; private set; }

//    public PinStyle PinStyle { get; set; }

//    public object Data { get; set; }

//    public event Action<IDestroyable> onBeingDestroyed;

//    public CityObject(string name, string infoText, ILocation location, City.CityMarker cityMarker, CityObjectData data = null, PinStyle pinStyle = null)
//    {
//        Name = name;
//        InfoText = infoText;
//        CityMarker = cityMarker;
//        PinStyle = pinStyle;
//        Data = data;

//        Debug.Assert(location != null);
//        Debug.Assert(location is  City.CityLocation);
//        Location = location as City.CityLocation;

//        if (location.Attach(this))
//            GameEvents.Instance.OnLocatableCreated?.Invoke(new LocatableCreatedEventData(this, location));
//        else
//            Debug.LogError($"Failed to attach city object {name} to location {location}");
//    }

//    public void Destroy()
//    {
//        Location.Detach();
//        onBeingDestroyed?.Invoke(this);
//        GameEvents.Instance.OnLocatableDestroyed?.Invoke(new LocatableDestroyedEventData(this));
//    }
//}

