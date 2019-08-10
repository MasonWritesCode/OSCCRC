using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class provides access to different assets under the same name. This allows us to create "resource packs" that are associated with a stage.

public class GameResources : MonoBehaviour {

    public Dictionary<string, Material> materials = new Dictionary<string, Material>();
    public Dictionary<string, Transform> objects  = new Dictionary<string, Transform>();
    public string resourcePack { get { return m_resourcePack; } set { loadResources(value); } }


    // Loads the resources into the interface that are under the name specified by "resourcePack"
    public void loadResources(string resourcePack)
    {
        if (resourcePack == m_resourcePack)
        {
            return;
        }

        materials.Clear();
        objects.Clear();

        m_resourcePack = resourcePack;
        string currentDir;

        currentDir = "/Materials/";
        materials.Add("Tile", resourceFromDir(currentDir + "Tile") as Material);
        materials.Add("TileAlt", resourceFromDir(currentDir + "TileAlt") as Material);
        materials.Add("TileTiledColor", resourceFromDir(currentDir + "TileTiledColor") as Material);
        materials.Add("Hole", resourceFromDir(currentDir + "Hole") as Material);
        materials.Add("Goal", resourceFromDir(currentDir + "Goal") as Material);
        materials.Add("Goal1", resourceFromDir(currentDir + "Goal1") as Material);
        materials.Add("Goal2", resourceFromDir(currentDir + "Goal2") as Material);
        materials.Add("Goal3", resourceFromDir(currentDir + "Goal3") as Material);
        materials.Add("Spawner", resourceFromDir(currentDir + "Spawner") as Material);
        materials.Add("Placeholder", resourceFromDir(currentDir + "ObjectPlace") as Material);

        currentDir = "/Prefabs/";
        objects.Add("Tile", (resourceFromDir(currentDir + "TilePrefab") as GameObject).transform);
        objects.Add("DirectionArrow", (resourceFromDir(currentDir + "DirectionArrow") as GameObject).transform);
        objects.Add("DirectionArrow1", (resourceFromDir(currentDir + "DirectionArrow1") as GameObject).transform);
        objects.Add("DirectionArrow2", (resourceFromDir(currentDir + "DirectionArrow2") as GameObject).transform);
        objects.Add("DirectionArrow3", (resourceFromDir(currentDir + "DirectionArrow3") as GameObject).transform);
        objects.Add("Wall", (resourceFromDir(currentDir + "WallPrefab") as GameObject).transform);
        objects.Add("Mouse", (resourceFromDir(currentDir + "Mouse") as GameObject).transform);
        objects.Add("BigMouse", (resourceFromDir(currentDir + "BigMouse") as GameObject).transform);
        objects.Add("SpecialMouse", (resourceFromDir(currentDir + "SpecialMouse") as GameObject).transform);
        objects.Add("Cat", (resourceFromDir(currentDir + "Cat") as GameObject).transform);
        objects.Add("Placeholder", (resourceFromDir(currentDir + "Placeholder") as GameObject).transform);
    }


    // Returns a resource located at the specified path or the default equivalent if not found
    private Object resourceFromDir(string path)
    {
        Object resource = Resources.Load(m_resourcePack + path);

        if (resource == null)
        {
            // Pull missing items from default pack to allow partial packs
            resource = Resources.Load("Default" + path);

            if (resource == null)
            {
                Debug.LogError("Was not able to find default resource: " + path);
            }
        }

        return resource;
    }


    void Awake()
    {
        loadResources("Default");
    }


    private string m_resourcePack = string.Empty;
}
