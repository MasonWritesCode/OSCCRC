﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class controls the map building functionality of the game, allowing creation of a new Game Map and play testing it.

public class Editor : MonoBehaviour {

    void OnDisable() {
        disablePlaceholder();
    }

	void Start () {
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
        m_gameResources = m_gameMap.GetComponent<GameResources>();
        m_gameControl = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        m_gameStage = m_gameControl.GetComponent<GameStage>();
        m_controls = GameObject.FindWithTag("Player").GetComponentInChildren<PlayerController>();
        m_mainCamera = Camera.main;

        m_placeholderType = ObjectType.None;
        m_selectedImprovement = MapTile.TileImprovement.None;
        m_placeholderObject = Instantiate(m_gameResources.objects["Placeholder"], m_gameMap.transform);
        disablePlaceholder();
        m_direction = Directions.Direction.East;
        m_positionOffset = Vector3.zero;

        m_gameControl.gameState.mainStateChange += onStateChange;
        m_gameMap.mapLoaded += onMapLoaded;
    }

	void Update () {
        MapTile selectedTile = m_controls.currentTile;

        ObjectType newType = ObjectType.None;
        MapTile.TileImprovement newImprovement = MapTile.TileImprovement.None;
        Directions.Direction newDir = m_direction;

        // I'm not familiar with UI in Unity, so select what you want to place with buttons for now until UI gets set up.

        // We ignore game input while an input field is focused
        bool allowInput =    m_gameControl.gameState.mainState == GameState.State.Started_Paused
                          && !m_gameControl.gameState.hasState(GameState.TagState.Suspended)
                          && !m_gameControl.gameState.hasState(GameState.TagState.InputFocused);

        if (allowInput)
        {
            // Keys to select which improvement
            // TODO: UI
            // This is basically a block of UI related stuff until the comment that says it isn't
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
                    // For now, we set a type to force an update of the placeholder object
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
                    // For now, we set a type to force an update of the placeholder object
                    newType = m_placeholderType;
                }
                if (newImprovement == MapTile.TileImprovement.None)
                {
                    newImprovement = m_selectedImprovement;
                }
            }
            // End of UI stuff block
        }
        else // if (allowInput)
        {
            // Make sure the placement preview is disabled if input is not allowed
            disablePlaceholder();
        }


        // Modifies the placement preview if it changes
        if (newType != ObjectType.None)
        {
            // If reselecting same object "put it away" instead so that no object is selected for placement
            if (m_placeholderType == newType && m_selectedImprovement == newImprovement && m_direction == newDir)
            {
                if (m_placeholderObject.gameObject.activeSelf)
                {
                    disablePlaceholder();
                }
                else
                {
                    activatePlaceholder();
                }
            }
            else
            {
                // Assign our selected properties before using them
                m_placeholderType = newType;
                m_selectedImprovement = newImprovement;
                m_direction = newDir;
                // We set this slightly above to make sure it never has z-fighting issues with anything of the other "floor level" tiles
                m_positionOffset = new Vector3(0.0f, 0.0002f, 0.0f);

                drawPlaceholder();
                activatePlaceholder();
            }
        }


        if (m_placeholderObject.gameObject.activeSelf)
        {
            // Moves the placement preview. We need to update position on type change too in case m_positionOffset changes.
            // Follow mouse precisely unless there is a tile to snap to
            if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0 || newType != ObjectType.None)
            {
                if (selectedTile == null)
                {
                    Vector3 mousePos = m_mainCamera.ScreenToWorldPoint(Input.mousePosition);
                    // We should use game map height as the height, but we weren't doing it before and I wont bother for now
                    m_placeholderObject.position = new Vector3(mousePos.x, 0.0f, mousePos.z) + m_positionOffset;
                }
                else
                {
                    m_placeholderObject.localPosition = selectedTile.transform.localPosition + m_positionOffset;
                }
            }

            // Place object if paused. We currently toggle the selected type between choice and false/none
            // We can change it to have right click remove and left click place, but I don't know if that is better.
            if (Input.GetButtonDown("Select") && selectedTile != null && allowInput)
            {
                placeObject(selectedTile);
            }
        }
    }


    // Creates a saved file of the stage with a file name of "saveName"
    public void createSave(string saveName)
    {
        m_gameStage.saveStage(saveName);
    }


    // Loads the stage file into the editor of name "saveName"
    public void loadSave(string saveName)
    {
        disablePlaceholder();
        m_gameStage.loadStage(saveName);
        mapMovingObjToTile(m_gameMap);

        // We need to re-start the game after loading a new map I think
        // We probably should get rid of this loadSave function and force people to load a map through the main menu
        m_gameControl.runGame(GameController.GameMode.Editor);
    }


    // Places a tile-object onto the tile specified by "selectedTile"
    private void placeObject(MapTile selectedTile)
    {
        if (m_placeholderType == ObjectType.Wall)
        {
            selectedTile.walls[m_direction] = !selectedTile.walls[m_direction];
        }
        else if (m_placeholderType == ObjectType.Improvement)
        {
            // We have to do special actions to handle movingObject gameobjects, since MapTiles do not keep track of attached moving objects
            // To do this cleanly, we have to do this last, after determining if we need to
            bool isCreatingMovingObj = false;

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

                    isCreatingMovingObj = true;

                    if (selectedTile.improvement != MapTile.TileImprovement.Direction)
                    {
                        selectedTile.improvement = MapTile.TileImprovement.None;
                    }
                }
            }
            else // Not a moving obj
            {
                if ( selectedTile.improvement == m_selectedImprovement &&
                     ( selectedTile.improvementDirection == m_direction ||
                       (selectedTile.improvement == MapTile.TileImprovement.Goal || selectedTile.improvement == MapTile.TileImprovement.Hole)
                     )
                   )
                {
                    selectedTile.improvement = MapTile.TileImprovement.None;
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
                }
            }

            // We need to destroy a moving object if removed or replaced
            if (selectedTile.movingObject == MapTile.TileImprovement.None || isCreatingMovingObj)
            {
                if (m_movingObjects.ContainsKey(selectedTile))
                {
                    if (m_movingObjects[selectedTile] != null)
                    {
                        GridMovement gm = m_movingObjects[selectedTile].GetComponent<GridMovement>();
                        if (gm)
                        {
                            if (gm is Mouse)
                            {
                                m_gameMap.destroyMouse(m_movingObjects[selectedTile].transform);
                            }
                            else if (gm is Cat)
                            {
                                m_gameMap.destroyCat(m_movingObjects[selectedTile].transform);
                            }
                        }
                    }
                    m_movingObjects.Remove(selectedTile);
                }
            }

            if (isCreatingMovingObj)
            {
                Transform newMovingObj = null;
                if (selectedTile.movingObject == MapTile.TileImprovement.Mouse)
                {
                    newMovingObj = m_gameMap.placeMouse(selectedTile.transform.localPosition, m_direction);
                }
                else if (selectedTile.movingObject == MapTile.TileImprovement.Cat)
                {
                    newMovingObj = m_gameMap.placeCat(selectedTile.transform.localPosition, m_direction);
                }
                m_movingObjects.Add(selectedTile, newMovingObj);
            }
        }
    }


    // Associates unity game objects that move around with the tile they are currently on
    // This is needed because the unity game objects aren't children of the tile they are placed on
    //   and we want to be able to destroy them in the editor based on the tile they are associated with
    private void mapMovingObjToTile(GameMap map)
    {
        m_movingObjects.Clear();

        GridMovement[] movingObjs = map.GetComponentsInChildren<GridMovement>();
        for (int i = 0; i < movingObjs.Length; ++i)
        {
            m_movingObjects.Add(map.tileAt(movingObjs[i].transform.localPosition), movingObjs[i].transform);
        }
    }


    // Hides the object under the mouse used to show what is going to be placed
    private void disablePlaceholder()
    {
        if (m_placeholderObject)
        {
            m_placeholderObject.gameObject.SetActive(false);
        }
    }


    // Shows the object under the mouse used to show what is going to be placed
    private void activatePlaceholder()
    {
        if (m_placeholderObject)
        {
            m_placeholderObject.gameObject.SetActive(true);
        }
    }


    // Creates and assigns a new mesh to the placeholder for the current stored placeholder data
    private void drawPlaceholder()
    {
        // We need to reset scale since not all prefabs whose mesh we use have same scale
        // We wouldn't have to do this if we made our own model for walls
        m_placeholderObject.localScale = Vector3.one;

        // We need to reset rotation since not all prefabs whose mesh we use have same rotation
        // We wouldn't have to do this if we made our own quad model for tiles (quads stand vertically by default)
        m_placeholderObject.localRotation = Quaternion.identity;

        // Select the mesh for the selected object
        Mesh newMesh = m_gameResources.objects["Tile"].GetComponent<MeshFilter>().sharedMesh;
        Texture newTex = m_gameResources.materials["Placeholder"].mainTexture;
        if (m_placeholderType == ObjectType.Wall)
        {
            newMesh = m_gameResources.objects["Wall"].GetComponent<MeshFilter>().sharedMesh;

            // Need to set a position offset to show which wall facing is used
            // Easiest way is to spawn a wall and then destroy it. This is wasteful, but only done once per object select.
            // The point of using a mesh is to avoid spawning objects, so this is also messy and bad. It can be changed later.
            Transform temp = m_gameMap.createWall(Vector3.zero, m_direction);
            m_positionOffset = temp.localPosition;
            m_gameMap.destroyWall(temp);

            m_placeholderObject.localScale = new Vector3(1.0f, 0.5f, 0.1f);
        }
        else if (m_placeholderType == ObjectType.Improvement)
        {
            // Mice and cats are currently built from multile game objects, so we have to loop over those objects to create a combined mesh
            if (m_selectedImprovement == MapTile.TileImprovement.Mouse)
            {
                MeshFilter[] meshFilters = m_gameResources.objects["Mouse"].GetComponentsInChildren<MeshFilter>();
                CombineInstance[] combine = new CombineInstance[meshFilters.Length];
                for (int i = 0; i < meshFilters.Length; ++i)
                {
                    combine[i].mesh = meshFilters[i].sharedMesh;
                    combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                }
                newMesh = new Mesh();
                newMesh.CombineMeshes(combine);
            }
            else if (m_selectedImprovement == MapTile.TileImprovement.Cat)
            {
                MeshFilter[] meshFilters = m_gameResources.objects["Cat"].GetComponentsInChildren<MeshFilter>();
                CombineInstance[] combine = new CombineInstance[meshFilters.Length];
                for (int i = 0; i < meshFilters.Length; ++i)
                {
                    combine[i].mesh = meshFilters[i].sharedMesh;
                    combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                }
                newMesh = new Mesh();
                newMesh.CombineMeshes(combine);
            }
            else
            {
                // Here we either spawn the improvement object, or a tile with the improvement texture if there isn't one
                if (MapTile.improvementObjects.ContainsKey(m_selectedImprovement))
                {
                    newMesh = m_gameResources.objects[MapTile.improvementObjects[m_selectedImprovement]].GetComponent<MeshFilter>().sharedMesh;

                    // Since direction arrow objects are unity quad tiles for now, we have to give them the tile treatment
                    if (m_selectedImprovement == MapTile.TileImprovement.Direction)
                    {
                        // Tiles have a 90 degree rotation since we are using a Unity quad for now
                        m_placeholderObject.transform.eulerAngles = new Vector3(90.0f, transform.eulerAngles.x, transform.eulerAngles.z);

                        newTex = m_gameResources.objects[MapTile.improvementObjects[m_selectedImprovement]].GetComponent<MeshRenderer>().sharedMaterial.mainTexture;
                    }
                }
                else
                {
                    newMesh = m_gameResources.objects["Tile"].GetComponent<MeshFilter>().sharedMesh;
                    // Tiles have a 90 degree rotation since we are using a Unity quad for now
                    m_placeholderObject.transform.eulerAngles = new Vector3(90.0f, transform.eulerAngles.x, transform.eulerAngles.z);

                    if (MapTile.improvementTextures.ContainsKey(m_selectedImprovement))
                    {
                        newTex = m_gameResources.materials[MapTile.improvementTextures[m_selectedImprovement]].mainTexture;
                    }
                }
            }
        }

        Directions.rotate(m_placeholderObject, m_direction);

        // Assign the new mesh
        MeshFilter phMesh = m_placeholderObject.GetComponent<MeshFilter>();
        phMesh.mesh = newMesh;
        MeshRenderer phRend = m_placeholderObject.GetComponent<MeshRenderer>();
        phRend.material.mainTexture = newTex;
    }

    private void onStateChange(GameState.State oldState, GameState.State newState)
    {
        if (newState == GameState.State.Started_Unpaused)
        {
            // Don't leave anything selected when unpausing
            disablePlaceholder();
        }
    }

    private void onMapLoaded()
    {
        mapMovingObjToTile(m_gameMap);
    }

    private enum ObjectType { None, Wall, Improvement }

    private Transform m_placeholderObject;
    private MapTile.TileImprovement m_selectedImprovement;
    private ObjectType m_placeholderType;
    private Directions.Direction m_direction;
    private Vector3 m_positionOffset; // Used for wall facing

    private Dictionary<MapTile, Transform> m_movingObjects = new Dictionary<MapTile, Transform>();
    private GameMap m_gameMap;
    private GameResources m_gameResources;
    private GameStage m_gameStage;
    private PlayerController m_controls;
    private GameController m_gameControl;
    private Camera m_mainCamera;
}
