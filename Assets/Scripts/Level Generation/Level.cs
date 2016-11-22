using UnityEngine;
using Utilities;

[System.Serializable]
public class Level : ScriptableObject
{
    public int goalCount;
    public Vector2 goalDistanceRange;
    public Tileset tileset;
    public Vector2 enemyGroupsRange;
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

    private Vector2 smallPropRange = new Vector2(100, 150);
    private Vector2 mediumPropRange = new Vector2(200, 350);
    private Vector2 largePropRange = new Vector2(400, 600);

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

    public int propCount
    {
        get
        {
            switch (levelSize)
            {
                case LevelSizes.Small:
                    return Roll.RangeInt(smallPropRange);
                case LevelSizes.Medium:
                    return Roll.RangeInt(mediumPropRange);
                case LevelSizes.Large:
                    return Roll.RangeInt(largePropRange);
                default:
                    return Roll.RangeInt(smallPropRange);
            }
        }
    }
}
