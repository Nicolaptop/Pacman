using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float TimeBeforeStart;

    [SerializeField]
    private Board _board = default;

    [SerializeField]
    private PacMan _pacman = default;

    private float _levelStartTime;
    private bool _levelStarted = false;
    private Tile _pacmanTile;

    private void Awake()
    {
        _levelStartTime = Time.time;
    }

    private void Update()
    {
        if (!_levelStarted && Time.time - _levelStartTime < TimeBeforeStart) return;
        if (!_levelStarted) _levelStarted = true;

        _pacman.Move();

        PositionLogic();
    }

    private void PositionLogic()
    {
        _pacmanTile = _board.GetTile(_pacman.transform.position);
        Debug.LogWarning("PacmanTile !", _pacmanTile);
        if (_pacmanTile.TileType == TileType.Portal && _pacman.IsCurrentNodeReached) _pacman.Teleport(_pacmanTile.ConnectedPortal);
    }
}
