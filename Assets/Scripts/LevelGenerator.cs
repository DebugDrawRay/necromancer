using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Utilities;

public class LevelGenerator : MonoBehaviour
{
    public Level levelToLoad;

    [Header("Generation Settings")]
    public bool generateLevel = true;
    public bool generateProps = true;
    public bool generateColliders = true;
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
        public Tile currentTile;
        public Tile lastTile;
    }
    private List<TileMaker> tileMakers = new List<TileMaker>();
    private int tileCount;

    private List<Tile> levelToProcess;

    private GameObject loadedLevel;
    private GameObject propsContainer;
    private GameObject collidersContainer;

    [Header("Level Processing")]
    public Vector3 levelRotation;

    void Start()
    {
        GenerateNewLevel();
        AstarPath.active.Scan();
    }

    [ContextMenu("Generate New Level")]
    public void GenerateNewLevel()
    {
        Destroy(loadedLevel);
        Destroy(propsContainer);

        tileMakers = new List<TileMaker>();
        tileCount = 0;

        if (generateLevel)
        {
            levelToProcess = GenerateLevel(levelToLoad);
            if (generateProps)
            {
                propsContainer = GenerateProps(levelToProcess, levelToLoad.tileset, levelToLoad.propsRange);
            }
            if(generateColliders)
            {
                collidersContainer = GenerateColliders(levelToProcess, levelToLoad.tileset);
            }
            if (processLevel)
            {
                loadedLevel = ProcessTiles(levelToProcess);
            }

            loadedLevel.transform.rotation = Quaternion.Euler(levelRotation);
            propsContainer.transform.rotation = Quaternion.Euler(levelRotation);
            collidersContainer.transform.rotation = Quaternion.Euler(levelRotation);
        }
    }

    List<Tile> GenerateLevel(Level level)
    {
        List<Tile> toDelete = new List<Tile>();
        List<Tile> processed = new List<Tile>();

        Tileset set = level.tileset;
        if (tileMakers.Count <= 0)
        {
            Tile tile = NewTile(set.groundTiles, 0);
            processed.Add(tile);
            for(int i = 0; i < startingTileMakers; i++)
            {
                TileMaker maker = new TileMaker();
                maker.currentTile = tile;
                tileMakers.Add(maker);
            }
        }
        while (tileCount < level.tileCount)
        {
            for(int i = 0; i < tileMakers.Count; i++)
            {
                if (tileCount < level.tileCount)
                {
                    //ref the current maker
                    TileMaker maker = tileMakers[i];
                    Tile tile = maker.currentTile;
                    //select a connector
                    int select = Random.Range(0, tile.connectors.Length);
                    Transform connector = tile.connectors[select];
                    //create a new tile
                    Tile newTile = NewTile(set.groundTiles);
                    //apply position
                    newTile.transform.position = connector.position;
                    //check collisions
                    if (!CheckTileCollision(processed, newTile))
                    {
                        tileCount++;
                    }
                    //Set parents
                    newTile.parent = tile;
                    //update maker reference
                    maker.currentTile = newTile;
                    //add tile to return
                    processed.Add(newTile);

                    float roll = Random.value;
                    if (roll <= tileMakerSpawnChance && tileMakers.Count < maxTileMakers)
                    {
                        TileMaker newMaker = new TileMaker();
                        maker.currentTile = newTile;
                        tileMakers.Add(maker);
                    }
                }
            }
        }

        for(int i = 0; i < processed.Count; i++)
        {
            Tile check = processed[i];
            if(CheckTileCollision(processed, check))
            {
                processed.RemoveAt(i);
                Destroy(check.gameObject);
                i--;
            }
        }
        Debug.Log(processed.Count);
        return processed;
    }

    GameObject GenerateProps(List<Tile> tiles, Tileset tileset, Vector2 countRange)
    {
        GameObject propContainer = new GameObject("Props");
        List<Transform> placements = new List<Transform>();
        foreach(Tile tile in tiles)
        {
            foreach(Transform trans in tile.propPlacements)
            {
                placements.Add(trans);
            }
        }
        int propsCount = Roll.RangeInt(countRange);

        if(propsCount > placements.Count)
        {
            propsCount = placements.Count;
        }

        for(int i = 0; i <= propsCount; i++)
        {
            int selectTrans = Random.Range(0, placements.Count);
            Transform place = placements[selectTrans];

            int selectProp = Random.Range(0, tileset.props.Length);
            GameObject prop = tileset.props[selectProp];

            GameObject newProp = (GameObject)Instantiate(prop, place.position, place.rotation, propContainer.transform);
            placements.RemoveAt(selectTrans);
        }

        return propContainer;
    }

    GameObject GenerateColliders(List<Tile> tiles, Tileset tileset)
    {
        GameObject container = new GameObject("Colliders");
        GameObject collider = tileset.emptyTileCollider;

        foreach(Tile tile in tiles)
        {
            foreach(Transform connector in tile.connectors)
            {
                if(!CheckTileCollision(tiles, connector.position) && !CheckColliderCollision(container.transform, connector.position))
                {
                    Instantiate(collider, connector.position, connector.rotation, container.transform);
                }
            }
        }

        return container;
    }
    List<GameObject> GenerateEnemies(List<Tile> tiles)
    {
        return null;
    }
    List<GameObject> GenerateGoals(List<Tile> tiles)
    {
        return null;
    }

    bool CheckColliderCollision(Transform colliders, Vector3 position)
    {
        bool collision = false;
        foreach (Transform collider in colliders)
        {
            if (position == collider.position)
            {
                collision = true;
            }
        }
        return collision;
    }
    bool CheckTileCollision(List<Tile> tiles, Tile tileToCheck)
    {
        bool collision = false;
        foreach(Tile tile in tiles)
        {
            if(tileToCheck.transform.position == tile.transform.position && tile != tileToCheck)
            {
                collision = true;
            }
        }
        return collision;
    }
    bool CheckTileCollision(List<Tile> tiles, Vector3 positionToCheck)
    {
        bool collision = false;
        foreach (Tile tile in tiles)
        {
            if (positionToCheck == tile.transform.position)
            {
                collision = true;
            }
        }
        return collision;
    }

    GameObject ProcessTiles(List<Tile> tiles)
    {
        //Create container
        GameObject level = new GameObject("Level");
        level.isStatic = true;
        level.AddComponent<MeshFilter>();
        level.AddComponent<MeshRenderer>();

        //Create lists for materials, meshes and combine instances
        List<MeshFilter> meshFilters = new List<MeshFilter>();
        List<Material> materials = new List<Material>();
        List<List<CombineInstance>> combineInstanceArrays = new List<List<CombineInstance>>();

        //Add mesh filters from tiles
        foreach(Tile tile in tiles)
        {
            meshFilters.Add(tile.GetComponentInChildren<MeshFilter>());
        }

        //Get materials from the renderer of the filter
        foreach (MeshFilter meshFilter in meshFilters)
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
        MeshFilter meshFilterCombine = level.GetComponent<MeshFilter>();

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

        //Convert materials list to array and add to renderer
        //Material[] materialsArray = materials.ToArray();
        //level.GetComponent<MeshRenderer>().materials = materialsArray;

        //Add tileset material for the ground
        level.GetComponent<MeshRenderer>().material = levelToLoad.tileset.groundMaterial;

        level.AddComponent<MeshCollider>();
        level.GetComponent<MeshCollider>().sharedMesh = level.GetComponent<MeshFilter>().sharedMesh;

        foreach (Tile tile in tiles)
        {
            Destroy(tile.gameObject);
        }
        return level;
    }
    Tile NewTile(GameObject[] tiles, int select)
    {
        return ((GameObject)Instantiate(tiles[select], Vector3.zero, Quaternion.identity)).GetComponent<Tile>();
    }
    Tile NewTile(GameObject[] tiles)
    {
        int select = Random.Range(0, tiles.Length);
        return ((GameObject)Instantiate(tiles[select], Vector3.zero, Quaternion.identity)).GetComponent<Tile>();
    }
}
