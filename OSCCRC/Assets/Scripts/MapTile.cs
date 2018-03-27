using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    // Material resource names will have to be manually added and adjusted in the start function as tile improvements change
    public enum TileImprovement { None, Hole, Goal, Spawner, Direction, Mouse, Cat }

    public class Walls
    {
        public bool this[Directions.Direction wallID] { get { return m_walls[wallID] != null; } set { changeWall(wallID, value); } }
        // Probably should use indexer only and make the direction properties deprecated?
        public bool north { get { return m_walls[Directions.Direction.North] != null; } set { changeWall(Directions.Direction.North, value); } }
        public bool east { get { return m_walls[Directions.Direction.East] != null; } set { changeWall(Directions.Direction.East, value); } }
        public bool south { get { return m_walls[Directions.Direction.South] != null; } set { changeWall(Directions.Direction.South, value); } }
        public bool west { get { return m_walls[Directions.Direction.West] != null; } set { changeWall(Directions.Direction.West, value); } }

        private Vector3 origin;
        private GameMap map;
        private readonly int maxHeightIndex, maxWidthIndex;
        private Dictionary<Directions.Direction, Transform> m_walls= new Dictionary<Directions.Direction, Transform>(4);

        public Walls(GameMap parentMap, Vector3 originPos)
        {
            map = parentMap;
            origin = originPos;
            maxHeightIndex = map.mapHeight - 1;
            maxWidthIndex = map.mapWidth - 1;

            m_walls.Add(Directions.Direction.North, null);
            m_walls.Add(Directions.Direction.East, null);
            m_walls.Add(Directions.Direction.South, null);
            m_walls.Add(Directions.Direction.West, null);
        }

        // Walls are shared by the two tiles they touch, so we must inform the other tile to accept the created wall as its own
        private void changeWall(Directions.Direction wallID, bool isCreating)
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
            float otherTileX = origin.x, otherTileZ = origin.z;
            if (wallID == Directions.Direction.North)
            {
                otherTileZ += map.tileSize;
                if (otherTileZ > (maxHeightIndex * map.tileSize))
                {
                    otherTileZ = 0;
                }
            }
            else if (wallID == Directions.Direction.East)
            {
                otherTileX += map.tileSize;
                if (otherTileX > (maxWidthIndex * map.tileSize))
                {
                    otherTileX = 0;
                }
            }
            else if (wallID == Directions.Direction.South)
            {
                otherTileZ -= map.tileSize;
                if (otherTileZ < 0)
                {
                    otherTileZ = maxHeightIndex * map.tileSize;
                }
            }
            else if (wallID == Directions.Direction.West)
            {
                otherTileX -= map.tileSize;
                if (otherTileX < 0)
                {
                    otherTileX = maxWidthIndex * map.tileSize;
                }
            }

            // The companion wall always has the same existance state
            if (Mathf.Abs(origin.z - otherTileZ) == (maxHeightIndex * map.tileSize) || Mathf.Abs(origin.x - otherTileX) == (maxWidthIndex * map.tileSize))
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
                    wallSet = m_walls[wallID];
                }
                map.tileAt(new Vector3(otherTileX, 0, otherTileZ)).walls.m_walls[Directions.getOppositeDir(wallID)] = wallSet;
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
    private int m_tileDamage;

    static MapTile()
    {
        m_improvementTextures.Add(TileImprovement.None, "Tile");
        m_improvementTextures.Add(TileImprovement.Hole, "Hole");
        m_improvementTextures.Add(TileImprovement.Goal, "Goal");
        m_improvementObjects.Add(TileImprovement.Direction, "DirectionArrow");
    }

    public void initTile(GameMap parentMap)
    {
        improvementDirection = Directions.Direction.North;
        movingObjDirection = Directions.Direction.North;
        m_tileObject = null;
        walls = new Walls(parentMap, GetComponent<Transform>().position);
        m_tileObject = null;
        improvement = TileImprovement.None;
        movingObject = TileImprovement.None;
    }

    public void damageTile()
    {
        const int hitsToDestroy = 2;

        ++m_tileDamage;

        if (m_tileDamage < hitsToDestroy)
        {
            float newScale = ((hitsToDestroy - (m_tileDamage * 0.5f)) / (float)hitsToDestroy);
            m_tileObject.transform.localScale = new Vector3(newScale, newScale, 1);
        }
        else
        {
            improvement = TileImprovement.None;
        }
    }

    private void setTileImprovement(TileImprovement improvement)
    {
        if (improvement == TileImprovement.Mouse || improvement == TileImprovement.Cat)
        {
            // A tile only owns a mouse/cat in the context of saving and loading maps.
            Debug.LogWarning("setTileImprovement function should not be used for placing Moving Objects.");
            m_movingObject = improvement;
            return;
        }
        // We don't actually want to do an early return when the improvement is the same as current
        // This is because we might load a different resource pack, and want to re-apply the improvements to get the new version
        /*
        if (improvement == m_improvement)
        {
            return;
        }
        */

        m_tileDamage = 0;

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
            if ((transform.position.x + transform.position.z) % 2 == 0)
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
