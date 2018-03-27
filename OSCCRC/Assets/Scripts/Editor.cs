﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Editor : MonoBehaviour {

    // TODO: Make placeholder creation safe for object pooling
    //       Make controllable via UI instead of arbitrary keys
    //           (all inputs are in update function so you only have to look in there)
    //       Allow map saving and loading with input name (needs ui)
    //       Add ability to adjust speed when unpaused (wait for ui?)

    private enum ObjectType { None, Wall, Improvement }

    private Transform m_placeholderObject;
    private MapTile.TileImprovement m_selectedImprovement;
    private ObjectType m_placeholderType;
    private Directions.Direction m_direction;
    private Vector3 m_positionOffset;

    private Dictionary<MapTile, Transform> m_movingObjects = new Dictionary<MapTile, Transform>();
    private GameMap m_gameMap;
    private GameStage m_gameStage;
    private PlayerController m_controls;
    private GameController m_gameControl;
    private bool m_wasUnpaused;
    private readonly Plane m_floorPlane = new Plane(Vector3.up, Vector3.zero);

    void OnDisable() {
        if (m_placeholderObject != null)
        {
            Destroy(m_placeholderObject.gameObject);
            m_placeholderObject = null;
        }
    }

	void Start () {
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
        m_gameStage = GameObject.FindWithTag("GameController").GetComponent<GameStage>();
        m_controls = GameObject.FindWithTag("Player").GetComponentInChildren<PlayerController>();
        m_gameControl = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        m_placeholderType = ObjectType.None;
        m_selectedImprovement = MapTile.TileImprovement.None;
        m_placeholderObject = null;
        m_direction = Directions.Direction.East;
        m_positionOffset = Vector3.zero;
        m_wasUnpaused = false;

        m_gameMap.saveMap("_editorAuto");
    }

	void Update () {
        MapTile selectedTile = m_controls.currentTile;

        // I'm not familiar with UI in Unity, so select what you want to place with buttons for now until UI gets set up.
        ObjectType newType = ObjectType.None;
        MapTile.TileImprovement newImprovement = MapTile.TileImprovement.None;
        Directions.Direction newDir = m_direction;

        // Keys to select which improvement
        // TODO: UI
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
            newImprovement = MapTile.TileImprovement.Direction;
            newDir = Directions.Direction.West;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            // Right
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.Direction;
            newDir = Directions.Direction.East;
        }
        if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
        {
            // Up
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.Direction;
            newDir = Directions.Direction.North;
        }
        if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
        {
            // Down
            newType = ObjectType.Improvement;
            newImprovement = MapTile.TileImprovement.Direction;
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
            if (newImprovement == MapTile.TileImprovement.None)
            {
                newImprovement = m_selectedImprovement;
            }
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            newDir = Directions.nextCounterClockwiseDir(m_direction);
            if (newType == ObjectType.None)
            {
                newType = m_placeholderType;
            }
            if (newImprovement == MapTile.TileImprovement.None)
            {
                newImprovement = m_selectedImprovement;
            }
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
                    if (m_selectedImprovement == MapTile.TileImprovement.Mouse)
                    {
                        m_placeholderObject = Instantiate(GameResources.objects["Mouse"]);
                        m_placeholderObject.GetComponent<GridMovement>().enabled = false;
                        Directions.rotate(ref m_placeholderObject, m_direction);
                    }
                    else if (m_selectedImprovement == MapTile.TileImprovement.Cat)
                    {
                        m_placeholderObject = Instantiate(GameResources.objects["Cat"]);
                        m_placeholderObject.GetComponent<GridMovement>().enabled = false;
                        Directions.rotate(ref m_placeholderObject, m_direction);
                    }
                    else
                    {
                        m_placeholderObject = m_gameMap.createTile(0, 0).transform;
                        m_placeholderObject.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                        MapTile placeholderTile = m_placeholderObject.GetComponent<MapTile>();
                        placeholderTile.improvementDirection = m_direction;
                        placeholderTile.improvement = m_selectedImprovement;
                    }
                }

                // We want to use a material with transparent render mode, but use the same texture as the object we are creating
                // This currently doesn't work with mice and cats because they aren't a single mesh and I'm not writing code to recursively check for sub-objects right now
                MeshRenderer matr = m_placeholderObject.GetComponent<MeshRenderer>();
                if (matr)
                {
                    Texture tex = matr.material.mainTexture;
                    matr.material = GameResources.materials["Placeholder"];
                    matr.material.mainTexture = tex;
                }
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
                float distance;
                if (m_floorPlane.Raycast(posRay, out distance))
                {
                    m_placeholderObject.position = posRay.GetPoint(distance) + m_positionOffset;
                }
            }
        }


        // Place object if paused. We currently toggle the selected type between choice and false/none
        // We can change it to have right click remove and left click place, but I don't know if that is better.
        if (Input.GetButtonDown("Select") && selectedTile != null && m_gameControl.isPaused)
        {
            placeObject(selectedTile);
        }

        if (!m_gameControl.isPaused)
        {
            if (!m_wasUnpaused)
            {
                m_wasUnpaused = true;

                // Don't leave anything selected when unpausing
                if (m_placeholderObject != null)
                {
                    Destroy(m_placeholderObject.gameObject);
                    m_placeholderObject = null;
                }
                m_placeholderType = ObjectType.None;
                m_selectedImprovement = MapTile.TileImprovement.None;

                m_gameMap.saveMap("_editorAuto");
            }
        }
        else if (m_wasUnpaused)
        {
            m_wasUnpaused = false;
            m_gameMap.loadMap("_editorAuto");
            mapMovingObjToTile(m_gameMap);
        }


        // For now just hotkey a save to "dev" until we have a UI that lets you choose save name
        // TODO: UI
        if (Input.GetKeyDown(KeyCode.F6))
        {
            createSave("dev");
        }
        else if (Input.GetKeyDown(KeyCode.F7))
        {
            loadSave("dev");
        }
    }


    private void createSave(string saveName)
    {
        m_gameStage.saveStage(saveName);
    }


    private void loadSave(string saveName)
    {
        m_gameStage.loadStage(saveName);
        mapMovingObjToTile(m_gameMap);

        // Go ahead and save to the loaded state
        m_gameMap.saveMap("_editorAuto");
    }


    private void placeObject(MapTile selectedTile)
    {
        if (m_placeholderType == ObjectType.Wall)
        {
            selectedTile.walls[m_direction] = !selectedTile.walls[m_direction];
        }
        else if (m_placeholderType == ObjectType.Improvement)
        {
            if (m_selectedImprovement == MapTile.TileImprovement.Mouse || m_selectedImprovement == MapTile.TileImprovement.Cat)
            {
                if (selectedTile.movingObject == m_selectedImprovement && selectedTile.movingObjDirection == m_direction)
                {
                    selectedTile.movingObject = MapTile.TileImprovement.None;
                }
                else
                {
                    selectedTile.movingObjDirection = m_direction;
                    selectedTile.movingObject = m_selectedImprovement;

                    if (selectedTile.improvement != MapTile.TileImprovement.None)
                    {
                        selectedTile.improvement = MapTile.TileImprovement.None;
                    }
                }
            }
            else
            {
                if (selectedTile.improvement == m_selectedImprovement && selectedTile.improvementDirection == m_direction)
                {
                    selectedTile.improvement = MapTile.TileImprovement.None;

                    if (m_selectedImprovement == MapTile.TileImprovement.Direction)
                    {
                        // We will probably change this to have the GameController pull placed direction tiles into available placements
                        // instead of saving them here. But I'll leave this here for now, at the very least for testing purposes.
                        m_gameStage.availablePlacements.AddLast(m_direction);
                    }
                }
                else
                {
                    selectedTile.improvementDirection = m_direction;
                    selectedTile.improvement = m_selectedImprovement;

                    if (selectedTile.improvement != MapTile.TileImprovement.Direction)
                    {
                        // We can only have a mouse or cat in addition to direction tiles
                        selectedTile.movingObject = MapTile.TileImprovement.None;
                    }
                    else
                    {
                        // We will probably change this to have the GameController pull placed direction tiles into available placements
                        // instead of saving them here. But I'll leave this here for now, at the very least for testing purposes.
                        m_gameStage.availablePlacements.Remove(m_direction);
                    }
                }
            }

            // If a mouse or cat was placed, we have to do special actions to remove it
            if (m_movingObjects.ContainsKey(selectedTile))
            {
                if (m_movingObjects[selectedTile] != null)
                {
                    Destroy(m_movingObjects[selectedTile].gameObject);
                }
                m_movingObjects.Remove(selectedTile);
            }

            if (selectedTile.movingObject == MapTile.TileImprovement.Mouse || selectedTile.movingObject == MapTile.TileImprovement.Cat)
            {
                Transform newMovingObj = null;
                if (selectedTile.movingObject == MapTile.TileImprovement.Mouse)
                {
                    newMovingObj = m_gameMap.placeMouse(selectedTile.transform.position.x, selectedTile.transform.position.z, m_direction);
                }
                else if (selectedTile.movingObject == MapTile.TileImprovement.Cat)
                {
                    newMovingObj = m_gameMap.placeCat(selectedTile.transform.position.x, selectedTile.transform.position.z, m_direction);
                }
                m_movingObjects.Add(selectedTile, newMovingObj);
            }
        }
    }


    private void mapMovingObjToTile(GameMap map)
    {
        m_movingObjects.Clear();

        List<GridMovement> movingObjs = new List<GridMovement>();
        map.GetComponentsInChildren<GridMovement>(true, movingObjs);
        foreach (GridMovement i in movingObjs)
        {
            // Should be destroyed by map import, but seems to still be around?
            // Checking if isActiveAndEnabled seems to work. 
            if (i.isActiveAndEnabled)
            {
                m_movingObjects.Add(map.tileAt(i.transform.position), i.transform);
            }
        }
    }
}
