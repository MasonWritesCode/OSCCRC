using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class controls the map building functionality of the game, allowing creation of a new Game Map and play testing it.

public class Editor : MonoBehaviour {

    // TODO: Make controllable via UI instead of arbitrary keys
    //           (all inputs are in update function for now so you only have to look in there)
    //       Allow map saving and loading with input name (needs ui)
    //       Add ability to adjust speed when unpaused (wait for ui?)
    //       Placed directional tiles aren't supposed to be saved.
    //          Have to figure out what is supposed to happen with playtesting here.
    //       There is a lot of cleanup needed in placement drawing once we create our own objects

    private enum ObjectType { None, Wall, Improvement }

    private Transform m_placeholderObject;
    private MapTile.TileImprovement m_selectedImprovement;
    private ObjectType m_placeholderType;
    private Directions.Direction m_direction;
    private Vector3 m_positionOffset; // Used for wall facing

    private Dictionary<MapTile, Transform> m_movingObjects = new Dictionary<MapTile, Transform>();
    private List<MapTile> arrowTiles = new List<MapTile>();
    private GameMap m_gameMap;
    private GameStage m_gameStage;
    private PlayerController m_controls;
    private GameController m_gameControl;
    private bool m_wasUnpaused;
    private readonly Plane m_floorPlane = new Plane(Vector3.up, Vector3.zero);

    void OnDisable() {
        disablePlaceholder();
    }

	void Start () {
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
        m_gameStage = GameObject.FindWithTag("GameController").GetComponent<GameStage>();
        m_controls = GameObject.FindWithTag("Player").GetComponentInChildren<PlayerController>();
        m_gameControl = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        m_placeholderType = ObjectType.None;
        m_selectedImprovement = MapTile.TileImprovement.None;
        m_placeholderObject = Instantiate(GameResources.objects["Placeholder"]);
        disablePlaceholder();
        m_direction = Directions.Direction.East;
        m_positionOffset = Vector3.zero;
        m_wasUnpaused = false;
    }

	void Update () {
        MapTile selectedTile = m_controls.currentTile;

        // I'm not familiar with UI in Unity, so select what you want to place with buttons for now until UI gets set up.
        ObjectType newType = ObjectType.None;
        MapTile.TileImprovement newImprovement = MapTile.TileImprovement.None;
        Directions.Direction newDir = m_direction;

        if (m_gameControl.isPaused)
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
            // End of UI stuff block
        }


        // Draws the placement preview
        if (newType != ObjectType.None)
        {
            // If reselecting same object "put it away" instead so that no object is selected for placement
            if (m_placeholderType == newType && m_selectedImprovement == newImprovement && m_direction == newDir)
            {
                disablePlaceholder();
            }
            else
            {
                // Assign our selected properties before using them
                m_placeholderType = newType;
                m_selectedImprovement = newImprovement;
                m_direction = newDir;
                // We have a default offset of 0.003 vertically to prevent z-buffer issues for placeholders that are a tile
                m_positionOffset = new Vector3(0.0f, 0.003f, 0.0f);

                // We need to reset scale since not all prefabs whose mesh we use have same scale
                // We wouldn't have to do this if we made our own model for walls
                m_placeholderObject.localScale = Vector3.one;

                // We need to reset rotation since not all prefabs whose mesh we use have same rotation
                // We wouldn't have to do this if we made our own quad model for tiles (quads stand vertically by default)
                m_placeholderObject.rotation = Quaternion.identity;

                // Select the mesh for the selected object
                Mesh newMesh = GameResources.objects["Tile"].GetComponent<MeshFilter>().sharedMesh;
                Texture newTex = GameResources.materials["Placeholder"].mainTexture;
                if (m_placeholderType == ObjectType.Wall)
                {
                    newMesh = GameResources.objects["Wall"].GetComponent<MeshFilter>().sharedMesh;

                    // Need to set a position offset to show which wall facing is used
                    // Easiest way is to spawn a wall and then destroy it. This is wasteful, but only done once per object select.
                    // The point of using a mesh is to avoid spawning objects, so this is also messy and bad. It can be changed later.
                    Transform temp = m_gameMap.createWall(0, 0, m_direction);
                    m_positionOffset = temp.localPosition;
                    m_gameMap.destroyWall(temp);

                    m_placeholderObject.localScale = new Vector3(1.0f, 0.5f, 0.1f);
                }
                else if (m_placeholderType == ObjectType.Improvement)
                {
                    if (m_selectedImprovement == MapTile.TileImprovement.Mouse)
                    {
                        MeshFilter[] meshFilters = GameResources.objects["Mouse"].GetComponentsInChildren<MeshFilter>();
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
                        MeshFilter[] meshFilters = GameResources.objects["Cat"].GetComponentsInChildren<MeshFilter>();
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
                        // Need to show the improvement selected by setting it's texture
                        if (MapTile.improvementObjects.ContainsKey(m_selectedImprovement))
                        {
                            newMesh = GameResources.objects[MapTile.improvementObjects[m_selectedImprovement]].GetComponent<MeshFilter>().sharedMesh;

                            // Since direction arrow objects are unity quad tiles for now, we have to give them the tile treatment
                            if (m_selectedImprovement == MapTile.TileImprovement.Direction)
                            {
                                // Tiles have a 90 degree rotation since we are using a Unity quad for now
                                m_placeholderObject.transform.eulerAngles = new Vector3(90.0f, transform.eulerAngles.x, transform.eulerAngles.z);

                                newTex = GameResources.objects[MapTile.improvementObjects[m_selectedImprovement]].GetComponent<MeshRenderer>().sharedMaterial.mainTexture;
                            }
                        }
                        else
                        {
                            newMesh = GameResources.objects["Tile"].GetComponent<MeshFilter>().sharedMesh;
                            // Tiles have a 90 degree rotation since we are using a Unity quad for now
                            m_placeholderObject.transform.eulerAngles = new Vector3(90.0f, transform.eulerAngles.x, transform.eulerAngles.z);

                            if (MapTile.improvementTextures.ContainsKey(m_selectedImprovement))
                            {
                                newTex = GameResources.materials[MapTile.improvementTextures[m_selectedImprovement]].mainTexture;
                            }
                        }
                    }
                }

                Directions.rotate(ref m_placeholderObject, m_direction);

                // Assign the new mesh
                MeshFilter phMesh = m_placeholderObject.GetComponent<MeshFilter>();
                phMesh.mesh = newMesh;
                MeshRenderer phRend = m_placeholderObject.GetComponent<MeshRenderer>();
                phRend.material.mainTexture = newTex;
                activatePlaceholder();
            }
        }


