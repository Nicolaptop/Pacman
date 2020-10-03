using UnityEngine;

public class Blinky : Ghost
{
    private void Start()
    {
        CalculateNewPath();
    }

    protected override Tile DetermineNextTarget(object[] args)
    {
        if (args.Length != 1 || !(args[0] is Tile)) Debug.LogError("Error in arguments given", this);
        return args[0] as Tile;
    }
}
