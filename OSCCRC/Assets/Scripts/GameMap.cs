using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameMap : MonoBehaviour
{

    public int mapHeight = 9;
    public int mapWidth = 12;
    public float tileSize;

    // These must be considered static for now, because I am treating the size of tiles as pulled from the prefab as static in Tile class
    public Transform TilePrefab;
    public Transform WallPrefab;

    public Transform MousePrefab;

    private Transform mapTransform;
    private MapTile[,] mapTiles;

    // create tile and create wall functions?

    void Start()
    {
        mapTiles = new MapTile[mapHeight, mapWidth];

        tileSize = TilePrefab.localScale.x;
        MapTile.Walls.map = this;

        mapTransform = GetComponent<Transform>();
        // We have to initialize all tiles before we can set any walls
        for (int j = 0; j < mapHeight; ++j)
        {
            for (int i = 0; i < mapWidth; ++i)
            {
                mapTiles[j, i] = createTile(i, j);
            }
        }
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

        //added for testing purposes. To be removed
        placeMouse(3, 2, GridMovement.Directions.north);
        placeMouse(3, 2, GridMovement.Directions.east);
        placeMouse(4, 5, GridMovement.Directions.south);
        placeMouse(6, 2, GridMovement.Directions.west);
        mapTiles[8, 5].walls.east = true;
        mapTiles[8, 5].walls.south = true;
        mapTiles[0, 4].walls.south = false;
        mapTiles[5, 0].walls.west = false;
        //
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6))
        {
            exportMap("dev");
        }
        else if (Input.GetKeyDown(KeyCode.F7))
        {
            importMap("dev");
        }
    }

    public MapTile tileAt(Vector3 point)
    {
        return mapTiles[Mathf.RoundToInt(point.z / tileSize), Mathf.RoundToInt(point.x / tileSize)];
    }

    public MapTile createTile(float xPos, float zPos)
    {
        Transform newTileTransform = Instantiate(TilePrefab, new Vector3(xPos * tileSize, 0, zPos * tileSize), TilePrefab.rotation, mapTransform);
        MapTile newTile = newTileTransform.gameObject.AddComponent<MapTile>();
        newTile.initTile();
        return newTile;
    }

    public Transform createWall(float xPos, float zPos, int wallID)
    {
        Transform newWall = Instantiate(WallPrefab, new Vector3(xPos * tileSize, 0, zPos * tileSize), WallPrefab.rotation, mapTransform);

        if (wallID == 0)
        {
            newWall.position += Vector3.forward * tileSize / 2;
        }
        else if (wallID == 1)
        {
            newWall.position += Vector3.right * tileSize / 2;
            newWall.Rotate(Vector3.up * 90);
        }
        else if (wallID == 2)
        {
            newWall.position += Vector3.back * tileSize / 2;
        }
        else if (wallID == 3)
        {
            newWall.position += Vector3.left * tileSize / 2;
            newWall.Rotate(Vector3.up * 90);
        }

        return newWall;
    }

    public void destroyWall(Transform wall)
    {
        Destroy(wall.gameObject);
    }

    public void importMap(string fileName)
    {
        using (StreamReader fin = new StreamReader(Application.dataPath + "/Maps/" + fileName + ".map"))
        {
            int versionNumber;
            bool recognizedVers = int.TryParse(fin.ReadLine(), out versionNumber);

            // For now, assume if this is good, everything else is good too
            if (!recognizedVers || versionNumber != 1)
            {
                Debug.Log("Failed to read map file " + fileName);
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
            mapTiles = new MapTile[mapHeight, mapWidth];

            tileSize = TilePrefab.localScale.x;
            MapTile.Walls.map = this;

            mapTransform = GetComponent<Transform>();
            // We have to initialize all tiles before we can set any walls
            for (int j = 0; j < mapHeight; ++j)
            {
                for (int i = 0; i < mapWidth; ++i)
                {
                    mapTiles[j, i] = createTile(i, j);
                }
            }
            for (int j = 0; j < mapHeight; ++j)
            {
                for (int i = 0; i < mapWidth; ++i)
                {
                    MapTile.TileImprovement tileImprovement = (MapTile.TileImprovement)int.Parse(fin.ReadLine());
                    if (tileImprovement == MapTile.TileImprovement.Mouse)
                    {
                        placeMouse(mapTiles[j, i].transform.position.x, mapTiles[j, i].transform.position.z, (GridMovement.Directions)int.Parse(fin.ReadLine()));
                        tileImprovement = MapTile.TileImprovement.None;
                    }
                    else if (tileImprovement == MapTile.TileImprovement.Cat)
                    {
                        // We aren't placing cats yet
                        GridMovement.Directions unused = (GridMovement.Directions)int.Parse(fin.ReadLine());
                        tileImprovement = MapTile.TileImprovement.None;
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

        Debug.Log(Application.dataPath);
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
                        fout.WriteLine((int)tile.directionID);
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

    public void placeMouse(float xPos, float zPos, GridMovement.Directions direction)
    {
        Transform newMouse = Instantiate(MousePrefab, new Vector3(xPos * tileSize, 0, zPos * tileSize), MousePrefab.rotation, mapTransform);
        newMouse.GetComponent<GridMovement>().direction = direction;
    }
}
