using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public bool isPaused { get { return m_isPaused; } set { if(mode != GameMode.Multiplayer) { m_isPaused = value; } } }

    public enum GameMode { Editor, Puzzle, Multiplayer };

    public static readonly GameMode mode = GameMode.Puzzle;

    private bool m_isPaused;

    public void requestPlacement(MapTile tile, MapTile.TileImprovement improvement)
    {
        // This checks if a player is allowed to place the desired improvement, and does so if they can
        // This is different between game modes, so will have to account for that when other modes are implemented
        // For now, allow any number of directional arrow placements, editor will handle its own placement
        if (mode == GameMode.Editor)
        {
            return;
        }

        if (   improvement == MapTile.TileImprovement.Left
                 || improvement == MapTile.TileImprovement.Right
                 || improvement == MapTile.TileImprovement.Up
                 || improvement == MapTile.TileImprovement.Down
                )
        {
            tile.improvement = improvement;
        }
    }

    // Use this for initialization
    void Start () {
		isPaused = false; // temporary
        if (mode == GameMode.Editor)
        {
            isPaused = true;
        }
	}
	
	// Update is called once per frame
	void Update () {

        //
	}
}
