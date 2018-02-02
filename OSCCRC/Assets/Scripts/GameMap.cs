using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameMap : MonoBehaviour
{
    public int mapHeight = 9;
    public int mapWidth = 12;
    public float tileSize;

    private Transform mapTransform;
    private MapTile[,] mapTiles;

    void createMap(int height, int width)
    {
        mapHeight = height; mapWidth = width;
        mapTiles = new MapTile[mapHeight, mapWidth];

        tileSize = GameResources.objects["Tile"].localScale.x;
        MapTile.Walls.map = this;

        mapTransform = GetComponent<Transform>();
        for (int j = 0; j < mapHeight; ++j)
        {
            for (int i = 0; i < mapWidth; ++i)
            {
                mapTiles[j, i] = createTile(i, j);
            }
        }
    }

    public MapTile tileAt(Vector3 point)
    {
        return mapTiles[Mathf.RoundToInt(point.z / tileSize), Mathf.RoundToInt(point.x / tileSize)];
    }

    public MapTile createTile(float xPos, float zPos)
    {
        Transform tilePrefab = GameResources.objects["Tile"];
        Transform newTileTransform = Instantiate(tilePrefab, new Vector3(xPos * tileSize, 0, zPos * tileSize), tilePrefab.rotation, mapTransform);
        MapTile newTile = newTileTransform.gameObject.AddComponent<MapTile>();
        newTile.initTile();
        return newTile;
    }

    public Transform createWall(float xPos, float zPos, Directions.Direction direction)
    {
        Transform wallPrefab = GameResources.objects["Wall"];
        Transform newWall = Instantiate(wallPrefab, new Vector3(xPos * tileSize, 0, zPos * tileSize), wallPrefab.rotation, mapTransform);

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

    public void destroyWall(Transform wall)
    {
        Destroy(wall.gameObject);
    }

    public void importMap(string fileName)
    {
        string mapPath = Application.dataPath + "/Maps/" + fileName + ".map";
        if (!File.Exists(mapPath))
        {
            Debug.LogWarning("Tried to load a map but it wasn't found! " + mapPath);
            return;
        }
        using (StreamReader fin = new StreamReader(mapPath))
        {
            int versionNumber;
            bool recognizedVers = int.TryParse(fin.ReadLine(), out versionNumber);

            // For now, assume if this is good, everything else is good too
            if (!recognizedVers || versionNumber != 1)
            {
                Debug.LogWarning("Failed to read map file " + fileName);
                return;
            }

            // Delete allocated game objects, since we are creating new ones
            for (int j = 0; j < mapHeight; ++j)
            {
                for (int i = 0; i < mapWidth; ++i)
                {
                    mapTiles[j, i].walls.north = mapTiles[j, i].walls.east = mapTiles[j, i].walls.south = mapTiles[j, i].walls.west = false;
                    Destroy(mapTiles[j, i].gameObject);
                    mapTiles[j, i] = null;
                }
            }
            mapTiles = null;
            // We aren't currently keeping track of mice or cats, so destroy all children with a GridMovement attached
            List<GridMovement> deadMeat = new List<GridMovement>();
            GetComponentsInChildren<GridMovement>(true, deadMeat);
            foreach (GridMovement i in deadMeat)
            {
                Destroy(i.gameObject);
            }

            // Set new map values
            mapHeight = int.Parse(fin.ReadLine());
            mapWidth = int.Parse(fin.ReadLine());

            createMap(mapHeight, mapWidth);

            for (int j = 0; j < mapHeight; ++j)
            {
                for (int i = 0; i < mapWidth; ++i)
                {
                    MapTile.TileImprovement tileImprovement = (MapTile.TileImprovement)int.Parse(fin.ReadLine());
                    if (tileImprovement == MapTile.TileImprovement.Mouse)
                    {
                        placeMouse(mapTiles[j, i].transform.position.x, mapTiles[j, i].transform.position.z, (Directions.Direction)int.Parse(fin.ReadLine()));
                    }
                    else if (tileImprovement == MapTile.TileImprovement.Cat)
                    {
                        // We aren't placing cats yet
                        placeCat(mapTiles[j, i].transform.position.x, mapTiles[j, i].transform.position.z, (Directions.Direction)int.Parse(fin.ReadLine()));
                    }
                    mapTiles[j, i].improvement = tileImprovement;

                    int wallsValue = int.Parse(fin.ReadLine());
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

    public void exportMap(string fileName)
    {
        const int versionNumber = 1;

        using (StreamWriter fout = new StreamWriter(Application.dataPath + "/Maps/" + fileName + ".map", false))
        {
            fout.WriteLine(versionNumber);
            fout.WriteLine(mapHeight);
            fout.WriteLine(mapWidth);

            for (int j = 0; j < mapHeight; ++j)
            {
                for (int i = 0; i < mapWidth; ++i)
                {
                    MapTile tile = mapTiles[j, i];

                    // We currently can hold both the tile improvement and walls value in a single digit int each
                    // We should combine these together into a single two-digit number later in case we add more tile improvements
                    MapTile.TileImprovement tileImprovement = tile.improvement;
                    fout.WriteLine((int)tileImprovement);
                    if (tileImprovement == MapTile.TileImprovement.Mouse || tileImprovement == MapTile.TileImprovement.Cat)
                    {
                        fout.WriteLine((int)tile.direction);
                    }

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
    }

    public Transform placeMouse(float xPos, float zPos, Directions.Direction direction)
    {
        Transform mousePrefab = GameResources.objects["Mouse"];
        Transform newMouse = Instantiate(mousePrefab, new Vector3(xPos * tileSize, 0, zPos * tileSize), mousePrefab.rotation, mapTransform);
        Directions.rotate(ref newMouse, direction);
        newMouse.GetComponent<GridMovement>().direction = direction;

        return newMouse;
    }

    public Transform placeCat(float xPos, float zPos, Directions.Direction direction)
    {
        Transform catPrefab = GameResources.objects["Cat"];
        Transform newCat = Instantiate(catPrefab, new Vector3(xPos * tileSize, 0, zPos * tileSize), catPrefab.rotation, mapTransform);
        Directions.rotate(ref newCat, direction);
        newCat.GetComponent<GridMovement>().direction = direction;

        return newCat;
    }

    void Start()
    {
        bool useImportedMap = false;
        if (useImportedMap)
        {
            importMap("Dev");
        }
        else
        {
            // mapHeight and mapWidth are initialized in the Unity editor
            createMap(mapHeight, mapWidth);

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
