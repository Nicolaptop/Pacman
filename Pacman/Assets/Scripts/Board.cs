using UnityEngine;

public class Board : MonoBehaviour
{
    private static int BoardWidth = 28, BoardLength = 31;
    public Tile[] Tiles;
    private Tile[,] _board = new Tile[BoardWidth, BoardLength];

    private void Awake()
    {
        foreach(Tile tile in Tiles)
        {
            _board[(int)tile.transform.localPosition.x, (int)tile.transform.localPosition.y] = tile;
        }
        print("board initialized");
    }
}
