using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[System.Serializable]
public sealed class RoadNode
{
    public string Id;
    public Vector2 Position;
    public List<RoadEdge> Outgoing = new List<RoadEdge>();
    public List<RoadEdge> Incoming = new List<RoadEdge>();
}

[System.Serializable]
public sealed class RoadEdge
{
    public string Id;
    public RoadNode From;
    public RoadNode To;

    public string[] Tags;

    // Unity Splines authoring model: container holding one or more splines
    public SplineContainer Container;
    public int SplineIndex;

    public float Length;
    public bool Bidirectional = true;

    public bool HasTag(string tag)
    {
        if (Tags == null || tag == null) return false;
        for (int i = 0; i < Tags.Length; i++)
        {
            if (string.Equals(Tags[i], tag, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    public Spline GetSpline()
    {
        if (Container == null) return null;
        var splines = Container.Splines;
        if (splines == null || SplineIndex < 0 || SplineIndex >= splines.Count) return null;
        return splines[SplineIndex];
    }

    public float LengthFromTo(float t1, float t2)
    {
        return Length * Mathf.Abs(t2 - t1);
    }

    public bool TryGetClosestPoint(Vector2 worldPosition, out float bestT, out float bestDist2, int coarseDivisions = 64, int refineIterations = 8)
    {
        bestT = 0f;
        bestDist2 = float.PositiveInfinity;

        if (Container == null)
        {
            return false;
        }

        var spline = GetSpline();
        if (spline == null)
        {
            return false;
        }

        var tr = Container.transform;

        // 1) Coarse pass
        for (int i = 0; i <= coarseDivisions; i++)
        {
            float t = i / (float)coarseDivisions;
            float d2 = DistanceSquaredToEdgeAt(spline, tr, worldPosition, t);
            if (d2 < bestDist2)
            {
                bestDist2 = d2;
                bestT = t;
            }
        }

        // 2) Local refinement around the best coarse sample
        float step = 1f / coarseDivisions;
        for (int iter = 0; iter < refineIterations; iter++)
        {
            float tLeft = Mathf.Clamp01(bestT - step);
            float tRight = Mathf.Clamp01(bestT + step);

            float d2Left = DistanceSquaredToEdgeAt(spline, tr, worldPosition, tLeft);
            if (d2Left < bestDist2)
            {
                bestDist2 = d2Left;
                bestT = tLeft;
            }

            float d2Right = DistanceSquaredToEdgeAt(spline, tr, worldPosition, tRight);
            if (d2Right < bestDist2)
            {
                bestDist2 = d2Right;
                bestT = tRight;
            }

            step *= 0.5f;
        }

        return true;
    }

    private static float DistanceSquaredToEdgeAt(Spline spline, Transform containerTransform, Vector2 worldPosition, float t)
    {
        var localPos = SplineUtility.EvaluatePosition(spline, t);
        var worldPos3 = containerTransform.TransformPoint((Vector3)localPos);
        var worldPos2 = new Vector2(worldPos3.x, worldPos3.y);
        return (worldPos2 - worldPosition).sqrMagnitude;
    }

    /// <summary>
    /// Returns the unit tangent direction in world XY plane when leaving this edge
    /// from the given node at the given percentage (0..1). Sets forward=true if
    /// motion goes From-&gt;To, false if To-&gt;From.
    /// </summary>
    public Vector2 GetTangentFromNode(RoadNode node, float percentage, out bool forward)
    {
        if (this == null)
            throw new System.ArgumentNullException(nameof(node), "Edge is null in GetTangentFromNode.");

        var spline = GetSpline();
        if (spline == null)
            throw new System.InvalidOperationException($"Edge '{Id}' has no valid spline.");

        forward = true;

        if (node == From)
        {
            var tanLocal = SplineUtility.EvaluateTangent(spline, Mathf.Clamp01(percentage));
            var tanWorld3 = Container != null
                ? Container.transform.TransformDirection((Vector3)tanLocal)
                : (Vector3)tanLocal;
            return ((Vector2)tanWorld3).normalized;
        }

        if (node == To && Bidirectional)
        {
            forward = false;
            var tanLocal = SplineUtility.EvaluateTangent(spline, 1f - Mathf.Clamp01(percentage));
            var tanWorld3 = Container != null
                ? Container.transform.TransformDirection((Vector3)tanLocal)
                : (Vector3)tanLocal;
            return -((Vector2)tanWorld3).normalized;
        }

        // Detailed error for debugging graph issues
        string nodeId = node != null ? node.Id : "null";
        string edgeId = Id ?? "null";
        bool isFrom = node == From;
        bool isTo = node == To;
        bool bidi = Bidirectional;

        throw new System.InvalidOperationException(
            $"Cannot compute tangent direction from node '{nodeId}' on edge '{edgeId}'. " +
            $"Relationship: isFrom={isFrom}, isTo={isTo}, edgeBidirectional={bidi}. " +
            $"If leaving from 'To' endpoint, the edge must be bidirectional."
        );
    }
}

public interface IJunctionPolicy
{
    // Returns allowed outgoing edges given desired direction and time/context.
    IEnumerable<RoadEdge> GetAllowedOutgoing(RoadNode atNode, RoadEdge fromEdge);
}