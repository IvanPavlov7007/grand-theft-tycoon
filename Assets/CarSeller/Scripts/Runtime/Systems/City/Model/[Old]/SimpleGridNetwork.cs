using System.Collections.Generic;
using UnityEngine;

public interface INodesNetwork
{
    Node GetClosestNode(Vector2 worldPosition);
    IList<Node> Nodes { get; }
}

public class SimpleGridNetwork : INodesNetwork
{
    /// <summary>
    /// World position of the left up corner of the grid
    /// </summary>
    public Vector2 LeftUpCorner;
    public int NodesCountX { get; private set; }
    public int NodesCountY { get; private set; }
    public float WorldGridSize { get; private set; }

    public List<Node> Nodes { get; private set; }

    IList<Node> INodesNetwork.Nodes => Nodes;

    public SimpleGridNetwork(Vector2 leftUpWorld, int nodesCountX, int nodesCountY, float nodesWorldStep)
    {
        LeftUpCorner = leftUpWorld;
        NodesCountX = nodesCountX;
        NodesCountY = nodesCountY;
        WorldGridSize = nodesWorldStep;
        Nodes = new List<Node>();
        initialize();
    }

    void initialize()
    {
        int curNindex = 0;

        for (int i = 0; i < NodesCountY; i++)
        {
            for (int j = 0; j < NodesCountX; j++)
            {
                Vector2 position = LeftUpCorner + new Vector2(j, i) * WorldGridSize;
                Node node = new Node(position);

                Nodes.Add(node);
                if (j > 0)
                {
                    node.ConnectWithNode(Nodes[curNindex - 1]);
                }
                if (i > 0)
                {
                    node.ConnectWithNode(Nodes[curNindex - NodesCountX]);
                }
                curNindex++;
            }
        }
    }

    public Node GetClosestNode(Vector2 worldPosition)
    {
        if (Nodes == null || Nodes.Count == 0)
            return null;

        // Translate world position into grid space relative to LeftUpCorner
        Vector2 local = worldPosition - LeftUpCorner;

        // Compute floating indices
        float fx = local.x / WorldGridSize;
        float fy = local.y / WorldGridSize;

        // Round to nearest integer indices to get closest node
        int ix = Mathf.RoundToInt(fx);
        int iy = Mathf.RoundToInt(fy);

        // Clamp to grid bounds
        ix = Mathf.Clamp(ix, 0, NodesCountX - 1);
        iy = Mathf.Clamp(iy, 0, NodesCountY - 1);

        int flatIndex = iy * NodesCountX + ix;
        return Nodes[flatIndex];
    }
}
