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
    public int tileCount;
}
