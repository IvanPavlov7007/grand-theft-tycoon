using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// provided by ChatGPT

public class CityViewStreetsBuilder
{
    /// <summary>
    /// Builds LineRenderers for all unique connections between nodes.
    /// Returns the created root GameObject so the caller owns its lifecycle.
    /// </summary>
    public GameObject BuildStreets(
        City city,
        Transform parent = null,
        Material lineMaterial = null,
        float lineWidth = 0.05f)
    {
        if (city == null || city.nodesNet == null || city.nodesNet.Nodes == null)
            return null;

        var streetsRoot = new GameObject("City Streets");
        if (parent != null)
            streetsRoot.transform.SetParent(parent, false);

        // Track unique undirected edges for this build only (no persistent state).
        var seenEdges = new HashSet<(Node, Node)>(new UndirectedEdgeComparer());

        foreach (var node in city.nodesNet.Nodes)
        {
            if (node == null || node.connectedNeighbors == null)
                continue;

            foreach (var neighbor in node.connectedNeighbors)
            {
                if (neighbor == null) continue;

                var edge = (node, neighbor);
                if (!seenEdges.Add(edge)) // already handled (a,b) or (b,a)
                    continue;

                CreateLineSegment(streetsRoot.transform, node, neighbor, lineMaterial, lineWidth);
            }
        }

        return streetsRoot;
    }

    private static void CreateLineSegment(Transform parent, Node a, Node b, Material lineMaterial, float lineWidth)
    {
        var go = new GameObject($"Street [{a.InitialPosition}] <-> [{b.InitialPosition}]");
        go.transform.SetParent(parent, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;

        // Pick current position if available, otherwise initial
        Vector3 p0 = a.CurrentPosition != default ? (Vector3)a.CurrentPosition : (Vector3)a.InitialPosition;
        Vector3 p1 = b.CurrentPosition != default ? (Vector3)b.CurrentPosition : (Vector3)b.InitialPosition;

        lr.SetPosition(0, p0);
        lr.SetPosition(1, p1);

        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        if (lineMaterial != null)
        {
            lr.material = lineMaterial;
        }
        else
        {
            var mat = new Material(Shader.Find("Sprites/Default")) { color = Color.gray };
            lr.material = mat;
        }

        lr.sortingOrder = -50;
        lr.numCornerVertices = 2;
        lr.numCapVertices = 2;
        lr.alignment = LineAlignment.View;
        lr.textureMode = LineTextureMode.Stretch;
    }

    /// <summary>
    /// Treats (a,b) and (b,a) as the same edge using reference identity.
    /// Uses RuntimeHelpers.GetHashCode to avoid issues if Node overrides GetHashCode.
    /// </summary>
    private sealed class UndirectedEdgeComparer : IEqualityComparer<(Node, Node)>
    {
        public bool Equals((Node, Node) x, (Node, Node) y)
        {
            return (ReferenceEquals(x.Item1, y.Item1) && ReferenceEquals(x.Item2, y.Item2)) ||
                   (ReferenceEquals(x.Item1, y.Item2) && ReferenceEquals(x.Item2, y.Item1));
        }

        public int GetHashCode((Node, Node) obj)
        {
            int h1 = obj.Item1 != null ? RuntimeHelpers.GetHashCode(obj.Item1) : 0;
            int h2 = obj.Item2 != null ? RuntimeHelpers.GetHashCode(obj.Item2) : 0;
            // Order-independent combine to ensure (a,b) and (b,a) collide.
            if (h2 < h1) { var t = h1; h1 = h2; h2 = t; }
            unchecked { return (h1 * 397) ^ h2; }
        }
    }
}