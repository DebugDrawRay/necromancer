using UnityEngine;
using System.Collections.Generic;
using System;
using Utilities;

public class LevelGenerator : MonoBehaviour
{
    public Level levelToLoad;

    [Header("Generation Settings")]
    public bool generateTiles = true;
    public bool generateProps = true;
    public bool generateBorders = true;
    public bool processLevel = true;
    [Header("Tile Makers")]
    [Tooltip("Each tile maker will place one tile at a selected connector")]
    public int startingTileMakers;
    [Tooltip("Adjust the max amount of tile makers to be spawned; set this to the same value as 'tileMakers' to remove additional tile makers spawns")]
    public int maxTileMakers;
    [Tooltip("Adjust the chance of a tile maker being spawned")]
    [Range(0f, 1f)]
    public float tileMakerSpawnChance;
    public class TileMaker
    {
        public Vector2 currentPosition;
    }
    private List<TileMaker> tileMakers = new List<TileMaker>();
    private int tileCount;

    private GameObject loadedLevel;

    [Header("Level Processing")]
    public Vector3 levelRotation;

    private Node wallHead;
    private Node tileHead;

    private Node[,] nodes;
    private Vector2[] traversalDirections = { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };
    void Start()
    {
        GenerateNewLevel(levelToLoad);
        if(AstarPath.active)
        {
            AstarPath.active.Scan();
        }
    }

    [ContextMenu("Generate New Level")]
    public void GenerateMenu()
    {
        GenerateNewLevel(levelToLoad);
    }

    public void GenerateNewLevel(Level load)
    {
        if(loadedLevel != null)
        {
            Destroy(loadedLevel);
        }

        tileMakers = new List<TileMaker>();
        tileCount = 0;
        
        if (generateTiles)
        {
            nodes = GenerateTiles(traversalDirections, load.tileCount, load.tileset.tileNode);
            if (generateBorders)
            {
                nodes = GenerateBorders(nodes, load.tileset.borderNode, traversalDirections);
            }
            if (generateProps)
            {

            }
            if(processLevel)
            {
                loadedLevel = new GameObject("Level");
                ProcessTiles(nodes, load.tileset).transform.SetParent(loadedLevel.transform);
                ProcessBorders(nodes, traversalDirections, load.tileset).transform.SetParent(loadedLevel.transform);
                loadedLevel.transform.rotation = Quaternion.Euler(levelRotation);
            }
        }

    }

    Node[,] GenerateTiles(Vector2[] directions, int maxTiles, Node nodeToPlace)
    {
        //Create a new node matrix
        Node[,] newNodes = new Node[maxTiles * 2, maxTiles * 2];
        //If there are no makers, create up to the starting amount
        if (tileMakers.Count <= 0)
        {
            for (int i = 0; i < startingTileMakers; i++)
            {
                TileMaker maker = new TileMaker();
                maker.currentPosition = new Vector2(maxTiles, maxTiles);
                tileMakers.Add(maker);
            }
        }
        //Initialize the current tiles
        int currentTiles = 0;

        //create tiles
        while (currentTiles < maxTiles)
        {
            //for each maker
            for(int i = 0; i < tileMakers.Count; i++)
            {
                //if tiles have reached max, break out
                if(currentTiles >= maxTiles)
                {
                    break;
                }
                //select the tile makers and clone a node at it's position
                TileMaker maker = tileMakers[i];
                newNodes[(int)maker.currentPosition.x, (int)maker.currentPosition.y] = nodeToPlace.CloneNode();
                newNodes[(int)maker.currentPosition.x, (int)maker.currentPosition.y].position = maker.currentPosition;

                //check for unoccupied positions
                List<Vector2> freePositions = new List<Vector2>();
                for (int j = 0; j < directions.Length; j++)
                {
                    Vector2 check = maker.currentPosition + directions[j];
                    if (newNodes[(int)check.x, (int)check.y] == null)
                    {
                        freePositions.Add(directions[j]);
                    }
                }
                //if there are free positions, use them, else randomly select a position
                if (freePositions.Count > 0)
                {
                    maker.currentPosition += freePositions[UnityEngine.Random.Range(0, freePositions.Count)];
                    currentTiles++;
                }
                else
                {
                    Vector2 dir = directions[UnityEngine.Random.Range(0, directions.Length)];
                    maker.currentPosition += dir;
                }
                //increment tiles

                //Check if a new maker gets generated
                if (UnityEngine.Random.value <= tileMakerSpawnChance && tileMakers.Count < maxTileMakers)
                {
                    Debug.Log("New Maker");
                    TileMaker newMaker = new TileMaker();
                    newMaker.currentPosition = maker.currentPosition;
                    tileMakers.Add(maker);
                }
            }
        }
        return newNodes;
    }

