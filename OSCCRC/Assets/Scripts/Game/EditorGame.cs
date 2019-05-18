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

        // We need to count the number of mice so that we know if a puzzle has been solved
        countMice();

        saveAutosave(ref m_pauseSaveData);

        m_paused = true;
        m_playing = true;
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
        if (victory)
        {
            m_gameState.mainState = GameState.State.Ended_Unpaused;
        }
        else
        {
            m_gameState.mainState = GameState.State.Ended_Paused;
        }
        // We reset for the player after a period of time when they win or lose by "pressing pause"
        setStateDelayed(GameState.State.Started_Paused, m_autoResetDelay);
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
            m_playing = true;
            loadAutosave(m_pauseSaveData);
            countMice();
        }
        else if (newState == GameState.State.Started_Unpaused)
        {
            m_paused = false;
            m_playing = true;
            saveAutosave(ref m_pauseSaveData);
        }
        else if (newState == GameState.State.Ended_Paused)
        {
            m_paused = true;
            m_playing = false;
        }
        else if (newState == GameState.State.Ended_Unpaused)
        {
            m_paused = false;
            m_playing = false;
        }
    }


    private void checkGameEnd(GameObject deadMeat)
    {
        // Mice can only die when paused when loading to reset, in which case we don't need to gameover
        if (m_paused || !m_playing)
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
                AudioSource audioData = GameObject.Find("CatGoalSound").GetComponent<AudioSource>();
                audioData.Play(0);

                Debug.Log("Cat hit goal, you lose.");
                m_gameMap.pingLocation(gm.transform.localPosition, m_autoResetDelay);
                endGame(false);
            }
        }
        else
        {
            if (gm.tile.improvement != MapTile.TileImprovement.Goal)
            {
                AudioSource audioData = GameObject.Find("MouseDiedSound").GetComponent<AudioSource>();
                audioData.Play(0);

                Debug.Log("A mouse was destroyed. Game Over.");
                m_gameMap.pingLocation(gm.transform.localPosition, m_autoResetDelay);
                endGame(false);
            }
            else
            {
                --m_currentMice;
                if (m_currentMice <= 0)
                {
                    AudioSource audioData = GameObject.Find("SuccessSound").GetComponent<AudioSource>();
                    audioData.Play(0);

                    Debug.Log("The last mouse hit a goal, you won.");
                    endGame(true);
                }
                else
                {
                    AudioSource audioData = GameObject.Find("MouseGoalSound").GetComponent<AudioSource>();
                    audioData.Play(0);
                }
            }
        }
    }


    // Updates the count of the number of mice on the map
    private void countMice()
    {
        m_numMice = 0;

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


    // Changes the state of m_gameState after the specified delay
    private void setStateDelayed(GameState.State state, float delayInSeconds)
    {
        // We have to rely on a closure to pass data to the timer event as far as I know
        // Because of this we are spawning a new timer to clear the lambda subscriber
        // This is very ugly but it will have to do for now
        m_timer = new Timer();
        m_timer.timerCompleted += () =>
        {
            m_gameState.mainState = state;
            m_timer = null;
        };
        m_timer.startTimer(delayInSeconds);
    }


    private int m_numMice = 0;
    private int m_currentMice = 0;
    private GameObject m_saveMenu;
    private bool m_paused;
    private bool m_playing;
    private float m_autoResetDelay = 1.5f;
    private Timer m_timer = null;
    private byte[] m_pauseSaveData;
    private GameMap m_gameMap;
    private GameState m_gameState;
}
