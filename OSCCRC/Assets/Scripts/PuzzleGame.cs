using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleGame : IGameMode {

    public void startGame()
    {
        return;
    }

    public void endGame()
    {
        return;
    }

    public void placeDirection(MapTile tile, Directions.Direction dir)
    {
        // TODO: there is a list of available tiles for puzzle mode.
        //       either grab it from a stage class member, or traverse map to count up all directional tiles.
        tile.improvement = MapTile.TileImprovement.Direction;
        tile.improvementDirection = dir;
    }
}
