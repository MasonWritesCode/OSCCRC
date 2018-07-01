using System.Collections.Generic;
using System.IO;
using UnityEngine;

// This class handles the game map, including the tiles, walls, and moving entities within.
// It will be used to create new map objects and to access them.

public class GameMap : MonoBehaviour
{
    [Range(1, 255)] public int mapHeight = 9;
    [Range(1, 255)] public int mapWidth = 12;
    [HideInInspector] public float tileSize;

    public delegate void objectEvent(GameObject caller);
    public static event objectEvent mouseDestroyed;

    private Transform mapTransform;
    private MapTile[,] mapTiles;

    // Generates a new rectangular map of the specified width and height
    void generateMap(int height, int width)
    {
        mapHeight = height; mapWidth = width;
        mapTiles = new MapTile[mapHeight, mapWidth];

        tileSize = GameResources.objects["Tile"].localScale.x;

        mapTransform = GetComponent<Transform>();
        for (int j = 0; j < mapHeight; ++j)
        {
            for (int i = 0; i < mapWidth; ++i)
            {
                mapTiles[j, i] = createTile(i * tileSize, j * tileSize);
            }
        }

        setCameraView(Camera.main);
    }

    // Returns the maptile info of the tile that the current world coordinate is inside of
    public MapTile tileAt(Vector3 point)
    {
        return mapTiles[Mathf.RoundToInt(point.z / tileSize), Mathf.RoundToInt(point.x / tileSize)];
    }

    // Creates a map tile
    // The returned object should only be destroyed by calling the destroyTile function
    public MapTile createTile(float xPos, float zPos)
    {
        Transform tilePrefab = GameResources.objects["Tile"];
        Transform newTileTransform = Instantiate(tilePrefab, new Vector3(xPos, 0, zPos), tilePrefab.rotation, mapTransform);
        MapTile newTile = newTileTransform.gameObject.AddComponent<MapTile>();
        newTile.initTile(this);
        return newTile;
    }

    // Creates a wall game object
    // The returned object should only be destroyed by calling the destroyWall function
    public Transform createWall(float xPos, float zPos, Directions.Direction direction)
    {
        Transform wallPrefab = GameResources.objects["Wall"];
        Transform newWall = Instantiate(wallPrefab, new Vector3(xPos, 0, zPos), wallPrefab.rotation, mapTransform);

        if (direction == Directions.Direction.North)
        {
            newWall.localPosition += Vector3.forward * (tileSize / 2);
        }
        else if (direction == Directions.Direction.East)
        {
            newWall.localPosition += Vector3.right * (tileSize / 2);
        }
        else if (direction == Directions.Direction.South)
        {
            newWall.localPosition += Vector3.back * (tileSize / 2);
        }
        else if (direction == Directions.Direction.West)
        {
            newWall.localPosition += Vector3.left * (tileSize / 2);
        }
        Directions.rotate(ref newWall, direction);

        return newWall;
    }

    // Removes a tile game object
    public void destroyTile(MapTile tile)
    {
        Destroy(tile.gameObject);
    }

    // Removes a wall game object with transform "wall"
    public void destroyWall(Transform wall)
    {
        Destroy(wall.gameObject);
    }

    // Removes a mouse game object with transform "mouse"
    public void destroyMouse(Transform mouse)
    {
        if (mouseDestroyed != null)
        {
            mouseDestroyed(mouse.gameObject);
        }
        Destroy(mouse.gameObject);
    }

    // Removes a cat game object with transform "cat"
    public void destroyCat(Transform cat)
    {
        Destroy(cat.gameObject);
    }

