using System.Collections.Generic;
using UnityEngine;

public static class PolygonTriangulator
{
    public static bool TryTriangulate(IReadOnlyList<Vector2> polygon, List<int> trianglesOut)
    {
        trianglesOut.Clear();

        if (polygon == null || polygon.Count < 3)
            return false;

        int n = polygon.Count;

        // Build index list in CCW order
        var V = new List<int>(n);
        if (SignedArea(polygon) > 0f)
        {
            for (int i = 0; i < n; i++)
                V.Add(i);
        }
        else
        {
            for (int i = 0; i < n; i++)
                V.Add((n - 1) - i);
        }

        int guard = 0;
        while (V.Count > 2 && guard++ < 10000)
        {
            bool earFound = false;

            for (int i = 0; i < V.Count; i++)
            {
                int prev = V[(i - 1 + V.Count) % V.Count];
                int curr = V[i];
                int next = V[(i + 1) % V.Count];

                if (!IsConvex(polygon[prev], polygon[curr], polygon[next]))
                    continue;

                if (ContainsAnyPointInTriangle(polygon, V, prev, curr, next))
                    continue;

                // Ear
                trianglesOut.Add(prev);
                trianglesOut.Add(curr);
                trianglesOut.Add(next);

                V.RemoveAt(i);
                earFound = true;
                break;
            }

            if (!earFound)
                return false; // likely self-intersecting or degenerate polygon
        }

        return trianglesOut.Count >= 3;
    }

    private static float SignedArea(IReadOnlyList<Vector2> p)
    {
        float a = 0f;
        for (int i = 0, j = p.Count - 1; i < p.Count; j = i++)
            a += (p[j].x * p[i].y) - (p[i].x * p[j].y);
        return a * 0.5f;
    }

    private static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
    {
        // For CCW polygon, convex if cross > 0
        float cross = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        return cross > 0f;
    }

    private static bool ContainsAnyPointInTriangle(IReadOnlyList<Vector2> polygon, List<int> vertexIndices, int ia, int ib, int ic)
    {
        Vector2 a = polygon[ia];
        Vector2 b = polygon[ib];
        Vector2 c = polygon[ic];

        for (int i = 0; i < vertexIndices.Count; i++)
        {
            int idx = vertexIndices[i];
            if (idx == ia || idx == ib || idx == ic)
                continue;

            if (PointInTriangle(polygon[idx], a, b, c))
                return true;
        }

        return false;
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        // Barycentric / same-side
        bool b1 = Sign(p, a, b) < 0f;
        bool b2 = Sign(p, b, c) < 0f;
        bool b3 = Sign(p, c, a) < 0f;

        return (b1 == b2) && (b2 == b3);
    }

    private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }
}