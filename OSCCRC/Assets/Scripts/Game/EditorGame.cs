﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// This is an interface between the game controller and the Puzzle game mode.

public class EditorGame : IGameMode
{

    // Begins a puzzle game
    public void startGame()
    {
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();

        m_saveMenu  = GameObject.Find("EditorMenu");
        m_saveMenu.GetComponent<Canvas>().enabled = true;

        GameStage stage = GameObject.FindWithTag("GameController").GetComponent<GameStage>();
        placements = new GameStage.availablePlacements(stage.placements);

        GameMap.mouseDestroyed += checkGameEnd;
        GameMap.catDestroyed += checkGameEnd;
        GameMap.mousePlaced += registerMouse;

        numMice = 0;

        m_paused = true;

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

        loadAutosave();

        m_paused = true;
    }


    public void unpauseGame()
    {
        if (!m_paused)
        {
            return;
        }

        saveAutosave();

        m_paused = false;
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


    // Creates a save of the map that will be loaded when going from playtest back to editor
    // This will be saved to memory rather than disk, so work on a map can be lost if it isn't normally saved. This was chosen because:
    //   We already had no system in place for loading the autosave file when an abnormal exit was detected,
    //   Disk operations are slow and this must be done synchronously so this prevents possible stutter when switching between editing and playtesting,
    //   I never added the autosave file to gitignore, so it was frequently making commit history messy.
    //   We can asynchronously copy the memory save to disk if we want to handle abnormal exit later and still prevent stutter
    private void saveAutosave()
    {
        //m_gameMap.saveMap("_editorAuto");
        using (MemoryStream ms = new MemoryStream())
        {
            using (StreamWriter sw = new StreamWriter(ms))
            {
                m_gameMap.exportMap(sw);
            }
            mapSaveData = ms.ToArray();
        }
    }


    // Loads a save of the map for when going from playtest back to editor
    private void loadAutosave()
    {
        //m_gameMap.loadMap("_editorAuto");
        using (MemoryStream ms = new MemoryStream(mapSaveData))
        {
            using (StreamReader sr = new StreamReader(ms))
            {
                m_gameMap.importMap(sr);
            }
        }
    }


    private int numMice = 0;
    private GameStage.availablePlacements placements;
    private GameObject m_saveMenu;
    private bool m_paused;
    private byte[] mapSaveData;
    private GameMap m_gameMap;
}
