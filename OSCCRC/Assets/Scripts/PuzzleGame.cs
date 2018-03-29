using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleGame : IGameMode {

    public void startGame()
    {
        GameStage stage = GameObject.FindWithTag("GameController").GetComponent<GameStage>();

        placements = new GameStage.availablePlacements(stage.placements);
        return;
    }

    public void endGame()
    {
        return;
    }

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
