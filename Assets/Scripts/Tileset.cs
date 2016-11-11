using UnityEngine;
using System.Collections;

[System.Serializable]
public class Tileset : ScriptableObject
{
    public GameObject[] groundTiles;
    public Material groundMaterial;
    public GameObject[] borderTiles;
    public GameObject[] props;
    public GameObject[] enemyGroups;
    public GameObject[] goals;

    public GameObject emptyTileCollider;
}
