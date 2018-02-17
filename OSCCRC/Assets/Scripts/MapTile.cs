using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    // Material resource names will have to be manually added and adjusted in the start function as tile improvements change
    public enum TileImprovement { None, Hole, Goal, Spawner, Direction, Mouse, Cat }

    public class Walls
    {
        public bool north { get { return m_walls[m_dirToInt[Directions.Direction.North]] != null; } set { changeWall(Directions.Direction.North, value); } }
        public bool east { get { return m_walls[m_dirToInt[Directions.Direction.East]] != null; } set { changeWall(Directions.Direction.East, value); } }
        public bool south { get { return m_walls[m_dirToInt[Directions.Direction.South]] != null; } set { changeWall(Directions.Direction.South, value); } }
        public bool west { get { return m_walls[m_dirToInt[Directions.Direction.West]] != null; } set { changeWall(Directions.Direction.West, value); } }

        internal Vector3 origin = new Vector3();

        internal static GameMap map;

        private Transform[] m_walls = new Transform[4];
        private int maxHeightIndex = map.mapHeight - 1, maxWidthIndex = map.mapWidth - 1;
        private static Dictionary<Directions.Direction, int> m_dirToInt = new Dictionary<Directions.Direction, int>();

        static Walls()
        {
            // Just in case someone wants to change the enum order, map the directions to an int to avoid possible breakage
            // This is a lot of extra stuff to do to avoid a problem that probably will never happen, but oh well.
            m_dirToInt.Add(Directions.Direction.North, 0);
            m_dirToInt.Add(Directions.Direction.East, 1);
            m_dirToInt.Add(Directions.Direction.South, 2);
            m_dirToInt.Add(Directions.Direction.West, 3);
        }

        // Walls are shared by the two tiles they touch, so we must inform the other tile to accept the created wall as its own
        private void changeWall(Directions.Direction wallID, bool isCreating)
        {
            int idNum = m_dirToInt[wallID];
            if (isCreating && m_walls[idNum] == null)
            {
                m_walls[idNum] = map.createWall(origin.x, origin.z, wallID);
            }
            else if (!isCreating && m_walls[idNum] != null)
            {
                map.destroyWall(m_walls[idNum]);
                m_walls[idNum] = null;
            }
            else
            {
                // Wall is not able to be created or destroyed
                return;
            }

            // Determine where the other wall is if it exists
            float otherTileX = origin.x, otherTileZ = origin.z;
            if (idNum == 0)
            {
                otherTileZ += map.tileSize;
                if (otherTileZ > (maxHeightIndex * map.tileSize))
                {
                    otherTileZ = 0;
                }
            }
            else if (idNum == 1)
            {
                otherTileX += map.tileSize;
                if (otherTileX > (maxWidthIndex * map.tileSize))
                {
                    otherTileX = 0;
                }
            }
            else if (idNum == 2)
            {
                otherTileZ -= map.tileSize;
                if (otherTileZ < 0)
                {
                    otherTileZ = maxHeightIndex * map.tileSize;
                }
            }
            else if (idNum == 3)
            {
                otherTileX -= map.tileSize;
                if (otherTileX < 0)
                {
                    otherTileX = maxWidthIndex * map.tileSize;
                }
            }

            // The companion wall always has the same existance state
            if (otherTileZ == (maxHeightIndex * map.tileSize) || otherTileX == (maxWidthIndex * map.tileSize) || otherTileZ == 0 || otherTileX == 0)
            {
                // Walls on edges share state with wrapped around tile, but they don't share the same wall object
                map.tileAt(new Vector3(otherTileX, 0, otherTileZ)).walls.changeWall(Directions.getOppositeDir(wallID), isCreating );
            }
            else
            {
                // This can be done on a single line if you don't hate the ugliness of the ternary operator like I do
                Transform wallSet = null;
                if (isCreating)
                {
                    wallSet = m_walls[idNum];
                }
                map.tileAt(new Vector3(otherTileX, 0, otherTileZ)).walls.m_walls[(idNum + 2) % 4] = wallSet;
            }
        }
    }


    public TileImprovement improvement { get { return m_improvement; } set { setTileImprovement(value); } }
    public TileImprovement movingObject { get { return m_movingObject; } set { m_movingObject = value; } }
    public Walls walls;
    public Directions.Direction improvementDirection { get { return m_improvementDir;  } set { m_improvementDir = value; Directions.rotate(ref m_tileObject, value); } }
    public Directions.Direction movingObjDirection { get { return m_movingDir; } set { m_movingDir = value; } }

    private static Dictionary<TileImprovement, string> m_improvementTextures = new Dictionary<TileImprovement, string>();
    private static Dictionary<TileImprovement, string> m_improvementObjects = new Dictionary<TileImprovement, string>();
    private TileImprovement m_improvement;
    private TileImprovement m_movingObject;
    private Directions.Direction m_improvementDir;
    private Directions.Direction m_movingDir;
    private Transform m_tileObject;

    static MapTile()
    {
        m_improvementTextures.Add(TileImprovement.None, "Tile");
        m_improvementTextures.Add(TileImprovement.Hole, "Hole");
        m_improvementTextures.Add(TileImprovement.Goal, "Goal");
        m_improvementObjects.Add(TileImprovement.Direction, "DirectionArrow");
    }

    public void initTile()
    {
        improvementDirection = Directions.Direction.North;
        movingObjDirection = Directions.Direction.North;
        m_tileObject = null;
        walls = new Walls();
        walls.origin = GetComponent<Transform>().position;
        m_tileObject = null;
        // workaround of ignoring attempts to set same tile improvement again, because prefab doesn't alternate the none tile
        m_improvement = TileImprovement.Hole;
        improvement = TileImprovement.None;
        movingObject = TileImprovement.None;
    }

    private void setTileImprovement(TileImprovement improvement)
    {
        if (improvement == TileImprovement.Mouse || improvement == TileImprovement.Cat)
        {
            // A tile only owns a mouse/cat in the context of saving and loading maps.
            m_movingObject = improvement;
            return;
        }
        if (improvement == m_improvement)
        {
            return;
        }

        string materialName = string.Empty;
        string objectName = string.Empty;

        if (m_improvementTextures.ContainsKey(improvement))
        {
            materialName = m_improvementTextures[improvement];
        }
        if (m_improvementObjects.ContainsKey(improvement))
        {
            objectName = m_improvementObjects[improvement];
        }

        if (improvement == TileImprovement.None || materialName == string.Empty)
        {
            // Since improvement textures aren't overlayed, we don't always want the same texture on an empty tile, so that we get a grid-like pattern
            if ((walls.origin.x + walls.origin.z) % 2 == 0)
            {
                materialName = "Tile";
            }
            else
            {
                materialName = "TileAlt";
            }
        }

		if (materialName != string.Empty)
        {
            if (GameResources.materials.ContainsKey(materialName))
            {
                GetComponent<MeshRenderer>().material = GameResources.materials[materialName];
            }
            else
            {
                Debug.LogWarning("Material " + materialName + " was not found!");
            }
        }

        if (m_tileObject != null)
        {
            Destroy(m_tileObject.gameObject);
            m_tileObject = null;
        }
        if (objectName != string.Empty)
        {
            if (GameResources.objects.ContainsKey(objectName))
            {
                m_tileObject = Instantiate(GameResources.objects[objectName], GetComponent<Transform>());
                Directions.rotate(ref m_tileObject, m_improvementDir);
            }
            else
            {
                Debug.LogWarning("Object Prefab " + objectName + " was not found!");
            }
        }

        m_improvement = improvement;
    }

}
