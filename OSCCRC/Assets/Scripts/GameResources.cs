using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour {

    // Class to potentially allow different texture packs, and make it easier to modify resources with minimal code changes

    // TODO: Since resources aren't created yet, material resource locations aren't assigned. This must be done once materials created.

    public static Dictionary<string, Material> materials;
    public static Dictionary<string, Transform> objects;

    private string m_resourcePack = string.Empty;

    void loadResources(string resourcePackDir)
    {
        materials = new Dictionary<string, Material>();
        objects = new Dictionary<string, Transform>();

        m_resourcePack = resourcePackDir;
        string currentDir;

        currentDir = resourcePackDir + "/Materials/";
        materials.Add("Tile", Resources.Load(currentDir + "Tile") as Material);
        materials.Add("TileAlt", Resources.Load(currentDir + "TileAlt") as Material);
        materials.Add("Hole", Resources.Load(currentDir + "Hole") as Material);
        materials.Add("Left", Resources.Load(currentDir + "Left") as Material);
        materials.Add("Right", Resources.Load(currentDir + "Right") as Material);
        materials.Add("Up", Resources.Load(currentDir + "Up") as Material);
        materials.Add("Down", Resources.Load(currentDir + "Down") as Material);
        materials.Add("Placeholder", Resources.Load(currentDir + "ObjectPlace") as Material);

        currentDir = resourcePackDir + "/Prefabs/";
        objects.Add("Tile", (Resources.Load(currentDir + "TilePrefab") as GameObject).transform);
        objects.Add("Wall", (Resources.Load(currentDir + "WallPrefab") as GameObject).transform);
        objects.Add("Mouse", (Resources.Load(currentDir + "Mouse") as GameObject).transform);
        objects.Add("Cat", (Resources.Load(currentDir + "Cat") as GameObject).transform);
    }

    void Awake () {
        loadResources("Default");
    }
}
