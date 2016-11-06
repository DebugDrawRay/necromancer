using UnityEngine;
using System.Collections;

[System.Serializable]
public class Level : ScriptableObject
{
    public int goalCount;
    public Tileset tileset;
    public int enemyGroups;
    public int levelSize;
}
