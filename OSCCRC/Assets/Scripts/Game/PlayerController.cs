using UnityEngine;
using UnityEngine.InputSystem;

// This class allows for player input and handles player interaction with the game (but currently not the editor).
// TODO: I think I incorrectly understood device.current before, and that it is actually player specific and we can use it instead of iterating over user devices

public class PlayerController : MonoBehaviour {

    [Range(0, 3)] public int playerID;
    public RectTransform cursor;    // Editor set
    public Transform highlighter;   // Editor set
    public GameObject pauseDisplay; // Editor set
    public Canvas fpsDisplay;       // Editor set
    public float stickSensitivity;  // Editor set
    public MapTile currentTile { get { return m_currentTile; } }


    void Start()
    {
        m_playerInput = GetComponent<PlayerInput>();
        m_gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
        m_fpsScript = m_gameController.GetComponent<FramerateDisplay>();
        m_mainCamera = Camera.main;
        cursor.gameObject.SetActive(true);

        m_playerInput.onControlsChanged += ctx => OnControlsChanged();
        m_playerInput.actions["DirectionalPlacement"].performed   += ctx => onDirectionalPlacement(ctx);
        m_playerInput.actions["Pause"].performed                  += ctx => onPause();
        m_playerInput.actions["Reset"].performed                  += ctx => onReset();
        m_playerInput.actions["Menu"].performed                   += ctx => onMenu();
        m_playerInput.actions["FramerateDisplayToggle"].performed += ctx => onFramerateDisplayToggle();
        m_playerInput.actions["SpeedToggle"].performed            += ctx => onSpeedToggle();
        m_playerInput.actions["MoonwalkToggle"].performed         += ctx => onMoonwalkToggle();
        m_playerInput.actions["FirstPersonToggle"].performed      += ctx => onFirstPersonToggle();

        OnControlsChanged();

        if (m_isGamepad)
        {
            cursor.position = m_mainCamera.WorldToScreenPoint(Vector3.zero);
        }
        else
        {
            cursor.position = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        }

        // We want to immediately update the highlighter position
        selectTile(m_mainCamera.ScreenToWorldPoint(cursor.position));
    }


    // We prefer to poll sometimes for input in Update since events generate garbage, which is a performance concern that matters with frequent events like CursorMovement.
    // In addition, even Pass Through doesn't generate events on sticks being actuated but only changed, which prevents us from having an event-only approach to cursor movement.
    void Update()
    {
        // We ignore game input while an input field is focused
        if (m_gameController.gameState.hasState(GameState.TagState.InputFocused))
        {
            return;
        }

        InputActionAsset inputActions = m_playerInput.actions;
        if (!m_gameController.gameState.hasState(GameState.TagState.Suspended))
        {
            // Mouse Movement is very expensive in the new input system, but polling mouse.delta directly doesn't alleviate the issue
            //   because the system seems to call expensive notify functions even if the CursorMovement action is removed. So sticking with the action map for now.
            if (inputActions["CursorMovement"].triggered || ( m_isGamepad && m_gamepad.leftStick.IsActuated() ))
            {
                Vector3 newCursorPos;
                if (m_isGamepad)
                {
                    Vector2 inputVec = m_gamepad.leftStick.ReadValue() * stickSensitivity;
                    newCursorPos = cursor.position + (new Vector3(inputVec[0], inputVec[1], 0.0f));
                }
                else
                {
                    newCursorPos = m_mouse.position.ReadValue();
                }

                // Make sure to clamp cursor position to screen edge so the player doesn't lose their cursor
                newCursorPos.x = Mathf.Clamp(newCursorPos.x, 0, Screen.width - 1.0f);
                newCursorPos.y = Mathf.Clamp(newCursorPos.y, 0, Screen.height - 1.0f);
                cursor.position = newCursorPos;

                selectTile(m_mainCamera.ScreenToWorldPoint(newCursorPos));
            }
        }
    }


