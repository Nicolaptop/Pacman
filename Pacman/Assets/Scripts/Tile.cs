using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Obstacle = 0,
    Path = 1,
    TunnelPath = 2,
    Node = 3,
    Portal = 4,
}

public class Tile : MonoBehaviour
{
    public TileType TileType;
    public Tile[] NeighboorNodes;
    public Tile[] PMNeighboorNodes; // Special for some nodes where the ghost cant go up 
    public Tile ConnectedPortal;
    public Collectable Collectable;

    private Vector2[] _validDirections;
    private Vector2[] _pmValidDirections;

    // A* Values
    //[HideInInspector]
    public int GCost;
    //[HideInInspector]
    public int HCost;
    //[HideInInspector]
    public int FCost;
    //[HideInInspector]
    public List<Tile> NeighbourTiles; // Used for the start Tile and the end Tile, because we only record nodes for other game purpose 
    //[HideInInspector]
    public Tile ParentTile;

    private void Awake()
    {
        if ((int)TileType > 2)
        {
            _validDirections = new Vector2[NeighboorNodes.Length];
            for (int i = 0; i < NeighboorNodes.Length; i++)
            {
                Vector2 direction = NeighboorNodes[i].transform.position - transform.position;
                _validDirections[i] = direction.normalized;
            }

            _pmValidDirections = new Vector2[PMNeighboorNodes.Length];
            for (int i = 0; i < PMNeighboorNodes.Length; i++)
            {
                Vector2 direction = PMNeighboorNodes[i].transform.position - transform.position;
                _pmValidDirections[i] = direction.normalized;
            }
        }
    }

    public Tile CheckForValidDirection(Vector2 direction)
    {
        for (int i = 0; i < _validDirections.Length; i++)
        {
            if (_validDirections[i] == direction) return NeighboorNodes[i];
        }
        for (int i = 0; i < _pmValidDirections.Length; i++)
        {
            if (_pmValidDirections[i] == direction) return PMNeighboorNodes[i];
        }
        return null;
    }

    public void ReCalculateFCost() { FCost = GCost + HCost; }

    public void ResetAstarValues()
    {
        GCost = int.MaxValue;
        HCost = 0;
        FCost = int.MaxValue;
        NeighbourTiles.Clear();
        ParentTile = null;
    }

    public List<Tile> AstarGetNeighbours(Tile incomingNode = null, Tile previousNode = null, bool noInfinitePortal = false)
    {
        List<Tile> neighbours = NeighbourTiles;
        foreach (Tile neighbourNode in NeighboorNodes)
        {
            if (incomingNode == null || this != incomingNode || neighbourNode != previousNode) // prevents the Ghost from going back
                neighbours.Add(neighbourNode);
        }
        if (TileType == TileType.Portal && !noInfinitePortal)
            neighbours.AddRange(ConnectedPortal.AstarGetNeighbours(incomingNode, previousNode, true));
        return neighbours;
    }
}
