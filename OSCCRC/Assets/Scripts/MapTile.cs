using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour {

    // TODO: Since resources aren't created yet, material resource locations aren't assigned. This must be done once materials created.

    // Material resource names will have to be manually added and adjusted in the start function as tile improvements change
    public enum TileImprovement { None, Hole, Goal, Left, Right, Up, Down, Wrap }
    public static Dictionary<TileImprovement, string> improvementResources = new Dictionary<TileImprovement, string>();

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


    public TileImprovement improvement { get { return m_improvement; } set { setTileMaterial(value); } }
    public Walls walls;

    private TileImprovement m_improvement;

    static MapTile()
    {
        improvementResources.Add(TileImprovement.None, "Tile");
        improvementResources.Add(TileImprovement.Hole, "Hole");
        improvementResources.Add(TileImprovement.Goal, "Hole");
        improvementResources.Add(TileImprovement.Left, "Hole");
        improvementResources.Add(TileImprovement.Right, "Hole");
        improvementResources.Add(TileImprovement.Up, "Hole");
        improvementResources.Add(TileImprovement.Down, "Hole");
        improvementResources.Add(TileImprovement.Wrap, "Hole");
    }

    public void initTile()
    {
        walls = new Walls();
        walls.origin = GetComponent<Transform>().position;
        improvement = TileImprovement.None;
    }

    private void setTileMaterial(TileImprovement improvement)
    {
        string materialPath = "";
        if (improvementResources.ContainsKey(improvement))
        {
            materialPath = improvementResources[improvement];
        }
        else
        {
            Debug.Log("Warning: Improvement " + improvement.ToString() + " was not assigned a resource name!");
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

        Material newMaterial = Resources.Load("Materials/" + materialPath) as Material;
        if (newMaterial)
        {
            GetComponent<MeshRenderer>().material = newMaterial;
        }
        else
        {
            Debug.Log("Warning: Material " + materialPath + " was not found!");
        }

        m_improvement = improvement;
    }

    // Use this for initialization
    void Start() {

    }

}