    // The player makes a directional tile placement request
    private void onDirectionalPlacement(InputAction.CallbackContext context)
    {
        if (!m_gameController.gameState.hasState(GameState.TagState.Suspended))
        {
            if (m_currentTile != null)
            {
                Vector2 placementDirVec = context.ReadValue<Vector2>();

                if (placementDirVec == Vector2.up)
                {
                    m_gameController.requestPlacement(m_currentTile, MapTile.TileImprovement.Direction, Directions.Direction.North, playerID);
                }
                else if (placementDirVec == Vector2.right)
                {
                    m_gameController.requestPlacement(m_currentTile, MapTile.TileImprovement.Direction, Directions.Direction.East, playerID);
                }
                else if (placementDirVec == Vector2.down)
                {
                    m_gameController.requestPlacement(m_currentTile, MapTile.TileImprovement.Direction, Directions.Direction.South, playerID);
                }
                else if (placementDirVec == Vector2.left)
                {
                    m_gameController.requestPlacement(m_currentTile, MapTile.TileImprovement.Direction, Directions.Direction.West, playerID);
                }
            }
        }
    }


    // "Pause" input does not suspend the game in the traditional sense of pause, but toggles the puzzle-placement state
    private void onPause()
    {
        if (!m_gameController.gameState.hasState(GameState.TagState.Suspended))
        {
            if (m_gameController.mode != GameController.GameMode.Competitive)
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
        }
    }


    // Reset to clear all placements. This will simply reload the map and re-initialize the game mode for now.
    private void onReset()
    {
        if (!m_gameController.gameState.hasState(GameState.TagState.Suspended))
        {
            m_gameController.resetGame();
        }
    }


    // Opens the Pause Menu
    private void onMenu()
    {
        if (!pauseDisplay.activeSelf)
        {
            pauseDisplay.SetActive(true);
            m_gameController.gameState.addState(GameState.TagState.Suspended);
        }
        else
        {
            pauseDisplay.SetActive(false);
            // Currently the Menu input is the only one that can suspend the game.
            // If this changes, we will need to make sure that we account for all toggles to suspend.
            m_gameController.gameState.removeState(GameState.TagState.Suspended);
        }
    }


    // Toggle framerate display between Basic, Advanced, and Off
    private void onFramerateDisplayToggle()
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
    private void onSpeedToggle()
    {
        if (m_gameController.mode != GameController.GameMode.Competitive)
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
    }


    // Moonwalking easter egg thing
    private void onMoonwalkToggle()
    {
        GridMovement.moonwalkEnabled = !GridMovement.moonwalkEnabled;
    }


    // First person easter egg thing
    private void onFirstPersonToggle()
    {
        toggleFPSMode();
    }


    // Updates the current tile and positions the highlighter accordingly
    private void selectTile(Vector3 position)
    {
        MapTile newTile = m_gameMap.tileAt(m_gameMap.transform.InverseTransformPoint(position));

        if (m_currentTile != newTile)
        {
            m_currentTile = newTile;

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
    }


    // Toggles between first person and normal camera modes
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


    private void OnControlsChanged()
    {
        // TODO: Do we move the mouse to the cursor position if we switch away from controller?

        using (var devicelist = m_playerInput.user.controlSchemeMatch.devices)
        {
            m_mouse = null;
            m_gamepad = null;

            for (int i = 0; i < devicelist.Count; ++i)
            {
                if (devicelist[i] is Gamepad)
                {
                    m_gamepad = devicelist[i] as Gamepad;
                }
                if (devicelist[i] is UnityEngine.InputSystem.Mouse)
                {
                    m_mouse = devicelist[i] as UnityEngine.InputSystem.Mouse;
                }
            }
        }
        m_isGamepad = m_playerInput.currentControlScheme != "Mouse+Keyboard";
    }


    private PlayerInput m_playerInput;
    private UnityEngine.InputSystem.Mouse m_mouse;
    private Gamepad m_gamepad;
    private bool m_isGamepad;
    private GameController m_gameController;
    private GameMap m_gameMap;
    private FramerateDisplay m_fpsScript;
    private Camera m_mainCamera;
    private MapTile m_currentTile = null;
}
