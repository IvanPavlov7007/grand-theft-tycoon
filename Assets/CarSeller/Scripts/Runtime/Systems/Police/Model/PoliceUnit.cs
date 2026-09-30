//TODO Maybe make police be more than just a city object in the future
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PoliceUnit : CityDestroyable, IPoliceUnitAIData, ILocatable
{
    public ICityPositionable CityPositionable;
    public CityPosition CityPosition => CityPositionable.Position;
    public Vector2? TargetPosition { get; set; }
    public IAIMovement Movement => GraphMovement;
    public IVision Vision => ConeVision;
    public PoliceUnitState State { get; set; }
    public PersonalityTag Personality{get; private set; }
    public SliceVision ConeVision { get; private set; }
    public GraphMovement GraphMovement { get; private set; }

    public PoliceUnit(SpeedVarations speedVariations, SliceVisionSettings settings, 
        PersonalityTag personality)
    {
        GraphMovement = new GraphMovement(this,speedVariations);
        ConeVision = new SliceVision(this, settings);
        Personality = personality;
    }

    /// <summary>
    /// Currently must be called right after construction.
    /// 
    /// TODO refactor to avoid this requirement.
    /// Probably Merge with CityEntity by making this a component of CityEntity.
    /// </summary>
    /// <param name="cityEntity"></param>
    public void Initialize(CityEntity cityEntity)
    {
        CityPositionable = cityEntity;
        randomizeInitialDirection();
    }

    private void randomizeInitialDirection()
    {
        var positionData = CityPosition;
        bool forward = positionData.Forward;
        float t = positionData.Percentage;
        if (positionData.Edge != null && positionData.Edge.Bidirectional && Random.value < 0.5f)
        {
            forward = !forward;
            t = 1f - t;
        }
        CityPositionable.Position = CityPosition.On(positionData.Edge,t, forward);
    }

    internal void Update(float deltaTime, PoliceAISystem aiSystem, PoliceAIContext stateMachine)
    {
        var positionData = CityPosition;
        RoadNode a = positionData.Node;//Anchor node - node from which we move from
        RoadEdge edge = positionData.Edge;
        bool forward = positionData.Forward;
        float t = positionData.Percentage;
        Vector2 chosenTangentDirection;

        //First check if we exactly on a node
        if (a != null)
        {
            //Check if there is an edge in the desired direction
            edge = aiSystem.PickTurn(stateMachine, this, a, edge);
            
            if (edge != null)
            {
                chosenTangentDirection = edge.GetTangentFromNode(a, 0f, out forward);
            }
            else
            {
                return; // No edge to go to
            }
        }
        else
        {
            //We are between nodes, need to understand which direction

            if (forward)
            {
                a = edge.From;
            }
            else
            {
                a = edge.To;
            }
        }

        float stepLength = deltaTime * GraphMovement.Speed;
        float length_a_to_b = edge.Length;


        //water flow in a noded network
        while (stepLength >= length_a_to_b * (1f - t))// While step is longer than remaining distance to B
        {
            stepLength -= length_a_to_b * (1f - t);
            a = (a == edge.From) ? edge.To : edge.From;
            edge = aiSystem.PickTurn(stateMachine, this, a, edge);
            if (edge == null)
            {
                //No edge to proceed further, stop at the current node
                stepLength = 0f;
                break;
            }

            length_a_to_b = edge.Length;
            t = 0f;
            forward = (edge.From == a);
        }

        if (edge == null)
        {
            //We have reached the end of the line, stop at node A
            positionData = CityPosition.At(a);
        }
        else
        {
            t = (t * length_a_to_b + stepLength) / length_a_to_b;

            positionData = CityPosition.On(edge, t, forward);
        }
        CityPositionable.Position = positionData;
    }
}

public class GraphMovement : IAIMovement, ISpeedProvider
{
    public bool TowardsTarget { get; set; }

    public PoliceUnit Owner { get; set; }
    public GraphMovement(PoliceUnit owner, SpeedVarations speedVarations)
    {
        this.speedVariations = speedVarations;
        Owner = owner;
    }

    public TempoState TempoState { get; set; }

    // NEW: cap imposed by AI for the current frame
    public float MaxSpeedOverride { get; set; }

    Vector2 up = Vector2.up;
    public Vector2 Up
    {
        get
        {
            var edge = Owner.CityPosition.Edge;
            if (edge == null)
            {
                return up;
            }

            up = Owner.CityPosition.Edge.GetTangentFromNode(
                Owner.CityPosition.Forward ? edge.From : edge.To,
                Owner.CityPosition.Percentage,
                out bool forward);
            return up;
        }
    }

    public Vector2 ProvidedDirection => Up;

    SpeedVarations speedVariations;
    public SpeedVarations SpeedVariations => G.PoliceManager.policeSpeedVariations;

    public float Speed
    {
        get
        {
            float baseSpeed = TempoState switch
            {
                TempoState.Hold   => 0f,
                TempoState.Slow   => SpeedVariations.Slow,
                TempoState.Medium => SpeedVariations.Medium,
                TempoState.Fast   => SpeedVariations.Fast,
                _                 => SpeedVariations.Medium,
            };

            // If AI set a cap for this frame, apply it.
            if (MaxSpeedOverride > 0f && MaxSpeedOverride < baseSpeed)
                return MaxSpeedOverride;

            return baseSpeed;
        }
    }

    //Currently ignored
    public float Acceleration { get; set; }
}

[Serializable]
public struct SpeedVarations
{
    public float Slow;
    public float Medium;
    public float Fast;
}

[Serializable]
public struct SliceVisionSettings
{
    public float Radius;
    /// <summary>
    /// Full cone angle in degrees.
    /// </summary>
    public float Angle;
}

public class SliceVision : IVision
{
    public readonly PoliceUnit Owner;
    SliceVisionSettings settings;
    public SliceVisionSettings Settings => G.PoliceManager.SliceVisionSettings; //{ get { return settings; } set { settings = value; } }

    public SliceVision(PoliceUnit owner, SliceVisionSettings settings)
    {
        Owner = owner;
        this.settings = settings;
    }

    public bool CanSeeTarget(CityPosition target)
    {
        var ownerPos = Owner.CityPosition.WorldPosition;
        var ownerUp = Owner.GraphMovement.Up;
        var targetPos = target.WorldPosition;

        // 1. Vector from owner to target
        Vector2 toTarget = targetPos - ownerPos;

        // 2. Distance check
        float distanceSqr = toTarget.sqrMagnitude;
        float radiusSqr = Settings.Radius * Settings.Radius;
        if (distanceSqr > radiusSqr)
            return false;

        // 3. Degenerate check
        if (toTarget.sqrMagnitude < Mathf.Epsilon || ownerUp.sqrMagnitude < Mathf.Epsilon)
            return false;

        // 4. Normalize
        Vector2 dirToTarget = toTarget.normalized;
        Vector2 forward = ownerUp.normalized;

        // 5. Angle check via dot product
        // cos(theta) = dot(a, b) when a and b are normalized.
        float dot = Vector2.Dot(forward, dirToTarget);

        // Precompute cosine of half-angle
        float halfAngleRad = (Settings.Angle * 0.5f) * Mathf.Deg2Rad;
        float cosHalfAngle = Mathf.Cos(halfAngleRad);

        // If dot >= cos(halfAngle), target is inside the cone.
        return dot >= cosHalfAngle;
    }
}