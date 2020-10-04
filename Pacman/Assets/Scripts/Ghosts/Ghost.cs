using System.Collections.Generic;
using System;
using UnityEngine;

public enum GhostState
{
    Scatter = 0,
    Chase = 1,
    Frightened = 2,
    Respawning = 3,
}

public abstract class Ghost : MonoBehaviour
{
    public float BaseSpeed;

    [SerializeField]
    protected Color _baseColor;
    [SerializeField]
    protected Color _frightColor;
    [SerializeField]
    protected Color _deadColor;
    [SerializeField]
    private SpriteRenderer _ghostRenderer;
    [SerializeField]
    private GameManager _gameManager;
    [SerializeField]
    protected Board _board;

    public bool isEnabled;
    [SerializeField]
    protected bool isLeavingHouse;
    [SerializeField]
    public GhostState GhostState { get; protected set; } = GhostState.Scatter;

    [SerializeField]
    protected Tile[] _leavingTiles; //leaving ghost home path
    protected int _currentleavingIndex;
    [SerializeField]
    protected Tile[] _scatterPath;
    protected Tile _currentScatterNode; //The scatter node the ghost is aiming in scatter state
    protected int _currentScatterNodeIndex;

    public int DotsBeforeRelease;

    protected Tile _previousNode;
    protected Tile _currentNode;
    protected Vector2 _currentDirection;
    public bool NeedsTeleportation;
    public bool NeedsNewPath;  // means that the ghost needs a new direction from the pathfinding algorithm
    private bool _noPreviousPath;

    private Vector3 _spawnPosition;

    public virtual void Initialize() //Called at the start and restart of every level
    {
        _currentScatterNodeIndex = 0;
        _currentleavingIndex = 0;
        _currentScatterNode = _scatterPath[_currentScatterNodeIndex];
        GhostState = GhostState.Scatter;
        _previousNode = null;
        _currentNode = null;
        _currentDirection = Vector2.zero;
        NeedsTeleportation = false;
        NeedsNewPath = false;
        _noPreviousPath = false;
        transform.position = _spawnPosition;
    }
    private void Awake()
    {
        _spawnPosition = transform.position;
        _currentScatterNode = _scatterPath[_currentScatterNodeIndex];
    }

