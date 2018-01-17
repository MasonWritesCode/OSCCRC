using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{

    // TODO: Since resources aren't created yet, material resource locations aren't assigned. This must be done once materials created.

    // Material resource names will have to be manually added and adjusted in the start function as tile improvements change
    public enum TileImprovement { None, Hole, Goal, Spawner, Left, Right, Up, Down, Mouse, Cat }

    public class Walls
    {
        public bool north { get { return m_walls[0] != null; } set { changeWall(0, value); } }
        public bool east { get { return m_walls[1] != null; } set { changeWall(1, value); } }
        public bool south { get { return m_walls[2] != null; } set { changeWall(2, value); } }
        public bool west { get { return m_walls[3] != null; } set { changeWall(3, value); } }

        internal Vector3 origin = new Vector3();
        internal Transform[] m_walls = new Transform[4];

        internal static GameMap map;

        private int maxHeightIndex = map.mapHeight - 1, maxWidthIndex = map.mapWidth - 1;

        // Walls are shared by the two tiles they touch, so we must inform the other tile to accept the created wall as its own
        private void changeWall(int wallID, bool isCreating)
        {
            if (isCreating && m_walls[wallID] == null)
            {
                m_walls[wallID] = map.createWall(origin.x, origin.z, wallID);
            }
            else if (!isCreating && m_walls[wallID] != null)
            {
                map.destroyWall(m_walls[wallID]);
                m_walls[wallID] = null;
            }
            else
            {
                // Wall is not able to be created or destroyed
                return;
            }

            // Determine where the other wall is if it exists
            int otherTileX = (int)origin.x, otherTileZ = (int)origin.z;
            if (wallID == 0)
            {
                ++otherTileZ;
                if (otherTileZ > maxHeightIndex)
                {
                    return;
                }
            }
            else if (wallID == 1)
            {
                ++otherTileX;
                if (otherTileX > maxWidthIndex)
                {
                    return;
                }
            }
            else if (wallID == 2)
            {
                --otherTileZ;
                if (otherTileZ < 0)
                {
                    return;
                }
            }
            else if (wallID == 3)
            {
                --otherTileX;
                if (otherTileX < 0)
                {
                    return;
                }
            }

            // The companion wall always has the same existance state
            if (isCreating)
            {
                map.tileAt(new Vector3(otherTileX, 0, otherTileZ)).walls.m_walls[(wallID + 2) % 4] = m_walls[wallID];
            }
            else if (!isCreating)
            {
                map.tileAt(new Vector3(otherTileX, 0, otherTileZ)).walls.m_walls[(wallID + 2) % 4] = null;
            }
        }
    }


    public TileImprovement improvement { get { return m_improvement; } set { setTileImprovement(value); } }
    public Walls walls;

    // relevant for mice and cats only, for use in saving and loading maps
    internal GridMovement.Directions directionID = GridMovement.Directions.east;

    private static Dictionary<TileImprovement, string> m_improvementTextures = new Dictionary<TileImprovement, string>();
    private static Dictionary<TileImprovement, string> m_improvementObjects = new Dictionary<TileImprovement, string>();
    private TileImprovement m_improvement;
    private GameObject m_tileObject;

    static MapTile()
    {
        m_improvementTextures.Add(TileImprovement.None, "Tile");
        m_improvementTextures.Add(TileImprovement.Hole, "Hole");
        m_improvementTextures.Add(TileImprovement.Left, "Hole");
        m_improvementTextures.Add(TileImprovement.Right, "Hole");
        m_improvementTextures.Add(TileImprovement.Up, "Hole");
        m_improvementTextures.Add(TileImprovement.Down, "Hole");
    }

    public void initTile()
    {
        m_tileObject = null;
        walls = new Walls();
        walls.origin = GetComponent<Transform>().position;
        m_tileObject = null;
        // workaround of ignoring attempts to set same tile improvement again, because prefab doesn't alternate the none tile
        m_improvement = TileImprovement.Hole;
        improvement = TileImprovement.None;
    }

    private void setTileImprovement(TileImprovement improvement)
    {
        if (improvement == m_improvement)
        {
            return;
        }

        string materialPath = string.Empty;
        string objectPath = string.Empty;

        if (m_improvementTextures.ContainsKey(improvement))
        {
            materialPath = m_improvementTextures[improvement];
        }
        if (m_improvementObjects.ContainsKey(improvement))
        {
            objectPath = m_improvementObjects[improvement];
        }
        if (improvement == TileImprovement.None)
        {
            // Since improvement textures aren't overlayed, we don't always want the same texture on an empty tile, so that we get a grid-like pattern
            if ((walls.origin.x + walls.origin.z) % 2 == 0)
            {
                materialPath = "Tile";
            }
            else
            {
                materialPath = "TileAlt";
            }
        }

        if (materialPath != string.Empty)
        {
            Material newMaterial = Resources.Load("Materials/" + materialPath) as Material;
            if (newMaterial)
            {
                GetComponent<MeshRenderer>().material = newMaterial;
            }
            else
            {
                Debug.Log("Warning: Material " + materialPath + " was not found!");
            }
        }

        if (m_tileObject != null)
        {
            Destroy(m_tileObject);
            m_tileObject = null;
        }
        if (objectPath != string.Empty)
        {
            GameObject newObject = Resources.Load("Prefabs/" + objectPath) as GameObject;
            if (newObject)
            {
                m_tileObject = Instantiate(newObject, GetComponent<Transform>());
            }
            else
            {
                Debug.Log("Warning: Object Prefab " + objectPath + " was not found!");
            }
        }

        m_improvement = improvement;
    }

}
