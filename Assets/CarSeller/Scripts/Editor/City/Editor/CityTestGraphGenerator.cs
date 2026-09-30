#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

public static class CityTestGraphGenerator
{
    private const int GridWidth = 34;
    private const int GridHeight = 17;

    private const string MainTag = "main";
    private const string SecondaryTag = "secondary";

    [MenuItem("City/Generate Test Graph (17x34)")]
    public static void Generate_17x34()
    {
        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Generate Test City Graph");

        var root = new GameObject($"TestGraph_{GridWidth}x{GridHeight}");
        Undo.RegisterCreatedObjectUndo(root, "Create Test Graph Root");

        var nodesParent = new GameObject("Nodes");
        Undo.RegisterCreatedObjectUndo(nodesParent, "Create Nodes Parent");
        Undo.SetTransformParent(nodesParent.transform, root.transform, "Parent Nodes");

        var edgesParent = new GameObject("Edges");
        Undo.RegisterCreatedObjectUndo(edgesParent, "Create Edges Parent");
        Undo.SetTransformParent(edgesParent.transform, root.transform, "Parent Edges");

        var edgesSecondarySecondaryParent = new GameObject("SecondarySecondary");
        Undo.RegisterCreatedObjectUndo(edgesSecondarySecondaryParent, "Create SecondarySecondary Edges Parent");
        Undo.SetTransformParent(edgesSecondarySecondaryParent.transform, edgesParent.transform, "Parent SecondarySecondary Edges");

        var markersParent = new GameObject("Markers");
        Undo.RegisterCreatedObjectUndo(markersParent, "Create Markers Parent");
        Undo.SetTransformParent(markersParent.transform, root.transform, "Parent Markers");

        var nodes = new RoadNodeAuthor[GridWidth, GridHeight];

        // 1) Create nodes in an orthogonal pattern:
        // - main nodes at even/even coords (2 units apart)
        // - secondary nodes in-between (odd coords => 1 unit spacing)
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                bool isMain = IsMainNode(x, y);
                string kind = isMain ? "Main" : "Secondary";

                var go = new GameObject($"Node_{x:D2}_{y:D2}_{kind}");
                Undo.RegisterCreatedObjectUndo(go, "Create Node");
                Undo.SetTransformParent(go.transform, nodesParent.transform, "Parent Node");

                go.transform.position = new Vector3(x, y, 0f);

                var author = Undo.AddComponent<RoadNodeAuthor>(go);
                author.EnsureId();

                nodes[x, y] = author;
            }
        }

        // 2) Create edges to direct neighbors (right + up) and markers for each edge.
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                var from = nodes[x, y];

                if (x + 1 < GridWidth)
                    CreateEdgeWithMarker(from, nodes[x + 1, y], edgesParent.transform, edgesSecondarySecondaryParent.transform, markersParent.transform);

                if (y + 1 < GridHeight)
                    CreateEdgeWithMarker(from, nodes[x, y + 1], edgesParent.transform, edgesSecondarySecondaryParent.transform, markersParent.transform);
            }
        }

        Selection.activeGameObject = root;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Undo.CollapseUndoOperations(undoGroup);
    }

    private static void CreateEdgeWithMarker(
        RoadNodeAuthor from,
        RoadNodeAuthor to,
        Transform edgesParent,
        Transform edgesSecondarySecondaryParent,
        Transform markersParent)
    {
        bool isSecondarySecondary = !IsMainNode(from.transform.position) && !IsMainNode(to.transform.position);
        var chosenEdgesParent = isSecondarySecondary ? edgesSecondarySecondaryParent : edgesParent;

        string edgeName = $"Edge_{from.name}_to_{to.name}";
        var edgeGo = new GameObject(edgeName);
        Undo.RegisterCreatedObjectUndo(edgeGo, "Create Edge");
        Undo.SetTransformParent(edgeGo.transform, chosenEdgesParent, "Parent Edge");

        var edge = Undo.AddComponent<RoadEdgeAuthor>(edgeGo);
        edge.From = from;
        edge.To = to;
        edge.Bidirectional = true;
        edge.Tags = new[] { isSecondarySecondary ? SecondaryTag : MainTag };
        edge.EnsureId();

        var container = Undo.AddComponent<SplineContainer>(edgeGo);
        edge.Container = container;
        edge.SplineIndex = 0;

        // Author spline
        edge.AlignToNodes();

        // Optional visual (matches RoadNodeAuthor.ConnectToSelected)
        var extrude = Undo.AddComponent<SplineExtrude>(edgeGo);
        extrude.Container = container;

        // Marker on each road
        var markerGo = new GameObject($"Marker_{edgeGo.name}");
        Undo.RegisterCreatedObjectUndo(markerGo, "Create Marker");
        Undo.SetTransformParent(markerGo.transform, markersParent, "Parent Marker");

        markerGo.transform.position = (from.transform.position + to.transform.position) * 0.5f;

        var marker = Undo.AddComponent<CityMarkerAuthor>(markerGo);
        marker.Kind = CityMarkerAuthor.AnchorKind.Edge;
        marker.Edge = edge;
        marker.T = 0.5f;
        marker.Forward = true;
        marker.EnsureId();
    }

    private static bool IsMainNode(int x, int y)
    {
        return (x % 2 == 0) && (y % 2 == 0);
    }

    private static bool IsMainNode(Vector3 worldPosition)
    {
        // Generator places nodes at integer coords -> safe to round.
        int x = Mathf.RoundToInt(worldPosition.x);
        int y = Mathf.RoundToInt(worldPosition.y);
        return IsMainNode(x, y);
    }
}
#endif