    // Imports map data to the current game map from the open stream specified by "fin"
    public bool importMap(StreamReader fin)
    {
        // Delete allocated game objects, since we are creating new ones
        for (int j = 0; j < mapHeight; ++j)
        {
            for (int i = 0; i < mapWidth; ++i)
            {
                // North and east walls will be removed by changing the south and west of it's partner tiles
                mapTiles[j, i].walls.south = mapTiles[j, i].walls.west = false;
            }
        }
        // We aren't currently keeping track of mice or cats, so destroy all children with a GridMovement attached
        List<GridMovement> deadMeat = new List<GridMovement>();
        GetComponentsInChildren<GridMovement>(true, deadMeat);
        foreach (GridMovement i in deadMeat)
        {
            if (i.isCat)
            {
                destroyCat(i.transform);
            }
            else
            {
                destroyMouse(i.transform);
            }
        }

        // Create a new map if necessary
        int newMapHeight = int.Parse(fin.ReadLine());
        int newMapWidth = int.Parse(fin.ReadLine());

        if (newMapHeight != mapHeight && newMapWidth != mapWidth)
        {
            for (int j = 0; j < mapHeight; ++j)
            {
                for (int i = 0; i < mapWidth; ++i)
                {
                    destroyTile(mapTiles[j, i]);
                    mapTiles[j, i] = null;
                }
            }
            mapTiles = null;

            generateMap(newMapHeight, newMapWidth);
        }

        // Add stuff onto the tiles
        for (int j = 0; j < mapHeight; ++j)
        {
            for (int i = 0; i < mapWidth; ++i)
            {
                MapTile.TileImprovement tileImprovement = (MapTile.TileImprovement)int.Parse(fin.ReadLine());
                if (tileImprovement != MapTile.TileImprovement.None)
                {
                    mapTiles[j, i].improvementDirection = (Directions.Direction)int.Parse(fin.ReadLine());
                }
                MapTile.TileImprovement movingObj = (MapTile.TileImprovement)int.Parse(fin.ReadLine());
                if (movingObj != MapTile.TileImprovement.None)
                {
                    mapTiles[j, i].movingObjDirection = (Directions.Direction)int.Parse(fin.ReadLine());

                    if (movingObj == MapTile.TileImprovement.Mouse)
                    {
                        placeMouse(mapTiles[j, i].transform.position.x, mapTiles[j, i].transform.position.z, mapTiles[j, i].movingObjDirection);
                    }
                    else if (movingObj == MapTile.TileImprovement.Cat)
                    {
                        placeCat(mapTiles[j, i].transform.position.x, mapTiles[j, i].transform.position.z, mapTiles[j, i].movingObjDirection);
                    }
                }
                mapTiles[j, i].improvement = tileImprovement;
                mapTiles[j, i].movingObject = movingObj;

                if (i == 0 || j == 0 || (i + j) % 2 == 0)
                {
                    int wallsValue = int.Parse(fin.ReadLine());
                    if (wallsValue != 0)
                    {
                        if ((wallsValue >> 0 & 1) == 1)
                        {
                            mapTiles[j, i].walls.north = true;
                        }
                        if ((wallsValue >> 1 & 1) == 1)
                        {
                            mapTiles[j, i].walls.east = true;
                        }
                        if ((wallsValue >> 2 & 1) == 1)
                        {
                            mapTiles[j, i].walls.south = true;
                        }
                        if ((wallsValue >> 3 & 1) == 1)
                        {
                            mapTiles[j, i].walls.west = true;
                        }
                    }
                }
            }
        }

        return true;
    }

    // Exports map data from the current game map to the open stream specified by "fout"
    public bool exportMap(StreamWriter fout)
    {
        fout.WriteLine(mapHeight);
        fout.WriteLine(mapWidth);

        for (int j = 0; j < mapHeight; ++j)
        {
            for (int i = 0; i < mapWidth; ++i)
            {
                MapTile tile = mapTiles[j, i];

                fout.WriteLine((int)tile.improvement);
                if (tile.improvement != MapTile.TileImprovement.None)
                {
                    fout.WriteLine((int)tile.improvementDirection);
                }

                fout.WriteLine((int)tile.movingObject);
                if (tile.movingObject != MapTile.TileImprovement.None)
                {
                    fout.WriteLine((int)tile.movingObjDirection);
                }

                if (i == 0 || j == 0 || (i + j) % 2 == 0)
                {
                    int wallsValue = 0;
                    if (tile.walls.north)
                    {
                        wallsValue |= 1 << 0;
                    }
                    if (tile.walls.east)
                    {
                        wallsValue |= 1 << 1;
                    }
                    if (tile.walls.south)
                    {
                        wallsValue |= 1 << 2;
                    }
                    if (tile.walls.west)
                    {
                        wallsValue |= 1 << 3;
                    }
                    fout.WriteLine(wallsValue);
                }
            }
        }

        return true;
    }

