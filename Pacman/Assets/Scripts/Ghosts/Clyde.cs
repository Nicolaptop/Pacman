using UnityEngine;

public class Clyde : Ghost
{
    private Tile _pacmanTile;
    private Tile _clydeTile;

    public override void Initialize()
    {
        base.Initialize();
        _previousNode = _leavingTiles[0];
        _currentNode = _leavingTiles[1];
        _currentleavingIndex = 1;

        DetermineDirection();
    }

    protected override Tile DetermineNextTarget(object[] args)
    {
        if (args.Length != 2 || !(args[0] is Tile) || !(args[1] is Tile)) Debug.LogError("Error in arguments given", this);
        _pacmanTile = args[0] as Tile;
        _clydeTile = args[1] as Tile;
        if (_board.CalculateDistance(_pacmanTile, _clydeTile) > 8) return _pacmanTile;

        if (_clydeTile == _currentScatterNode)
        {
            _currentScatterNodeIndex = (_currentScatterNodeIndex + 1) % _scatterPath.Length;
            _currentScatterNode = _scatterPath[_currentScatterNodeIndex];
        }
        return _currentScatterNode;
    }
}