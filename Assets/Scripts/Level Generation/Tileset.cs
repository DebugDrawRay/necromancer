using UnityEngine;
using System.Collections;

[System.Serializable]
public class Tileset : ScriptableObject
{
    [SerializeField]
    public Node tileNode;
    [SerializeField]
    public Node borderNode;
    public GameObject groundTile;
    public Material groundMaterial;
    public GameObject borderTile;
    public Material borderMaterial;
    public GameObject[] props;
    public GameObject[] enemyGroups;
    public GameObject[] goals;

    public GameObject emptyTileCollider;
}
