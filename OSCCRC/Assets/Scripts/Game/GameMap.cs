using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

// This class handles the game map, including the tiles, walls, and moving entities within.
// It will be used to create new map objects and to access them.

public class GameMap : MonoBehaviour
{
    public delegate void voidEvent();
    public event voidEvent mapLoaded;

    public int mapHeight { get { return m_mapHeight; } }
    public int mapWidth { get { return m_mapWidth; } }
    public float tileSize { get { return m_tileSize; } }

    public Transform ringPrefab;
    public Transform floorPrefab;


    void Awake()
    {
        m_gameResources = GetComponent<GameResources>();
        m_transform = transform;
        m_mainCamera = Camera.main.GetComponent<CameraController>();
    }


    // Returns the maptile info of the tile that the current GameMap-local coordinate is inside of
    public MapTile tileAt(Vector3 point)
    {
        int yIndex = Mathf.RoundToInt(point.z / m_tileSize);
        int xIndex = Mathf.RoundToInt(point.x / m_tileSize);
        if (yIndex < 0 || xIndex < 0 || yIndex >= mapHeight || xIndex >= mapWidth)
        {
            return null;
        }
        return m_mapTiles[yIndex, xIndex];
    }


    // Returns the maptile info of the tile that the current GameMap-local coordinate is inside of
    public MapTile tileAt(float y, float x)
    {
        int yIndex = Mathf.RoundToInt(y / m_tileSize);
        int xIndex = Mathf.RoundToInt(x / m_tileSize);
        if (yIndex < 0 || xIndex < 0 || yIndex >= mapHeight || xIndex >= mapWidth)
        {
            return null;
        }
        return m_mapTiles[yIndex, xIndex];
    }


    // Returns true if a tile is on the edge of the map, otherwise returns false
    public bool isEdgeTile(MapTile tile)
    {
        Vector3 tilePos = tile.transform.localPosition;
        int yIndex = Mathf.FloorToInt(tilePos.z / m_tileSize);
        int xIndex = Mathf.FloorToInt(tilePos.x / m_tileSize);

        return xIndex == 0 || xIndex == (mapWidth - 1) || yIndex == 0 || yIndex == (mapHeight - 1);
    }


    // Wraps a coordinate to always be a location on the GameMap
    public Vector3 wrapCoord(Vector3 coord)
    {
        float halfTileSize = (0.5f * tileSize);
        coord.x = (coord.x + halfTileSize) % (mapWidth * tileSize) - halfTileSize;
        coord.z = (coord.z + halfTileSize) % (mapHeight * tileSize) - halfTileSize;
        if (coord.x < -halfTileSize)
        {
            coord.x = mapWidth + coord.x;
        }
        if (coord.z < -halfTileSize)
        {
            coord.z = mapHeight + coord.z;
        }

        return coord;
    }


    // Creates a wall game object The position is map-relative.
    // The returned object should only be destroyed by calling the destroyWall function
    public Transform createWall(Vector3 position, Directions.Direction direction)
    {
        Transform wallPrefab = m_gameResources.objects["Wall"];
        Transform newWall = Instantiate(wallPrefab, m_transform);
        newWall.localPosition = position;

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
        Directions.rotate(newWall, direction);

        return newWall;
    }


    // Creates a mouse object and returns its transform. The position is map-relative.
    // The returned object should only be destroyed with the destroyMouse function instead of Unity's destroy
    public Transform placeMouse(Vector3 position, Directions.Direction direction)
    {
        Transform mousePrefab = m_gameResources.objects["Mouse"];
        Transform newMouse = Instantiate(mousePrefab, m_transform);
        newMouse.localPosition = position;
        Directions.rotate(newMouse, direction);
        newMouse.GetComponent<GridMovement>().direction = direction;

        return newMouse;
    }


    // Creates a big mouse object and returns its transform. The position is map-relative.
    // The returned object should only be destroyed with the destroyMouse function instead of Unity's destroy
    public Transform placeBigMouse(Vector3 position, Directions.Direction direction)
    {
        Transform mousePrefab = m_gameResources.objects["BigMouse"];
        Transform newMouse = Instantiate(mousePrefab, m_transform);
        newMouse.localPosition = position;
        Directions.rotate(newMouse, direction);
        newMouse.GetComponent<GridMovement>().direction = direction;

        return newMouse;
    }


    // Creates a cat object and returns its transform. The position is map-relative.
    // The returned object should only be destroyed with the destroyCat function instead of Unity's destroy
    public Transform placeCat(Vector3 position, Directions.Direction direction)
    {
        Transform catPrefab = m_gameResources.objects["Cat"];
        Transform newCat = Instantiate(catPrefab, m_transform);
        newCat.localPosition = position;
        Directions.rotate(newCat, direction);
        newCat.GetComponent<GridMovement>().direction = direction;

        return newCat;
    }


    // Removes a wall game object with transform "wall"
    public void destroyWall(Transform wall)
    {
        Destroy(wall.gameObject);
    }


    // Removes a Mouse game object with transform "mouse"
    // GameController's destroyMover should be used instead to account for the effect on game logic, this just deletes the object
    public void destroyMouse(Transform mouse)
    {
        //mouse.gameObject.SetActive(false);
        if (m_mainCamera.isAttached && mouse.GetComponentInChildren<Camera>())
        {
            m_mainCamera.setCameraOrthographic();
            m_mainCamera.setCameraView(this);
        }
        Destroy(mouse.gameObject);
    }


    // Removes a Cat game object with transform "cat"
    // GameController's destroyMover should be used instead to account for the effect on game logic, this just deletes the object
    public void destroyCat(Transform cat)
    {
        //cat.gameObject.SetActive(false);
        if (m_mainCamera.isAttached && cat.GetComponentInChildren<Camera>())
        {
            m_mainCamera.setCameraOrthographic();
            m_mainCamera.setCameraView(this);
        }
        Destroy(cat.gameObject);
    }


