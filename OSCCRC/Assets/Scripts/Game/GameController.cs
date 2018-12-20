using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class controls various parts of gameplay and manages the state of the game.

public class GameController : MonoBehaviour {

    // isPaused needs to be moved to IGameMode probably
	public bool isPaused
    {
        get
        {
            return m_isPaused;
        }
        set
        {
            m_isPaused = value;
            if (value)
            {
                game.pauseGame();
            }
            else
            {
                game.unpauseGame();
            }
        }
    }

    public enum GameMode { None, Editor, Puzzle, Multiplayer };

    //public static readonly GameMode mode = GameMode.Puzzle;
    public static GameMode mode = GameMode.None;

    private IGameMode game;
    private bool m_isPaused;


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
                game.placeDirection(tile, dir);
            }
        }
    }

    public void runGame(GameMode mode)
    {
        /*
        if (game != null)
        {
            game.endGame();
        }
        */

        if (mode == GameMode.Puzzle)
        {
            game = new PuzzleGame();
        }
        else if (mode == GameMode.Editor)
        {
            game = new EditorGame();
        }

        Editor editor = GetComponent<Editor>();
        if (editor)
        {
            if (mode == GameMode.Editor && !editor.enabled)
            {
                editor.enabled = true;
            }
            else if (mode != GameMode.Editor && editor.enabled)
            {
                editor.enabled = false;
            }
        }

        if (mode != GameMode.None)
        {
            game.startGame();
        }
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


    // Update is called once per frame
    void Update () {

        //
	}
}
