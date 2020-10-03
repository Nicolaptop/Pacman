using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    private static int _boardWidth = 28, _boardLength = 31;
    private static Vector2[] _expansionVectors = new Vector2[] { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

    public Tile[] Tiles;
    public int DotCount;

    private Tile[,] _board = new Tile[_boardWidth, _boardLength];
    private List<Tile> _nodes;
    private List<Tile> _portals;

    private void Awake()
    {
        _board = new Tile[_boardWidth, _boardLength];
        _nodes = new List<Tile>();
        _portals = new List<Tile>();
        foreach (Tile tile in Tiles)
        {
            _board[(int)tile.transform.position.x, (int)tile.transform.position.y] = tile;
            if (tile.TileType == TileType.Node) _nodes.Add(tile);
            if (tile.TileType == TileType.Portal)
            {
                _nodes.Add(tile);
                _portals.Add(tile);
            }
            if (tile.Collectable != null && tile.Collectable.Type != CollectableType.Fruit) DotCount++;
        }
        print("board initialized");
    }

    public Tile GetTile(Vector2 position)
    {
        return _board[Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y)];
    }


    public Tile GetClosestAttainableTile(Vector2 position)
    {
        int closestXPosition = Mathf.Max(0, Mathf.Min(Mathf.FloorToInt(position.x), _boardWidth - 1));
        int closestYPosition = Mathf.Max(0, Mathf.Min(Mathf.FloorToInt(position.y), _boardLength - 1));
        Tile closestTile = _board[closestXPosition, closestYPosition];

        if (closestTile.TileType > 0) return closestTile;

        List<Tile> newTiles = new List<Tile> { closestTile };
        List<Tile> visitiedTiles = new List<Tile>();

        Tile currentTile;
        List<Tile> currentTileNeighbours;
        while (newTiles.Count != 0)
        {
            currentTile = newTiles[0];
            if (currentTile.TileType > 0) return currentTile;
            newTiles.RemoveAt(0);
            visitiedTiles.Add(currentTile);

            currentTileNeighbours = GetNeighbourTiles((int)currentTile.transform.position.x, (int)currentTile.transform.position.y);
            foreach (Tile currentTileNeighbour in currentTileNeighbours)
            {
                if (newTiles.Contains(currentTileNeighbour) || visitiedTiles.Contains(currentTileNeighbour)) continue;
                newTiles.Add(currentTileNeighbour);
            }

        }
        return null;
    }

    private List<Tile> GetNeighbourTiles(int X, int Y)
    {
        List<Tile> list = new List<Tile>();
        if (X > 0) list.Add(_board[X - 1, Y]);
        if (X < _boardWidth - 1) list.Add(_board[X + 1, Y]);
        if (Y > 0) list.Add(_board[X, Y - 1]);
        if (Y < _boardLength - 1) list.Add(_board[X, Y + 1]);
        return list;
    }

    //ASTAR functions

    public List<Tile> GetPath(Tile startingTile, Tile endingTile, Tile currentNode = null, Tile previousNode = null)
    {
        foreach (Tile node in _nodes) { node.ResetAstarValues(); }
        startingTile.ResetAstarValues();
        endingTile.ResetAstarValues();

        //If the starting or ending tile are not nodes, we need to make them known from the Node system. This seems a bit far-fetched, but it allows to not check every adjacent tile
        List<Tile> nodesAdjacentToStartingTile = new List<Tile>();
        List<Tile> nodesAdjacentToEndingTile = new List<Tile>();

        if ((int)startingTile.TileType < 3)
        {
            if (currentNode == null)
            {
                nodesAdjacentToStartingTile = GetNodesSurronding(startingTile);
                startingTile.NeighbourTiles.AddRange(nodesAdjacentToStartingTile);
            }
            else
            {
                nodesAdjacentToStartingTile = new List<Tile> { currentNode, previousNode };
                startingTile.NeighbourTiles.Add(currentNode); // Forces the ghost to keep on the current path by setting the current node as the only startingTile's neighbour
            }
        }

        if ((int)endingTile.TileType < 3)
        {
            nodesAdjacentToEndingTile = GetNodesSurronding(endingTile);
            foreach (Tile node in nodesAdjacentToEndingTile)
            {
                if (currentNode == null || node != currentNode || GameManager.GetNormalizedVector(previousNode, currentNode) != GameManager.GetNormalizedVector(endingTile, currentNode)) //prevents from going back
                    node.NeighbourTiles.Add(endingTile);
            }
        }

        List<Tile> newNodes = new List<Tile> { startingTile };
        List<Tile> visitedNodes = new List<Tile>();

        startingTile.GCost = 0;
        startingTile.HCost = CalculateHCost(startingTile, endingTile);
        startingTile.ReCalculateFCost();

        while (newNodes.Count > 0)
        {
            Tile currentNodeInInspection = GetLowestFCostNode(newNodes);
            if (currentNodeInInspection == endingTile) // final Tile reached 
            {
                var astarPath = CalculatePath(endingTile);
                // we want to return a path based on the nodes, not on the tiles in between to populate _previousNode and _currentNode to determine the direction to take
                if ((int)startingTile.TileType < 3)
                {
                    astarPath.RemoveAt(0);
                    astarPath.Insert(0, nodesAdjacentToStartingTile.Find(tile => tile != astarPath[0])); // the first Tile will be the previous node
                }
                if ((int)endingTile.TileType < 3)
                {
                    astarPath.RemoveAt(astarPath.Count - 1);
                    astarPath.Add(nodesAdjacentToEndingTile.Find(tile => tile != astarPath[astarPath.Count - 1])); // the last Tile will overshot the aimed tile
                }
                return astarPath;
            }

            newNodes.Remove(currentNodeInInspection);
            visitedNodes.Add(currentNodeInInspection);

            foreach (Tile neighbourNode in currentNodeInInspection.AstarGetNeighbours(currentNode, previousNode))
            {
                if (visitedNodes.Contains(neighbourNode)) continue;

                int gCost = currentNodeInInspection.GCost + CalculateDistance(currentNodeInInspection, neighbourNode);
                if (gCost < neighbourNode.GCost)
                {
                    neighbourNode.ParentTile = currentNodeInInspection;
                    neighbourNode.GCost = gCost;
                    neighbourNode.HCost = CalculateHCost(neighbourNode, endingTile);
                    neighbourNode.ReCalculateFCost();

                    if (!newNodes.Contains(neighbourNode)) newNodes.Add(neighbourNode);
                }
            }
        }

        //no more new nodes
        return null;
    }

    public List<Tile> GetNodesSurronding(Tile tile)
    {
        Vector2 boardPosition = tile.transform.position;
        List<Tile> _nodesSurronding = new List<Tile>();
        Vector2 tmpPosition;
        Tile tmpTile;

        for (int i = 0; i < _expansionVectors.Length; i++)
        {
            tmpPosition = boardPosition + _expansionVectors[i];
            tmpTile = GetTile(tmpPosition);
            if (tmpTile.TileType != TileType.Obstacle)
            {
                int j = 1;
                while ((int)tmpTile.TileType < 3)
                {
                    tmpPosition += _expansionVectors[i];
                    tmpTile = GetTile(tmpPosition);
                    j++;
                }
                _nodesSurronding.Add(tmpTile);
                if (_nodesSurronding.Count > 2) break; // by definition of the nodes, only 2 nodes are accessible from a basic tile
            }
        }
        return _nodesSurronding;
    }

    public int CalculateDistance(Tile a, Tile b)
    {
        Vector2 distAB = b.transform.position - a.transform.position;
        return (int)Mathf.Abs(distAB.x) + (int)Mathf.Abs(distAB.y);
    }

    private int CalculateHCost(Tile a, Tile b) // This check of the portal heuristic is top make sure going through a portal that seems farther isnt in fact faster
    {
        int minDistance = CalculateDistance(a, b);
        foreach (Tile portal in _portals)
        {
            minDistance = Mathf.Min(minDistance, (CalculateDistance(a, portal) + CalculateDistance(portal.ConnectedPortal, b))); // to determine the heuristic of a portal, we get the heuristic of its "brother"
        }
        return minDistance;
    }

    private Tile GetLowestFCostNode(List<Tile> nodeList)
    {
        Tile lowestFCostNode = nodeList[0];
        for (int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i].FCost < lowestFCostNode.FCost) lowestFCostNode = nodeList[i];
        }

        return lowestFCostNode;
    }

    private List<Tile> CalculatePath(Tile endNode)
    {
        List<Tile> path = new List<Tile> { endNode };
        Tile currentNode = endNode;
        while (currentNode.ParentTile != null)
        {
            currentNode = currentNode.ParentTile;
            path.Add(currentNode);
        }
        path.Reverse();
        return path;
    }
}
