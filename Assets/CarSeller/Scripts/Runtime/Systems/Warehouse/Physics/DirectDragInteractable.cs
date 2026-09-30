using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DirectDragInteractable : Interactable
{
    Rigidbody2D rb;

    bool dragging = false;
    RigidbodyType2D? originalBodyType; 
    Vector2 dragTarget;
    private TargetJoint2D targetJoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        if (dragging)
        {
            targetJoint.target = G.GameCursor.transform.position;
        }
    }

    protected override void OnCursorDragStart()
    {
        base.OnCursorDragStart();

        originalBodyType = rb.bodyType;
        rb.bodyType = RigidbodyType2D.Dynamic;
        dragging = true;

        targetJoint = gameObject.AddComponent<TargetJoint2D>();
        targetJoint.anchor = rb.transform.InverseTransformPoint(G.GameCursor.transform.position);
        targetJoint.target = G.GameCursor.transform.position;

        targetJoint.maxForce = 1000f;
        targetJoint.dampingRatio = 1f;
        targetJoint.frequency = 5f;

    }
    protected override void OnCursorDragEnd()
    {
        base.OnCursorDragEnd();

        if (targetJoint != null)
        {
            Destroy(targetJoint);
            targetJoint = null;
        }

        rb.bodyType = originalBodyType.Value;
        dragging = false;
    }
}
