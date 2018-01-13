using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour {

    // TODO: Since resources aren't created yet, material resource locations aren't assigned. This must be done once materials created.

        //  Fix walls to use GameMap

    // Material resource locations will have to be manually added and adjusted in the start function as tile improvements change
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
        // How we create walls might change later, depending on how we want a potential editor to do wall placement. Right now, always place on tiles.
        // If we do add an editor, it would likely make sense to have a separate prefab for each wall direction. I'm not doing that now because I didn't.
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
                map.mapTiles[otherTileX, otherTileZ].walls.m_walls[(wallID + 2) % 4] = m_walls[wallID];
            }
            else if (!isCreating)
            {
                map.mapTiles[otherTileX, otherTileZ].walls.m_walls[(wallID + 2) % 4] = null;
            }
        }
    }


    public TileImprovement improvement { get { return m_improvement; } set { m_improvement = value; setTileMaterial(); } }
    public Walls walls;

    public static float tileSize;

    private TileImprovement m_improvement;

    private static bool isStaticInitialized = false;

    public void initTile()
    {
        m_improvement = TileImprovement.None;
        walls = new Walls();
        walls.origin = GetComponent<Transform>().position;
    }

    private void setTileMaterial()
    {
        Material newMaterial = Resources.Load(improvementResources[m_improvement]) as Material;
        if (newMaterial)
        {
            GetComponent<MeshRenderer>().material = newMaterial;
        }
    }

    // Use this for initialization
    void Start() {
        if (!isStaticInitialized)
        {
            isStaticInitialized = true;
            improvementResources.Add(TileImprovement.None, "Materials/Tile(Yellow)");
            improvementResources.Add(TileImprovement.Hole, "Materials/Hole");
            improvementResources.Add(TileImprovement.Goal, "Materials/Hole");
            improvementResources.Add(TileImprovement.Left, "Materials/Hole");
            improvementResources.Add(TileImprovement.Right, "Materials/Hole");
            improvementResources.Add(TileImprovement.Up, "Materials/Hole");
            improvementResources.Add(TileImprovement.Down, "Materials/Hole");
            improvementResources.Add(TileImprovement.Wrap, "Materials/Hole");
        }
    }

}
