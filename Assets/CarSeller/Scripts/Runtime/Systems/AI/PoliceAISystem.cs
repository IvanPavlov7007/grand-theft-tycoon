using System;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;
public class PoliceAISystem
{
    public void UpdateState(PoliceAIContext state)
    {
        checkForSuspect(state);
        updateUnits(state);
    }

    void checkForSuspect(PoliceAIContext stateMachine)
    {
        var unitsSeeingSuspect = new List<IPoliceUnitAIData>();
        var tryingToRelocateUnits = new List<IPoliceUnitAIData>();
        var lastSeenPositionReached = false;

        foreach (var unit in stateMachine.PoliceUnits)
        {
            if (unit.Vision.CanSeeTarget(stateMachine.SuspectRealPosition))
            {
                unitsSeeingSuspect.Add(unit);
            }
        }

        if (unitsSeeingSuspect.Count > 0)
        {
            // Suspect visible: Chase/Backup
            foreach (var unit in stateMachine.PoliceUnits)
            {
                unit.State = unitsSeeingSuspect.Contains(unit)
                    ? PoliceUnitState.Chase
                    : PoliceUnitState.Backup;
            }

            updateLastSeenPosition(stateMachine, stateMachine.SuspectRealPosition);
            stateMachine.State = PoliceAISystemState.Chase;
            return;
        }

        // No one sees the suspect: TryRelocate / Calm handling
        foreach (var unit in stateMachine.PoliceUnits)
        {
            switch (unit.State)
            {
                case PoliceUnitState.Chase:
                case PoliceUnitState.Backup:
                    unit.State = PoliceUnitState.TryRelocate;
                    tryingToRelocateUnits.Add(unit);
                    break;

                case PoliceUnitState.TryRelocate:
                    tryingToRelocateUnits.Add(unit);
                    break;

                case PoliceUnitState.Calm:
                    break;

                default:
                    Debug.LogWarning("Unhandled PoliceUnitState in checkForSuspect: " + unit.State + " of " + unit);
                    break;
            }

            if (stateMachine.SuspectLastSeenPosition.HasValue &&
                unitPositionCheckedSuspectPosition(unit.CityPosition, stateMachine.SuspectLastSeenPosition.Value))
            {
                lastSeenPositionReached = true;
            }
        }

        if (tryingToRelocateUnits.Count > 0 && !lastSeenPositionReached)
        {
            // Still trying to relocate around last seen position
            stateMachine.State = PoliceAISystemState.Chase;
        }
        else
        {
            // Gave up: suspect lost, calm down and seek again
            resetLastSeenPosition(stateMachine);
            stateMachine.State = PoliceAISystemState.Seek;

            foreach (var unit in stateMachine.PoliceUnits)
            {
                unit.State = PoliceUnitState.Calm;
            }
        }
    }
    void updateUnits(PoliceAIContext stateMachine)
    {
        foreach(var unit in stateMachine.PoliceUnits)
        {
            switch (unit.State)
            {
                case PoliceUnitState.Chase:
                    updateUnitChase(unit, stateMachine);
                    break;
                case PoliceUnitState.Backup:
                    updateUnitBackup(unit, stateMachine);
                    break;
                case PoliceUnitState.TryRelocate:
                    updateUnitTryRelocate(unit, stateMachine);
                    break;
                case PoliceUnitState.Calm:
                    updateUnitCalm(unit, stateMachine);
                    // Logic for calm behavior
                    break;
                default:
                    Debug.LogWarning("Unhandled PoliceUnitState in updateUnits: " + unit.State + "of " + unit);
                    break;
            }
        }
    }

    bool unitPositionCheckedSuspectPosition(CityPosition unitPosition, CityPosition suspectPosition)
    {
        const float reachedThreshold = 1.0f; // tune this

        var unitPos = unitPosition.WorldPosition;
        var suspectPos = suspectPosition.WorldPosition;

        return Vector2.Distance(unitPos, suspectPos) <= reachedThreshold;
    }

    private void updateUnitCalm(IPoliceUnitAIData unit, PoliceAIContext stateMachine)
    {
        unit.Movement.TempoState = TempoState.Slow;
        unit.TargetPosition = null;
    }

    private void updateUnitTryRelocate(IPoliceUnitAIData unit, PoliceAIContext stateMachine)
    {
        unit.Movement.TempoState = TempoState.Medium;
        
        unit.TargetPosition = stateMachine.SuspectLastSeenPosition.HasValue ?
            stateMachine.SuspectLastSeenPosition.Value.WorldPosition :
            null;

    }
    private Vector2 closestEdgeNode(CityPosition position, Vector2 unitPos)
    {
        var edge = position.Edge;
        var node = position.Node;
        if (edge != null)
        {
            var toToPosition = unitPos - edge.To.Position;
            var fromToPosition = unitPos - edge.From.Position;

            return toToPosition.sqrMagnitude < fromToPosition.sqrMagnitude ?
                edge.To.Position :
                edge.From.Position;
        }
        else if (node != null)
        {
            return node.Position;
        }
        Debug.LogWarning("Position has no edge or node: " + position);
        return Vector2.zero;
    }

    private void updateUnitBackup(IPoliceUnitAIData unit, PoliceAIContext stateMachine)
    {
        //for now
        updateUnitChase(unit, stateMachine);
    }

