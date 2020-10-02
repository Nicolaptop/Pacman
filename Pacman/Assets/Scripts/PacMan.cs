using UnityEngine;

public class PacMan : MonoBehaviour
{
    public float BaseSpeed;
    public bool IsCurrentNodeReached;

    [SerializeField]
    private Tile _previousNode;
    [SerializeField]
    private Tile _currentNode;
    private Tile _nextNode;

    [SerializeField]
    private Vector2 _currentDirection;
    private Vector2 _nextDirection;

    private void Awake()
    {
        
    }

    public void Move()
    {
        CheckForNewDirection();
        if (IsCurrentNodeReached && _nextNode == null) return;
        if (IsCurrentNodeReached && _nextNode != null)
        {
            _previousNode = _currentNode;
            _currentNode = _nextNode;
            _currentDirection = _nextDirection;
            _nextDirection = Vector2.zero;
            _nextNode = null;
            IsCurrentNodeReached = false;
        }
        transform.position += (Vector3)(_currentDirection * BaseSpeed) * Time.deltaTime;
        if (CheckOverShot()) // Pacman overshot his target Node
        {
            IsCurrentNodeReached = true;
            if (_nextDirection == Vector2.zero) //No new direction has been specified, checking if pacman can continue on the same direction
            {
                Tile nextNode = _currentNode.CheckForValidDirection(_currentDirection);
                if (nextNode != null) // there is a path, allows the overshot
                {
                    _nextNode = nextNode;
                    _nextDirection = _currentDirection;
                }
                else transform.position = _currentNode.transform.position; // there is no path, aligns pacman
            }
            else if (_nextDirection != _currentDirection) transform.position = _currentNode.transform.position;
        }
    }

    private void CheckForNewDirection()
    {
        Vector2 wishedDirection = Vector2.zero;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) wishedDirection = Vector2.left;
        if (Input.GetKeyDown(KeyCode.RightArrow)) wishedDirection = Vector2.right;
        if (Input.GetKeyDown(KeyCode.UpArrow)) wishedDirection = Vector2.up;
        if (Input.GetKeyDown(KeyCode.DownArrow)) wishedDirection = Vector2.down;
        if (wishedDirection != Vector2.zero && wishedDirection != _currentDirection)
        {
            if (wishedDirection == -1 * _currentDirection)
            {
                Tile tmpTile = _previousNode;
                _previousNode = _currentNode;
                _currentNode = tmpTile;
                _currentDirection = wishedDirection;
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
        return distance_pacman_currentnode.normalized == _currentDirection;
    }

    public void Teleport(Tile tile)
    {
        transform.position = tile.transform.position;
        _previousNode = tile;
        _currentNode = tile.NeighboorNodes[0];
        IsCurrentNodeReached = false;
    }
}
