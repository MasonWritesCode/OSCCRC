using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class controls various parts of gameplay and manages the state of the game.

public class GameController : MonoBehaviour {

    // isPaused needs to be moved to IGameMode probably
	public bool isPaused { get { return m_isPaused; } set { game.pauseGame();  m_isPaused = value; } }

    public enum GameMode { None, Editor, Puzzle, Multiplayer };

    //public static readonly GameMode mode = GameMode.Puzzle;
    public static readonly GameMode mode = GameMode.Editor;

    private IGameMode game;
    private bool m_isPaused;


    // Checks if a player is allowed to place the desired improvement, and does so if they can
    public void requestPlacement(MapTile tile, MapTile.TileImprovement improvement = MapTile.TileImprovement.Direction, Directions.Direction dir = Directions.Direction.North)
    {
        if (isPaused)
        {
            return;
        }

        if (improvement == MapTile.TileImprovement.Direction)
        {
            game.placeDirection(tile, dir);
        }
    }


    void Start () {
        /*
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        */

        string currentStage = GlobalData.currentStageFile;
        if (currentStage != null)
        {
            GameStage stage = GetComponent<GameStage>();
            stage.loadStage(currentStage);
        }

		m_isPaused = true;

        if (mode == GameMode.Editor || mode == GameMode.Puzzle)
        {
            game = new PuzzleGame();

            if (mode == GameMode.Editor)
            {
                Editor editor = GetComponent<Editor>();
                if (editor && !editor.enabled)
                {
                    editor.enabled = true;
                }
            }
        }
        game.startGame();
    }


    // Update is called once per frame
    void Update () {

        //
	}
}
