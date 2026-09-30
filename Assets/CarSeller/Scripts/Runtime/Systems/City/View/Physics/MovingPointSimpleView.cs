using UnityEngine;

public class MovingPointSimpleView : MonoBehaviour
{
    GraphMovement graphMovement;

    public void Initialize(GraphMovement graphMovement)
    {
        this.graphMovement = graphMovement;
    }

    private void LateUpdate()
    {
        Debug.Assert(graphMovement != null);
        Debug.Assert(graphMovement.Owner != null);
        var positionData = graphMovement.Owner.CityPosition;
        transform.position = positionData.WorldPosition;
        transform.up = Vector3.Slerp(transform.up, graphMovement.Up, Time.deltaTime * 10f);
    }

}