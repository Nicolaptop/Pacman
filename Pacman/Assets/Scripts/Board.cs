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
            _board[(int)tile.transform.position.x, (int)tile.transform.position.y] = tile;
        }
        print("board initialized");
    }

    public Tile GetTile(Vector2 position)
    {
        return _board[Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y)];
    }
}
