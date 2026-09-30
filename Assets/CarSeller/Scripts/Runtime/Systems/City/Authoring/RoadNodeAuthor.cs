using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Splines;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
public class RoadNodeAuthor : MonoBehaviour
{
    [SerializeField, ReadOnly] private string id;
    [ShowInInspector, ReadOnly] public string Id => id;

    // Allow flipping the start look direction rule
    [SerializeField] public bool StartLooksAway = false;

    [Button, DisableInPlayMode]
    public void EnsureId()
    {
        if (string.IsNullOrEmpty(id))
            id = System.Guid.NewGuid().ToString("N");
    }

    [Button("Connect To Selected Node"), DisableInPlayMode]
    public void ConnectToSelected()
    {
#if UNITY_EDITOR
        var sel = UnityEditor.Selection.activeGameObject;
        if (sel == null) { Debug.Log("No GameObject selected."); return; }
        var other = sel.GetComponent<RoadNodeAuthor>();
        if (other == null) { Debug.Log("Selected object is not a RoadNodeAuthor."); return; }
        if (other == this) { Debug.Log("Select a different node."); return; }

        EnsureId(); other.EnsureId();
        EnsureUniqueIdInCurrentStage();
        other.EnsureUniqueIdInCurrentStage();

        var edgeGO = new GameObject($"Edge_{Id}_to_{other.Id}");
        edgeGO.transform.SetParent(transform.parent, false);

        var edge = edgeGO.AddComponent<RoadEdgeAuthor>();
        edge.From = this;
        edge.To = other;
        edge.Bidirectional = true;
        edge.EnsureId();

        var container = edgeGO.AddComponent<SplineContainer>();
        edge.Container = container;
        edge.SplineIndex = 0;

        var spline = container.Splines.Count > 0 ? container.Splines[0] : container.AddSpline();
        spline.Clear();

        // World-space endpoints
        Vector3 aWorld = transform.position;
        Vector3 bWorld = other.transform.position;

        // Local positions (planar XY)
        Vector3 a = container.transform.InverseTransformPoint(aWorld);
        Vector3 b = container.transform.InverseTransformPoint(bWorld);
        a.z = 0f; b.z = 0f;

        // One rotation that faces from A to B
        Quaternion rotation = Quaternion.LookRotation(b - a, Vector3.forward);

        // Planar Z-only tangents
        float handleLen = Mathf.Min((a - b).magnitude * 0.1f,1f);
        Vector3 outZ = new Vector3(0f, 0f, handleLen);
        Vector3 inZ  = new Vector3(0f, 0f, -handleLen);

        var knotA = new BezierKnot(a, Vector3.zero, outZ, rotation);
        var knotB = new BezierKnot(b, inZ, Vector3.zero, rotation);

        spline.Add(knotA);
        spline.Add(knotB);

        var splineExtrude = edgeGO.AddComponent<SplineExtrude>();
        splineExtrude.Container = container;

        UnityEditor.Undo.RegisterCreatedObjectUndo(edgeGO, "Create Road Edge");
        UnityEditor.Selection.activeGameObject = edgeGO;
#endif
    }

#if UNITY_EDITOR
    // Stage-aware uniqueness: only checks within the current scene or prefab stage.
    private void EnsureUniqueIdInCurrentStage()
    {
        if (string.IsNullOrEmpty(id)) return;

        // Determine current stage. In Prefab Mode, only check objects in that stage.
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        RoadNodeAuthor[] all;

        if (prefabStage != null)
        {
            // Search under the prefab stage root only.
            all = prefabStage.stageHandle.FindComponentsOfType<RoadNodeAuthor>();
        }
        else
        {
            // Search in the active scene only.
            all = Object.FindObjectsByType<RoadNodeAuthor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        foreach (var other in all)
        {
            if (other == this) continue;
            if (other.Id == id)
            {
                id = System.Guid.NewGuid().ToString("N");
                // Avoid SetDirty in validation paths; only mark dirty for explicit operations.
                EditorUtility.SetDirty(this);
                break;
            }
        }
    }
#endif

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            // Only assign an ID if missing; do not run duplicate resolution automatically.
            if (string.IsNullOrEmpty(id))
                id = System.Guid.NewGuid().ToString("N");
        }
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        // In editor sessions, after duplication or creation, we can ensure stage-local uniqueness.
        if (!Application.isPlaying)
        {
            // Skip for prefab assets to avoid churn when opening prefab mode.
            if (!PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                EnsureUniqueIdInCurrentStage();
            }
        }
    }
#endif
}