using System.Collections.Generic;

public class CarFlexibleJunctionPolicy : IJunctionPolicy
{
    public readonly CityEntity Entity;
    public readonly Car Car;

    public static bool IgnoreRules = false;

    public CarFlexibleJunctionPolicy(CityEntity entity)
    {
        Entity = entity;
        Car = entity.Subject as Car;
    }

    public IEnumerable<RoadEdge> GetAllowedOutgoing(RoadNode atNode, RoadEdge fromEdge)
    {
        bool canNarrow = Car.HasModifier<CanNarrowStreet>();

        foreach (var edge in atNode.Outgoing)
        {
            yield return edge;
        }

        yield break;

        if (G.City.TryGetTrafficLightAtNode(atNode, out var tl))
        {
            if (!tl.TryGetStateForEdge(fromEdge, out var state) || state == TrafficLightState.Stop)
                yield break;
        }

        foreach (var edge in atNode.Outgoing)
        {
            if (!IgnoreRules && !canNarrow && edge.HasTag("secondary"))
                continue;

            // Traffic lights gating (only if node has a TL)
            if (!IgnoreRules && tl != null)
            {
                if (!tl.TryGetStateForEdge(edge, out var state) || state == TrafficLightState.Stop)
                    continue;
            }

            yield return edge;
        }
    }
}