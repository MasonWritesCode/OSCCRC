using System.Collections.Generic;
using UnityEngine;

public class GameMap : MonoBehaviour {

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

    void Start () {
        mapTiles = new MapTile[mapHeight, mapWidth];

        tileSize = TilePrefab.localScale.x;
        MapTile.Walls.map = this;

        mapTransform = GetComponent<Transform>();
        for (int j = 0; j < mapHeight; ++j)
        {
            for (int i = 0; i < mapWidth; ++i)
            {
                mapTiles[j, i] = createTile(i, j);

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
		placeMouse ( 3, 2, GridMovement.Directions.north);
		placeMouse ( 3, 2, GridMovement.Directions.east);
		placeMouse ( 4, 5, GridMovement.Directions.south);
		placeMouse ( 6, 2, GridMovement.Directions.west);
		mapTiles[8,5].walls.east = true;
		mapTiles[8,5].walls.south = true;
		//
	}

    public MapTile tileAt(Vector3 point)
    {
		return mapTiles[(int)Mathf.Round(point.z / tileSize), (int)Mathf.Round(point.x / tileSize)];
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

	public void placeMouse(float xPos, float zPos, GridMovement.Directions direction)
	{
		Transform newMouse = Instantiate (MousePrefab, new Vector3 (xPos * tileSize, 0, zPos * tileSize), MousePrefab.rotation, mapTransform);
		newMouse.GetComponent<GridMovement>().direction = direction;
	}
}
