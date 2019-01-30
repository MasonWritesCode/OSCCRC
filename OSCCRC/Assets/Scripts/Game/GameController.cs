using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class controls various parts of gameplay and manages the state of the game.

public class GameController : MonoBehaviour {

    public enum GameMode { None, Editor, Puzzle, Multiplayer };
    public GameMode mode = GameMode.None;

    public GameState gameState;

    // Checks if a player is allowed to place the desired improvement, and does so if they can
    public void requestPlacement(MapTile tile, MapTile.TileImprovement improvement = MapTile.TileImprovement.Direction, Directions.Direction dir = Directions.Direction.North)
    {
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
        if (m_game != null)
        {
            m_game.endGame();
        }

        if (newMode == GameMode.Puzzle)
        {
            m_game = new PuzzleGame(gameState);
        }
        else if (newMode == GameMode.Editor)
        {
            m_game = new EditorGame(gameState);
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

        gameState = new GameState();

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
        if (gameState.getMainState() != GameState.State.Started_Unpaused)
        {
            return;
        }

        Physics.Simulate(Time.fixedDeltaTime);
    }

    private IGameMode m_game;
}
