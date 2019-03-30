using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

// This is an interface between the game controller and the Puzzle game mode.
// This mode requires being passed information about the game state.

public class PuzzleGame : IGameMode {

    public PuzzleGame(GameState gameStateRef)
    {
        m_gameState = gameStateRef;
    }

    // Begins a puzzle game
    public void startGame()
    {
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();

        m_placementsDisplay = GameObject.Find("PlacementsDisplay (UI)");
        m_placementsDisplay.GetComponent<Canvas>().enabled = true;

        m_numMice = 0;

        // We need to remove all direction tiles, and count them up into a placements object
        // We also need to count up the number of mice at the same time, as the map has already been loaded before startGame is called
        m_placements = new AvailablePlacements();
        float tileSize = m_gameMap.tileSize;
        for (int w = m_gameMap.mapWidth - 1; w >= 0; --w)
        {
            for (int h = m_gameMap.mapHeight - 1; h >= 0; --h)
            {
                MapTile tile = m_gameMap.tileAt(h * tileSize, w * tileSize);
                if (tile.improvement == MapTile.TileImprovement.Direction)
                {
                    m_placements.add(tile.improvementDirection);
                    tile.improvement = MapTile.TileImprovement.None;
                }
                if (tile.movingObject == MapTile.TileImprovement.Mouse)
                {
                    ++m_numMice;
                }
            }
        }
        m_currentMice = m_numMice;

        m_gameMap.mouseDestroyed += checkGameEnd;
        m_gameMap.catDestroyed += checkGameEnd;

        setAvailablePlacements();
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
        m_gameState.mainStateChange -= onStateChange;
    }


    // TODO: Temporary and needs to be addressed
    public void endGame(bool victory)
    {
        if (victory)
        {
            m_gameState.mainState = GameState.State.Ended;
        }
        return;
    }


    // Places a tile if it is in the stage's list of available placements
    public void placeDirection(MapTile tile, Directions.Direction dir)
    {
        if (!m_paused)
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

        setAvailablePlacements();
    }


    private void onStateChange(GameState.State oldState, GameState.State newState)
    {
        if (newState == GameState.State.Started_Paused)
        {
            m_paused = true;
            loadAutosave();
            m_currentMice = m_numMice;
        }
        else if (newState == GameState.State.Started_Unpaused)
        {
            m_paused = false;
            saveAutosave();
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
            --m_currentMice;

            if (gm.tile.improvement != MapTile.TileImprovement.Goal)
            {
                Debug.Log("A mouse was destroyed. Game Over.");
                endGame(false);
            }
            else if (m_currentMice <= 0)
            {
                Debug.Log("The last mouse hit a goal, you won.");
                endGame(true);
            }
        }
    }


    private void setAvailablePlacements()
    {
        m_placementsDisplay.transform.Find("UpText").GetComponentInChildren<Text>().text = "x" + m_placements.get(Directions.Direction.North).ToString();
        m_placementsDisplay.transform.Find("DownText").GetComponentInChildren<Text>().text = "x" + m_placements.get(Directions.Direction.South).ToString();
        m_placementsDisplay.transform.Find("LeftText").GetComponentInChildren<Text>().text = "x" + m_placements.get(Directions.Direction.West).ToString();
        m_placementsDisplay.transform.Find("RightText").GetComponentInChildren<Text>().text = "x" + m_placements.get(Directions.Direction.East).ToString();
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


    private int m_numMice = 0;
    private int m_currentMice = 0;
    private AvailablePlacements m_placements;
    private GameObject m_placementsDisplay;
    private bool m_paused;
    private byte[] mapSaveData;
    private GameMap m_gameMap;
    private GameState m_gameState;
}
