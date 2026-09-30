using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CityTrafficLightsSpawner
{
    private const string TrafficLightsRootName = "TrafficLights";

    public static List<TrafficLightRuntimeController> Spawn(City city, CityGraphAsset graph, Transform cityRoot)
    {
        var created = new List<TrafficLightRuntimeController>();

        if (city == null || graph == null || cityRoot == null)
            return created;

        if (graph.TrafficLights == null || graph.TrafficLights.Count == 0)
            return created;

        if (graph.TrafficLightPrefab == null)
        {
            Debug.LogWarning("CityTrafficLightsSpawner: CityGraphAsset.TrafficLightPrefab is null.");
            return created;
        }

        var root = GetOrCreateRuntimeRoot(cityRoot);
        ClearChildren(root);

        var nodeById = city.Nodes.Where(n => n != null && !string.IsNullOrEmpty(n.Id)).ToDictionary(n => n.Id);
        var edgeById = city.Edges.Where(e => e != null && !string.IsNullOrEmpty(e.Id)).ToDictionary(e => e.Id);

        foreach (var tl in graph.TrafficLights)
        {
            if (tl == null || string.IsNullOrEmpty(tl.NodeId))
                continue;

            if (!nodeById.TryGetValue(tl.NodeId, out var node))
                continue;

            var go = Object.Instantiate(graph.TrafficLightPrefab, root);
            go.name = string.IsNullOrEmpty(tl.Id) ? "TrafficLight" : $"TrafficLight_{tl.Id}";
            go.transform.position = node.Position;

            var view = go.GetComponent<TrafficLightViewController>();
            if (view == null)
            {
                Debug.LogWarning($"CityTrafficLightsSpawner: '{go.name}' prefab is missing TrafficLightViewController.");
                continue;
            }

            var directionsByKey = new Dictionary<string, Vector2>();
            var keyByEdgeId = new Dictionary<string, string>();

            if (tl.EdgeSlots != null)
            {
                for (int i = 0; i < tl.EdgeSlots.Count; i++)
                {
                    var slot = tl.EdgeSlots[i];
                    if (slot == null || string.IsNullOrEmpty(slot.Key) || string.IsNullOrEmpty(slot.EdgeId))
                        continue;

                    keyByEdgeId[slot.EdgeId] = slot.Key;

                    if (!edgeById.TryGetValue(slot.EdgeId, out var edge) || edge == null)
                        continue;

                    Vector2 dir;
                    try
                    {
                        dir = edge.GetTangentFromNode(node, 0.05f, out _);
                        if (dir.sqrMagnitude <= 0f)
                            throw new System.Exception("Zero tangent.");
                    }
                    catch
                    {
                        var other = edge.From == node ? edge.To : edge.From;
                        dir = other != null ? (other.Position - node.Position).normalized : Vector2.up;
                    }

                    directionsByKey[slot.Key] = dir;
                }
            }

            view.InstantiateLights(directionsByKey);

            var runtime = go.GetComponent<TrafficLightRuntimeController>();
            if (runtime == null)
                runtime = go.AddComponent<TrafficLightRuntimeController>();

            runtime.Initialize(
                id: tl.Id,
                nodeId: tl.NodeId,
                view: view,
                keyByEdgeId: keyByEdgeId,
                allKeys: directionsByKey.Keys,
                program: tl.Program,
                preparationTimeSeconds: tl.PreparationTimeSeconds,
                initialTimeOffsetSeconds: tl.InitialTimeOffsetSeconds);

            created.Add(runtime);
        }

        return created;
    }

    private static Transform GetOrCreateRuntimeRoot(Transform cityRoot)
    {
        var runtimeParent = cityRoot.parent;

        if (runtimeParent != null)
        {
            var existingSibling = runtimeParent.Find(TrafficLightsRootName);
            if (existingSibling != null)
                return existingSibling;

            var siblingRoot = new GameObject(TrafficLightsRootName);
            siblingRoot.transform.SetParent(runtimeParent, false);
            return siblingRoot.transform;
        }

        var scene = cityRoot.gameObject.scene;
        if (scene.IsValid())
        {
            foreach (var rootObject in scene.GetRootGameObjects())
            {
                if (rootObject.name == TrafficLightsRootName)
                    return rootObject.transform;
            }
        }

        var runtimeRoot = new GameObject(TrafficLightsRootName);
        if (scene.IsValid())
            SceneManager.MoveGameObjectToScene(runtimeRoot, scene);

        return runtimeRoot.transform;
    }

    private static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Object.Destroy(root.GetChild(i).gameObject);
    }
}