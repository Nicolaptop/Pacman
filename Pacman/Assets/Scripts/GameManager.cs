using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public float TimeBeforeStart;
    public float TimeBeforeRestart;
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
    private float _levelRestartTime;
    private bool _levelStarted = false;
    private bool _levelRestarting = false;
    private bool _gameOver = false;
    private Tile _pacmanTile;
    private Tile _blinkyTile;
    private Tile _clydeTile;
    private Tile[] _ghostTiles;

    private int _dotsLefts;
    private int _score;
    private int _livesleft = 2;
    private int _ghostchain = 0;

    private static string _LEVEL_START = "Ready ?";
    private static string _LEVEL_COMPLETE = "Level Complete !";
    private static string _LEVEL_RESTART = "Lost a life ! Restarting...";
    private static string _GAME_OVER = "Game Over";

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

    //Bonus Logic
    [SerializeField]
    private int[] _dotsBonusTrigger;
    [SerializeField]
    private float _bonusDuration;
    private float _timesinceLastBonus;
    private int _currentBonusIndex;

    [SerializeField]
    private Button _restartButton;

    private void Start()
    {
        _ghostTiles = new Tile[_ghosts.Length];
        _restartButton.onClick.AddListener(NewGame);
        NewGame();
    }

    private void NewGame()
    {
        _restartButton.gameObject.SetActive(false);
        _gameOver = false;
        _score = 0;
        _scoreValue.text = _score.ToString();
        _livesleft = 2;
        _livesLeftValue.text = _livesleft.ToString();
        NewLevel();
    }

    private void NewLevel()
    {
        _board.ResetBoard();
        _pacman.Initialize();

        for (int i = 0; i < _ghosts.Length; i++)
        {
            _ghosts[i].Initialize();
            _ghostTiles[i] = _board.GetTile(_ghosts[i].transform.position);
        }

        _permaChase = false;
        _ongoingFright = false;
        _currentAlternance = 0;
        _dotsLefts = _board.DotCount;
        CurrentGhostState = GhostState.Scatter;

        _textMessage.gameObject.SetActive(true);
        _textMessage.text = _LEVEL_START;
        _levelStartTime = Time.time;
        _levelStarted = false;

        _currentBonusIndex = 0;
        _timesinceLastBonus = 0;
        _board.BonusTile.Collectable.gameObject.SetActive(false);
    }

    private void RestartLevel()
    {
        _pacman.Initialize();

        for (int i = 0; i < _ghosts.Length; i++)
        {
            _ghosts[i].Initialize();
            _ghostTiles[i] = _board.GetTile(_ghosts[i].transform.position);
        }

        _permaChase = false;
        _ongoingFright = false;
        _currentAlternance = 0;
        CurrentGhostState = GhostState.Scatter;

        _textMessage.gameObject.SetActive(true);
        _textMessage.text = _LEVEL_START;
        _levelStartTime = Time.time;
        _levelStarted = false;

        _timesinceLastBonus = 0;
        _board.BonusTile.Collectable.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_gameOver) return;
        if (_levelRestarting && Time.time - _levelRestartTime < TimeBeforeRestart) return;
        if (_levelRestarting)
        {
            if (_dotsLefts == 0)
            {
                NewLevel();
            }
            else
            {
                RestartLevel();
            }
            _levelRestarting = false;
            return;
        }

        if (!_levelStarted && Time.time - _levelStartTime < TimeBeforeStart) return;
        if (!_levelStarted)
        {
            _textMessage.gameObject.SetActive(false);
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
            switch (_pacmanTile.Collectable.Type)
            {
                case CollectableType.Dot:
                    _score += 10;
                    _dotsLefts--;

                    if (_dotsLefts == 0)
                    {
                        _textMessage.gameObject.SetActive(true);
                        _textMessage.text = _LEVEL_COMPLETE;
                        _levelRestarting = true;
                        _levelRestartTime = Time.time;
                        return;
                    }

                    foreach (Ghost ghost in _ghosts)
                    {
                        if (!ghost.IsEnabled && _board.DotCount - _dotsLefts >= ghost.DotsBeforeRelease) ghost.IsEnabled = true;
                    }

                    if (_currentBonusIndex < _dotsBonusTrigger.Length && _board.DotCount - _dotsLefts == _dotsBonusTrigger[_currentBonusIndex])
                    {
                        _board.BonusTile.Collectable.gameObject.SetActive(true);
                        _timesinceLastBonus = Time.time;
                        _currentBonusIndex++;
                    }
                    break;

                case CollectableType.Energizer:
                    _score += 50;
                    _dotsLefts--;

                    if (_dotsLefts == 0)
                    {
                        _textMessage.gameObject.SetActive(true);
                        _textMessage.text = _LEVEL_COMPLETE;
                        _levelRestarting = true;
                        _levelRestartTime = Time.time;
                        return;
                    }

                    foreach (Ghost ghost in _ghosts)
                    {
                        if (!ghost.IsEnabled && _board.DotCount - _dotsLefts >= ghost.DotsBeforeRelease) ghost.IsEnabled = true;
                        ghost.SetGhostState(GhostState.Frightened);
                    }

                    if (_currentBonusIndex < _dotsBonusTrigger.Length && _board.DotCount - _dotsLefts == _dotsBonusTrigger[_currentBonusIndex])
                    {
                        _board.BonusTile.Collectable.gameObject.SetActive(true);
                        _timesinceLastBonus = Time.time;
                        _currentBonusIndex++;
                    }

                    _timeSinceLastFright = Time.time;
                    _timeSinceLastAlternance += _frightDuration;
                    _ongoingFright = true;
                    _ghostchain = 0;
                    break;

                case CollectableType.Bonus:
                    _score += 100;
                    _board.BonusTile.Collectable.gameObject.SetActive(false);
                    _timesinceLastBonus = 0f;
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
                        _textMessage.text = _GAME_OVER;

                        _gameOver = true;
                        _restartButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        _livesleft--;
                        _textMessage.gameObject.SetActive(true);
                        _textMessage.text = _LEVEL_RESTART;
                        _livesLeftValue.text = _livesleft.ToString();

                        _levelRestarting = true;
                        _levelRestartTime = Time.time;
                    }
                }
            }
        }
    }

    private void TimeLogic()
    {
        if (_ongoingFright && Time.time - _timeSinceLastFright >= _frightDuration)
        {
            foreach (Ghost ghost in _ghosts)
            {
                ghost.SetGhostState(CurrentGhostState, true);
            }
            _ongoingFright = false;
        }

        if (_timesinceLastBonus > 0 && Time.time - _timesinceLastBonus > _bonusDuration)
        {
            _board.BonusTile.Collectable.gameObject.SetActive(false);
            _timesinceLastBonus = 0;
        }

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
