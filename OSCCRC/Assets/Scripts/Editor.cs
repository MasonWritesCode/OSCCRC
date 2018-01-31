using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Editor : MonoBehaviour {

    // TODO: Make controllable via UI instead of arbitrary keys
    //       Allow map saving and loading with input name (needs ui)

    private enum ObjectType { None, Wall, Improvement }

    private Transform m_placeholderObject;
    private MapTile.TileImprovement m_selectedImprovement;
    private ObjectType m_placeholderType;
    private Directions.Direction m_direction;
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
        m_direction = Directions.Direction.East;
        m_positionOffset = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
        MapTile selectedTile = m_controls.currentTile;

        // I'm not familiar with UI in Unity, so select what you want to place with buttons for now until UI gets set up.
        ObjectType newType = ObjectType.None;
        MapTile.TileImprovement newImprovement = MapTile.TileImprovement.None;
        Directions.Direction newDir = m_direction;

        // Keys to select which improvement
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            // Wall North
            newType = ObjectType.Wall;
            newDir = Directions.Direction.North;
        }
        if (Input.GetKeyDown(KeyCode.Period))
        {
            // Wall East
            newType = ObjectType.Wall;
            newDir = Directions.Direction.East;
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            // Wall South
            newType = ObjectType.Wall;
            newDir = Directions.Direction.South;
        }
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            // Wall West
            newType = ObjectType.Wall;
            newDir = Directions.Direction.West;
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
            newDir = Directions.Direction.West;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            // Right
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.Right;
            newDir = Directions.Direction.East;
        }
        if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
        {
            // Up
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.Up;
            newDir = Directions.Direction.North;
        }
        if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
        {
            // Down
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.Down;
            newDir = Directions.Direction.South;
        }
        if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
        {
            // Mouse
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.Mouse;
        }
        if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
        {
            // Cat
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.Cat;
        }
        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            // Blank tile
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.None;
        }

        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
        {
            newDir = Directions.nextClockwiseDir(m_direction);
            if (newType == ObjectType.None)
            {
                newType = m_placeholderType;
            }
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            newDir = Directions.nextCounterClockwiseDir(m_direction);
            if (newType == ObjectType.None)
            {
                newType = m_placeholderType;
            }
        }

        // Allow rotating arrow tiles to get different tiles.
        // Kind of annoying because we treat directional tiles as separate improvements
        if (   newType == ObjectType.Improvement
            && (   m_selectedImprovement == MapTile.TileImprovement.Left
                || m_selectedImprovement == MapTile.TileImprovement.Right
                || m_selectedImprovement == MapTile.TileImprovement.Up
                || m_selectedImprovement == MapTile.TileImprovement.Down
               )
           )
        {
            if (newDir == Directions.Direction.North)
            {
                newImprovement = MapTile.TileImprovement.Up;
            }
            else if (newDir == Directions.Direction.East)
            {
                newImprovement = MapTile.TileImprovement.Right;
            }
            else if (newDir == Directions.Direction.South)
            {
                newImprovement = MapTile.TileImprovement.Down;
            }
            else if (newDir == Directions.Direction.West)
            {
                newImprovement = MapTile.TileImprovement.Left;
            }
        }


        if (newType != ObjectType.None)
        {
            if (m_placeholderObject != null)
            {
                Destroy(m_placeholderObject.gameObject);
                m_placeholderObject = null;
                m_placeholderType = ObjectType.None;
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
                    MapTile placeholderTile = m_placeholderObject.GetComponent<MapTile>();
                    placeholderTile.improvement = m_selectedImprovement;
                }

                // We want to use a material with transparent render mode, but use the same texture as the object we are creating
                MeshRenderer matr = m_placeholderObject.GetComponent<MeshRenderer>();
                Texture tex = matr.material.mainTexture;
                matr.material = GameResources.materials["Placeholder"];
                matr.material.mainTexture = tex;
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
                Ray posRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane floor = new Plane(Vector3.up, Vector3.zero);
                float distance;
                if (floor.Raycast(posRay, out distance))
                {
                    m_placeholderObject.position = posRay.GetPoint(distance) + m_positionOffset;
                }
            }
        }


        // Place object. We currently toggle the selected type between choice and false/none
        // We can change it to have right click remove and left click place, but I don't know which is better.
        if (Input.GetButtonDown("Select") && selectedTile != null)
        {
            if (m_placeholderType == ObjectType.Wall)
            {
                if (m_direction == Directions.Direction.North)
                {
                    selectedTile.walls.north = !selectedTile.walls.north;
                }
                if (m_direction == Directions.Direction.East)
                {
                    selectedTile.walls.east = !selectedTile.walls.east;
                }
                if (m_direction == Directions.Direction.South)
                {
                    selectedTile.walls.south = !selectedTile.walls.south;
                }
                if (m_direction == Directions.Direction.West)
                {
                    selectedTile.walls.west = !selectedTile.walls.west;
                }
            }
            else if (m_placeholderType == ObjectType.Improvement)
            {
                selectedTile.direction = m_direction;

                if (selectedTile.improvement == m_selectedImprovement)
                {
                    selectedTile.improvement = MapTile.TileImprovement.None;
                }
                else
                {
                    selectedTile.improvement = m_selectedImprovement;
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
