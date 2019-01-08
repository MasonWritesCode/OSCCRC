using System.Collections.Generic;
using System.IO;
using UnityEngine;

// This class handles the game map, including the tiles, walls, and moving entities within.
// It will be used to create new map objects and to access them.

public class GameMap : MonoBehaviour
{
    [HideInInspector] public int mapHeight = 0;
    [HideInInspector] public int mapWidth = 0;
    [HideInInspector] public float tileSize;

    public delegate void objectEvent(GameObject caller);
    public static event objectEvent mouseDestroyed;
    public static event objectEvent mousePlaced;
    public static event objectEvent catDestroyed;

    private MapTile[,] mapTiles;
    private Transform m_bigTile = null;

    // Generates a new rectangular map of the specified width and height
    void generateMap(int height, int width)
    {
        mapHeight = height; mapWidth = width;
        mapTiles = new MapTile[mapHeight, mapWidth];

        tileSize = GameResources.objects["Tile"].localScale.x;

        for (int j = 0; j < mapHeight; ++j)
        {
            for (int i = 0; i < mapWidth; ++i)
            {
                mapTiles[j, i] = createTile(i * tileSize, j * tileSize);
            }
        }

        if (GlobalData.x_useBigTile)
        {
            // Here we set a single stretched tile that represents all blank tiles
            // Since most tiles are blank, this saves draw calls by only enabling a tile's renderer when it is not blank
            if (m_bigTile == null)
            {
                Transform tilePrefab = GameResources.objects["Tile"];
                m_bigTile = Instantiate(tilePrefab, Vector3.zero, tilePrefab.rotation, transform);
            }

            // We need to set the scale to mapsize
            // Position needs to be set to ((mapsize - 1) / 2) (divided by two because scale stretches in both directions)
            // Material tiling has to be set to (mapsize / 2)
            m_bigTile.transform.localScale = new Vector3(mapWidth, mapHeight, 1.0f);
            m_bigTile.transform.position = new Vector3((mapWidth - 1.0f) / 2.0f, 0.0f, (mapHeight - 1.0f) / 2.0f);
            MeshRenderer bigTileRend = m_bigTile.GetComponent<MeshRenderer>();
            bigTileRend.material = GameResources.materials["TileTiledColor"];
            bigTileRend.material.mainTextureScale = new Vector2(mapWidth / 2.0f, mapHeight / 2.0f);

            bigTileRend.enabled = true;
        }

        setCameraView(Camera.main);
    }

    // Returns the maptile info of the tile that the current GameMap-local coordinate is inside of
    public MapTile tileAt(Vector3 point)
    {
        return mapTiles[Mathf.RoundToInt(point.z / tileSize), Mathf.RoundToInt(point.x / tileSize)];
    }

    // Returns the maptile info of the tile that the current GameMap-local coordinate is inside of
    public MapTile tileAt(float y, float x)
    {
        return mapTiles[Mathf.RoundToInt(y / tileSize), Mathf.RoundToInt(x / tileSize)];
    }

    // Creates a map tile
    // The returned object should only be destroyed by calling the destroyTile function
    public MapTile createTile(float xPos, float zPos)
    {
        Transform tilePrefab = GameResources.objects["Tile"];
        // Height is being set to a small value for x_useBigTile, to prevent z-fighting with the big tile. Mathf.Epsilon doesn't seem to be enough for this.
        Transform newTileTransform = Instantiate(tilePrefab, new Vector3(xPos, 0.00001f, zPos), tilePrefab.rotation, transform);
        MapTile newTile = newTileTransform.gameObject.AddComponent<MapTile>();
        newTile.initTile(this);
        return newTile;
    }

    // Creates a wall game object
    // The returned object should only be destroyed by calling the destroyWall function
    public Transform createWall(float xPos, float zPos, Directions.Direction direction)
    {
        Transform wallPrefab = GameResources.objects["Wall"];
        Transform newWall = Instantiate(wallPrefab, new Vector3(xPos, 0, zPos), wallPrefab.rotation, transform);

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
        if (catDestroyed != null)
        {
            catDestroyed(cat.gameObject);
        }
        Destroy(cat.gameObject);
    }

    // Imports map data to the current game map from the open stream specified by "fin"
    public bool importMap(StreamReader fin)
    {
        // Delete allocated game objects, since we are creating new ones
        // Removing walls will be handled at the same point in code where they are placed

        // We aren't currently keeping track of mice or cats, so destroy all children with a GridMovement attached
        GridMovement[] deadMeat = GetComponentsInChildren<GridMovement>(true);
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
            if (mapTiles != null)
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
            }

