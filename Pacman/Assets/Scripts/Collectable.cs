using UnityEngine;

public enum CollectableType
{
    Dot = 0,
    Energizer = 1,
    Fruit = 2,
}

public class Collectable : MonoBehaviour
{
    public CollectableType Type;
}
