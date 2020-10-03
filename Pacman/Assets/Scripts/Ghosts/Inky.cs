using UnityEngine;

public class Inky : Ghost
{
    private Tile _pacmanTile;
    private Vector2 _pacmanDirection;
    private Tile _blinkyTile;

    private void Start()
    {
        _previousNode = _leavingTiles[0];
        _currentNode = _leavingTiles[1];
        _currentleavingIndex = 1;

        DetermineDirection();
    }

    protected override Tile DetermineNextTarget(object[] args)
    {
        if (args.Length != 3 || !(args[0] is Tile) || !(args[1] is Vector2) || !(args[2] is Tile)) Debug.LogError("Error in arguments given", this);
        _pacmanTile = args[0] as Tile;
        _pacmanDirection = (Vector2)args[1];
        _blinkyTile = args[2] as Tile;
        Vector2 pivot = (Vector2)_pacmanTile.transform.position + 2 * _pacmanDirection;
        Vector2 blinky_pivot_vector = pivot - (Vector2)_blinkyTile.transform.position;

        return _board.GetClosestAttainableTile(2 * blinky_pivot_vector + (Vector2)_blinkyTile.transform.position);
    }
}
