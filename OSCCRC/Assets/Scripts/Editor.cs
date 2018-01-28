using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Editor : MonoBehaviour {

    // TODO: Finish after doing necessary refactoring first
    //       Make controllable via UI instead of arbitrary keys
    //       Add abilty to change map size (even if it never makes it to editor UI)
    //       Allow map saving and loading with input name (needs ui)

    private enum ObjectType { None, Wall, Improvement, MovingObject }

    private Transform m_placeholderObject;
    private MapTile.TileImprovement m_selectedImprovement;
    private ObjectType m_placeholderType;
    private GridMovement.Directions m_direction;
    private GameMap m_gameMap;
    private PlayerController m_controls;
    private Vector3 m_positionOffset;

    void OnDisable() {
        if (m_placeholderObject != null)
        {
            Destroy(m_placeholderObject.gameObject);
            m_placeholderObject = null;
        }
    }

	// Use this for initialization
	void Start () {
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
        m_controls = GameObject.FindWithTag("Player").GetComponentInChildren<PlayerController>();
        m_placeholderType = ObjectType.None;
        m_selectedImprovement = MapTile.TileImprovement.None;
        m_placeholderObject = null;
        m_direction = GridMovement.Directions.east;
        m_positionOffset = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
        MapTile selectedTile = m_controls.currentTile;

        // I'm not familiar with UI in Unity, so select what you want to place with buttons for now until UI gets set up.
        ObjectType newType = ObjectType.None;
        MapTile.TileImprovement newImprovement = MapTile.TileImprovement.None;
        GridMovement.Directions newDir = m_direction;

        // Keys to select which improvement
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            // Wall North
            newType = ObjectType.Wall;
            newDir = GridMovement.Directions.north;
        }
        if (Input.GetKeyDown(KeyCode.Period))
        {
            // Wall East
            newType = ObjectType.Wall;
            newDir = GridMovement.Directions.east;
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            // Wall South
            newType = ObjectType.Wall;
            newDir = GridMovement.Directions.south;
        }
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            // Wall West
            newType = ObjectType.Wall;
            newDir = GridMovement.Directions.west;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            // Hole
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.Hole;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            // Goal
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.Goal;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            // Spawner
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.Spawner;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            // Left
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.Left;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            // Right
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.Right;
        }
        if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
        {
            // Up
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.Up;
        }
        if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
        {
            // Down
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.Down;
        }
        if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
        {
            // Mouse
            newType = ObjectType.MovingObject;
            newImprovement = MapTile.TileImprovement.Mouse;
        }
        if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
        {
            // Cat
            newType = ObjectType.MovingObject;
            newImprovement = MapTile.TileImprovement.Cat;
        }
        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            // Blank tile
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.None;
        }


        if (newType != ObjectType.None)
        {
            if (m_placeholderObject != null)
            {
                Destroy(m_placeholderObject.gameObject);
                m_placeholderObject = null;
            }

            // If reselecting same object "put it away" instead so that no object is selected for placement
            if (m_placeholderType == newType && m_selectedImprovement == newImprovement && m_direction == newDir)
            {
                m_placeholderType = ObjectType.None;
                m_selectedImprovement = MapTile.TileImprovement.None;
            }
            else
            {
                m_placeholderType = newType;
                m_selectedImprovement = newImprovement;
                m_direction = newDir;
                m_positionOffset = Vector3.zero;

                // Instantiate placeholder and set its material
                if (m_placeholderType == ObjectType.Wall)
                {
                    m_placeholderObject = m_gameMap.createWall(0, 0, m_direction);
                    m_positionOffset = m_placeholderObject.position;
                }
                else if (m_placeholderType == ObjectType.Improvement)
                {
                    // TODO: Need to use associated object, and tile if there isn't an associated object
                    m_placeholderObject = m_gameMap.createTile(0, 0).transform;
                    m_placeholderObject.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                }
                else if (m_placeholderType == ObjectType.MovingObject)
                {
                    // TODO
                    // Need to map improvement to resource like maptile, but don't want to copy from maptile or make public...
                    // m_placeholderObject = Instantiate(GameResources.objects[m_selectedImprovement]);
                }

                m_placeholderObject.GetComponent<MeshRenderer>().material = GameResources.materials["Placeholder"];
            }
        }


        if (m_placeholderObject != null)
        {
            // Follow mouse precisely unless there is a tile to snap to
            if (selectedTile != null)
            {
                m_placeholderObject.position = selectedTile.transform.position + m_positionOffset;
            }
            else
            {
                m_placeholderObject.position = Input.mousePosition + m_positionOffset;
            }
        }


        // Place object. We currently toggle the selected type between choice and false/none
        // We can change it to have right click remove and left click place, but I don't know which is better.
        if (Input.GetButtonDown("Select") && selectedTile != null)
        {
            if (m_placeholderType == ObjectType.Wall)
            {
                if (m_direction == GridMovement.Directions.north)
                {
                    selectedTile.walls.north = !selectedTile.walls.north;
                }
                if (m_direction == GridMovement.Directions.east)
                {
                    selectedTile.walls.east = !selectedTile.walls.east;
                }
                if (m_direction == GridMovement.Directions.south)
                {
                    selectedTile.walls.south = !selectedTile.walls.south;
                }
                if (m_direction == GridMovement.Directions.west)
                {
                    selectedTile.walls.west = !selectedTile.walls.west;
                }
            }
            else if (m_placeholderType == ObjectType.Improvement || m_placeholderType == ObjectType.MovingObject)
            {
                if (selectedTile.improvement == m_selectedImprovement)
                {
                    selectedTile.improvement = MapTile.TileImprovement.None;
                }
                else
                {
                    selectedTile.improvement = m_selectedImprovement;
                }

                if (m_placeholderType == ObjectType.MovingObject)
                {
                    // TODO: Need to map rotation to a direction
                }
            }
        }


        // Save map
        if (Input.GetKeyDown(KeyCode.F6))
        {
            m_gameMap.exportMap("dev");
        }
        else if (Input.GetKeyDown(KeyCode.F7))
        {
            m_gameMap.importMap("dev");
        }
    }
}
