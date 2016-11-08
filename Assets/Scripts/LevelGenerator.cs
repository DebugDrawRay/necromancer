using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Utilities;

public class LevelGenerator : MonoBehaviour
{
    public Level levelToLoad;

    [Header("Tile Makers")]
    [Tooltip("Each tile maker will place one tile at a selected connector")]
    public int startingTileMakers;
    [Tooltip("Adjust the max amount of tile makers to be spawned; set this to the same value as 'tileMakers' to remove additional tile makers spawns")]
    public int maxTileMakers;
    [Tooltip("Adjust the chance of a tile maker being spawned")]
    [Range(0f, 1f)]
    public float tileMakerSpawnChance;

    private int tileCount;
    
    public class TileMaker
    {
        public Tile currentTile;
        public Tile lastTile;
    }
    private List<TileMaker> tileMakers = new List<TileMaker>();
    private GameObject loadedLevel;

    [Header("Tile generation")]
    public LayerMask tileCollisionLayer;
    private List<Tile> levelToProcess;

    void Start()
    {
        levelToProcess = GenerateLevel(levelToLoad);
        GenerateProps(levelToProcess, levelToLoad.tileset, levelToLoad.propsRange);
        loadedLevel = ProcessTiles(levelToProcess);
    }

    [ContextMenu("Generate New Level")]
    public void GenerateNewLevel()
    {
        Destroy(loadedLevel);

        tileMakers = new List<TileMaker>();
        tileCount = 0;

        levelToProcess = GenerateLevel(levelToLoad);
        GenerateProps(levelToProcess, levelToLoad.tileset, levelToLoad.propsRange);
        loadedLevel = ProcessTiles(levelToProcess);
    }

    List<Tile> GenerateLevel(Level level)
    {
        List<Tile> generated = new List<Tile>();
        Tileset set = level.tileset;
        if (tileMakers.Count <= 0)
        {
            Tile tile = NewTile(set.groundTiles, 0);
            generated.Add(tile);
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
                    if (tile.parent == null || (tile.parent != null && newTile.transform.position != tile.parent.transform.position))
                    {
                        Debug.Log("Placed");
                        //Set parents
                        newTile.parent = tile;
                        generated.Add(newTile);
                        //update maker referenced and increment
                        maker.currentTile = newTile;
                        tileCount++;
                    }
                    else
                    {
                        Debug.Log("Collision");
                        //dont count tile and destroy
                        Destroy(newTile.gameObject);
                    }

                    float roll = Random.value;
                    if (roll <= tileMakerSpawnChance)
                    {
                        TileMaker newMaker = new TileMaker();
                        maker.currentTile = tile;
                        tileMakers.Add(maker);
                    }
                }
            }
        }
        return generated;
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
    List<GameObject> GenerateEnemies(List<Tile> tiles)
    {
        return null;
    }
    List<GameObject> GenerateGoals(List<Tile> tiles)
    {
        return null;
    }


    GameObject ProcessTiles(List<Tile> tiles)
    {
        GameObject level = new GameObject("Level");
        level.AddComponent<MeshFilter>();
        level.AddComponent<MeshRenderer>();

        // Find all mesh filter submeshes and separate them by their cooresponding materials
        ArrayList materials = new ArrayList();
        ArrayList combineInstanceArrays = new ArrayList();

        List<MeshFilter> meshFilters = new List<MeshFilter>();
        foreach(Tile tile in tiles)
        {
            meshFilters.Add(tile.GetComponentInChildren<MeshFilter>());
        }

        foreach (MeshFilter meshFilter in meshFilters)
        {
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            // Handle bad input
            if (!meshRenderer)
            {
                Debug.LogError("MeshFilter does not have a coresponding MeshRenderer.");
                continue;
            }
            if (meshRenderer.materials.Length != meshFilter.sharedMesh.subMeshCount)
            {
                Debug.LogError("Mismatch between material count and submesh count. Is this the correct MeshRenderer?");
                continue;
            }

            for (int s = 0; s < meshFilter.sharedMesh.subMeshCount; s++)
            {
                int materialArrayIndex = 0;
                for (materialArrayIndex = 0; materialArrayIndex < materials.Count; materialArrayIndex++)
                {
                    if (materials[materialArrayIndex] == meshRenderer.sharedMaterials[s])
                    {
                        break;
                    }
                }

                if (materialArrayIndex == materials.Count)
                {
                    materials.Add(meshRenderer.sharedMaterials[s]);
                    combineInstanceArrays.Add(new ArrayList());
                }

                CombineInstance combineInstance = new CombineInstance();
                combineInstance.transform = meshRenderer.transform.localToWorldMatrix;
                combineInstance.subMeshIndex = s;
                combineInstance.mesh = meshFilter.sharedMesh;
                (combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);
            }
        }

        // Get / Create mesh filter
        MeshFilter meshFilterCombine = level.GetComponent<MeshFilter>();

        // Combine by material index into per-material meshes
        // also, Create CombineInstance array for next step
        Mesh[] meshes = new Mesh[materials.Count];
        CombineInstance[] combineInstances = new CombineInstance[materials.Count];

        for (int m = 0; m < materials.Count; m++)
        {
            CombineInstance[] combineInstanceArray = (combineInstanceArrays[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
            meshes[m] = new Mesh();
            meshes[m].CombineMeshes(combineInstanceArray, true, true);

            combineInstances[m] = new CombineInstance();
            combineInstances[m].mesh = meshes[m];
            combineInstances[m].subMeshIndex = 0;
        }

        // Combine into one
        meshFilterCombine.sharedMesh = new Mesh();
        meshFilterCombine.sharedMesh.CombineMeshes(combineInstances, false, false);

        // Assign materials
        Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];
        level.GetComponent<MeshRenderer>().materials = materialsArray;

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
