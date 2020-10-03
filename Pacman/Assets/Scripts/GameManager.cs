using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float TimeBeforeStart;
    public GhostState CurrentGhostState;

    [SerializeField]
    private Board _board = default;

    [SerializeField]
    private PacMan _pacman = default;

    [SerializeField]
    private Ghost[] _ghosts = default;

    private float _levelStartTime;
    private bool _levelStarted = false;
    private Tile _pacmanTile;
    private Tile _blinkyTile;
    private Tile _clydeTile;
    private Tile[] _ghostTiles;

    private void Awake()
    {
        _levelStartTime = Time.time;
        _ghostTiles = new Tile[_ghosts.Length];
        for (int i = 0; i < _ghosts.Length; i++)
        {
            _ghostTiles[i] = _board.GetTile(_ghosts[i].transform.position);
        }
    }

    private void Update()
    {
        if (!_levelStarted && Time.time - _levelStartTime < TimeBeforeStart) return;
        if (!_levelStarted) _levelStarted = true;

        _pacman.Move();
        for (int i = 0; i < _ghosts.Length; i++)
        {
            _ghosts[i].Move(_ghostTiles[i]);
        }

        PositionLogic();
        GhostNextTile();

        if (Input.GetKeyDown(KeyCode.S)) foreach (Ghost ghost in _ghosts) { ghost.SetGhostState(GhostState.Scatter); }
        if (Input.GetKeyDown(KeyCode.C)) foreach (Ghost ghost in _ghosts) { ghost.SetGhostState(GhostState.Chase); }
        if (Input.GetKeyDown(KeyCode.F)) foreach (Ghost ghost in _ghosts) { ghost.SetGhostState(GhostState.Frightened); }
        if (Input.GetKeyDown(KeyCode.R)) foreach (Ghost ghost in _ghosts) { ghost.SetGhostState(GhostState.Respawning); }
    }

    private void PositionLogic()
    {
        _pacmanTile = _board.GetTile(_pacman.transform.position);
        if (_pacmanTile.TileType == TileType.Portal && _pacman.IsCurrentNodeReached)
        {
            _pacman.Teleport(_pacmanTile.ConnectedPortal);
            _pacmanTile = _pacmanTile.ConnectedPortal;
        }
        if (_pacmanTile.collectable != null && _pacmanTile.collectable.gameObject.activeSelf)
        {
            // Collect Collectable
        }
        for (int i = 0; i < _ghosts.Length; i++)
        {
            _ghostTiles[i] = _board.GetTile(_ghosts[i].transform.position);
            if (_ghosts[i] is Blinky) _blinkyTile = _ghostTiles[i];
            if (_ghosts[i] is Clyde) _clydeTile = _ghostTiles[i];
            if (_ghostTiles[i].TileType == TileType.Portal && _ghosts[i].NeedsTeleportation)
            {
                _ghosts[i].Teleport(_ghostTiles[i].ConnectedPortal);
                _ghostTiles[i] = _ghostTiles[i].ConnectedPortal;
            }
            if (_ghostTiles[i] == _pacmanTile && _ghosts[i].GhostState != GhostState.Respawning) // trouble Ahead !
            {
                if (_ghosts[i].GhostState != GhostState.Frightened)
                {
                    // Eat Ghost
                }
                else
                {
                    // Lost a life
                }
            }
        }
    }

    private void GhostNextTile()
    {
        for (int i = 0; i < _ghosts.Length; i++)
        {
            if (_ghosts[i].NeedsNewPath)
            {
                if (_ghosts[i] is Blinky)
                {
                    object[] args = new object[] { _pacmanTile };
                    _ghosts[i].CalculateNewPath(args);
                }
                if (_ghosts[i] is Pinky)
                {
                    object[] args = new object[] { _pacmanTile, _pacman.CurrentDirection };
                    _ghosts[i].CalculateNewPath(args);
                }
                if (_ghosts[i] is Inky)
                {
                    object[] args = new object[] { _pacmanTile, _pacman.CurrentDirection, _blinkyTile };
                    _ghosts[i].CalculateNewPath(args);
                }
                if (_ghosts[i] is Clyde)
                {
                    object[] args = new object[] { _pacmanTile, _clydeTile };
                    _ghosts[i].CalculateNewPath(args);
                }
            }
        }
    }

    public static Vector2 GetNormalizedVector(Tile a, Tile b)
    {
        return (a.transform.position - b.transform.position).normalized;
    }
}
