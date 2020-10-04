using UnityEngine;

public class Pinky : Ghost
{
    private Tile _pacmanTile;
    private Vector2 _pacmanDirection;

    public override void Initialize()
    {
        base.Initialize();
        _previousNode = _leavingTiles[0];
        _currentNode = _leavingTiles[1];
        _currentleavingIndex = 1;
        isLeavingHouse = true;

        DetermineDirection();
    }

    protected override Tile DetermineNextTarget(object[] args)
    {
        if (args.Length != 2 || !(args[0] is Tile) || !(args[1] is Vector2)) Debug.LogError("Error in arguments given", this);
        _pacmanTile = args[0] as Tile;
        _pacmanDirection = (Vector2)args[1];

        return _board.GetClosestAttainableTile((Vector2)_pacmanTile.transform.position + 4 * _pacmanDirection);
    }
}
