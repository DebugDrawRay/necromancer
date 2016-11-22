using UnityEngine;
using System;

[System.Serializable]
public class Node : ICloneable
{
    public enum Type
    {
        Tile,
        Border
    }
    public Type nodeType;

    [HideInInspector]
    public Vector2 position;
    [HideInInspector]
    public bool visited;
    public Vector3[] enemyGroups;
    public Vector3[] propGroups;

    public object Clone()
    {
        return MemberwiseClone();
    }
    public Node CloneNode()
    {
        return (Node)Clone();
    }
}
