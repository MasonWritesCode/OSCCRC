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
    public Directions.Direction improvementDirection { get { return m_improvementDir;  } set { m_improvementDir = value; Directions.rotate(ref m_tileObject, value); } }
    public Directions.Direction movingObjDirection { get { return m_movingDir; } set { m_movingDir = value; } }
    public Walls walls;


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
        improvement = TileImprovement.None;
        movingObject = TileImprovement.None;
        improvementDirection = Directions.Direction.North;
        movingObjDirection = Directions.Direction.North;
        walls = new Walls(parentMap, transform.localPosition);
        m_tileObject = null;

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
                m_tileObject = Instantiate(GameResources.objects[objectName], transform);
                if (improvement == TileImprovement.Direction)
                {
                    // Directional arrows are objects that are implemented as tile objects themselves, which can cause z-fighting.
                    // We avoid z-fighting issues for directional tile objects by placing slightly above. Mathf.Epsilon doesn't seem to be enough for this.
                    m_tileObject.localPosition = new Vector3(0.0f, 0.0f, -0.0001f);
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

    private TileImprovement m_improvement;
    private TileImprovement m_movingObject;
    private Directions.Direction m_improvementDir;
    private Directions.Direction m_movingDir;
    private Transform m_tileObject;
    private int m_tileDamage;
    private MeshRenderer m_rendererRef;
}