    private void updateUnitChase(IPoliceUnitAIData unit, PoliceAIContext stateMachine)
    {
        unit.TargetPosition = stateMachine.SuspectRealPosition.WorldPosition;

        // Compute planar distance
        Vector2 unitPos = unit.CityPosition.WorldPosition;
        Vector2 suspectPos = stateMachine.SuspectRealPosition.WorldPosition;
        float distance = Vector2.Distance(unitPos, suspectPos);
        var movement = unit.Movement;

        // Reset any previous frame's cap
        movement.MaxSpeedOverride = -1f;

        // If too close to suspect, stop to avoid overshooting and losing sight
        if (distance <= stateMachine.StopDistance)
        {
            movement.TempoState = TempoState.Hold;
            return;
        }

        // Otherwise, chase 
        movement.TempoState = TempoState.Fast;
    }

    private void updateLastSeenPosition(PoliceAIContext stateMachine, CityPosition suspectPosition)
    {
        // Update the last seen edge based on the suspect's position
        // if suspect is on a node, equivalently set to null
        stateMachine.SuspectLastSeenPosition = suspectPosition;
    }

    private void resetLastSeenPosition(PoliceAIContext stateMachine)
    {
        stateMachine.SuspectLastSeenPosition = null;
    }

    public TempoState OnEdge(PoliceAIContext stateMachine, IPoliceUnitAIData unit, RoadEdge edge)
    {
        // Extension point: adjust tempo based on edge characteristics, system state, personality, etc.
        
        
        return unit.Movement.TempoState;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateMachine"></param>
    /// <param name="unit"></param>
    /// <param name="turnNode"></param>
    /// <param name="lastEdge"></param>
    /// <returns> preferred edge to go further, null to stay on the node</returns>
    public RoadEdge PickTurn(PoliceAIContext stateMachine, IPoliceUnitAIData unit,
        RoadNode turnNode, RoadEdge lastEdge)
    {
        Debug.Assert(stateMachine != null);
        Debug.Assert(unit != null);
        Debug.Assert(turnNode != null);
        //TODO change depending on personality?
        if (unit.TargetPosition != null)
        {
            return pickEdgeCloserToTarget(turnNode, unit.TargetPosition.Value, lastEdge);
        }
        else
        {
            return pickRandomEdge(turnNode, lastEdge);
        }
    }

    private RoadEdge pickRandomEdge(RoadNode turnNode, RoadEdge lastEdge)
    {
        List<RoadEdge> possibleEdges = new List<RoadEdge>();
        foreach (var edge in turnNode.Outgoing)
        {
            if (edge != lastEdge)
            {
                possibleEdges.Add(edge);
            }
        }
        if (possibleEdges.Count == 0)
        {
            // only option is to go back
            return lastEdge;
        }
        int index = Random.Range(0, possibleEdges.Count);
        return possibleEdges[index];
    }

    private RoadEdge pickEdgeCloserToTarget(RoadNode turnNode, Vector2 targetPosition, RoadEdge lastEdge)
    {
        List<(RoadEdge, float)> edgeDistances = new List<(RoadEdge, float)>();
        float lastEdgeDistance = float.MaxValue;
        foreach (var edge in turnNode.Outgoing)
        {
            var otherNode = edge.To == turnNode ? edge.From : edge.To;
            float distance = Vector2.Distance(otherNode.Position, targetPosition);
            if (lastEdge == edge)
            {
                // skipping last edge, since we don't want to go back unless it's the only option
                lastEdgeDistance = distance;
            }
            else
                edgeDistances.Add((edge, distance));
        }
        if (edgeDistances.Count == 0)
        {
            // No outgoing edges found; fallback
            return lastEdge;
        }

        edgeDistances.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        if (lastEdge != null)
        {
            // only add the last edge as the last option
            edgeDistances.Add((lastEdge, lastEdgeDistance));
        }
        // return the edge that gets closer to the suspect
        return edgeDistances[0].Item1;
    }
}


public class PoliceAIContext
{
    public IPoliceUnitAIData[] PoliceUnits { get; }
    public CityPosition SuspectRealPosition { get; set; }
    public IMovement SuspectRealMovement { get; set; }
    public float CloseInDistance { get; set; } = 20f;
    public float StopDistance { get; set; }
    public CityPosition? SuspectLastSeenPosition { get; set; }
    public PoliceAISystemState State { get; set; }

    public PoliceAIContext(IPoliceUnitAIData[] policeUnits)
    {
        PoliceUnits = policeUnits ?? Array.Empty<IPoliceUnitAIData>();
        State = PoliceAISystemState.Seek;
    }
}

public enum PoliceAISystemState
{
    Seek,
    Chase
}

public interface IPoliceUnitAIData
{
    CityPosition CityPosition { get; }
    Vector2? TargetPosition { get; set; }
    IAIMovement Movement { get; }
    IVision Vision { get; }
    PoliceUnitState State { get; set; }
    PersonalityTag Personality { get; }
}



public interface IAIMovement : IMovement, ISpeedCap
{
    bool TowardsTarget { get; set; }
    TempoState TempoState { get; set; }
}

public interface IVision
{
    public bool CanSeeTarget(CityPosition target);
}

// Turning happens instantly so no need to model it??
public enum TempoState
{
    Hold,
    Slow,
    Medium,
    Fast
}

public enum PersonalityTag
{
    Blinky,
    Pinky,
    Inky,
    Clyde
}

public enum PoliceUnitState
{
    /// <summary>
    /// No threat detected, patrolling normally
    /// </summary>
    Calm,
    /// <summary>
    /// Suspect is in sight, actively pursuing
    /// </summary>
    Chase,
    /// <summary>
    /// Last known position of suspect, trying to relocate
    /// </summary>
    TryRelocate,
    /// <summary>
    /// Closing in to the probable area, when too far away
    /// </summary>
    ClosingIn,
    /// <summary>
    /// Suspect is in sight of another unit, moving to provide support
    /// </summary>
    Backup,
    /// <summary>
    /// Avoid the suspect by spreading out
    /// </summary>
    Scatter
}