    public void Move(Tile ghostTile)
    {
        if (!isEnabled) return;
        if (GhostState == GhostState.Respawning && ghostTile == _leavingTiles[_leavingTiles.Length - 1] && _previousNode != ghostTile) // Check for returning to ghosthome
        {
            _currentNode = ghostTile;
            _currentleavingIndex = _leavingTiles.Length - 1;
        }
        transform.position += (Vector3)(_currentDirection * BaseSpeed) * Time.deltaTime;
        if (CheckOverShot()) // Ghost overshot its target Node
        {
            transform.position = _currentNode.transform.position;

            if (isLeavingHouse)
            {
                if (_currentleavingIndex == _leavingTiles.Length - 1)
                {
                    isLeavingHouse = false;
                    NeedsNewPath = true;
                    _noPreviousPath = true;
                    return;
                }
                else
                {
                    _previousNode = _currentNode;
                    _currentleavingIndex++;
                    _currentNode = _leavingTiles[_currentleavingIndex];
                    DetermineDirection();
                    return;
                }

            }

            switch (GhostState)
            {
                case GhostState.Scatter:  
                    if (_currentNode == _currentScatterNode) // Ghost is in a patrol movement
                    {
                        _previousNode = _currentNode;
                        _currentScatterNodeIndex = (_currentScatterNodeIndex + 1) % _scatterPath.Length;
                        _currentScatterNode = _scatterPath[_currentScatterNodeIndex];
                        _currentNode = _currentScatterNode;
                    }
                    else // Ghost returns to his patrol path
                    {
                        if (_currentNode.TileType == TileType.Portal) NeedsTeleportation = true;
                        else if (_currentNode.NeighboorNodes.Length > 2) NeedsNewPath = true; // case multiple possible paths
                        else // 2 neighbors and the fact a ghost cant go back only gives one path possibility
                        {
                            if (_currentNode.NeighboorNodes[0] == _previousNode) // prevents from going back
                            {
                                _previousNode = _currentNode;
                                _currentNode = _currentNode.NeighboorNodes[1];
                                DetermineDirection();
                            }
                            else if (_currentNode.NeighboorNodes[1] == _previousNode)
                            {
                                _previousNode = _currentNode;
                                _currentNode = _currentNode.NeighboorNodes[0];
                                DetermineDirection();
                            }
                            else NeedsNewPath = true;
                        }
                    }
                    DetermineDirection();
                    break;

                case GhostState.Chase:
                    if (_currentNode.TileType == TileType.Portal) NeedsTeleportation = true;
                    else if (_currentNode.NeighboorNodes.Length > 2) NeedsNewPath = true; // case multiple possible paths
                    else // 2 neighbors and the fact a ghost cant go back only gives one path possibility  // In fact they are some 3ways node that alows the ghost to only walk one way
                    {
                        if (_currentNode.NeighboorNodes[0] == _previousNode) // prevents from going back
                        {
                            _previousNode = _currentNode;
                            _currentNode = _currentNode.NeighboorNodes[1];
                            DetermineDirection();
                        }
                        else if (_currentNode.NeighboorNodes[1] == _previousNode)
                        {
                            _previousNode = _currentNode;
                            _currentNode = _currentNode.NeighboorNodes[0];
                            DetermineDirection();
                        }
                        else NeedsNewPath = true;
                    }
                    break;

                case GhostState.Frightened:
                    if (_currentNode.TileType == TileType.Portal) NeedsTeleportation = true;
                    else if (_currentNode.NeighboorNodes.Length == 2)
                    {
                        if (_currentNode.NeighboorNodes[0] == _previousNode) // prevents from going back
                        {
                            _previousNode = _currentNode;
                            _currentNode = _currentNode.NeighboorNodes[1];
                            DetermineDirection();
                        }
                        else if (_currentNode.NeighboorNodes[1] == _previousNode)
                        {
                            _previousNode = _currentNode;
                            _currentNode = _currentNode.NeighboorNodes[0];
                            DetermineDirection();
                        }
                        else NeedsNewPath = true;
                    }
                    else
                    {
                        Tile[] possiblePaths = Array.FindAll(_currentNode.NeighboorNodes, node => node != _previousNode);
                        int randomPick = UnityEngine.Random.Range(0, possiblePaths.Length);
                        _previousNode = _currentNode;
                        _currentNode = possiblePaths[randomPick];
                        DetermineDirection();
                    }
                    break;

                case GhostState.Respawning:
                    if (_currentNode == _leavingTiles[0])
                    {
                        Respawn();
                    }
                    else if (_currentNode == _leavingTiles[_currentleavingIndex])
                    {
                        _currentleavingIndex--;
                        _previousNode = _currentNode;
                        _currentNode = _leavingTiles[_currentleavingIndex];
                        DetermineDirection();
                    }
                    else
                    {
                        if (_currentNode.TileType == TileType.Portal) NeedsTeleportation = true;
                        else if (_currentNode.NeighboorNodes.Length > 2) NeedsNewPath = true; // case multiple possible paths
                        else // 2 neighbors and the fact a ghost cant go back only gives one path possibility
                        {
                            if (_currentNode.NeighboorNodes[0] == _previousNode) // prevents from going back
                            {
                                _previousNode = _currentNode;
                                _currentNode = _currentNode.NeighboorNodes[1];
                            }
                            else
                            {
                                _previousNode = _currentNode;
                                _currentNode = _currentNode.NeighboorNodes[0];
                            }

                            DetermineDirection();
                        }
                    }
                    break;
            }
        }
    }

    private bool CheckOverShot()
    {
        Vector2 distance_ghost_currentnode = transform.position - _currentNode.transform.position;
        return distance_ghost_currentnode.normalized == _currentDirection;
    }

    protected void DetermineDirection()
    {
        _currentDirection = GameManager.GetNormalizedVector(_currentNode, _previousNode);
    }

    public void Teleport(Tile tile)
    {
        transform.position = tile.transform.position;
        _previousNode = tile;
        _currentNode = tile.NeighboorNodes[0];
        NeedsTeleportation = false;
    }

