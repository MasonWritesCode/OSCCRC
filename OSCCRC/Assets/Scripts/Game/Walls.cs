using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class that holds wall information that will be associated with a tile.

public class Walls
{
    public bool this[Directions.Direction wallID] { get { return m_walls[wallID] != null; } set { changeWall(wallID, value); } }
    // Probably should use indexer only and make the direction properties deprecated?
    public bool north { get { return m_walls[Directions.Direction.North] != null; } set { changeWall(Directions.Direction.North, value); } }
    public bool east { get { return m_walls[Directions.Direction.East] != null; } set { changeWall(Directions.Direction.East, value); } }
    public bool south { get { return m_walls[Directions.Direction.South] != null; } set { changeWall(Directions.Direction.South, value); } }
    public bool west { get { return m_walls[Directions.Direction.West] != null; } set { changeWall(Directions.Direction.West, value); } }


    public Walls(GameMap parentMap, Vector3 originPos)
    {
        m_map = parentMap;
        m_origin = originPos;
        m_maxHeightIndex = m_map.mapHeight - 1;
        m_maxWidthIndex = m_map.mapWidth - 1;

        m_walls = new Dictionary<Directions.Direction, Transform>(4, new Directions.DirectionComparer()){
            { Directions.Direction.North, null },
            { Directions.Direction.East, null },
            { Directions.Direction.South, null },
            { Directions.Direction.West, null }
        };
    }


    // Removes all four walls if they exist
    public void clear()
    {
        foreach (Directions.Direction key in m_walls.Keys)
        {
            if (m_walls[key] != null)
            {
                m_map.destroyWall(m_walls[key]);
                m_walls[key] = null;
            }
        }
    }


    // Walls are shared by the two tiles they touch, so we must inform the other tile to accept the created wall as its own
    private void changeWall(Directions.Direction wallID, bool isCreating)
    {
        if (isCreating && m_walls[wallID] == null)
        {
            m_walls[wallID] = m_map.createWall(m_origin, wallID);
        }
        else if (!isCreating && m_walls[wallID] != null)
        {
            m_map.destroyWall(m_walls[wallID]);
            m_walls[wallID] = null;
        }
        else
        {
            // Wall is not able to be created or destroyed
            return;
        }

        // We set the companion wall to the same state if it exists
        Vector3 companionTilePos = m_origin + (Directions.toVector(wallID) * m_map.tileSize);
        MapTile companionTile = m_map.tileAt(companionTilePos);
        if (companionTile == null)
        {
            // Walls on opposite edges share state with wrapped around tile, but they don't share the same wall object
            m_map.tileAt(m_map.wrapCoord(companionTilePos)).walls.changeWall(Directions.getOppositeDir(wallID), isCreating);
        }
        else
        {
            // Walls that are not on edges share the same wall object
            companionTile.walls.m_walls[Directions.getOppositeDir(wallID)] = m_walls[wallID];
        }
    }


    private Vector3 m_origin;
    private GameMap m_map;
    private readonly int m_maxHeightIndex, m_maxWidthIndex;
    private Dictionary<Directions.Direction, Transform> m_walls;
}
