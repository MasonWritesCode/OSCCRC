using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is an interface between the game controller and the Puzzle game mode.

public class PuzzleGame : IGameMode {

    // Begins a puzzle game
    public void startGame()
    {
        GameStage stage = GameObject.FindWithTag("GameController").GetComponent<GameStage>();

        placements = new GameStage.availablePlacements(stage.placements);
        return;
    }

    // Ends a puzzle game
    public void endGame()
    {
        return;
    }

    // Places a tile if it is in the stage's list of available placements
    public void placeDirection(MapTile tile, Directions.Direction dir)
    {
        if (tile.improvement == MapTile.TileImprovement.Direction && tile.improvementDirection == dir)
        {
            tile.improvement = MapTile.TileImprovement.None;

            placements.add(dir);
        }
        else if (placements.get(dir) > 0)
        {
            tile.improvementDirection = dir;
            tile.improvement = MapTile.TileImprovement.Direction;

            placements.remove(dir);
        }
        else
        {
            // Play a "No, you can't do this" sound?
        }
    }

    private GameStage.availablePlacements placements;
}
