using UnityEngine;
using UnityEditor;

public class CreateLevel
{
    [MenuItem("Assets/Create/Level Assets/Level")]
    public static Level Create()
    {
        Level asset = ScriptableObject.CreateInstance<Level>();

        AssetDatabase.CreateAsset(asset, "Assets/NewLevel.asset");
        AssetDatabase.SaveAssets();
        return asset;
    }
}
public class CreateTileset
{
    [MenuItem("Assets/Create/Level Assets/Tileset")]
    public static Tileset Create()
    {
        Tileset asset = ScriptableObject.CreateInstance<Tileset>();

        AssetDatabase.CreateAsset(asset, "Assets/NewTileset.asset");
        AssetDatabase.SaveAssets();
        return asset;
    }
}
