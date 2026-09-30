using System.Collections.Generic;
using UnityEngine;
public static class RoadGraphMaintenance
{
    public static void AddEdge(RoadEdge edge)
    {
        Debug.Assert(edge != null && edge.From != null && edge.To != null, "Edge and endpoints required.");
        var from = edge.From;
        var to = edge.To;

        if (!from.Outgoing.Contains(edge)) from.Outgoing.Add(edge);
        if (!to.Incoming.Contains(edge)) to.Incoming.Add(edge);

        if (edge.Bidirectional)
        {
            if (!from.Incoming.Contains(edge)) from.Incoming.Add(edge);
            if (!to.Outgoing.Contains(edge)) to.Outgoing.Add(edge);
        }
        else
        {
            from.Incoming.Remove(edge);
            to.Outgoing.Remove(edge);
        }
    }

    public static void RemoveEdge(RoadEdge edge)
    {
        if (edge == null) return;
        edge.From?.Outgoing.Remove(edge);
        edge.To?.Incoming.Remove(edge);
        edge.From?.Incoming.Remove(edge);
        edge.To?.Outgoing.Remove(edge);
    }

    public static void SetBidirectional(RoadEdge edge, bool bidirectional)
    {
        edge.Bidirectional = bidirectional;
        // Re-apply invariants
        RemoveEdge(edge);
        AddEdge(edge);
    }

    public static void RebuildAdjacency(IReadOnlyList<RoadNode> nodes, IReadOnlyList<RoadEdge> edges)
    {
        foreach (var n in nodes)
        {
            n.Outgoing.Clear();
            n.Incoming.Clear();
        }
        foreach (var e in edges) AddEdge(e);
    }
}