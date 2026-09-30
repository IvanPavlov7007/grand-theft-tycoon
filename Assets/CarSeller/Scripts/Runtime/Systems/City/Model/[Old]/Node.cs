using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public List<Node> connectedNeighbors{ get; private set; } = new List<Node>();

    public Vector2 InitialPosition { get; private set; }
    public Vector2 CurrentPosition { get; private set; }

    public Node(Vector2 InitialPosition)
    {
        this.InitialPosition = InitialPosition;
        CurrentPosition = InitialPosition;
    }

    public void ConnectWithNode(Node another)
    {
        connectedNeighbors.Add(another);
        another.connectedNeighbors.Add(this);
    }

    public (Node, Vector2)[] GetNeighborsDirection()
    {
        (Node, Vector2)[] neighborsDirection = new (Node, Vector2)[connectedNeighbors.Count];
        foreach (Node node in connectedNeighbors)
        {
            neighborsDirection[connectedNeighbors.IndexOf(node)] = (node, (node.CurrentPosition - CurrentPosition).normalized);
        }
        return neighborsDirection;
    }

    public Node PickClosestNeighbourDirection(Vector2 direction, out Vector2 closestDirection)
    {
        closestDirection = Vector2.negativeInfinity;
        Node closestNode = null;
        float maxDot = -2f;

        foreach (var pair in GetNeighborsDirection())
        {
            float dot = Vector2.Dot(direction, pair.Item2);
            if (dot > maxDot)
            {
                maxDot = dot;
                closestNode = pair.Item1;
                closestDirection = pair.Item2;
            }
        }
        return closestNode;
    }

    // Old wandering code

    //public float maxDestinationRaidius = 0.5f,maxNextDestinationTime = 3f;
    //Vector2 curDestination, lastDestination;
    //float destinationTime = 0f, t = 0f;
    //void Update()
    //{
    //    if(t>= destinationTime)
    //    {
    //        t = 0f;
    //        lastDestination = transform.position;
    //        curDestination = Random.insideUnitCircle * maxDestinationRaidius + InitialPosition;
    //        destinationTime = Random.Range(0, maxNextDestinationTime);
    //    }
    //    t += Time.deltaTime;
    //    transform.position = Vector2.Lerp(lastDestination, curDestination, t / maxNextDestinationTime);
    //}
}
