﻿using System.Collections.Generic;
using UnityEngine;

public class GameMap : MonoBehaviour {

    public int mapHeight = 9;
    public int mapWidth = 12;
    public MapTile[,] mapTiles;

    // These must be considered static for now, because I am treating the size of tiles as pulled from the prefab as static.
    public Transform TilePrefab;
    public Transform WallPrefab;

    private Transform mapTransform;

    // create tile and create wall functions?

    void Start () {
        mapTiles = new MapTile[mapHeight, mapWidth];

        MapTile.tileSize = TilePrefab.localScale.x;
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
	}

    public MapTile createTile(float xPos, float zPos)
    {
        Transform newTileTransform = Instantiate(TilePrefab, new Vector3(xPos * MapTile.tileSize, 0, zPos * MapTile.tileSize), TilePrefab.rotation, mapTransform);
        MapTile newTile = newTileTransform.gameObject.AddComponent<MapTile>();
        newTile.initTile();
        return newTile;
    }

    public Transform createWall(float xPos, float zPos, int wallID)
    {
        Transform newWall = Instantiate(WallPrefab, new Vector3(xPos * MapTile.tileSize, 0, zPos * MapTile.tileSize), WallPrefab.rotation, mapTransform);

        if (wallID == 0)
        {
            newWall.position += Vector3.forward * MapTile.tileSize / 2;
        }
        else if (wallID == 1)
        {
            newWall.position += Vector3.right * MapTile.tileSize / 2;
            newWall.Rotate(Vector3.up * 90);
        }
        else if (wallID == 2)
        {
            newWall.position += Vector3.back * MapTile.tileSize / 2;
        }
        else if (wallID == 3)
        {
            newWall.position += Vector3.left * MapTile.tileSize / 2;
            newWall.Rotate(Vector3.up * 90);
        }

        return newWall;
    }

    public void destroyWall(Transform wall)
    {
        Destroy(wall.gameObject);
    }

    // Only used temporarily for testing. This should be removed soon.
    ///*
    void Update () {
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            mapTiles[0, 0].improvement = MapTile.TileImprovement.None;
        }
        else if (Input.GetKeyDown(KeyCode.Period))
        {
            mapTiles[0, 0].improvement = MapTile.TileImprovement.Hole;
        }

        if (Input.GetKeyDown(KeyCode.Semicolon))
        {
            mapTiles[0, 0].walls.north = !mapTiles[0, 0].walls.north;
        }
        else if (Input.GetKeyDown(KeyCode.Quote))
        {
            mapTiles[0, 0].walls.west = !mapTiles[0, 0].walls.west;
        }
    }
    //*/
}
