using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// This class allows for player input and handles player interaction with the game (but currently not the editor).

public class PlayerController : MonoBehaviour {

    // I heard Unity is going to be overhauling its input system soon. It would be nice to subscribe to event callbacks rather than poll every frame.

    [Range(1, 4)] public int playerID;
    public Transform highlighter;
    [HideInInspector] public MapTile currentTile = null;
    [HideInInspector] public bool menuPaused = false;

    private GameController m_gameController;

	void Start () {
        m_gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        // We will need to differentiate the inputs of players if we add multiplayer.
        // I don't know how that will work yet, but just assign a playerID of 1 to the player controls for now
        playerID = 1;
	}
	
	void Update () {
        // We ignore game input while a text field is focused
        if (EventSystem.current.currentSelectedGameObject)
        {
            return;
        }

        // The mouse hovers over a tile to select it as the one where improvements will be placed
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            Ray tileSelector = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitObject;
            if (Physics.Raycast(tileSelector, out hitObject, Mathf.Infinity, 1 << LayerMask.NameToLayer("Player Selectable")))
            {
                MapTile newTile = hitObject.transform.GetComponent<MapTile>();
                if (newTile != null && newTile != currentTile)
                {
                    // Currently using a spotlight to highlight the currently slected tile
                    highlighter.position = newTile.transform.position + Vector3.up * 2;
                    currentTile = newTile;
                }
            }
            else
            {
                highlighter.position = Vector3.one * -99;
                currentTile = null;
            }
        }

        // Placements: pretty sure the user can only place directional tiles in game
        if (currentTile != null)
        {
            if (Input.GetButtonDown("Up"))
            {
                m_gameController.requestPlacement(currentTile, MapTile.TileImprovement.Direction, Directions.Direction.North);
            }
            else if (Input.GetButtonDown("Right"))
            {
                m_gameController.requestPlacement(currentTile, MapTile.TileImprovement.Direction, Directions.Direction.East);
            }
            else if (Input.GetButtonDown("Down"))
            {
                m_gameController.requestPlacement(currentTile, MapTile.TileImprovement.Direction, Directions.Direction.South);
            }
            else if (Input.GetButtonDown("Left"))
            {
                m_gameController.requestPlacement(currentTile, MapTile.TileImprovement.Direction, Directions.Direction.West);
            }
        }

        // Other controls, like pausing or menu
        if (Input.GetButtonDown("Pause"))
        {
            m_gameController.isPaused = !m_gameController.isPaused;
        }

        // We don't have a menu yet, so quit the game when menu should be opened just for now
        // TODO: UI
        if (Input.GetButtonDown("Menu"))
        {
            Canvas pauseDisplay = GameObject.Find("PauseMenu").GetComponent<Canvas>();

            if (!pauseDisplay.enabled)
            {
                pauseDisplay.enabled = true;
                m_gameController.isPaused = true;
                if (!m_gameController.isPaused) menuPaused = true;
            }
            else
            {
                pauseDisplay.enabled = false;
                if (menuPaused) {
                    m_gameController.isPaused = false;
                    menuPaused = false;
                }
            }
        }

        // Toggle framerate display between Basic, Advanced, and Off
        if (Input.GetButtonDown("FramerateToggle"))
        {
            FramerateDisplay fpsScript = m_gameController.GetComponent<FramerateDisplay>();
            Canvas fpsDisplay = GameObject.Find("FPSDisplay").GetComponent<Canvas>();

            if (!fpsScript.enabled)
            {
                fpsDisplay.enabled = true;
                fpsScript.enabled = true;
                fpsScript.isAdvanced = false;
            }
            else if (!fpsScript.isAdvanced)
            {
                fpsScript.isAdvanced = true;
            }
            else
            {
                fpsScript.enabled = false;
                fpsDisplay.enabled = false;
            }
        }
    }
}
