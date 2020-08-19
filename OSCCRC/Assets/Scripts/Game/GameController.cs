using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// This class controls various parts of gameplay and manages the state of the game.
// TODO: Get rid of the ugly mode property. Anything that needs to query the game mode should rather call a GameController function
//       which will call a function on the mode object (or if the mode object shouldn't do it then GameController should be an abstract class).

public class GameController : MonoBehaviour {

    public enum GameMode { None, Editor, Puzzle, Competitive };
    public GameMode mode { get { return m_mode; } }
    public GameState gameState { get { return m_gameState; } }
    public Transform puzzleUI;      // Editor Set
    public Transform editorUI;      // Editor Set
    public Transform competitiveUI; // Editor Set


    // Checks if a player is allowed to place the desired improvement, and does so if they can
    public void requestPlacement(MapTile tile, MapTile.TileImprovement improvement = MapTile.TileImprovement.Direction, Directions.Direction dir = Directions.Direction.North, int player = 0)
    {
        if (m_gameState.hasState(GameState.TagState.Suspended))
        {
            return;
        }

        if (improvement == MapTile.TileImprovement.Direction)
        {
            if (tile.improvement == MapTile.TileImprovement.None || tile.improvement == MapTile.TileImprovement.Direction)
            {
                m_game.placeDirection(tile, dir, player);
            }
        }
    }


    // Removes a grid mover from the game
    public void destroyMover(GridMovement mover)
    {
        m_game.destroyMover(mover);
    }


    // Begins a new game of the specified mode
    // Only call this after a new GameStage has just been loaded
    public void runGame(GameMode newMode)
    {
        if (m_game != null)
        {
            m_game.endGame();
        }

        switch (newMode)
        {
            case GameMode.Puzzle:
                m_game = new PuzzleGame(m_gameState, puzzleUI);
                break;
            case GameMode.Editor:
                m_game = new EditorGame(m_gameState, editorUI);
                break;
            case GameMode.Competitive:
                m_game = new CompetitiveGame(m_gameState, competitiveUI);
                break;
        }

        Editor editor = GetComponent<Editor>();
        if (editor)
        {
            if (newMode == GameMode.Editor && !editor.enabled)
            {
                editor.enabled = true;
            }
            else if (newMode != GameMode.Editor && editor.enabled)
            {
                editor.enabled = false;
            }
        }

        // Set up our players
        GameObject players = GameObject.FindWithTag("Player");
        if (players)
        {
            // We enable the other players for 4 player modes
            for (int i = 0; i < players.transform.childCount - 1; ++i)
            {
                GameObject playerObject = players.transform.GetChild(i).gameObject;

                // For now, set as many human players as we have input devices
                bool isHuman = GlobalData.isHumanPlayer[i];
                playerObject.GetComponent<PlayerController>().enabled =  isHuman;
                playerObject.GetComponent<PlayerInput>().enabled      =  isHuman;
                playerObject.GetComponent<AIController>().enabled     = !isHuman;

                if (i > 0)
                {
                    playerObject.SetActive(newMode == GameMode.Competitive);
                }
            }
        }

        if (newMode != GameMode.None)
        {
            m_game.startGame();
        }

        m_mode = newMode;
    }


    // Resets the game of the currently started mode
    public void resetGame()
    {
        if (m_game == null)
        {
            return;
        }

        m_game.resetGame();
    }


    void Start () {
        if (GlobalData.d_uncapFrames)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
        }

        Cursor.visible = false;

        m_eventSystem = EventSystem.current;
        m_wasInputFocused = false;

        m_gameState = new GameState();
        m_gameState.stateAdded += onTagStateAdd;
        m_gameState.stateRemoved += onTagStateRemove;

        GameStage stage = GetComponent<GameStage>();
        string currentStage = GlobalData.currentStagePath;
        if (currentStage != null)
        {
            stage.loadStage(currentStage.Remove(currentStage.Length - ".stage".Length));
        }
        else
        {
            stage.loadStage("Internal/default");
        }

        runGame(GlobalData.mode);
    }


    void Update()
    {
        // Unity does not have an event for a change in the currently selected game object, so we have to do a poll...
        bool isInputFocused = false;
        if (m_eventSystem.currentSelectedGameObject)
        {
            InputField field = m_eventSystem.currentSelectedGameObject.GetComponent<InputField>();
            isInputFocused = field != null && field.isFocused;
        }

        if (isInputFocused != m_wasInputFocused) // focus changed, so change input focus state
        {
            m_wasInputFocused = isInputFocused;

            if (isInputFocused && !m_gameState.hasState(GameState.TagState.InputFocused))
            {
                m_gameState.addState(GameState.TagState.InputFocused);
            }
            else if (!isInputFocused && m_gameState.hasState(GameState.TagState.InputFocused))
            {
                m_gameState.removeState(GameState.TagState.InputFocused);
            }
        }
    }


    void FixedUpdate()
    {
        // Here we manually simulate physics, so that we can avoid physics processing while paused (or possibly other scenarios)
        if (Physics.autoSimulation)
        {
            return;
        }
        // One scenario we could consider adding is to disable physics when there are no cats to collide with.
        if (m_gameState.mainState == GameState.State.Started_Paused || m_gameState.hasState(GameState.TagState.Suspended))
        {
            return;
        }

        Physics.Simulate(Time.fixedDeltaTime);
    }

    // We have to make sure we restore the time scale when the scene changes
    void OnDestroy()
    {
        Cursor.visible = true;

        if (m_pauseInstance != null)
        {
            TimeManager.removeTimePause(m_pauseInstance);
        }
    }

    // Set timescale to 0 for suspending the game
    // Animators wont animate, but will still use the same resources, so scripts attached to an animated object should consider disabling the animation on this event
    private void onTagStateAdd(GameState.TagState state)
    {
        if (state == GameState.TagState.Suspended)
        {
            Cursor.visible = true;

            m_pauseInstance = TimeManager.addTimePause();
        }
    }

    // Removes the actions of suspending the game
    // Scripts should avoid doing things while the game is suspended, so that their actions aren't undone by the unsuspend
    private void onTagStateRemove(GameState.TagState state)
    {
        if (state == GameState.TagState.Suspended)
        {
            Cursor.visible = false;

            if (m_pauseInstance != null)
            {
                TimeManager.removeTimePause(m_pauseInstance);
                m_pauseInstance = null;
            }
        }
    }


    private EventSystem m_eventSystem;
    private GameState m_gameState;
    private PauseInstance m_pauseInstance = null;

    private GameMode m_mode = GameMode.None;
    private IGameMode m_game;
    private bool m_wasInputFocused;
}
