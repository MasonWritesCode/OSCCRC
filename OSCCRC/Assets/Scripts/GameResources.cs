﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour {

    // Class to potentially allow different texture packs, and make it easier to modify resources with minimal code changes

    // TODO: Since resources aren't created yet, material resource locations aren't assigned. This must be done once materials created.

    public static Dictionary<string, Material> materials;
    public static Dictionary<string, Transform> objects;
    public static string resourcePack { get { return m_resourcePack; } set { loadResources(value); } }

    private static string m_resourcePack = string.Empty;

    static void loadResources(string resourcePack)
    {
        materials = new Dictionary<string, Material>();
        objects = new Dictionary<string, Transform>();

        m_resourcePack = resourcePack;
        string currentDir;

        currentDir = "/Materials/";
        materials.Add("Tile", resourceFromDir(currentDir + "Tile") as Material);
        materials.Add("TileAlt", resourceFromDir(currentDir + "TileAlt") as Material);
        materials.Add("Hole", resourceFromDir(currentDir + "Hole") as Material);
        materials.Add("Goal", resourceFromDir(currentDir + "Goal") as Material);
        materials.Add("Placeholder", resourceFromDir(currentDir + "ObjectPlace") as Material);

        currentDir = "/Prefabs/";
        objects.Add("Tile", (resourceFromDir(currentDir + "TilePrefab") as GameObject).transform);
        objects.Add("DirectionArrow", (resourceFromDir(currentDir + "DirectionArrow") as GameObject).transform);
        objects.Add("Wall", (resourceFromDir(currentDir + "WallPrefab") as GameObject).transform);
        objects.Add("Mouse", (resourceFromDir(currentDir + "Mouse") as GameObject).transform);
        objects.Add("Cat", (resourceFromDir(currentDir + "Cat") as GameObject).transform);
    }

    static private Object resourceFromDir(string path)
    {
        Object resource = Resources.Load(m_resourcePack + path);

        if (resource == null)
        {
            // Pull missing items from default pack to allow partial packs
            resource = Resources.Load("Default" + path);
        }
        if (resource == null)
        {
            Debug.LogWarning("Was not able to find default resource: " + path);
        }

        return resource;
    }

    void Awake()
    {
        loadResources("Default");
    }
}
