using System.Collections.Generic;
using System.IO;
using UnityEngine;

// This class handles the game map, including the tiles, walls, and moving entities within.
// It will be used to create new map objects and to access them.

public class GameMap : MonoBehaviour
{
    public delegate void voidEvent();
    public delegate void objectEvent(GameObject caller);
    public event voidEvent mapLoaded;
    public event objectEvent mouseDestroyed;
    public event objectEvent mousePlaced;
    public event objectEvent catDestroyed;

    public int mapHeight { get { return m_mapHeight; } }
    public int mapWidth { get { return m_mapWidth; } }
    public float tileSize { get { return m_tileSize; } }


    // Returns the maptile info of the tile that the current GameMap-local coordinate is inside of
    public MapTile tileAt(Vector3 point)
    {
        return m_mapTiles[Mathf.RoundToInt(point.z / m_tileSize), Mathf.RoundToInt(point.x / m_tileSize)];
    }


    // Returns the maptile info of the tile that the current GameMap-local coordinate is inside of
    public MapTile tileAt(float y, float x)
    {
        return m_mapTiles[Mathf.RoundToInt(y / m_tileSize), Mathf.RoundToInt(x / m_tileSize)];
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
            newWall.localPosition += Vector3.forward * (m_tileSize / 2);
        }
        else if (direction == Directions.Direction.East)
        {
            newWall.localPosition += Vector3.right * (m_tileSize / 2);
        }
        else if (direction == Directions.Direction.South)
        {
            newWall.localPosition += Vector3.back * (m_tileSize / 2);
        }
        else if (direction == Directions.Direction.West)
        {
            newWall.localPosition += Vector3.left * (m_tileSize / 2);
        }
        Directions.rotate(ref newWall, direction);

        return newWall;
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


