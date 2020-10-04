using UnityEngine;

public class PacMan : MonoBehaviour
{
    public float BaseSpeed;
    public bool IsCurrentNodeReached;
    public Vector2 CurrentDirection;

    [SerializeField]
    private Tile _previousNode; // last node visited
    [SerializeField]
    private Tile _currentNode; // current node aimed

    private Tile _nextNode;
    private Vector2 _nextDirection;

    //Restart variables
    private Vector3 _spawnPosition;
    private Vector2 _spawnDirection;
    private Tile _spawnPreviousNode;
    private Tile _spawnCurrentNode;

    private void Awake()
    {
        _spawnPosition = transform.position;
        _spawnDirection = CurrentDirection;
        _spawnPreviousNode = _previousNode;
        _spawnCurrentNode = _currentNode;
    }

    public void Initialize()
    {
        transform.position = _spawnPosition;
        CurrentDirection = _spawnDirection;
        _previousNode = _spawnPreviousNode;
        _currentNode = _spawnCurrentNode;

        _nextNode = null;
        _nextDirection = Vector2.zero;

        IsCurrentNodeReached = false;
    }

    public void Move()
    {
        CheckForNewDirection();
        if (IsCurrentNodeReached && _nextNode == null) return;
        if (IsCurrentNodeReached && _nextNode != null)
        {
            _previousNode = _currentNode;
            _currentNode = _nextNode;
            CurrentDirection = _nextDirection;
            _nextDirection = Vector2.zero;
            _nextNode = null;
            IsCurrentNodeReached = false;
        }
        transform.position += (Vector3)(CurrentDirection * BaseSpeed) * Time.deltaTime;
        if (CheckOverShot()) // Pacman overshot his target Node
        {
            IsCurrentNodeReached = true;
            if (_nextDirection == Vector2.zero) //No new direction has been specified, checking if pacman can continue on the same direction
            {
                Tile nextNode = _currentNode.CheckForValidDirection(CurrentDirection);
                if (nextNode != null) // there is a path, allows the overshot
                {
                    _nextNode = nextNode;
                    _nextDirection = CurrentDirection;
                }
                else transform.position = _currentNode.transform.position; // there is no path, aligns pacman
            }
            else if (_nextDirection != CurrentDirection) transform.position = _currentNode.transform.position;
        }
    }

    private void CheckForNewDirection()
    {
        Vector2 wishedDirection = Vector2.zero;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) wishedDirection = Vector2.left;
        if (Input.GetKeyDown(KeyCode.RightArrow)) wishedDirection = Vector2.right;
        if (Input.GetKeyDown(KeyCode.UpArrow)) wishedDirection = Vector2.up;
        if (Input.GetKeyDown(KeyCode.DownArrow)) wishedDirection = Vector2.down;
        if (wishedDirection != Vector2.zero && wishedDirection != CurrentDirection)
        {
            if (wishedDirection == -1 * CurrentDirection) //opposite direction
            {
                Tile tmpTile = _previousNode;
                _previousNode = _currentNode;
                _currentNode = tmpTile;
                CurrentDirection = wishedDirection;
                IsCurrentNodeReached = false;
                _nextDirection = Vector2.zero;
                _nextNode = null;
                return;
            }
            Tile nextNode = _currentNode.CheckForValidDirection(wishedDirection);
            if (nextNode != null)
            {
                _nextNode = nextNode;
                _nextDirection = wishedDirection;
            }
        }
    }

    private bool CheckOverShot()
    {
        Vector2 distance_pacman_currentnode = transform.position - _currentNode.transform.position;
        return distance_pacman_currentnode.normalized == CurrentDirection;
    }

    public void Teleport(Tile tile)
    {
        transform.position = tile.transform.position;
        _previousNode = tile;
        _currentNode = tile.NeighboorNodes[0];
        IsCurrentNodeReached = false;
    }
}