    public void CalculateNewPath(object[] args = null)
    {
        Tile startingTile = _board.GetTile(transform.position);
        Tile aimedTile;
        List<Tile> tmpPath;
        switch (GhostState)
        {
            case GhostState.Scatter:
                aimedTile = _scatterPath[_currentScatterNodeIndex];

                if (aimedTile == startingTile)
                {
                    _currentScatterNodeIndex = (_currentScatterNodeIndex + 1) % _scatterPath.Length;
                    _previousNode = aimedTile;
                    _currentNode = _scatterPath[_currentScatterNodeIndex];
                    DetermineDirection();
                    break;
                }

                tmpPath = _board.GetPath(startingTile, aimedTile, currentNode: _noPreviousPath ? null : _currentNode, previousNode: _noPreviousPath ? null : _previousNode);

                _previousNode = tmpPath[0];
                tmpPath.RemoveAt(0);
                _currentNode = tmpPath[0];

                DetermineDirection();
                break;

            case GhostState.Chase:
                aimedTile = DetermineNextTarget(args);
                if (aimedTile == startingTile) aimedTile = _previousNode;
                tmpPath = _board.GetPath(startingTile, aimedTile, currentNode: _noPreviousPath ? null : _currentNode, previousNode: _noPreviousPath ? null : _previousNode);

                _previousNode = tmpPath[0];
                tmpPath.RemoveAt(0);
                _currentNode = tmpPath[0];

                DetermineDirection();
                break;

            case GhostState.Respawning:
                aimedTile = _leavingTiles[_leavingTiles.Length - 1];

                if (startingTile == aimedTile)
                {
                    _previousNode = aimedTile;
                    _currentleavingIndex = _leavingTiles.Length - 2;
                    _currentNode = _leavingTiles[_currentleavingIndex];
                    DetermineDirection();
                    break;
                }

                tmpPath = _board.GetPath(startingTile, aimedTile, currentNode: _noPreviousPath ? null : _currentNode, previousNode: _noPreviousPath ? null : _previousNode);

                _previousNode = tmpPath[0];
                tmpPath.RemoveAt(0);
                _currentNode = tmpPath[0];

                DetermineDirection();
                break;

            case GhostState.Frightened:
                var possibleNodes = _board.GetNodesSurronding(startingTile);
                int randomNumber = UnityEngine.Random.Range(0, possibleNodes.Count);
                _previousNode = possibleNodes[randomNumber];
                _currentNode = possibleNodes[1 - randomNumber];
                DetermineDirection();
                break;
        }
        _noPreviousPath = false;
        NeedsNewPath = false;
    }

    protected abstract Tile DetermineNextTarget(object[] args);

    private void Respawn()
    {
        GhostState = _gameManager.CurrentGhostState;
        _previousNode = _leavingTiles[0];
        _currentNode = _leavingTiles[1];
        _currentleavingIndex = 1;
        isLeavingHouse = true;
        DetermineDirection();
        SetGhostStateVisual();
    }

    public void SetGhostState(GhostState state, bool endOfFright = false)
    {
        if ((int)state == (int)GhostState) return;
        switch (GhostState)
        {
            case GhostState.Scatter:
                GhostState = state;
                ReverseDirection();
                SetGhostStateVisual();
                break;

            case GhostState.Chase:
                GhostState = state;
                ReverseDirection();
                SetGhostStateVisual();
                break;

            case GhostState.Frightened:
                if (state != GhostState.Respawning && !endOfFright) return;
                GhostState = state;
                SetGhostStateVisual();
                break;
        }
        NeedsNewPath = false;
        NeedsTeleportation = false;
    }

    private void ReverseDirection()
    {
        if (isEnabled && !isLeavingHouse) // Goes in reverse direction if not in the house or leaving it
        {
            Tile tmpNode = _previousNode;
            _previousNode = _currentNode;
            _currentNode = tmpNode;
            DetermineDirection();
        }
    }

    private void SetGhostStateVisual()
    {
        switch (GhostState)
        {
            case GhostState.Frightened:
                _ghostRenderer.color = _frightColor;
                break;

            case GhostState.Respawning:
                _ghostRenderer.color = _deadColor;
                break;

            default:
                _ghostRenderer.color = _baseColor;
                break;
        }
    }
}
