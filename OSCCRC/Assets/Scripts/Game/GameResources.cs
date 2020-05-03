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

        const string materialDir = "/Materials/";
        materials.Add("TileTiledColor", resourceFromDir(materialDir + "TileTiledColor") as Material);
        materials.Add("Hole",           resourceFromDir(materialDir + "Hole")           as Material);
        materials.Add("Goal",           resourceFromDir(materialDir + "Goal")           as Material);
        materials.Add("Goal1",          resourceFromDir(materialDir + "Goal1")          as Material);
        materials.Add("Goal2",          resourceFromDir(materialDir + "Goal2")          as Material);
        materials.Add("Goal3",          resourceFromDir(materialDir + "Goal3")          as Material);
        materials.Add("Spawner",        resourceFromDir(materialDir + "Spawner")        as Material);

        const string prefabDir = "/Prefabs/";
        objects.Add("Tile",            (resourceFromDir(prefabDir + "TilePrefab")      as GameObject).transform);
        objects.Add("DirectionArrow",  (resourceFromDir(prefabDir + "DirectionArrow")  as GameObject).transform);
        objects.Add("DirectionArrow1", (resourceFromDir(prefabDir + "DirectionArrow1") as GameObject).transform);
        objects.Add("DirectionArrow2", (resourceFromDir(prefabDir + "DirectionArrow2") as GameObject).transform);
        objects.Add("DirectionArrow3", (resourceFromDir(prefabDir + "DirectionArrow3") as GameObject).transform);
        objects.Add("Wall",            (resourceFromDir(prefabDir + "WallPrefab")      as GameObject).transform);
        objects.Add("Mouse",           (resourceFromDir(prefabDir + "Mouse")           as GameObject).transform);
        objects.Add("BigMouse",        (resourceFromDir(prefabDir + "BigMouse")        as GameObject).transform);
        objects.Add("SpecialMouse",    (resourceFromDir(prefabDir + "SpecialMouse")    as GameObject).transform);
        objects.Add("Cat",             (resourceFromDir(prefabDir + "Cat")             as GameObject).transform);
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
