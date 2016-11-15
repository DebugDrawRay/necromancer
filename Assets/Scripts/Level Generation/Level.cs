using UnityEngine;
using System.Collections;

[System.Serializable]
public class Level : ScriptableObject
{
    public int goalCount;
    public Vector2 goalDistanceRange;
    public Tileset tileset;
    public Vector2 enemyGroupsRange;
    public Vector2 propsRange;
    public enum LevelSizes
    {
        Small,
        Medium,
        Large
    }
    public LevelSizes levelSize;

    private const int smallSize = 250;
    private const int mediumSize = 500;
    private const int largeSize = 750;

    public int tileCount
    {
        get
        {
            switch(levelSize)
            {
                case LevelSizes.Small:
                    return smallSize;
                case LevelSizes.Medium:
                    return mediumSize;
                case LevelSizes.Large:
                    return largeSize;
                default:
                    return smallSize;
            }
        }
    }
}