    Node[,] GenerateBorders(Node[,] nodes, Node nodeToPlace, Vector2[] directions)
    {
        for(int x = 0; x < nodes.GetLength(0); x++)
        {
            for(int y = 0; y < nodes.GetLength(0); y++)
            {
                Node select = nodes[x, y];
                if(select != null && select.nodeType == Node.Type.Tile)
                {
                    foreach(Vector2 dir in directions)
                    {
                        Vector2 checkDir = new Vector2(x + dir.x, y + dir.y);
                        if(checkDir.x < nodes.GetLength(0) && checkDir.y < nodes.GetLength(0) && nodes[(int)checkDir.x, (int)checkDir.y] == null)
                        {
                            nodes[(int)checkDir.x, (int)checkDir.y] = nodeToPlace.CloneNode();
                            nodes[(int)checkDir.x, (int)checkDir.y].position = checkDir;
                        }
                    }
                }
            }
        }
        return nodes;
    }

    GameObject ProcessBorders(Node[,] nodes, Vector2[] directions, Tileset set)
    {
        GameObject borders = new GameObject("Borders");
        int size = nodes.GetLength(0) / 2;
        foreach (Node node in nodes)
        {
            if(node != null && !node.visited && node.nodeType == Node.Type.Border)
            {
                List<Node> group = FindAdjacent(node, new List<Node>(), nodes, directions, Node.Type.Border);
                GameObject newGroup = new GameObject(group.Count.ToString());
                foreach (Node result in group)
                {
                    GameObject newObj = (GameObject)Instantiate(set.borderTile, newGroup.transform);
                    newObj.transform.position = new Vector3((result.position.x - size) * newObj.transform.localScale.x, 0, (result.position.y - size) * newObj.transform.localScale.z);
                }
                GameObject newBorder = ProcessMesh(newGroup.transform, "Border", set.borderMaterial);
                newBorder.transform.SetParent(borders.transform);
                newBorder.layer = set.borderTile.layer;
                Destroy(newGroup);
            }
        }
        return borders;
    }

    List<Node> FindAdjacent(Node head, List<Node> found, Node[,] nodes, Vector2[] directions, Node.Type type)
    {
        if (!head.visited)
        {
            head.visited = true;
            found.Add(head);
            foreach (Vector2 dir in directions)
            {
                Node child = nodes[(int)(head.position.x + dir.x), (int)(head.position.y + dir.y)];
                if (child != null && !child.visited && child.nodeType == type)
                {
                    FindAdjacent(child, found, nodes, directions, type);
                }
            }
        }
        return found;
    }

    GameObject ProcessTiles(Node[,] nodes, Tileset set)
    {
        GameObject container = new GameObject();
        int size = nodes.GetLength(0) / 2;
        for (int x = 0; x < nodes.GetLength(0); x++)
        {
            for (int y = 0; y < nodes.GetLength(0); y++)
            {
                Node node = nodes[x, y];
                if (node != null && node.nodeType == Node.Type.Tile)
                {
                    GameObject newObj = (GameObject)Instantiate(set.groundTile, container.transform);
                    newObj.transform.position = new Vector3((x - size) * newObj.transform.localScale.x, 0, (y - size) * newObj.transform.localScale.y);
                }
            }
        }
        GameObject tiles = ProcessMesh(container.transform, "Tiles", set.groundMaterial);
        Destroy(container);
        return tiles;
    }

