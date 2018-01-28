using UnityEngine;

public class PlayerController : MonoBehaviour {

    // I heard Unity is going to be overhauling its input system soon. It would be nice to subscribe to event callbacks rather than poll every frame.

    public int playerID;
    public Transform highlighter;
    public MapTile currentTile = null;

    private GameController m_gameController;

	void Start () {
        m_gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        // We will need to differentiate the inputs of players if we add multiplayer.
        // I don't know how that will work yet, but just assign a playerID of 1 to the player controls for now
        playerID = 1;
	}
	
	void Update () {
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
                m_gameController.requestPlacement(currentTile, MapTile.TileImprovement.Up);
            }
            else if (Input.GetButtonDown("Right"))
            {
                m_gameController.requestPlacement(currentTile, MapTile.TileImprovement.Right);
            }
            else if (Input.GetButtonDown("Down"))
            {
                m_gameController.requestPlacement(currentTile, MapTile.TileImprovement.Down);
            }
            else if (Input.GetButtonDown("Left"))
            {
                m_gameController.requestPlacement(currentTile, MapTile.TileImprovement.Left);
            }
        }

        // Other controls, like pausing or menu
        if (Input.GetButtonDown("Pause"))
        {
            m_gameController.isPaused = !m_gameController.isPaused;
        }
        // We don't have a menu yet, so quit the game when menu should be opened just for now
        if (Input.GetButtonDown("Menu"))
        {
            Application.Quit();
        }
        // Toggle framerate display between Basic, Advanced, and Off
        if (Input.GetButtonDown("FramerateToggle"))
        {
            FramerateDisplay fpsScript = GameObject.Find("GameController").GetComponent<FramerateDisplay>();
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
