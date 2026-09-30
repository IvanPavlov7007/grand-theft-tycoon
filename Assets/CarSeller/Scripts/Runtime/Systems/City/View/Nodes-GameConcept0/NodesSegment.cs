using UnityEngine;
public class NodesSegment : MonoBehaviour
{
    public Transform pointA, pointB;
    public Color col = Color.white;
    public int sortingOrder = -100;
    LineRenderer lr;
    private void Awake()
    {
        lr = gameObject.GetComponent<LineRenderer>();
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.startColor = col;
        lr.endColor = col;
        lr.sortingOrder = sortingOrder;
    }

    public void SetPoints(Transform a, Transform b)
    {
        pointA = a;
        pointB = b;
    }

    private void LateUpdate()
    {
        lr.SetPosition(0, pointA.position);
        lr.SetPosition(1, pointB.position);
    }
}