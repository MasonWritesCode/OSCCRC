using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class controls various parts of gameplay and manages the state of the game.

public class GameController : MonoBehaviour {

    public delegate void toggleEvent(bool value);
    public static event toggleEvent gamePauseChange;

    public enum GameMode { None, Editor, Puzzle, Multiplayer };
    public GameMode mode = GameMode.None;

    // Sets and returns the game pause state, and throws an event if it changes
    public bool isPaused
    {
        get
        {
            m_isPaused = m_game.isPaused;
            return m_isPaused;
        }
        set
        {
            if (m_isPaused != value)
            {
                m_game.isPaused = m_isPaused = value;
                if (gamePauseChange != null)
                {
                    gamePauseChange(value);
                }
            }
        }
    }

    


    // Checks if a player is allowed to place the desired improvement, and does so if they can
    public void requestPlacement(MapTile tile, MapTile.TileImprovement improvement = MapTile.TileImprovement.Direction, Directions.Direction dir = Directions.Direction.North)
    {
        if (!isPaused)
        {
            return;
        }

        if (improvement == MapTile.TileImprovement.Direction)
        {
            if (tile.improvement == MapTile.TileImprovement.None || tile.improvement == MapTile.TileImprovement.Direction)
            {
                m_game.placeDirection(tile, dir);
            }
        }
    }

    public void runGame(GameMode newMode)
    {
        /*
        if (game != null)
        {
            game.endGame();
        }
        */

        if (newMode == GameMode.Puzzle)
        {
            m_game = new PuzzleGame();
        }
        else if (newMode == GameMode.Editor)
        {
            m_game = new EditorGame();
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

        if (newMode != GameMode.None)
        {
            m_game.startGame();
        }

        mode = newMode;
    }


    void Start () {
        if (GlobalData.d_uncapFrames)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
        }

        GameStage stage = GetComponent<GameStage>();
        string currentStage = GlobalData.currentStageFile;
        if (currentStage != null)
        {
            stage.loadStage(currentStage);
        }
        else
        {
            stage.loadStage("Internal/default");
        }

        m_isPaused = true;

        runGame(GlobalData.mode);
    }


    void FixedUpdate()
    {
        // Here we manually simulate physics, so that we can avoid physics processing while paused (or possibly other scenarios)
        if (Physics.autoSimulation)
        {
            return;
        }
        // One scenario we could consider adding is to disable physics when there are no cats to collide with.
        if (m_isPaused)
        {
            return;
        }

        Physics.Simulate(Time.fixedDeltaTime);
    }


    // Update is called once per frame
    void Update () {

        // Unfortunately we have this messy poll for pause state change initiated by the game mode.
        // This is because we don't want the game mode to talk to the game controller; communication should be one way only.
        if (m_isPaused != m_game.isPaused)
        {
            isPaused = m_game.isPaused;
        }
	}

    private IGameMode m_game;
    private bool m_isPaused; // Needed to check if value is equal to the game mode's
}
