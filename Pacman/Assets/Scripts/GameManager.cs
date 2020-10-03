using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField]
    private Text _scoreValue = default;
    [SerializeField]
    private Text _livesLeftValue = default;
    [SerializeField]
    private Text _textMessage = default;

    private float _levelStartTime;
    private bool _levelStarted = false;
    private Tile _pacmanTile;
    private Tile _blinkyTile;
    private Tile _clydeTile;
    private Tile[] _ghostTiles;

    private int _dotsLefts;
    private int _score;
    private int _livesleft = 2;
    private int _ghostchain = 0;

    private static string LEVEL_START = "Ready ?";
    private static string LEVEL_COMPLETE = "Level Complete !";
    private static string LEVEL_RESTART = "Lost a life ! Restarting...";
    private static string GAME_OVER = "Game Over";

    //Ghost State Change Logic
    [SerializeField]
    private int _frightDuration;
    [SerializeField]
    private float[] _timeBetweenAlternance;
    private int _currentAlternance;
    private float _timeSinceLastAlternance;
    private float _timeSinceLastFright;
    private bool _permaChase;
    private bool _ongoingFright;


    private void Awake()
    {
        _score = 0;
        _scoreValue.text = _score.ToString();
        _livesLeftValue.text = _livesleft.ToString();
        _textMessage.gameObject.SetActive(true);
        _textMessage.text = LEVEL_START;

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
        if (!_levelStarted)
        {
            _textMessage.gameObject.SetActive(false);
            _dotsLefts = _board.DotCount;
            _levelStarted = true;
            _currentAlternance = 0;
            _timeSinceLastAlternance = Time.time;
            _permaChase = false;
            CurrentGhostState = GhostState.Scatter;
        }

        _pacman.Move();
        for (int i = 0; i < _ghosts.Length; i++)
        {
            _ghosts[i].Move(_ghostTiles[i]);
        }

        PositionLogic();
        TimeLogic();
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
        if (_pacmanTile.Collectable != null && _pacmanTile.Collectable.gameObject.activeSelf)
        {
            switch(_pacmanTile.Collectable.Type)
            {
                case CollectableType.Dot:
                    _score += 10;
                    _dotsLefts--;
                    foreach(Ghost ghost in _ghosts)
                    {
                        if (!ghost.isEnabled && _board.DotCount - _dotsLefts >= ghost.DotsBeforeRelease) ghost.isEnabled = true;
                    }
                    if (_dotsLefts == 0)
                    {
                        _textMessage.gameObject.SetActive(true);
                        _textMessage.text = LEVEL_COMPLETE;
                        //Restarts the level
                    }
                    break;
                case CollectableType.Energizer:
                    _score += 50;
                    _dotsLefts--;
                    foreach (Ghost ghost in _ghosts)
                    {
                        if (!ghost.isEnabled && _board.DotCount - _dotsLefts >= ghost.DotsBeforeRelease) ghost.isEnabled = true;
                        ghost.SetGhostState(GhostState.Frightened);
                    }
                    _timeSinceLastFright = Time.time;
                    _timeSinceLastAlternance += _frightDuration;
                    _ghostchain = 0;
                    if (_dotsLefts == 0)
                    {
                        _textMessage.gameObject.SetActive(true);
                        _textMessage.text = LEVEL_COMPLETE;
                        //Restarts the level
                    }
                    break;
                case CollectableType.Fruit:
                    _score += 100;
                    break;
            }

            _scoreValue.text = _score.ToString();
            _pacmanTile.Collectable.gameObject.SetActive(false);
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
            if (_ghostTiles[i] == _pacmanTile && _ghosts[i].GhostState != GhostState.Respawning)
            {
                if (_ghosts[i].GhostState == GhostState.Frightened)
                {
                    _ghosts[i].SetGhostState(GhostState.Respawning);
                    _ghostchain++;
                    _score += (int)Mathf.Pow(2, _ghostchain) * 100;
                    _scoreValue.text = _score.ToString();
                }
                else
                {
                    if (_livesleft == 0)
                    {
                        _textMessage.gameObject.SetActive(true);
                        _textMessage.text = GAME_OVER;
                        //GameOver
                    }
                    else
                    {
                        _livesleft -= 1;
                        _textMessage.gameObject.SetActive(true);
                        _textMessage.text = LEVEL_RESTART;
                        _livesLeftValue.text = _livesleft.ToString();
    //RestartLevel
}
                }
            }
        }
    }

    private void TimeLogic()
    {
        if (_permaChase) return;
        if (Time.time - _timeSinceLastAlternance >= _timeBetweenAlternance[_currentAlternance])
        {
            if (CurrentGhostState == GhostState.Chase)
            {
                foreach (Ghost ghost in _ghosts)
                {
                    ghost.SetGhostState(GhostState.Scatter);
                }
                CurrentGhostState = GhostState.Scatter;
            }
            else
            {
                foreach (Ghost ghost in _ghosts)
                {
                    ghost.SetGhostState(GhostState.Chase);
                }
                CurrentGhostState = GhostState.Chase;
            }
            if (++_currentAlternance == _timeBetweenAlternance.Length) _permaChase = true;
            _timeSinceLastAlternance = Time.time;
        }

        if (_ongoingFright && Time.time - _timeSinceLastFright >= _frightDuration)
        {
            foreach (Ghost ghost in _ghosts)
            {
                ghost.SetGhostState(CurrentGhostState, true);
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
