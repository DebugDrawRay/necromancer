using UnityEngine;
using System.Collections;

[System.Serializable]
public class Tileset : ScriptableObject
{
    public GameObject[] groundTiles;
    public GameObject[] borderTiles;
    public GameObject[] props;
    public GameObject[] enemyGroups;
    public GameObject[] goals;
}
