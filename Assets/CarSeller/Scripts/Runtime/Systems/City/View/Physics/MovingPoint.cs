using Sirenix.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovingPoint : MonoBehaviour, IMovement
{
    /// <summary>
    /// Velocity direction provided by the movement system.
    /// </summary>
    public Vector2 ProvidedDirection => currentDirection;
    public float Speed => currentSpeed;
    public float Acceleration { get; private set; }

    ICityPositionable cityPositionable;
    IDirectionProvider directionProvider;
    ISpeedProvider speedProvider;
    Transform arrowRotationPoint;
    Transform body;
    Car Car;

    CarFlexibleJunctionPolicy _flexPolicy;

    private const float Epsilon = 1e-4f;
    private const float Tolerance = 0.1f; // To avoid floating point issues

    float currentSpeed = 0f;
    Vector2 currentDirection = Vector2.up;

    private void Awake()
    {
        directionProvider = GetComponent<IDirectionProvider>();
        speedProvider = GetComponent<ISpeedProvider>();
        body = transform.GetChild(0);
        arrowRotationPoint = transform.GetChild(1);
    }

    public void Initialize(CityEntity entity) 
    {
        this.cityPositionable = entity; 
        _flexPolicy = new CarFlexibleJunctionPolicy(entity);
        Car = entity.Subject as Car;
    }



    // TODO for cayote: check that all of the returns properly handle it
    void LateUpdate()
    {
        processMovement();
        body.rotation = safeSlerp(body.rotation, currentDirection, 10f * Time.deltaTime);
        //body.up = Vector3.Slerp(body.up, currentDirection, 10f * Time.deltaTime);
    }

    private Quaternion safeSlerp(Quaternion currentRotation, Vector2 currentDirection, float t)
    {
        float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;

        Quaternion quaternion = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        return Quaternion.Slerp(currentRotation, quaternion,t);
    }

    struct CityPositionWithDistance
    {
        public readonly CityPosition Position;
        public readonly float Distance;
        public CityPositionWithDistance(CityPosition position, float distance)
        {
            Position = position;
            Distance = distance;
        }
    }

    private IReadOnlyList<CityPositionWithDistance> findRecursivePathToClosest(
        RoadEdge currentEdge,
        Vector2 target,
        float minDistance,
        ImmutableHashSet<RoadEdge> visitedEdges)
    {
        if (visitedEdges.Contains(currentEdge))
        {
            return new List<CityPositionWithDistance>();
        }

        CityPosition closest = CityPosition.GetClosestOnEdge(currentEdge, target);
        float closestDistance = Vector2.Distance(closest.WorldPosition, target);
        if (closestDistance >= minDistance)
        {
            return new List<CityPositionWithDistance>();
        }
        minDistance = closestDistance;

        var newVisitedEdges = new HashSet<RoadEdge>(visitedEdges);
        newVisitedEdges.Add(currentEdge);

        var outgoings = new List<RoadEdge>(_flexPolicy.GetAllowedOutgoing(currentEdge.From,currentEdge));
        outgoings.AddRange(_flexPolicy.GetAllowedOutgoing(currentEdge.To, currentEdge));

        var currentEntry = new CityPositionWithDistance(closest, closestDistance);

        List<CityPositionWithDistance> bestPath = null;
        foreach (var edge in outgoings)
        {
            if (newVisitedEdges.Contains(edge))
            {
                continue;
            }

            var path = findRecursivePathToClosest(
                    edge,
                    target,
                    closestDistance,
                    new ImmutableHashSet<RoadEdge>(newVisitedEdges))
                .ToList();

            if (path.Count == 0)
            {
                continue;
            }

            var last = path.Last();
            if (last.Distance < minDistance)
            {
                minDistance = last.Distance;
                bestPath = path;
            }
        }

        if (bestPath == null)
        {
            return new List<CityPositionWithDistance>() { currentEntry };
        }

        bestPath.Insert(0, currentEntry);
        return bestPath;
    }

    CityPosition debugFinal;

    private CityPosition processMovementGeneral(CityPosition currentPosition, Vector2 target, float distance)
    {
        List<CityPosition> closestPoints;
        // if we start on a node, we can just pick the best edge and go there
        if (currentPosition.Edge == null)
        {
            closestPoints = bestPathFromNode(currentPosition.Node, target);
            if(closestPoints != null && closestPoints.Count > 0)
            {
                bool nodeIsFrom = closestPoints[0].Edge.From == currentPosition.Node;

                currentPosition = CityPosition.On(closestPoints[0].Edge, nodeIsFrom ? 0f : 1f);
            }
        }
        else
        {
            closestPoints
                = findRecursivePathToClosest(currentPosition.Edge,
                target,
                Mathf.Infinity,
                new ImmutableHashSet<RoadEdge>(new HashSet<RoadEdge>())
                ).Select(p => p.Position).ToList();
        }
        if(closestPoints == null || closestPoints.Count == 0)
        {
            return currentPosition; // No path to target
        }
        debugFinal = closestPoints.Last();
        return moveToNextPoint(closestPoints, currentPosition, distance);
    }

    // Here we assume that closestPoints[0] is on the same edge as current
    private CityPosition moveToNextPoint(IReadOnlyList<CityPosition> closestPoints, CityPosition current, float remainingDistance)
    {
        //how many points
        if (closestPoints.Count == 1)
        {
            return moveToNextPointFinally(current, closestPoints[0], remainingDistance);
        }

        var currentEdgeTargetPosition = current.GetConnectionPositionTowardsEdge(closestPoints[1].Edge);

        // IMPORTANT: connection positions can come back with a different Forward.
        // Align them so all "ahead/behind" and distance computations are consistent.
        if (currentEdgeTargetPosition.Forward != current.Forward)
        {
            currentEdgeTargetPosition = currentEdgeTargetPosition.Reversed();
        }

        RoadEdge currentEdge = current.Edge;
        float stepDistance = current.DistanceToAnotherOnSameEdge(currentEdgeTargetPosition);

        if (remainingDistance > stepDistance)
        {
            var nextCurrent = closestPoints[1].GetConnectionPositionFromAnotherEdge(currentEdge);
            return moveToNextPoint(closestPoints.Skip(1).ToList(), nextCurrent, remainingDistance - stepDistance);
        }

        return moveToNextPointFinally(current, currentEdgeTargetPosition, remainingDistance);
    }

    const float tolerance = 0.01f; // To avoid floating point issues
    private CityPosition moveToNextPointFinally(CityPosition currernt, CityPosition final, float remainingDistance)
    {
        Debug.Assert(currernt.Edge == final.Edge, "Final point must be on the same edge as current point,\n" +
            $"Currently {currernt.Edge} and {final.Edge}");

        // Align parameterization (Forward/Percentage) before any "flow" or distance logic.
        if (final.Forward != currernt.Forward)
        {
            final = final.Reversed();
        }

        if (!currernt.FlowsIntoAnotherOnTheSameEdge(final) &&  !Car.HasModifier<CanTurnAround>())
        {
            Debug.Log($"Cannot flow from {currernt} to {final} on the same edge");
            // T and forward of both:
            Debug.Log($"Current: {currernt.Percentage} {currernt.Forward}, Final: {final.Percentage} {final.Forward}");
            return currernt; // We cannot move to the final point
        }

        float distanceToFinal = currernt.DistanceToAnotherOnSameEdge(final);
        if (distanceToFinal < tolerance)
        {
            return final; // We are close enough to the final point
        }

        return currernt.TowardsAnotherForDistance(final, Mathf.Min(distanceToFinal, remainingDistance));
    }

    private List<CityPosition> bestPathFromNode(RoadNode node, Vector2 target)
    {
        IReadOnlyList<CityPositionWithDistance> bestOptionPath = null;
        float bestOptionDistance = float.MaxValue;

        foreach (var edge in node.Outgoing)
        {
            var path = findRecursivePathToClosest(
                    edge,
                    target,
                    bestOptionDistance,
                    new ImmutableHashSet<RoadEdge>(new HashSet<RoadEdge>()))
                .ToList();

            if (path.Count == 0)
            {
                continue;
            }

            var last = path.Last();
            if (last.Distance < bestOptionDistance)
            {
                bestOptionDistance = last.Distance;
                bestOptionPath = path;
            }
        }

        if (bestOptionPath == null)
        {
            return null;
        }

        return bestOptionPath.Select(p => p.Position).ToList();
    }

    private void processMovement()
    {
        Vector2 inputDirection = Vector2.ClampMagnitude(directionProvider.ProvidedDirection, 1f);
        float inputMagnitude = inputDirection.magnitude;
        Vector2 inputDirectionN = inputDirection.sqrMagnitude > Epsilon ? inputDirection.normalized : Vector2.zero;

        float lerpV = 10f * Time.deltaTime;

        Vector2 arrowDir = inputDirectionN.sqrMagnitude > Epsilon ? inputDirectionN : Vector2.up;
        arrowRotationPoint.rotation = Quaternion.Lerp(
            arrowRotationPoint.rotation,
            Quaternion.FromToRotation(Vector2.up, arrowDir),
            lerpV);

        arrowRotationPoint.localScale = new Vector3(
            1f,
            Mathf.Lerp(arrowRotationPoint.localScale.y, Mathf.Clamp01(inputMagnitude), lerpV),
            1f);

        var cityPosition = cityPositionable.Position;

        Vector2 cappedTarget = inputDirection * 1f + cityPosition.WorldPosition;

        // Speed scales with |inputDirection| and brakes when input is reduced.
        float desiredSpeed = speedProvider.Speed * inputMagnitude;

        const float brakeMultiplier = 2.5f;
        float previousSpeed = currentSpeed;

        float speedChangePerSecond =
            desiredSpeed > currentSpeed
                ? speedProvider.Acceleration
                : speedProvider.Acceleration * brakeMultiplier;

        currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, speedChangePerSecond * Time.deltaTime);
        Acceleration = Time.deltaTime > Epsilon ? (currentSpeed - previousSpeed) / Time.deltaTime : 0f;

        Debug.DrawLine(cityPosition.WorldPosition, cappedTarget, Color.green);

        cityPosition = processMovementGeneral(cityPosition, cappedTarget, Time.deltaTime * currentSpeed);

        Debug.Assert(cityPosition.Edge != null || cityPosition.Node != null, "Position must be either on an edge or on a node");

        Vector2 chosenTangentDirection =
            cityPosition.Edge != null ? cityPosition.GetCurrentTangent() :
            (inputDirectionN.sqrMagnitude > Epsilon ? inputDirectionN : currentDirection);

        cityPositionable.Position = cityPosition;
        currentDirection = chosenTangentDirection;
        transform.position = cityPosition.WorldPosition;
    }

    private void OnDrawGizmos()
    {
        Vector2 worldPos = debugFinal.WorldPosition;
        //draw a circle at worldPos
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(worldPos, 0.2f);
    }
}
