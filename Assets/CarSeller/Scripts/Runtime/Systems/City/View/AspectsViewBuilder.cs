using System.Collections.Generic;

public sealed class AspectsViewBuilder
{
    public Dictionary<CityEntity, CityViewObjectController> cityViewObjects;

    public void buildPinAspect(CityEntity entity, PinAspect aspect)
    {
        var view = cityViewObjects[entity];

    }

}
