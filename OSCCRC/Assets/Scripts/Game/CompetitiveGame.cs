using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is an interface between the game controller and the Competitive game mode.
// This mode requires being passed information about the game state.

public class CompetitiveGame : IGameMode
{
    public CompetitiveGame(GameState gameStateRef)
    {
        m_gameState = gameStateRef;
    }

    // Begins a puzzle game
    public void startGame()
    {
        // TODO
        return;
    }


    public void resetGame()
    {
        // TODO
        return;
    }


    public void endGame()
    {
        // TODO
        return;
    }


    public void placeDirection(MapTile tile, Directions.Direction dir, int playerID)
    {
        if (tile.improvement != MapTile.TileImprovement.None)
        {
            // Players cannot change an already placed tile
            return;
        }

        const int maxPlacements = 3;

        Player player = players[playerID];

        // Players are limited to the 3 most recently placed
        if (player.placements.Count >= maxPlacements)
        {
            Placement tileToClear = player.placements.Dequeue();
            tileToClear.tile.improvement = MapTile.TileImprovement.None;
        }

        tile.owner = playerID;
        tile.improvementDirection = dir;
        tile.improvement = MapTile.TileImprovement.Direction;

        Timer ptimer = new Timer();
        ptimer.timerCompleted += () =>
        {
            // TODO: Start blinking or whatever at 1 second remaining. Would have the timer start a new timer?
            tile.improvement = MapTile.TileImprovement.None;
        };
        ptimer.startTimer(10.0f);

        Placement newPlacement;
        newPlacement.tile = tile;
        newPlacement.timer = ptimer;

        player.placements.Enqueue(newPlacement);
    }


    public void destroyMover(GridMovement deadMeat)
    {
        // TODO
        return;
    }


    private struct Placement
    {
        public MapTile tile;
        public Timer timer;
    }

    private class Player
    {
        public Queue<Placement> placements = new Queue<Placement>();
        public int score = 0;
    }

    private GameState m_gameState;
    private Player[] players = new Player[4];
}
