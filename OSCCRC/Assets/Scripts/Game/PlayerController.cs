using UnityEngine;

// This class allows for player input and handles player interaction with the game (but currently not the editor).

public class PlayerController : MonoBehaviour {

    [Range(1, 4)] public int playerID;
    public Transform highlighter;
    public Canvas pauseDisplay;
    public Canvas fpsDisplay;
    public MapTile currentTile { get { return m_currentTile; } }


	void Start () {
        m_gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
        m_fpsScript = m_gameController.GetComponent<FramerateDisplay>();
        m_mainCamera = Camera.main;

        // We will need to differentiate the inputs of players if we add multiplayer.
        // I don't know how that will work yet, but just assign a playerID of 1 to the player controls for now
        playerID = 1;
	}


	void Update () {
        // We ignore game input while an input field is focused
        if (m_gameController.gameState.hasState(GameState.TagState.InputFocused))
        {
            return;
        }

        // We want to ignore some inputs while the game is suspended
        if (!m_gameController.gameState.hasState(GameState.TagState.Suspended))
        {
            // The mouse hovers over a tile to select it as the one where improvements will be placed
            if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            {
                Vector3 mousePosition = m_mainCamera.ScreenToWorldPoint(Input.mousePosition);
                m_currentTile = m_gameMap.tileAt(m_gameMap.transform.InverseTransformPoint(mousePosition));

                // Move tile highlighter to the mouse position
                if (m_currentTile == null)
                {
                    if (highlighter.gameObject.activeSelf)
                    {
                        highlighter.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (!highlighter.gameObject.activeSelf)
                    {
                        highlighter.gameObject.SetActive(true);
                    }
                    highlighter.position = m_currentTile.transform.position + Vector3.up * 2;
                }
            }

            if (m_currentTile != null)
            {
                if (Input.GetButtonDown("Up"))
                {
                    m_gameController.requestPlacement(m_currentTile, MapTile.TileImprovement.Direction, Directions.Direction.North);
                }
                else if (Input.GetButtonDown("Right"))
                {
                    m_gameController.requestPlacement(m_currentTile, MapTile.TileImprovement.Direction, Directions.Direction.East);
                }
                else if (Input.GetButtonDown("Down"))
                {
                    m_gameController.requestPlacement(m_currentTile, MapTile.TileImprovement.Direction, Directions.Direction.South);
                }
                else if (Input.GetButtonDown("Left"))
                {
                    m_gameController.requestPlacement(m_currentTile, MapTile.TileImprovement.Direction, Directions.Direction.West);
                }
            }

            // "Pause" input does not suspend the game in the traditional sense of pause, but toggles the puzzle-placement state
            if (Input.GetButtonDown("Pause"))
            {
                if (m_gameController.gameState.mainState == GameState.State.Started_Unpaused || m_gameController.gameState.mainState == GameState.State.Ended_Failure)
                {
                    m_gameController.gameState.mainState = GameState.State.Started_Paused;
                }
                else if (m_gameController.gameState.mainState == GameState.State.Started_Paused)
                {
                    m_gameController.gameState.mainState = GameState.State.Started_Unpaused;
                }
            }

            // Reset to clear all placements. This will simply reload the map and re-initialize the game mode for now.
            if (Input.GetButtonDown("Reset"))
            {
                m_gameController.resetGame();
            }
        }

        if (Input.GetButtonDown("Menu"))
        {
            if (!pauseDisplay.enabled)
            {
                pauseDisplay.enabled = true;
                m_gameController.gameState.addState(GameState.TagState.Suspended);
            }
            else
            {
                pauseDisplay.enabled = false;
                // Currently the Menu input is the only one that can suspend the game.
                // If this changes, we will need to make sure that we account for all toggles to suspend.
                m_gameController.gameState.removeState(GameState.TagState.Suspended);
            }
        }

        // Toggle framerate display between Basic, Advanced, and Off
        if (Input.GetButtonDown("FramerateToggle"))
        {
            if (!m_fpsScript.enabled)
            {
                fpsDisplay.enabled = true;
                m_fpsScript.enabled = true;
                m_fpsScript.isAdvanced = false;
            }
            else if (!m_fpsScript.isAdvanced)
            {
                m_fpsScript.isAdvanced = true;
            }
            else
            {
                m_fpsScript.enabled = false;
                fpsDisplay.enabled = false;
            }
        }

        // For now, we use a button press to toggle double movement speed
        if (Input.GetButtonDown("SpeedToggle"))
        {
            if (GridMovement.speedMultiplier > 1.0f)
            {
                GridMovement.speedMultiplier = 1.0f;
            }
            else
            {
                GridMovement.speedMultiplier = 2.0f;
            }
        }

        // Moonwalking easter egg thing
        if (Input.GetButtonDown("MoonwalkToggle"))
        {
            GridMovement.moonwalkEnabled = !GridMovement.moonwalkEnabled;
        }

        // First person easter egg thing
        if (Input.GetButtonDown("FPSToggle"))
        {
            toggleFPSMode();
        }
    }


    private void toggleFPSMode()
    {
        CameraController cam = m_mainCamera.GetComponent<CameraController>();

        if (cam.isAttached)
        {
            cam.setCameraOrthographic();
            cam.setCameraView(m_gameMap);
        }
        else
        {
            // We randomly select which grid mover to follow
            System.Random rng = new System.Random();
            GridMovement[] gms = m_gameMap.GetComponentsInChildren<GridMovement>();

            if (gms.Length == 0)
            {
                return;
            }

            Transform gm = gms[rng.Next(gms.Length)].transform;
            cam.setCameraFollow(gm);
        }
    }


    private GameController m_gameController;
    private GameMap m_gameMap;
    private FramerateDisplay m_fpsScript;
    private Camera m_mainCamera;
    private MapTile m_currentTile = null;
}