        // Moves the placement preview
        if (m_placeholderObject.gameObject.activeSelf)
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


        // Switch between placement and playtest
        if (!m_gameControl.isPaused)
        {
            if (!m_wasUnpaused)
            {
                m_wasUnpaused = true;
                // Don't leave anything selected when unpausing
                disablePlaceholder();
            }
        }
        else if (m_wasUnpaused)
        {
            m_wasUnpaused = false;
            mapMovingObjToTile(m_gameMap);
        }


        // For now just hotkey a save to "dev" until we have a UI that lets you choose save name
        // TODO: UI
        // This is a much smaller block of UI stuff
        /*
        if (Input.GetKeyDown(KeyCode.F6))
        {
            createSave("dev");
        }
        else if (Input.GetKeyDown(KeyCode.F7))
        {
            loadSave("dev");
        }
        */
        // End of smaller UI block
    }


    // Creates a saved file of the stage with a file name of "saveName"
    public void createSave(string saveName)
    {
        // purge arrow tiles, since we store the arrows as available placements
        for (int i = 0; i < arrowTiles.Count; ++i)
        {
            arrowTiles[i].improvement = MapTile.TileImprovement.None;
        }

        m_gameStage.saveStage(saveName);

        // restore them for the editor at some point?
    }


    // Loads the stage file into the editor of name "saveName"
    public void loadSave(string saveName)
    {
        disablePlaceholder();
        m_gameStage.loadStage(saveName);
        mapMovingObjToTile(m_gameMap);

        // We need to re-start the game after loading a new map
        // We probably should get rid of this loadSave function and force people to load a map through the main menu
        m_gameControl.runGame(GameController.GameMode.Editor);
    }


    // Places an tile-object onto the tile specified by "selectedTile"
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
                        m_gameStage.placements.remove(m_direction);
                        arrowTiles.Remove(selectedTile);
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
                        m_gameStage.placements.add(m_direction);
                        arrowTiles.Add(selectedTile);
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
                    newMovingObj = m_gameMap.placeMouse(selectedTile.transform.localPosition.x, selectedTile.transform.localPosition.z, m_direction);
                }
                else if (selectedTile.movingObject == MapTile.TileImprovement.Cat)
                {
                    newMovingObj = m_gameMap.placeCat(selectedTile.transform.localPosition.x, selectedTile.transform.localPosition.z, m_direction);
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

        List<GridMovement> movingObjs = new List<GridMovement>();
        map.GetComponentsInChildren<GridMovement>(true, movingObjs);
        foreach (GridMovement i in movingObjs)
        {
            // Should be destroyed by map import, but seems to still be around?
            // Checking if isActiveAndEnabled seems to work. 
            if (i.isActiveAndEnabled)
            {
                m_movingObjects.Add(map.tileAt(i.transform.localPosition), i.transform);
            }
        }
    }


    // Hides the object under the mouse used to show what is going to be placed
    private void disablePlaceholder()
    {
        if (m_placeholderObject)
        {
            m_placeholderObject.GetComponent<MeshRenderer>().enabled = false;
        }
        m_placeholderType = ObjectType.None;
        m_selectedImprovement = MapTile.TileImprovement.None;
    }


    // Shows the object under the mouse used to show what is going to be placed
    private void activatePlaceholder()
    {
        if (m_placeholderObject)
        {
            m_placeholderObject.GetComponent<MeshRenderer>().enabled = true;
        }
    }
}
