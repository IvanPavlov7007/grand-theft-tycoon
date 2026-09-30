using UnityEngine;
using UnityEngine.Splines;
using Sirenix.OdinInspector;
using Unity.Mathematics;

[ExecuteAlways]
public class RoadEdgeAuthor : MonoBehaviour
{
    [BoxGroup("Endpoints")] public RoadNodeAuthor From;
    [BoxGroup("Endpoints")] public RoadNodeAuthor To;

    [BoxGroup("Spline")] public SplineContainer Container;
    [BoxGroup("Spline"), ReadOnly] public int SplineIndex = 0;
    [BoxGroup("Options")] public bool Bidirectional = true;
    [BoxGroup("Options")] public bool ClampToXY = true;

    [BoxGroup("Identification")] public string[] Tags;

    [BoxGroup("Identification"), SerializeField, ReadOnly] private string id;
    [BoxGroup("Identification"), ShowInInspector, ReadOnly] public string Id => id;

    [Button, DisableInPlayMode]
    public void EnsureId()
    {
        if (string.IsNullOrEmpty(id))
            id = System.Guid.NewGuid().ToString("N");
    }

    [Button("Align Spline To Nodes"), DisableInPlayMode]
    public void AlignToNodes()
    {
        if (From == null || To == null || Container == null)
        {
            Debug.LogWarning("From/To/Container must be set.");
            return;
        }

        var spline = Container.Splines.Count > 0 ? Container.Splines[0] : Container.AddSpline();
        spline.Clear();

        // World-space endpoints
        Vector3 aWorld = From.transform.position;
        Vector3 bWorld = To.transform.position;

        // Local positions for the container (planar XY)
        Vector3 a = Container.transform.InverseTransformPoint(aWorld);
        Vector3 b = Container.transform.InverseTransformPoint(bWorld);
        if (ClampToXY) { a.z = 0f; b.z = 0f; }

        // One rotation that faces from A to B on the XY plane
        Quaternion rotation = Quaternion.LookRotation(b - a, Vector3.forward);

        // Planar Z-only tangents
        float handleLen = Mathf.Min((a - b).magnitude * 0.1f, 1f);
        Vector3 outZ = new Vector3(0f, 0f, handleLen);
        Vector3 inZ  = new Vector3(0f, 0f, -handleLen);

        var knotA = new BezierKnot(a, Vector3.zero, outZ, rotation);
        var knotB = new BezierKnot(b, inZ, Vector3.zero, rotation);

        spline.Add(knotA);
        spline.Add(knotB);
    }

    [Button("Add Control Knot Midpoint"), DisableInPlayMode]
    public void AddMidKnot()
    {
        if (Container == null) { Debug.LogWarning("Container missing."); return; }
        var spline = Container.Splines.Count > 0 ? Container.Splines[0] : Container.AddSpline();
        if (spline.Count < 2) { Debug.LogWarning("Need at least two knots."); return; }

        var a = spline[0].Position;
        var b = spline[spline.Count - 1].Position;
        if (ClampToXY) { a.z = 0f; b.z = 0f; }
        var mid = Vector3.Lerp(a, b, 0.5f);
        if (ClampToXY) mid.z = 0f;

        float handleLen = Mathf.Min(math.length(a - b) * 0.1f, 0.5f);
        Vector3 outZ = new Vector3(0f, 0f, handleLen);
        Vector3 inZ  = new Vector3(0f, 0f, -handleLen);

        // Same facing as start (A -> B)
        Quaternion rotation = Quaternion.LookRotation(b - a, Vector3.forward);

        var midKnot = new BezierKnot(mid, inZ, outZ, rotation);
        spline.Insert(1, midKnot);
    }

    [Button("Reverse Direction"), DisableInPlayMode]
    public void ReverseDirection()
    {
        if (Container == null) return;
        var spline = Container.Splines.Count > 0 ? Container.Splines[0] : null;
        if (spline == null || spline.Count < 2) return;

        int count = spline.Count;
        var reversed = new BezierKnot[count];
        for (int i = 0; i < count; i++)
        {
            var k = spline[i];
            reversed[count - 1 - i] = new BezierKnot(
                position: k.Position,
                tangentIn: -k.TangentOut,
                tangentOut: -k.TangentIn,
                rotation: k.Rotation
            );
        }

        spline.Clear();
        for (int i = 0; i < reversed.Length; i++) spline.Add(reversed[i]);

        (From, To) = (To, From);
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            // Only assign an ID if missing; no other mutations to avoid dirty flags.
            if (string.IsNullOrEmpty(id))
                id = System.Guid.NewGuid().ToString("N");
        }
    }
}