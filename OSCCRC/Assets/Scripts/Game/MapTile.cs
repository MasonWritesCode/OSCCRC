using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class allows interfacing with a tile that is located on the game map, usually accessing or changing the improvement placed onto it.

public class MapTile : MonoBehaviour
{
    // Material resource names will have to be manually added and adjusted in the start function as tile improvements change
    public enum TileImprovement { None, Hole, Goal, Spawner, Direction, Mouse, Cat }

    // This needs to be public so that the editor can see the texture used for an improvement
    public static Dictionary<TileImprovement, string> improvementTextures = new Dictionary<TileImprovement, string>();
    public static Dictionary<TileImprovement, string> improvementObjects = new Dictionary<TileImprovement, string>();

    public TileImprovement improvement { get { return m_improvement; } set { setTileImprovement(value); } }
    public TileImprovement movingObject { get { return m_movingObject; } set { m_movingObject = value; } }
    public Directions.Direction improvementDirection { get { return m_improvementDir;  } set { if (m_improvementDir != value) { Directions.rotate(m_tileObject, value, m_mapTransform); m_improvementDir = value; } } }
    public Directions.Direction movingObjDirection { get { return m_movingDir; } set { m_movingDir = value; } }
    public int owner { get { return m_owner; } set { if (m_owner != value) { m_owner = value; setTileImprovement(m_improvement); } } }
    public Walls walls;


    // This static constructor is used to generate a map used to interface with resource packs
    static MapTile()
    {
        improvementTextures.Add(TileImprovement.None, "Tile");
        improvementTextures.Add(TileImprovement.Hole, "Hole");
        improvementTextures.Add(TileImprovement.Goal, "Goal");
        improvementTextures.Add(TileImprovement.Spawner, "Spawner");

        improvementObjects.Add(TileImprovement.Direction, "DirectionArrow");
    }


    // Initializes this tile using data from "parentMap"
    // If I remember correctly, this is used instead of Start so that parentMap can be passed in
    public void initTile(GameMap parentMap)
    {
        m_rendererRef = GetComponent<MeshRenderer>();
        m_gameResources = parentMap.GetComponent<GameResources>();
        m_mapTransform = parentMap.transform; // We need this to rotate tile objects relative to map instead of relative to tile (which is currently rotated differently)

        m_tileObject = null;
        improvementDirection = Directions.Direction.North;
        movingObjDirection = Directions.Direction.North;
        improvement = TileImprovement.None;
        movingObject = TileImprovement.None;

        walls = new Walls(parentMap, transform.localPosition);
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

        // Doing early return here when the improvement (and material in case of resource pack change) is the same seems to make no difference to load speed
        // So avoid the added complexity of doing it for now in case it somehow causes an issue later, but leave it as comment in case it becomes useful or I am bad at measuring
        // Note: This would also need to be changed to check if the material changed on the tileobject as well, since I forgot to do that
        /*
        if (improvement == m_improvement)
        {
            if (   improvementTextures.ContainsKey(improvement)
                && m_rendererRef.sharedMaterial == m_gameResources.materials[improvementTextures[improvement]]
               )
            {
                return;
            }
        }
        */

        string materialName = null;
        string objectName = null;

        // Anything that doesn't have a player specific version should be placed as player 0
        if (improvementTextures.ContainsKey(improvement))
        {
            materialName = improvementTextures[improvement];
            if (m_owner != 0)
            {
                materialName += m_owner;
            }
        }
        if (improvementObjects.ContainsKey(improvement))
        {
            objectName = improvementObjects[improvement];
            if (m_owner != 0)
            {
                materialName += m_owner;
            }
        }

        // Set associated tile texture
        if (improvement == TileImprovement.None || materialName == null)
        {
            // We disable the renderer here instead of setting to Blank tile material, see GameMap for more info

            m_rendererRef.enabled = false;
        }
        else
        {
            if (m_gameResources.materials.ContainsKey(materialName))
            {
                m_rendererRef.material = m_gameResources.materials[materialName];

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

        // Spawn an object if there is one associated with the improvement
        if (m_tileObject != null)
        {
            Destroy(m_tileObject.gameObject);
            m_tileObject = null;
        }
        if (objectName != null)
        {
            if (m_gameResources.objects.ContainsKey(objectName))
            {
                m_tileObject = Instantiate(m_gameResources.objects[objectName], transform);
                if (improvement == TileImprovement.Direction)
                {
                    // Directional arrows are objects that are implemented as tile objects themselves, which can cause z-fighting.
                    // We avoid z-fighting issues for directional tile objects by placing slightly above. Mathf.Epsilon doesn't seem to be enough for this.
                    m_tileObject.localPosition = new Vector3(0.0f, 0.0f, -0.0001f);
                }

                Directions.rotate(m_tileObject, m_improvementDir, m_mapTransform);
            }
            else
            {
                Debug.LogWarning("Object Prefab " + objectName + " was not found!");
            }
        }

        m_improvement = improvement;
    }

    private TileImprovement m_improvement;
    private TileImprovement m_movingObject;
    private Directions.Direction m_improvementDir;
    private Directions.Direction m_movingDir;
    private int m_owner;
    private Transform m_tileObject;
    private Transform m_mapTransform;
    private int m_tileDamage;
    private MeshRenderer m_rendererRef;
    private GameResources m_gameResources;
}
