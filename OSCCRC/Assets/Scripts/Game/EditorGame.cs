using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// This is an interface between the game controller and the Puzzle game mode.
// This mode requires being passed information about the game state.
// This mode requires being passed its related UI transform.

public class EditorGame : IGameMode
{
    public EditorGame(GameState gameStateRef, Transform modeUI)
    {
        m_gameState = gameStateRef;
        m_display = modeUI;
    }

    // Begins a puzzle game
    public void startGame()
    {
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
        m_audioParent = GameObject.FindWithTag("Audio").GetComponent<Transform>();

        // We need to count the number of mice so that we know if a puzzle has been solved
        countMice();

        saveAutosave(ref m_pauseSaveData);

        m_paused = true;
        m_playing = true;
        m_gameState.mainState = GameState.State.Started_Paused;
        m_gameState.mainStateChange += onStateChange;

        m_display.GetComponent<Canvas>().enabled = true;

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
        m_gameState.mainStateChange -= onStateChange;

        if (m_timer != null)
        {
            m_timer.Dispose();
        }
    }


    // TODO: Temporary and needs to be addressed
    public void endGame(bool victory)
    {
        if (victory)
        {
            m_gameState.mainState = GameState.State.Ended_Victory;
        }
        else
        {
            m_gameState.mainState = GameState.State.Ended_Failure;
        }
        // We reset for the player after a period of time when they win or lose by "pressing pause"
        setStateDelayed(GameState.State.Started_Paused, m_autoResetDelay);
    }


    // Places a tile if it is in the stage's list of available placements
    public void placeDirection(MapTile tile, Directions.Direction dir, int player)
    {
        // We only place tiles in edit mode (handled by Editor.cs), not during play
        return;
    }


    // Checks if a grid mover should be destroyed, and checks for game over
    public void destroyMover(GridMovement deadMeat)
    {
        if (!deadMeat || !deadMeat.tile)
        {
            return;
        }

        if (m_paused || !m_playing)
        {
            m_gameMap.destroyMover(deadMeat);
            return;
        }

        if (deadMeat is Cat)
        {
            if (deadMeat.tile.improvement == MapTile.TileImprovement.Goal)
            {
                AudioSource audioData = m_audioParent.Find("CatGoalSound").GetComponent<AudioSource>();
                audioData.Play(0);

                m_gameMap.pingLocation(deadMeat.transform.localPosition, m_autoResetDelay);
                endGame(false);
            }
        }
        else
        {
            if (deadMeat.tile.improvement != MapTile.TileImprovement.Goal)
            {
                AudioSource audioData = m_audioParent.Find("MouseDiedSound").GetComponent<AudioSource>();
                audioData.Play(0);

                m_gameMap.pingLocation(deadMeat.transform.localPosition, m_autoResetDelay);
                endGame(false);
            }
            else
            {
                --m_currentMice;
                m_gameMap.destroyMover(deadMeat);
                if (m_currentMice <= 0)
                {
                    AudioSource audioData = m_audioParent.Find("SuccessSound").GetComponent<AudioSource>();
                    audioData.Play(0);

                    endGame(true);
                }
                else
                {
                    AudioSource audioData = m_audioParent.Find("MouseGoalSound").GetComponent<AudioSource>();
                    audioData.Play(0);
                }
            }
        }
    }



    private void onStateChange(GameState.State oldState, GameState.State newState)
    {
        if (m_timer != null)
        {
            m_timer.stopTimer();
        }

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
            countMice();
        }
        else if (newState == GameState.State.Ended_Failure)
        {
            m_paused = true;
            m_playing = false;
        }
        else if (newState == GameState.State.Ended_Victory)
        {
            m_paused = false;
            m_playing = false;
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
            if (m_gameState.mainState != state)
            {
                m_gameState.mainState = state;
            }
            m_timer = null;
        };
        m_timer.startTimer(delayInSeconds);
    }


    private Transform m_display;
    private Transform m_audioParent;
    private GameMap m_gameMap;
    private GameState m_gameState;
    private Timer m_timer = null;

    private byte[] m_pauseSaveData;
    private int m_numMice = 0;
    private int m_currentMice = 0;
    private float m_autoResetDelay = 1.5f;
    private bool m_paused;
    private bool m_playing;
}
