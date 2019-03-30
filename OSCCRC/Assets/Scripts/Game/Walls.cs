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

        m_walls.Add(Directions.Direction.North, null);
        m_walls.Add(Directions.Direction.East, null);
        m_walls.Add(Directions.Direction.South, null);
        m_walls.Add(Directions.Direction.West, null);
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
            m_walls[wallID] = m_map.createWall(m_origin.x, m_origin.z, wallID);
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

        // Determine where the other wall is if it exists
        float otherTileX = m_origin.x, otherTileZ = m_origin.z;
        if (wallID == Directions.Direction.North)
        {
            otherTileZ += m_map.tileSize;
            if (otherTileZ > (m_maxHeightIndex * m_map.tileSize))
            {
                otherTileZ = 0;
            }
        }
        else if (wallID == Directions.Direction.East)
        {
            otherTileX += m_map.tileSize;
            if (otherTileX > (m_maxWidthIndex * m_map.tileSize))
            {
                otherTileX = 0;
            }
        }
        else if (wallID == Directions.Direction.South)
        {
            otherTileZ -= m_map.tileSize;
            if (otherTileZ < 0)
            {
                otherTileZ = m_maxHeightIndex * m_map.tileSize;
            }
        }
        else if (wallID == Directions.Direction.West)
        {
            otherTileX -= m_map.tileSize;
            if (otherTileX < 0)
            {
                otherTileX = m_maxWidthIndex * m_map.tileSize;
            }
        }

        // The companion wall always has the same existence state
        if (Mathf.Abs(m_origin.z - otherTileZ) == (m_maxHeightIndex * m_map.tileSize) || Mathf.Abs(m_origin.x - otherTileX) == (m_maxWidthIndex * m_map.tileSize))
        {
            // Walls on edges share state with wrapped around tile, but they don't share the same wall object
            m_map.tileAt(new Vector3(otherTileX, 0, otherTileZ)).walls.changeWall(Directions.getOppositeDir(wallID), isCreating);
        }
        else
        {
            Transform wallSet = null;
            if (isCreating)
            {
                wallSet = m_walls[wallID];
            }
            m_map.tileAt(new Vector3(otherTileX, 0, otherTileZ)).walls.m_walls[Directions.getOppositeDir(wallID)] = wallSet;
        }
    }


    private Vector3 m_origin;
    private GameMap m_map;
    private readonly int m_maxHeightIndex, m_maxWidthIndex;
    private Dictionary<Directions.Direction, Transform> m_walls = new Dictionary<Directions.Direction, Transform>(4);
}
