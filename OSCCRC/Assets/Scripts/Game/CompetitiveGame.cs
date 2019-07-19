using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    public void placeDirection(MapTile tile, Directions.Direction dir)
    {
        // TODO
        return;
    }


    public void destroyMover(GridMovement deadMeat)
    {
        // TODO
        return;
    }

    private GameState m_gameState;
}
