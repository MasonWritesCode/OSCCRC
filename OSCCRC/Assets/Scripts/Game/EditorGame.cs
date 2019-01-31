using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// This is an interface between the game controller and the Puzzle game mode.
// This mode requires being passed information about the game state.

public class EditorGame : IGameMode
{

    public EditorGame(GameState gameStateRef)
    {
        m_gameState = gameStateRef;
    }

    // Begins a puzzle game
    public void startGame()
    {
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();

        m_saveMenu  = GameObject.Find("EditorMenu");
        m_saveMenu.GetComponent<Canvas>().enabled = true;

        m_gameMap.mouseDestroyed += checkGameEnd;
        m_gameMap.catDestroyed += checkGameEnd;
        m_gameMap.mousePlaced += registerMouse;

        numMice = 0;

        // We need to count the number of mice so that we know if a puzzle has been solved
        float tileSize = m_gameMap.tileSize;
        for (int w = m_gameMap.mapWidth - 1; w >= 0; --w)
        {
            for (int h = m_gameMap.mapHeight - 1; h >= 0; --h)
            {
                MapTile tile = m_gameMap.tileAt(h * tileSize, w * tileSize);
                if (tile.movingObject == MapTile.TileImprovement.Mouse)
                {
                    ++numMice;
                }
            }
        }

        saveAutosave();

        m_paused = true;
        m_gameState.mainState = GameState.State.Started_Paused;
        m_gameState.mainStateChange += onStateChange;

        return;
    }


    // Ends a puzzle game
    public void endGame()
    {
        m_gameMap.mouseDestroyed -= checkGameEnd;
        m_gameMap.catDestroyed -= checkGameEnd;
        m_gameMap.mousePlaced -= registerMouse;
        m_gameState.mainStateChange -= onStateChange;
    }


    // TODO: Temporary and needs to be addressed
    public void endGame(bool victory)
    {
        // pause?

        // UI WIN / LOSE screen thing

        return;
    }


    // Places a tile if it is in the stage's list of available placements
    public void placeDirection(MapTile tile, Directions.Direction dir)
    {
        // We only place tiles in edit mode (handled by Editor.cs), not during play
        return;
    }


    private void onStateChange(GameState.State oldState, GameState.State newState)
    {
        if (newState == GameState.State.Started_Paused)
        {
            m_paused = true;
            loadAutosave();
        }
        else if (newState == GameState.State.Started_Unpaused)
        {
            m_paused = false;
            saveAutosave();
        }
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

        if (gm.isCat)
        {
            if (gm.tile.improvement == MapTile.TileImprovement.Goal)
            {
                Debug.Log("Cat hit goal, you lose.");
                endGame(false);
            }
        }
        else
        {
            if (gm.tile.improvement != MapTile.TileImprovement.Goal)
            {
                Debug.Log("A mouse was destroyed. Game Over.");
                endGame(false);
            }
            else if (numMice <= 0)
            {
                Debug.Log("The last mouse hit a goal, you won.");
                endGame(true);
            }
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
        using (MemoryStream ms = new MemoryStream(mapSaveData))
        {
            using (StreamReader sr = new StreamReader(ms))
            {
                m_gameMap.importMap(sr);
            }
        }
    }


    private int numMice = 0;
    private GameObject m_saveMenu;
    private bool m_paused;
    private byte[] mapSaveData;
    private GameMap m_gameMap;
    private GameState m_gameState;
}
