using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

// This is an interface between the game controller and the Puzzle game mode.
// This mode requires being passed information about the game state.
// This mode requires being passed its related UI transform.

public class PuzzleGame : IGameMode {

    public PuzzleGame(GameState gameStateRef, Transform modeUI)
    {
        m_gameState = gameStateRef;
        m_display = modeUI;
    }

    // Begins a puzzle game
    public void startGame()
    {
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
        m_audioParent = GameObject.FindWithTag("Audio").GetComponent<Transform>();

        m_numMice = 0;
        // We need to remove all direction tiles, and count them up into a placements object
        // We also need to count up the number of mice at the same time, as the map has already been loaded before startGame is called
        m_originalPlacements = new AvailablePlacements();
        float tileSize = m_gameMap.tileSize;
        for (int w = m_gameMap.mapWidth - 1; w >= 0; --w)
        {
            for (int h = m_gameMap.mapHeight - 1; h >= 0; --h)
            {
                MapTile tile = m_gameMap.tileAt(h * tileSize, w * tileSize);
                if (tile.improvement == MapTile.TileImprovement.Direction)
                {
                    m_originalPlacements.add(tile.improvementDirection);
                    tile.improvement = MapTile.TileImprovement.None;
                }
                if (tile.movingObject == MapTile.TileImprovement.Mouse)
                {
                    ++m_numMice;
                }
            }
        }
        m_placements = new AvailablePlacements(m_originalPlacements);
        m_currentMice = m_numMice;

        saveAutosave(ref m_resetSaveData);
        m_pauseSaveData = (byte[]) m_resetSaveData.Clone();

        m_paused = true;
        m_playing = true;
        m_gameState.mainState = GameState.State.Started_Paused;
        m_gameState.mainStateChange += onStateChange;

        m_display.gameObject.SetActive(true);
        showAvailablePlacements();
    }


    // Resets a puzzle game
    public void resetGame()
    {
        // Switching to started_paused state will load the autosave on the event, so we swap data in that case
        if (m_gameState.mainState != GameState.State.Started_Paused)
        {
            m_resetSaveData.CopyTo(m_pauseSaveData, 0);
            m_gameState.mainState = GameState.State.Started_Paused;
        }
        else
        {
            loadAutosave(m_resetSaveData);
            m_currentMice = m_numMice;
        }

        m_placements = new AvailablePlacements(m_originalPlacements);
        showAvailablePlacements();
    }


    // Ends a puzzle game
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
            CompletionTracker.markCompleted(GlobalData.currentStagePath);
            m_display.Find("CompleteMenu").gameObject.SetActive(true);
        }
        else
        {
            // We reset for the player after a period of time when they fail by "pressing pause"
            m_gameState.mainState = GameState.State.Ended_Failure;
            setStateDelayed(GameState.State.Started_Paused, m_autoResetDelay);
        }
    }


    // Places a tile if it is in the stage's list of available placements
    public void placeDirection(MapTile tile, Directions.Direction dir, int player)
    {
        if (!m_paused || !m_playing)
        {
            // We aren't in a state where placement is allowed, so just ignore the request
            return;
        }

        if (tile.improvement == MapTile.TileImprovement.Direction && tile.improvementDirection == dir)
        {
            tile.improvement = MapTile.TileImprovement.None;

            m_placements.add(dir);
        }
        else if (m_placements.get(dir) > 0)
        {
            if (tile.improvement == MapTile.TileImprovement.Direction)
            {
                m_placements.add(tile.improvementDirection);
            }
            else
            {
                tile.improvement = MapTile.TileImprovement.Direction;
            }

            tile.improvementDirection = dir;

            m_placements.remove(dir);
        }
        else
        {
            // Play a "No, you can't do this" sound?
        }

        showAvailablePlacements();
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
            m_currentMice = m_numMice;
        }
        else if (newState == GameState.State.Started_Unpaused)
        {
            m_paused = false;
            m_playing = true;
            saveAutosave(ref m_pauseSaveData);
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


    // Updates the display of available placements
    private void showAvailablePlacements()
    {
        m_display.Find("UpText").GetComponentInChildren<Text>().text = "x" + m_placements.get(Directions.Direction.North).ToString();
        m_display.Find("DownText").GetComponentInChildren<Text>().text = "x" + m_placements.get(Directions.Direction.South).ToString();
        m_display.Find("LeftText").GetComponentInChildren<Text>().text = "x" + m_placements.get(Directions.Direction.West).ToString();
        m_display.Find("RightText").GetComponentInChildren<Text>().text = "x" + m_placements.get(Directions.Direction.East).ToString();
    }


    // Creates a save of the map that will be loaded when reseting the puzzle
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


    // Loads a save of the map for reseting the puzzle
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

    private AvailablePlacements m_placements;
    private AvailablePlacements m_originalPlacements;
    private byte[] m_pauseSaveData;
    private byte[] m_resetSaveData;
    private int m_numMice = 0;
    private int m_currentMice = 0;
    private float m_autoResetDelay = 1.5f;
    private bool m_paused;
    private bool m_playing;
}
