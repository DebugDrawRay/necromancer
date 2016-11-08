using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    public Transform[] connectors;
    public Transform[] propPlacements;
    public Transform[] enemyPlacements;

    [HideInInspector]
    public Tile parent;

}
