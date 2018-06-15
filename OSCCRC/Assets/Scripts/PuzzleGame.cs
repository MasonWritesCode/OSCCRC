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

        GameMap.mouseDestroyed += checkGameEnd;

        return;
    }

    // Ends a puzzle game
    public void endGame()
    {
        // pause?

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

    private void checkGameEnd(GameObject deadMouse)
    {
        Debug.Log("Should check if victory or failure and call endGame() if so");

        if (deadMouse.GetComponent<GridMovement>().tile.improvement != MapTile.TileImprovement.Goal)
        {
            Debug.Log("A mouse was destroyed. Game Over.");
            endGame();
        }
        else
        {
            Debug.Log("A mouse hit a goal. Need to check if it was the last mouse to know if victory.");
        }
    }

    private GameStage.availablePlacements placements;
}
