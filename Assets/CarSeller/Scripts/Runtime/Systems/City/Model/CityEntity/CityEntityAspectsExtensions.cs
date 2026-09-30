using UnityEngine;

public static class CityEntityAspectsExtensions
{
    public static bool TryAddAspect<TAspect>(this CityEntity entity, TAspect aspect, bool allowDuplicateType = false)
        where TAspect : class, CityEntityAspect
    {
        Debug.Assert(entity != null, "entity cannot be null");
        Debug.Assert(aspect != null, "aspect cannot be null");
        Debug.Assert(G.City != null);

        return G.City.AspectsService.TryAddAspect(entity, aspect, allowDuplicateType);
    }

    public static bool TryRemoveAspect<TAspect>(this CityEntity entity)
        where TAspect : CityEntityAspect
    {
        Debug.Assert(entity != null, "entity cannot be null");
        Debug.Assert(G.City != null);

        return G.City.AspectsService.TryRemoveAspect<TAspect>(entity);
    }
}
