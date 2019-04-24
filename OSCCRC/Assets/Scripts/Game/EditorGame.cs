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

        m_gameMap.mouseDestroyed += checkGameEnd;
        m_gameMap.catDestroyed += checkGameEnd;

        m_numMice = 0;

        // We need to count the number of mice so that we know if a puzzle has been solved
        float tileSize = m_gameMap.tileSize;
        for (int w = m_gameMap.mapWidth - 1; w >= 0; --w)
        {
            for (int h = m_gameMap.mapHeight - 1; h >= 0; --h)
            {
                MapTile tile = m_gameMap.tileAt(h * tileSize, w * tileSize);
                if (tile.movingObject == MapTile.TileImprovement.Mouse)
                {
                    ++m_numMice;
                }
            }
        }
        m_currentMice = m_numMice;

        saveAutosave(ref m_pauseSaveData);

        m_paused = true;
        m_gameState.mainState = GameState.State.Started_Paused;
        m_gameState.mainStateChange += onStateChange;

        m_saveMenu = GameObject.Find("EditorMenu (UI)");
        m_saveMenu.GetComponent<Canvas>().enabled = true;

        return;
    }


    // Resets an editor mode game
    public void resetGame()
    {
        // For editor mode, we just need to swap to started_paused state. If it is already paused, we don't have to do anything.
        // I would rather it be GUI only to reset the map to blank so that it isn't easy to do on accient. So this will just unpause I think.
        if (m_gameState.mainState != GameState.State.Started_Paused)
        {
            m_gameState.mainState = GameState.State.Started_Paused;
        }
    }


    // Ends an editor mode game
    public void endGame()
    {
        m_gameMap.mouseDestroyed -= checkGameEnd;
        m_gameMap.catDestroyed -= checkGameEnd;
        m_gameState.mainStateChange -= onStateChange;
    }


    // TODO: Temporary and needs to be addressed
    public void endGame(bool victory)
    {
        // We reset for the player after a period of time when they win or lose
        if (!victory)
        {
            m_gameMap.pingLocation(m_deadMouse.transform.localPosition, 1.5f);
        }
        m_gameState.mainState = GameState.State.Started_Paused;
        loadAutosaveDelayed(m_pauseSaveData, 1.5f);
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

            if (m_deadMouse == null)
            {
                // We don't want to immediately load the map when the player has failed so that we can show what failed
                // For now, we will just ignore it here and let the fail-triggered actions reload for us, even though it is ugly
                loadAutosave(m_pauseSaveData);
            }
            m_currentMice = m_numMice;
        }
        else if (newState == GameState.State.Started_Unpaused)
        {
            m_paused = false;
            saveAutosave(ref m_pauseSaveData);
        }
    }


    private void checkGameEnd(GameObject deadMeat)
    {
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

        if (gm is Cat)
        {
            if (gm.tile.improvement == MapTile.TileImprovement.Goal)
            {
                Debug.Log("Cat hit goal, you lose.");
                endGame(false);
            }
        }
        else
        {
            m_deadMouse = gm;
            if (gm.tile.improvement != MapTile.TileImprovement.Goal)
            {
                Debug.Log("A mouse was destroyed. Game Over.");
                endGame(false);
            }
            else
            {
                --m_currentMice;
                if (m_currentMice <= 0)
                {
                    Debug.Log("The last mouse hit a goal, you won.");
                    endGame(true);
                }
            }
        }
    }


    // Creates a save of the map that will be loaded when going from playtest back to editor
    // This will be saved to memory rather than disk, so work on a map can be lost if it isn't normally saved. This was chosen because:
    //   We already had no system in place for loading the autosave file when an abnormal exit was detected,
    //   Disk operations are slow and this must be done synchronously so this prevents possible stutter when switching between editing and playtesting,
    //   I never added the autosave file to gitignore, so it was frequently making commit history messy.
    //   We can asynchronously copy the memory save to disk if we want to handle abnormal exit later and still prevent stutter
    private void saveAutosave(ref byte[] saveLocation)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (StreamWriter sw = new StreamWriter(ms))
            {
                m_gameMap.exportMap(sw);
            }
            saveLocation = ms.ToArray();
        }
    }


    // Loads a save of the map for when going from playtest back to editor
    private void loadAutosave(byte[] saveLocation)
    {
        using (MemoryStream ms = new MemoryStream(saveLocation))
        {
            using (StreamReader sr = new StreamReader(ms))
            {
                m_gameMap.importMap(sr);
            }
        }
    }


    // Loads a save after the specified time has elapsed
    private void loadAutosaveDelayed(byte[] saveLocation, float delayInSeconds)
    {
        // We have to rely on a closure to pass data to the timer event as far as I know
        // Because of this we are spawning a new timer to clear the lambda subscriber
        // This is very ugly but it will have to do for now
        m_timer = new Timer();
        m_timer.timerCompleted += () =>
        {
            loadAutosave(saveLocation);
            m_timer = null;
        };
        m_timer.startTimer(delayInSeconds);
    }


    private int m_numMice = 0;
    private int m_currentMice = 0;
    private GameObject m_saveMenu;
    private bool m_paused;
    private GridMovement m_deadMouse = null;
    private Timer m_timer = null;
    private byte[] m_pauseSaveData;
    private GameMap m_gameMap;
    private GameState m_gameState;
}
