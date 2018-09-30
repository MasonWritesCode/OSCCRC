using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This is an interface between the game controller and the Puzzle game mode.

public class PuzzleGame : IGameMode {

    // Begins a puzzle game
    public void startGame()
    {
        m_placementsDisplay = GameObject.Find("PlacementsDisplay");
        m_placementsDisplay.GetComponent<Canvas>().enabled = true;

        GameStage stage = GameObject.FindWithTag("GameController").GetComponent<GameStage>();
        placements = new GameStage.availablePlacements(stage.placements);

        GameMap.mouseDestroyed += checkGameEnd;
        GameMap.catDestroyed += checkGameEnd;
        GameMap.mousePlaced += registerMouse;

        numMice = 0;

        setAvailablePlacements();

        return;
    }

    // Ends a puzzle game
    public void endGame()
    {
        // pause?

        return;
    }

    public void endGame(bool victory)
    {
        // pause?

        // UI WIN / LOSE screen thing

        return;
    }

    public void pauseGame()
    {
        //
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

        setAvailablePlacements();
    }

    private void checkGameEnd(GameObject deadMeat)
    {
        --numMice;

        GridMovement gm = deadMeat.GetComponent<GridMovement>();
        if (!gm || !gm.tile)
        {
            return;
        }

        if (!gm.isCat && gm.tile.improvement != MapTile.TileImprovement.Goal)
        {
            Debug.Log("A mouse was destroyed. Game Over.");
            endGame(false);
        }
        else if (!gm.isCat && numMice <= 0)
        {
            Debug.Log("The last mouse hit a goal, you won.");
            endGame(false);
        }
        else if (gm.isCat && gm.tile.improvement == MapTile.TileImprovement.Goal)
        {
            Debug.Log("Cat hit goal, you lose.");
            endGame(false);
        }
    }

    private void registerMouse(GameObject mouse)
    {
        ++numMice;
    }

    private void setAvailablePlacements()
    {
        m_placementsDisplay.transform.Find("UpText").GetComponentInChildren<Text>().text = "x" + placements.get(Directions.Direction.North).ToString();
        m_placementsDisplay.transform.Find("DownText").GetComponentInChildren<Text>().text = "x" + placements.get(Directions.Direction.South).ToString();
        m_placementsDisplay.transform.Find("LeftText").GetComponentInChildren<Text>().text = "x" + placements.get(Directions.Direction.West).ToString();
        m_placementsDisplay.transform.Find("RightText").GetComponentInChildren<Text>().text = "x" + placements.get(Directions.Direction.East).ToString();
    }

    private int numMice = 0;
    private GameStage.availablePlacements placements;
    private GameObject m_placementsDisplay;
}