    GameObject ProcessMesh(Transform objectToCombine, string name, Material combineMaterial)
    {
        //Create container
        GameObject container = new GameObject(name);
        container.isStatic = true;
        container.AddComponent<MeshFilter>();
        container.AddComponent<MeshRenderer>();

        //Create lists for materials, meshes and combine instances
        List<MeshFilter> meshesToCombine = new List<MeshFilter>();
        foreach(Transform obj in objectToCombine)
        {
            meshesToCombine.Add(obj.GetComponent<MeshFilter>());
        }
        List<Material> materials = new List<Material>();
        List<List<CombineInstance>> combineInstanceArrays = new List<List<CombineInstance>>();

        //Get materials from the renderer of the filter
        foreach (MeshFilter meshFilter in meshesToCombine)
        {
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            //Iterate through all meshes and get materials
            for (int s = 0; s < meshFilter.sharedMesh.subMeshCount; s++)
            {
                int materialArrayIndex = 0;
                for (materialArrayIndex = 0; materialArrayIndex < materials.Count; materialArrayIndex++)
                {
                    //if material was already added, skip it
                    if (materials[materialArrayIndex] == meshRenderer.sharedMaterials[s])
                    {
                        break;
                    }
                }

                //if the material wasn't found in the array, add it to the material list and add a new combine instance list for it
                if (materialArrayIndex == materials.Count)
                {
                    materials.Add(meshRenderer.sharedMaterials[s]);
                    combineInstanceArrays.Add(new List<CombineInstance>());
                }
                //Create a new combine instance and set its properties
                CombineInstance combineInstance = new CombineInstance();
                combineInstance.transform = meshRenderer.transform.localToWorldMatrix;
                //set index to the same as the submesh;
                combineInstance.subMeshIndex = s;
                //add the entire mesh
                combineInstance.mesh = meshFilter.sharedMesh;
                //add the combine instance to the correct list corresponding to the material
                //if the material corresponding to the submesh already exists in the array, this will add the submesh to the list corresponding to the material
                combineInstanceArrays[materialArrayIndex].Add(combineInstance);
            }
        }

        //Reference the container's mesh filter
        MeshFilter meshFilterCombine = container.GetComponent<MeshFilter>();

        //Create array of meshes by material count, and create array of combine instances with the same param
        Mesh[] meshes = new Mesh[materials.Count];
        CombineInstance[] combineInstances = new CombineInstance[materials.Count];

        //For each material, get the list of corresponding submeshes and convert to array
        for (int m = 0; m < materials.Count; m++)
        {
            CombineInstance[] combineInstanceArray = combineInstanceArrays[m].ToArray();
            //Create new mesh and combine submeshes
            meshes[m] = new Mesh();
            meshes[m].CombineMeshes(combineInstanceArray, true, true);

            //add the new mesh to the combine instances and set its submeshindex
            combineInstances[m] = new CombineInstance();
            combineInstances[m].mesh = meshes[m];
            combineInstances[m].subMeshIndex = 0;
        }

        // Combine all submeshes
        meshFilterCombine.sharedMesh = new Mesh();
        meshFilterCombine.sharedMesh.CombineMeshes(combineInstances, false, false);
        meshFilterCombine.sharedMesh.Optimize();
        //Convert materials list to array and add to renderer
        //Material[] materialsArray = materials.ToArray();
        //level.GetComponent<MeshRenderer>().materials = materialsArray;

        //Assign material
        container.GetComponent<MeshRenderer>().material = combineMaterial;

        container.AddComponent<MeshCollider>();
        container.GetComponent<MeshCollider>().sharedMesh = container.GetComponent<MeshFilter>().sharedMesh;
        return container;
    }
}