            generateMap(newMapHeight, newMapWidth);
        }

        // Old objects are gone, so now Add stuff onto the tiles
        for (int j = 0; j < mapHeight; ++j)
        {
            for (int i = 0; i < mapWidth; ++i)
            {
                MapTile tile = mapTiles[j, i];

                // An underscore indicates there is no improvement
                if (fin.Peek() != '_')
                {
                    MapTile.TileImprovement tileImprovement = (MapTile.TileImprovement)int.Parse(fin.ReadLine());
                    if (tileImprovement != MapTile.TileImprovement.None)
                    {
                        tile.improvementDirection = (Directions.Direction)int.Parse(fin.ReadLine());
                    }
                    MapTile.TileImprovement movingObj = (MapTile.TileImprovement)int.Parse(fin.ReadLine());
                    if (movingObj != MapTile.TileImprovement.None)
                    {
                        tile.movingObjDirection = (Directions.Direction)int.Parse(fin.ReadLine());

                        if (movingObj == MapTile.TileImprovement.Mouse)
                        {
                            placeMouse(tile.transform.position.x, tile.transform.position.z, tile.movingObjDirection);
                        }
                        else if (movingObj == MapTile.TileImprovement.Cat)
                        {
                            placeCat(tile.transform.position.x, tile.transform.position.z, tile.movingObjDirection);
                        }
                    }
                    tile.improvement = tileImprovement;
                    tile.movingObject = movingObj;
                }
                else
                {
                    tile.improvement = MapTile.TileImprovement.None;
                    tile.movingObject = MapTile.TileImprovement.None;

                    // Pass by the underscore
                    fin.ReadLine();
                }

                if (i == 0 || j == 0 || (i + j) % 2 == 0)
                {
                    int wallsValue = int.Parse(fin.ReadLine());
                    tile.walls.north = (wallsValue >> 0 & 1) == 1;
                    tile.walls.east = (wallsValue >> 1 & 1) == 1;
                    tile.walls.south = (wallsValue >> 2 & 1) == 1;
                    tile.walls.west = (wallsValue >> 3 & 1) == 1;
                }
            }
        }

        return true;
    }

    // Exports map data from the current game map to the open stream specified by "fout"
    public bool exportMap(StreamWriter fout)
    {
        if (mapTiles == null)
        {
            // There is currently no map to export
            Debug.LogWarning("Tried to export map while none exists.");
            return false;
        }

        fout.WriteLine(mapHeight);
        fout.WriteLine(mapWidth);

        for (int j = 0; j < mapHeight; ++j)
        {
            for (int i = 0; i < mapWidth; ++i)
            {
                MapTile tile = mapTiles[j, i];

                // We can combine these two into a single character if they are the same. Thus a completely blank default tile will be indicated simply by only an underscore
                // This actually only saves two characters in this sistuation though, as a blank tile before was two single digit numbers (each with a new line)
                // 2 characters per tile is actually pretty significant relatively though
                if (tile.improvement == MapTile.TileImprovement.None && tile.movingObject == MapTile.TileImprovement.None)
                {
                    fout.WriteLine('_');
                }
                else
                {
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
        string mapPath = Application.streamingAssetsPath + "/Maps/" + fileName + ".map";
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
        string mapPath = Application.streamingAssetsPath + "/Maps/" + fileName + ".map";
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
        Transform newMouse = Instantiate(mousePrefab, new Vector3(xPos, 0, zPos), mousePrefab.rotation, transform);
        Directions.rotate(ref newMouse, direction);
        newMouse.GetComponent<GridMovement>().direction = direction;

        if (mousePlaced != null)
        {
            mousePlaced(newMouse.gameObject);
        }

        return newMouse;
    }

    // Creates a cat object and returns its transform
    // The returned object should only be destroyed with the destroyCat function
    public Transform placeCat(float xPos, float zPos, Directions.Direction direction)
    {
        Transform catPrefab = GameResources.objects["Cat"];
        Transform newCat = Instantiate(catPrefab, new Vector3(xPos, 0, zPos), catPrefab.rotation, transform);
        Directions.rotate(ref newCat, direction);
        newCat.GetComponent<GridMovement>().direction = direction;

        return newCat;
    }

    // Sets the position of camera "cam" to be able to see the entire map
    public void setCameraView(Camera cam)
    {
        // There is probably a correct thing to do here, but just do whatever for now since map sizes will probably always be the same.

        const float scaleFactor = 4.25f;
        float cameraAngleAdjust = Mathf.Sin(cam.transform.eulerAngles.x * Mathf.Deg2Rad) * (scaleFactor / 2);

        // Because maps are currently always 12x9, don't bother doing extra calculations for for that case (this is probably worth the cost of the if statement)
        if (mapHeight == 9 && mapWidth == 12)
        {
            cam.transform.position = new Vector3(22.5f,
                                                 33.6f + cameraAngleAdjust,
                                                 10.5f - cameraAngleAdjust
                                                ) / scaleFactor; 
            return;
        }

        cam.transform.position = new Vector3((mapWidth * 1.5f) + (mapHeight * 0.5f),
                                             (mapWidth + mapHeight) * 1.6f + cameraAngleAdjust,
                                             (mapWidth * 0.5f) + (mapHeight * 0.5f) - cameraAngleAdjust
                                            ) / scaleFactor;
    }
}
