using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class allows interfacing with a tile that is located on the game map, usually accessing or changing the improvement placed onto it.

public class MapTile : MonoBehaviour
{
    // Material resource names will have to be manually added and adjusted in the start function as tile improvements change
    public enum TileImprovement { None, Hole, Goal, Spawner, Direction, Mouse, Cat }

    // Class that holds wall information that will be associated with a tile.
    // This is within the MapTile class because only a tile should access walls directly.
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
        private Dictionary<Directions.Direction, Transform> m_walls = new Dictionary<Directions.Direction, Transform>(4);

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

    // This needs to be public so that the editor can see the texture used for an improvement
    public static Dictionary<TileImprovement, string> improvementTextures = new Dictionary<TileImprovement, string>();
    public static Dictionary<TileImprovement, string> improvementObjects = new Dictionary<TileImprovement, string>();

    private TileImprovement m_improvement;
    private TileImprovement m_movingObject;
    private Directions.Direction m_improvementDir;
    private Directions.Direction m_movingDir;
    private Transform m_tileObject;
    private int m_tileDamage;
    private MeshRenderer m_rendererRef;


    // This static constructor is used to generate a map used to interface with resource packs
    static MapTile()
    {
        improvementTextures.Add(TileImprovement.None, "Tile");
        improvementTextures.Add(TileImprovement.Hole, "Hole");
        improvementTextures.Add(TileImprovement.Goal, "Goal");
        improvementObjects.Add(TileImprovement.Direction, "DirectionArrow");
    }


    // Initializes this tile using data from "parentMap"
    // If I remember correctly, this is used instead of Start so that parentMap can be passed in
    public void initTile(GameMap parentMap)
    {
        improvementDirection = Directions.Direction.North;
        movingObjDirection = Directions.Direction.North;
        m_tileObject = null;
        walls = new Walls(parentMap, GetComponent<Transform>().localPosition);
        m_tileObject = null;
        improvement = TileImprovement.None;
        movingObject = TileImprovement.None;

        m_rendererRef = GetComponent<MeshRenderer>();
        if (m_rendererRef)
        {
            m_rendererRef.enabled = !GlobalData.x_useBigTile;
        }
    }


    // Damages the improvement placed onto a tile, possibly destroying it
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


    // Places a new improvement onto this tile
    private void setTileImprovement(TileImprovement improvement)
    {
        if (improvement == TileImprovement.Mouse || improvement == TileImprovement.Cat)
        {
            Debug.LogWarning("setTileImprovement function should not be used for placing Moving Objects.");
            m_movingObject = improvement;
            return;
        }

        m_tileDamage = 0;

        // We don't actually want to do an early return when the improvement is the same as current
        // This is because we might load a different resource pack, and want to re-apply the improvements to get the new version
        /*
        if (improvement == m_improvement)
        {
            return;
        }
        */

        string materialName = null;
        string objectName = null;

        if (improvementTextures.ContainsKey(improvement))
        {
            materialName = improvementTextures[improvement];
        }
        if (improvementObjects.ContainsKey(improvement))
        {
            objectName = improvementObjects[improvement];
        }

        // Set associated tile texture
        if (GlobalData.x_useBigTile)
        {
            if (improvement == TileImprovement.None || materialName == null)
            {
                // We disable the renderer here instead of setting to Blank tile material
                // See GameMap for more info

                if (m_rendererRef)
                {
                    m_rendererRef.enabled = false;
                }
            }
            else
            {
                if (GameResources.materials.ContainsKey(materialName))
                {
                    m_rendererRef.material = GameResources.materials[materialName];

                    if (!m_rendererRef.enabled)
                    {
                        m_rendererRef.enabled = true;
                    }
                }
                else
                {
                    Debug.LogWarning("Material " + materialName + " was not found!");
                }
            }
        }
        else
        {
            if (improvement == TileImprovement.None || materialName == null)
            {
                // Since improvement textures aren't overlayed, we don't always want the same texture on an empty tile, so that we get a grid-like pattern
                if ((transform.localPosition.x + transform.localPosition.z) % 2 == 0)
                {
                    materialName = "Tile";
                }
                else
                {
                    materialName = "TileAlt";
                }
            }

            if (materialName != null)
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
        }

        // Spawn an object if there is one associated with the improvement
        if (m_tileObject != null)
        {
            Destroy(m_tileObject.gameObject);
            m_tileObject = null;
        }
        if (objectName != null)
        {
            if (GameResources.objects.ContainsKey(objectName))
            {
                m_tileObject = Instantiate(GameResources.objects[objectName], GetComponent<Transform>());
                if (improvement == TileImprovement.Direction)
                {
                    // Directional arrows are objects that are implemented as tile objects themselves, which can cause z-fighting.
                    // We avoid z-fighting issues for directional tile objects by placing slightly above. Mathf.Epsilon doesn't seem to be enough for this.
                    m_tileObject.localPosition = new Vector3(0.0f, 0.0f, -0.00001f);
                }
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
