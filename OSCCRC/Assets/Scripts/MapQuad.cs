using System.Collections.Generic;
using UnityEngine;

public class MapQuad : MonoBehaviour {

    // TODO: Since resources aren't created yet, material resource locations aren't assigned. This must be done once materials created.

    // Material resource locations will have to be manually added and adjusted in the start function as tile improvements change
    public enum TileImprovement { None, Hole, Goal, Left, Right, Up, Down, Wrap }
    Dictionary<TileImprovement, string> improvementResources = new Dictionary<TileImprovement, string>();

    public class Walls
    {
        public bool north { get{ return m_walls[0] != null; } set{ changeWall(0, value); } }
        public bool east  { get{ return m_walls[1] != null; } set{ changeWall(1, value); } }
        public bool south { get{ return m_walls[2] != null; } set{ changeWall(2, value); } }
        public bool west  { get{ return m_walls[3] != null; } set{ changeWall(3, value); } }

        internal Vector3 origin = new Vector3();
        internal Transform[] m_walls = new Transform[4];

        internal static Transform parent;
        internal static Transform prefab;
        internal static Tile[,] mapTiles;

        private int maxHeight = mapTiles.Rank, maxWidth = mapTiles.GetLength(0);

        // Walls are shared by the two tiles they touch, so we must inform the other tile to accept the created wall as its own
        // How we create walls might change later, depending on how we want a potential editor to do wall placement. Right now, always place on tiles.
        // If we do add an editor, it would likely make sense to have a separate prefab for each wall direction. I'm not doing that now because I didn't.
        private void changeWall(int wallID, bool isCreating)
        {
            if (isCreating && m_walls[wallID] == null)
            {
                m_walls[wallID] = Instantiate(prefab, parent);

                if (wallID == 0)
                {
                    m_walls[wallID].position = origin + Vector3.forward * Tile.tileSize / 2;
                }
                else if (wallID == 1)
                {
                    m_walls[wallID].position = origin + Vector3.right * Tile.tileSize / 2;
                    m_walls[wallID].Rotate(Vector3.up * 90);
                }
                else if (wallID == 2)
                {
                    m_walls[wallID].position = origin + Vector3.back * Tile.tileSize / 2;
                }
                else if (wallID == 3)
                {
                    m_walls[wallID].position = origin + Vector3.left * Tile.tileSize / 2;
                    m_walls[wallID].Rotate(Vector3.up * 90);
                }
            }
            else if (!isCreating && m_walls[wallID] != null)
            {
                Destroy(m_walls[wallID].gameObject);
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
                if (otherTileZ > maxHeight)
                {
                    return;
                }
            }
            else if (wallID == 1)
            {
                ++otherTileX;
                if (otherTileX > maxWidth)
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
                mapTiles[otherTileX, otherTileZ].walls.m_walls[(wallID + 2) % 4] = m_walls[wallID];
            }
            else if (!isCreating)
            {
                mapTiles[otherTileX, otherTileZ].walls.m_walls[(wallID + 2) % 4] = null;
            }
        }
    }

    public class Tile
    {
        public TileImprovement improvement { get{ return m_improvement; } set{ m_improvement = value; setTileMaterial(); } }
        public Walls walls;

        public static float tileSize;

        internal static Transform parent;
        internal static Transform prefab;
        internal static Dictionary<TileImprovement, string> improvementResources;

        private TileImprovement m_improvement;
        private Transform m_tile;

        public Tile()
        {
            improvement = TileImprovement.None;
            walls = new Walls();
            m_tile = Instantiate(prefab, parent);
        }

        public Tile(int row, int column) : this ()
        {
            m_tile.position = new Vector3(row * tileSize, 0, column * tileSize);
            walls.origin = m_tile.position;
        }

        private void setTileMaterial()
        {
            if (m_tile == null)
            {
                return;
            }

            Material newMaterial = Resources.Load(improvementResources[m_improvement]) as Material;
            if (newMaterial)
            {
                m_tile.GetComponent<MeshRenderer>().material = newMaterial;
            }
        }
    }

    public int mapHeight = 9;
    public int mapWidth = 12;

    // Note that these prefabs must be treated as internal static, but are not marked as such so they can be assigned within Unity editor for now, but will be loaded in script later
    public Transform TilePrefab;
    public Transform WallPrefab;

    private Tile[,] mapTiles;

	void Start () {
        mapTiles = new Tile[mapHeight, mapWidth];

        improvementResources.Add(TileImprovement.None, "Materials/Tile(Yellow)");
        improvementResources.Add(TileImprovement.Hole, "Materials/Hole");
        improvementResources.Add(TileImprovement.Goal, "Materials/Hole");
        improvementResources.Add(TileImprovement.Left, "Materials/Hole");
        improvementResources.Add(TileImprovement.Right, "Materials/Hole");
        improvementResources.Add(TileImprovement.Up, "Materials/Hole");
        improvementResources.Add(TileImprovement.Down, "Materials/Hole");
        improvementResources.Add(TileImprovement.Wrap, "Materials/Hole");

        Tile.tileSize = TilePrefab.localScale.x;
        Tile.parent = GetComponent<Transform>();
        Tile.prefab = TilePrefab;
        Walls.parent = Tile.parent;
        Walls.prefab = WallPrefab;
        Walls.mapTiles = mapTiles;
        Tile.improvementResources = improvementResources;

        for (int j = 0; j < mapHeight; ++j)
        {
            for (int i = 0; i < mapWidth; ++i)
            {
                mapTiles[j, i] = new Tile(i, j);

                if (j == 0)
                {
                    mapTiles[j, i].walls.south = true;
                }
                if (i == 0)
                {
                    mapTiles[j, i].walls.west = true;
                }
                if (j == mapHeight - 1)
                {
                    mapTiles[j, i].walls.north = true;
                }
                if (i == mapWidth - 1)
                {
                    mapTiles[j, i].walls.east = true;
                }
            }
        }
	}
	
	// Only used temporarily for testing. This should be removed soon.
    ///*
	void Update () {
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            mapTiles[0, 0].improvement = TileImprovement.None;
        }
        else if (Input.GetKeyDown(KeyCode.Period))
        {
            mapTiles[0, 0].improvement = TileImprovement.Hole;
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