    // Draws a ring around the specified location on the map for the specified length of time to draw the user's attention.
    // Only one ring will be shown at a time for now, which is the most recent. Location is a map-relative position.
    public void pingLocation(Vector3 location, float timeInSeconds)
    {
        Transform prevRing = m_transform.Find("Ring(Clone)");
        if (prevRing != null)
        {
            Destroy(prevRing.gameObject);
        }

        Transform pingRing = Instantiate(ringPrefab, m_transform);
        pingRing.localPosition = new Vector3(location.x, 5.0f, location.z);
        Destroy(pingRing.gameObject, timeInSeconds);
    }


    // Imports map data to the current game map from the open stream specified by "fin"
    // Comments documenting the file format can be found in the exportMap function
    public bool importMap(StreamReader fin)
    {
        Profiler.BeginSample("GameMap.importMap");

        // First delete allocated game objects, since we are creating new ones
        // Removing walls will be handled at the same point in code where they are placed

        // We aren't currently keeping track of mice or cats, so destroy all children with a GridMovement attached
        GridMovement[] deadMeat = GetComponentsInChildren<GridMovement>();
        for (int i = 0; i < deadMeat.Length; ++i)
        {
            if (deadMeat[i] is Mouse)
            {
                destroyMouse(deadMeat[i].transform);
            }
            else if (deadMeat[i] is Cat)
            {
                destroyCat(deadMeat[i].transform);
            }
        }

        // Remove ping ring
        // If there is a nice way to find them all we might consider supporting multiple rings, but to be simple require one and use find for now
        Transform ring = m_transform.Find("Ring(Clone)");
        if (ring != null)
        {
            Destroy(ring.gameObject);
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
            // We need to manually make sure we update the floor texture in case the resource pack changed
            if (m_floor != null)
            {
                m_floor.GetComponent<MeshRenderer>().material = m_gameResources.materials["TileTiledColor"];
            }
        }

        for (int j = 0; j < m_mapHeight; ++j)
        {
            for (int i = 0; i < m_mapWidth; ++i)
            {
                MapTile tile = m_mapTiles[j, i];

                int tileFlags = int.Parse(fin.ReadLine());

                if ((tileFlags >> 3 & 1) == 1)
                {
                    tile.owner = int.Parse(fin.ReadLine());
                }

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
                        placeMouse(tile.transform.localPosition, tile.movingObjDirection);
                    }
                    else if (movingObj == MapTile.TileImprovement.Cat)
                    {
                        placeCat(tile.transform.localPosition, tile.movingObjDirection);
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

        Profiler.EndSample();

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

        Profiler.BeginSample("GameMap.exportMap");

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
                //                      |     Owner is not default  
                //                      WallsFlags(w,s,e,n)
                int tileFlags = 0;

                if (tile.owner != 0)
                {
                    tileFlags |= 1 << 3;
                }
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
                if ((tileFlags >> 3 & 1) == 1)
                {
                    fout.WriteLine(tile.owner);
                }
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

        Profiler.EndSample();

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


    // Creates a map tile. The position is map-relative.
    // The returned object should only be destroyed by calling the destroyTile function
    private MapTile createTile(float xPos, float zPos)
    {
        Transform tilePrefab = m_gameResources.objects["Tile"];
        Transform newTileTransform = Instantiate(tilePrefab, m_transform);
        // Height is being set to a small value for x_useBigTile, to prevent z-fighting with the big tile. Mathf.Epsilon doesn't seem to be enough for this.
        newTileTransform.localPosition = new Vector3(xPos, 0.0001f, zPos);
        MapTile newTile = newTileTransform.GetComponent<MapTile>();
        newTile.initTile(this);
        return newTile;
    }


    // Removes a tile game object
    private void destroyTile(MapTile tile)
    {
        tile.walls.clear();
        Destroy(tile.gameObject);
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

        m_tileSize = m_gameResources.objects["Tile"].localScale.x;

        for (int j = 0; j < m_mapHeight; ++j)
        {
            for (int i = 0; i < m_mapWidth; ++i)
            {
                m_mapTiles[j, i] = createTile(i * m_tileSize, j * m_tileSize);
            }
        }

        // Here we set a single stretched tile that represents all blank tiles
        // Since usually most tiles are blank, this saves draw calls by only enabling a tile's renderer when it is not blank
        if (m_floor == null)
        {
            m_floor = Instantiate(floorPrefab, m_transform);
        }

        // We need to set the scale to mapsize
        // Position needs to be set to ((mapsize - 1) / 2) (divided by two because scale stretches in both directions)
        // Material tiling has to be set to (mapsize / 2)
        m_floor.localScale = new Vector3(m_mapWidth, m_mapHeight, 1.0f);
        m_floor.localPosition = new Vector3((m_mapWidth - 1.0f) / 2.0f, 0.0f, (m_mapHeight - 1.0f) / 2.0f);
        MeshRenderer bigTileRend = m_floor.GetComponent<MeshRenderer>();
        bigTileRend.material = m_gameResources.materials["TileTiledColor"];
        bigTileRend.material.mainTextureScale = new Vector2(m_mapWidth / 2.0f, m_mapHeight / 2.0f);

        m_mainCamera.setCameraView(this);
    }


    private int m_mapHeight = 0;
    private int m_mapWidth = 0;
    private float m_tileSize = 1;
    private MapTile[,] m_mapTiles = null;
    private Transform m_floor = null;
    private Transform m_transform;
    private GameResources m_gameResources;
    private CameraController m_mainCamera;
}