    // Loads the map from the file specified by "fileName"
    public void loadMap(string fileName)
    {
        string mapPath = Application.dataPath + "/Maps/" + fileName + ".map";
        if (!File.Exists(mapPath))
        {
            Debug.LogWarning("Tried to load a map but it wasn't found! " + mapPath);
            return;
        }
        using (StreamReader fin = new StreamReader(mapPath))
        {
            bool wasLoaded = importMap(fin);
            if (!wasLoaded)
            {
                Debug.LogWarning("Failed to read map file " + fileName);
            }
        }
    }

    // Saves the current map to the file specified by "fileName"
    public void saveMap(string fileName)
    {
        string mapPath = Application.dataPath + "/Maps/" + fileName + ".map";
        using (StreamWriter fout = new StreamWriter(mapPath, false))
        {
            exportMap(fout);
        }
    }

    // Creates a mouse object and returns its transform
    // The returned object should only be destroyed with the destroyMouse function
    public Transform placeMouse(float xPos, float zPos, Directions.Direction direction)
    {
        Transform mousePrefab = GameResources.objects["Mouse"];
        Transform newMouse = Instantiate(mousePrefab, new Vector3(xPos, 0, zPos), mousePrefab.rotation, mapTransform);
        Directions.rotate(ref newMouse, direction);
        newMouse.GetComponent<GridMovement>().direction = direction;

        return newMouse;
    }

    // Creates a cat object and returns its transform
    // The returned object should only be destroyed with the destroyCat function
    public Transform placeCat(float xPos, float zPos, Directions.Direction direction)
    {
        Transform catPrefab = GameResources.objects["Cat"];
        Transform newCat = Instantiate(catPrefab, new Vector3(xPos, 0, zPos), catPrefab.rotation, mapTransform);
        Directions.rotate(ref newCat, direction);
        newCat.GetComponent<GridMovement>().direction = direction;

        return newCat;
    }

    // Sets the position of camera "cam" to be able to see the entire map
    public void setCameraView(Camera cam)
    {
        // There is probably a correct thing to do here, but just do whatever for now since map sizes will probably always be the same.
        float scaleFactor = 4.25f;
        float cameraAngle = cam.transform.eulerAngles.x;
        cam.transform.position = new Vector3((mapWidth * 1.5f) + (mapHeight * 0.5f),
                                             (mapWidth + mapHeight) * 1.6f + (Mathf.Sin(cameraAngle * Mathf.Deg2Rad) * (scaleFactor / 2)),
                                             (mapWidth * 0.5f) + (mapHeight * 0.5f) - (Mathf.Sin(cameraAngle * Mathf.Deg2Rad) * (scaleFactor / 2))
                                            ) / scaleFactor;
    }

    void Start()
    {
        bool useImportedMap = false;
        if (useImportedMap)
        {
            loadMap("Dev");
        }
        else
        {
            if (mapTiles != null)
            {
                // We already created a map by loading a save elsewhere or something. So don't make a blank one.
                return;
            }

            // mapHeight and mapWidth are initialized in the Unity editor
            generateMap(mapHeight, mapWidth);

            // Create a wall around edges of map by default
            for (int j = 0; j < mapHeight; ++j)
            {
                for (int i = 0; i < mapWidth; ++i)
                {
                    if (j == 0)
                    {
                        mapTiles[j, i].walls.south = true;
                    }
                    else if (j == mapHeight - 1)
                    {
                        mapTiles[j, i].walls.north = true;
                    }
                    if (i == 0)
                    {
                        mapTiles[j, i].walls.west = true;
                    }
                    else if (i == mapWidth - 1)
                    {
                        mapTiles[j, i].walls.east = true;
                    }
                }
            }
        }
    }
}
