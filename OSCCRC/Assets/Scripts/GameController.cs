using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    // isPaused needs to be moved to IGameMode probably
	public bool isPaused { get { return m_isPaused; } set { if(mode != GameMode.Multiplayer) { m_isPaused = value; } } }

    public enum GameMode { None, Editor, Puzzle, Multiplayer };

    //public static readonly GameMode mode = GameMode.Puzzle;
    public static readonly GameMode mode = GameMode.Editor;

    private IGameMode game;
    private bool m_isPaused;

    public void requestPlacement(MapTile tile, MapTile.TileImprovement improvement = MapTile.TileImprovement.Direction, Directions.Direction dir = Directions.Direction.North)
    {
        // This checks if a player is allowed to place the desired improvement, and does so if they can
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

        //added for testing purposes. To be removed
        GameMap map = GameObject.FindWithTag("Map").GetComponent<GameMap>();
        if (mode == GameMode.Puzzle)
        {
            map.loadMap("dev");
        }
        /*
        map.placeMouse(3, 2, Directions.Direction.North);
        map.placeMouse(3, 2, Directions.Direction.East);
        map.placeMouse(4, 5, Directions.Direction.South);
        map.placeMouse(6, 2, Directions.Direction.West);
        map.tileAt(new Vector3(5, 0, 8)).walls.east = true;
        map.tileAt(new Vector3(5, 0, 8)).walls.south = true;
        map.tileAt(new Vector3(4, 0, 0)).walls.south = false;
        map.tileAt(new Vector3(0, 0, 5)).walls.west = false;
        // */
        //
    }

    // Update is called once per frame
    void Update () {

        //
	}
}