    // Removes a tile game object
    public void destroyTile(MapTile tile)
    {
        tile.walls.clear();
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
    // Comments documenting the file format can be found in the exportMap() function
    public bool importMap(StreamReader fin)
    {
        // First delete allocated game objects, since we are creating new ones
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

        if (newMapHeight != m_mapHeight || newMapWidth != m_mapWidth)
        {
            generateMap(newMapHeight, newMapWidth);
        }
        else
        {
            // We need to manually make sure we update the bigTile texture in case the resource pack changed
            m_bigTile.GetComponent<MeshRenderer>().material = GameResources.materials["TileTiledColor"];
        }

        for (int j = 0; j < m_mapHeight; ++j)
        {
            for (int i = 0; i < m_mapWidth; ++i)
            {
                MapTile tile = m_mapTiles[j, i];

                int tileFlags = int.Parse(fin.ReadLine());

                if ((tileFlags >> 0 & 1) == 1)
                {
                    MapTile.TileImprovement tileImprovement = (MapTile.TileImprovement)int.Parse(fin.ReadLine());
                    tile.improvementDirection = (Directions.Direction)int.Parse(fin.ReadLine());
                    tile.improvement = tileImprovement;
                }
                else
                {
                    tile.improvement = MapTile.TileImprovement.None;
                }

                if ((tileFlags >> 1 & 1) == 1)
                {
                    MapTile.TileImprovement movingObj = (MapTile.TileImprovement)int.Parse(fin.ReadLine());
                    tile.movingObjDirection = (Directions.Direction)int.Parse(fin.ReadLine());
                    tile.movingObject = movingObj;

                    if (movingObj == MapTile.TileImprovement.Mouse)
                    {
                        placeMouse(tile.transform.position.x, tile.transform.position.z, tile.movingObjDirection);
                    }
                    else if (movingObj == MapTile.TileImprovement.Cat)
                    {
                        placeCat(tile.transform.position.x, tile.transform.position.z, tile.movingObjDirection);
                    }
                }
                else
                {
                    tile.movingObject = MapTile.TileImprovement.None;
                }

                if ((tileFlags >> 2 & 1) == 1)
                {
                    tile.walls.north = (tileFlags >> 4 & 1) == 1;
                    tile.walls.east = (tileFlags >> 5 & 1) == 1;
                    tile.walls.south = (tileFlags >> 6 & 1) == 1;
                    tile.walls.west = (tileFlags >> 7 & 1) == 1;
                }
            }
        }

        if (mapLoaded != null)
        {
            mapLoaded();
        }
        return true;
    }


    // Exports map data from the current game map to the open stream specified by "fout"
    public bool exportMap(StreamWriter fout)
    {
        if (m_mapTiles == null)
        {
            // There is currently no map to export
            Debug.LogWarning("Tried to export map while none exists.");
            return false;
        }

        fout.WriteLine(m_mapHeight);
        fout.WriteLine(m_mapWidth);

        for (int j = 0; j < m_mapHeight; ++j)
        {
            for (int i = 0; i < m_mapWidth; ++i)
            {
                MapTile tile = m_mapTiles[j, i];

                // tileFlags holds all of the boolean values for the tile
                // Currently, the values each bit represents is as follows:
                // 0 0 0 0  0 0 0 0  0 0 0 0  0 0 0 0
                //                   \ \ / /  | | | TileImprovement
                //                      |     | | MovingObject
                //                      |     | HasWalls
                //                      |     Reserved-For-Future-Use    
                //                      WallsFlags(w,s,e,n)
                int tileFlags = 0;

                if (tile.improvement != MapTile.TileImprovement.None)
                {
                    tileFlags |= 1 << 0;
                }
                if (tile.movingObject != MapTile.TileImprovement.None)
                {
                    tileFlags |= 1 << 1;
                }
                if (i == 0 || j == 0 || (i + j) % 2 == 0)
                {
                    tileFlags |= 1 << 2;

                    if (tile.walls.north)
                    {
                        tileFlags |= 1 << 4;
                    }
                    if (tile.walls.east)
                    {
                        tileFlags |= 1 << 5;
                    }
                    if (tile.walls.south)
                    {
                        tileFlags |= 1 << 6;
                    }
                    if (tile.walls.west)
                    {
                        tileFlags |= 1 << 7;
                    }
                }
                fout.WriteLine(tileFlags);

                // Write remaining data if necessary
                if ((tileFlags >> 0 & 1) == 1)
                {
                    fout.WriteLine((int)tile.improvement);
                    fout.WriteLine((int)tile.improvementDirection);
                }
                if ((tileFlags >> 1 & 1) == 1)
                {
                    fout.WriteLine((int)tile.movingObject);
                    fout.WriteLine((int)tile.movingObjDirection);
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


    // Sets the position of camera "cam" to be able to see the entire map
    private void setCameraView(Camera cam)
    {
        // There is probably a correct thing to do here, but just do whatever for now since map sizes will probably always be the same.

        const float scaleFactor = 4.25f;
        float cameraAngleAdjust = Mathf.Sin(cam.transform.eulerAngles.x * Mathf.Deg2Rad) * (scaleFactor / 2);

        // Because maps are currently always 12x9, don't bother doing extra calculations for for that case (this is probably worth the cost of the if statement?)
        if (m_mapHeight == 9 && m_mapWidth == 12)
        {
            cam.transform.position = new Vector3(22.5f,
                                                 33.6f + cameraAngleAdjust,
                                                 10.5f - cameraAngleAdjust
                                                ) / scaleFactor; 
            return;
        }

        cam.transform.position = new Vector3((m_mapWidth * 1.5f) + (m_mapHeight * 0.5f),
                                             (m_mapWidth + m_mapHeight) * 1.6f + cameraAngleAdjust,
                                             (m_mapWidth * 0.5f) + (m_mapHeight * 0.5f) - cameraAngleAdjust
                                            ) / scaleFactor;
    }


    // Generates a new rectangular map of the specified width and height
    private void generateMap(int height, int width)
    {
        if (m_mapTiles != null)
        {
            // We need to delete the old map first
            for (int j = 0; j < m_mapHeight; ++j)
            {
                for (int i = 0; i < m_mapWidth; ++i)
                {
                    destroyTile(m_mapTiles[j, i]);
                }
            }
        }

        m_mapHeight = height; m_mapWidth = width;
        m_mapTiles = new MapTile[m_mapHeight, m_mapWidth];

        m_tileSize = GameResources.objects["Tile"].localScale.x;

        for (int j = 0; j < m_mapHeight; ++j)
        {
            for (int i = 0; i < m_mapWidth; ++i)
            {
                m_mapTiles[j, i] = createTile(i * m_tileSize, j * m_tileSize);
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
            m_bigTile.transform.localScale = new Vector3(m_mapWidth, m_mapHeight, 1.0f);
            m_bigTile.transform.position = new Vector3((m_mapWidth - 1.0f) / 2.0f, 0.0f, (m_mapHeight - 1.0f) / 2.0f);
            MeshRenderer bigTileRend = m_bigTile.GetComponent<MeshRenderer>();
            bigTileRend.material = GameResources.materials["TileTiledColor"];
            bigTileRend.material.mainTextureScale = new Vector2(m_mapWidth / 2.0f, m_mapHeight / 2.0f);

            bigTileRend.enabled = true;
        }

        setCameraView(Camera.main);
    }


    public int m_mapHeight = 0;
    public int m_mapWidth = 0;
    public float m_tileSize = 1;
    private MapTile[,] m_mapTiles = null;
    private Transform m_bigTile = null;
}
