﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

// This is an interface between the game controller and the Puzzle game mode.

public class PuzzleGame : IGameMode {

    // Begins a puzzle game
    public void startGame()
    {
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();

        m_placementsDisplay = GameObject.Find("PlacementsDisplay");
        m_placementsDisplay.GetComponent<Canvas>().enabled = true;

        numMice = 0;

        // We need to remove all direction tiles, and count them up into a placements object
        // We also need to count up the number of mice at the same time, as the map has already been loaded before startGame is called
        placements = new AvailablePlacements();
        float tileSize = m_gameMap.tileSize;
        for (int w = m_gameMap.mapWidth - 1; w >= 0; --w)
        {
            for (int h = m_gameMap.mapHeight - 1; h >= 0; --h)
            {
                MapTile tile = m_gameMap.tileAt(h * tileSize, w * tileSize);
                if (tile.improvement == MapTile.TileImprovement.Direction)
                {
                    placements.add(tile.improvementDirection);
                    tile.improvement = MapTile.TileImprovement.None;
                }
                if (tile.movingObject == MapTile.TileImprovement.Mouse)
                {
                    ++numMice;
                }
            }
        }

        GameMap.mouseDestroyed += checkGameEnd;
        GameMap.catDestroyed += checkGameEnd;
        GameMap.mousePlaced += registerMouse;

        m_paused = true;

        setAvailablePlacements();

        saveAutosave();

        return;
    }


    // Ends a puzzle game
    public void endGame()
    {
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
        if (m_paused)
        {
            return;
        }

        m_paused = true;

        loadAutosave();
    }


    public void unpauseGame()
    {
        if (!m_paused)
        {
            return;
        }

        m_paused = false;

        saveAutosave();
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

        // Mice can only die when paused when loading to reset, in which case we don't need to gameover
        if (m_paused)
        {
            return;
        }

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


    // Creates a save of the map that will be loaded when reseting the puzzle
    // This will be saved to memory rather than disk, so work on a map can be lost if it isn't normally saved. This was chosen because:
    //   We already had no system in place for loading the autosave file when an abnormal exit was detected,
    //   Disk operations are slow and this must be done synchronously so this prevents possible stutter when switching between editing and playtesting,
    //   I never added the autosave file to gitignore, so it was frequently making commit history messy.
    //   We can asynchronously copy the memory save to disk if we want to handle abnormal exit later and still prevent stutter
    private void saveAutosave()
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (StreamWriter sw = new StreamWriter(ms))
            {
                m_gameMap.exportMap(sw);
            }
            mapSaveData = ms.ToArray();
        }
    }


    // Loads a save of the map for reseting the puzzle
    private void loadAutosave()
    {
        using (MemoryStream ms = new MemoryStream(mapSaveData))
        {
            using (StreamReader sr = new StreamReader(ms))
            {
                m_gameMap.importMap(sr);
            }
        }
    }


    private int numMice = 0;
    private AvailablePlacements placements;
    private GameObject m_placementsDisplay;
    private bool m_paused;
    private byte[] mapSaveData;
    private GameMap m_gameMap;
}
