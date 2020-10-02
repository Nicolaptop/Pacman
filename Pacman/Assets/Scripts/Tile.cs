using UnityEngine;

public enum TileType
{
    Obstacle = 0,
    Path = 1,
    TunnelPath = 2,
    Node = 3,
    GhostNode = 4,
    Portal = 5,
}

public class Tile : MonoBehaviour
{
    public TileType TileType;
    public Tile[] NeighboorNodes;
    public Tile[] PMNeighboorNodes; // Special for some nodes where the ghost cant go up 
    public Tile ConnectedPortal;
    public Collectable collectable;

    private Vector2[] _validDirections;
    private Vector2[] _pmValidDirections;

    private void Awake()
    {
        if (TileType == TileType.Node || TileType == TileType.GhostNode || TileType == TileType.Portal)
        {
            _validDirections = new Vector2[NeighboorNodes.Length];
            for (int i = 0; i < NeighboorNodes.Length; i++)
            {
                Vector2 direction = NeighboorNodes[i].transform.position - transform.position;
                _validDirections[i] = direction.normalized;
            }
        }

        if (TileType == TileType.Node || TileType == TileType.Portal)
        {
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
        for (int i = 0; i< _validDirections.Length; i++)
        {
            if (_validDirections[i] == direction) return NeighboorNodes[i];
        }
        for (int i = 0; i < _pmValidDirections.Length; i++)
        {
            if (_pmValidDirections[i] == direction) return PMNeighboorNodes[i];
        }
        return null;
    }
}
