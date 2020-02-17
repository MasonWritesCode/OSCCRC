using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This is an interface between the game controller and the 4 player Competitive game mode.
// This mode requires being passed information about the game state.
// This mode requires being passed its related UI transform.

// TODO: Limit on number of spawned mice
// TODO: Modifier removes previous, i.e. Place Arrows Again will end a Slow Down

public class CompetitiveGame : IGameMode
{
    public CompetitiveGame(GameState gameStateRef, Transform modeUI)
    {
        m_gameState = gameStateRef;
        m_display = modeUI;

        m_modifierFunctions = new ModifierFunction[] {
            new ModifierFunction(beginMouseMania), null, new ModifierFunction(beginCatMania), null,
            new ModifierFunction(removePlacements), null, null, new ModifierFunction(beginSlowMode)
        };
        Debug.Assert(m_modifierFunctions.Length == m_modifierText.Length);
    }

    // Begins a puzzle game
    public void startGame()
    {
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
        m_audioParent = GameObject.FindWithTag("Audio").GetComponent<Transform>();

        // We need to get a reference to all spawners so we can spawn with them
        float tileSize = m_gameMap.tileSize;
        for (int w = m_gameMap.mapWidth - 1; w >= 0; --w)
        {
            for (int h = m_gameMap.mapHeight - 1; h >= 0; --h)
            {
                MapTile tile = m_gameMap.tileAt(h * tileSize, w * tileSize);
                if (tile.improvement == MapTile.TileImprovement.Spawner)
                {
                    m_spawnTiles.Add(tile);
                }
            }
        }
        if (m_spawnTiles.Count == 0)
        {
            Debug.LogWarning("No spawn tiles found. Competitive mode opened on non-competitive map");
        }

        m_display.GetComponent<Canvas>().enabled = true;
        m_timerText = m_display.Find("Timer").GetComponentInChildren<Text>();

        for (int i = 0; i < m_players.Length; ++i)
        {
            m_players[i] = new Player(i, m_display.Find("Score" + i).GetComponentInChildren<Text>());
            m_players[i].playerName = "Player " + (i + 1);
        }

        m_gameState.mainState = GameState.State.Started_Paused;
        m_gameState.stateAdded += onTagStateAdd;
        m_gameState.stateRemoved += onTagStateRemove;

        // Go ahead and set the time
        m_remainingTime = 181;
        tickGameTimer();

        // TODO: Show stuff right before game starts
        m_startCountdown.timerCompleted += () => {
            m_gameState.mainState = GameState.State.Started_Unpaused;

            // TODO: Maybe flashy animation on timer to show it has started?

            m_gameTimer.timerCompleted += finishGame;
            m_gameTimer.timerUpdate += tickGameTimer;
            m_gameTimer.startTimerWithUpdate(180.0f, 1.0f);

            beginNormalSpawn();
        };
        m_startCountdown.startTimer(3.0f);
    }


    public void resetGame()
    {
        Debug.LogWarning("Competitive Mode was told to reset but does not support resetting");
    }


    // Ends a competitive game
    public void endGame()
    {
        if (m_catSpawnTimer != null)
        {
            m_catSpawnTimer.Dispose();
        }
        if (m_mouseSpawnTimer != null)
        {
            m_mouseSpawnTimer.Dispose();
        }
        if (m_spawnFrequencyTimer != null)
        {
            m_spawnFrequencyTimer.Dispose();
        }
        if (m_modifierTimer != null)
        {
            m_modifierTimer.Dispose();
        }

        m_gameTimer.Dispose();
        m_startCountdown.Dispose();

        m_gameState.stateAdded -= onTagStateAdd;
        m_gameState.stateRemoved -= onTagStateRemove;
    }


    public void placeDirection(MapTile tile, Directions.Direction dir, int playerID)
    {
        if (tile.improvement != MapTile.TileImprovement.None || m_gameState.mainState != GameState.State.Started_Unpaused)
        {
            // Players cannot change an already placed tile or place before game starts
            return;
        }

        const int maxPlacements = 3;
        Player player = m_players[playerID];

        // Players are limited to the 3 most recently placed
        if (player.placementCount >= maxPlacements)
        {
            player.dequeuePlacement();
        }

        tile.owner = playerID;
        tile.improvementDirection = dir;
        tile.improvement = MapTile.TileImprovement.Direction;

        Timer ptimer = new Timer();
        ptimer.timerUpdate += () =>
        {
            if (tile.improvement != MapTile.TileImprovement.Direction)
            {
                // A cat destroyed the direction tile, so we just stop the blinking timer
                ptimer.stopTimer();
            }
            else
            {
                m_gameMap.blinkTile(tile, 1.0f);
            }
        };
        ptimer.timerCompleted += () =>
        {
            tile.improvement = MapTile.TileImprovement.None;
        };
        ptimer.startTimerWithUpdate(10.0f, 9.0f);

        Placement newPlacement;
        newPlacement.tile = tile;
        newPlacement.timer = ptimer;

        player.enqueuePlacement(newPlacement);
    }


    public void destroyMover(GridMovement deadMeat)
    {
        if (deadMeat is Cat)
        {
            if (deadMeat.tile.improvement == MapTile.TileImprovement.Goal)
            {
                AudioSource audioData = m_audioParent.Find("CatGoalSound").GetComponent<AudioSource>();
                audioData.Play(0);

                int owner = deadMeat.tile.owner;
                // Here we don't want to use integer division, because we don't want to round down.
                m_players[owner].score = m_players[owner].score * 2 / 3;
            }

            m_gameMap.destroyMover(deadMeat);
            --m_catCounter;
        }
        else if (deadMeat is Mouse)
        {
            if (deadMeat.tile.improvement == MapTile.TileImprovement.Goal)
            {
                AudioSource audioData = m_audioParent.Find("MouseGoalSound").GetComponent<AudioSource>();
                audioData.Play(0);

                int owner = deadMeat.tile.owner;
                int newScore = m_players[owner].score;

                if (deadMeat is BigMouse)
                {
                    newScore += 50;
                }
                else
                {
                    // Not sure if special mice are supposed to increase score or not
                    newScore += 1;

                    if (deadMeat is SpecialMouse)
                    {
                        addRandomGameModifier();
                    }
                }

                m_players[owner].score = Mathf.Min(newScore, 999);
            }

            if (deadMeat is SpecialMouse)
            {
                --m_specialMouseCounter;
            }

            m_gameMap.destroyMover(deadMeat);
        }
    }



    private struct Placement
    {
        public MapTile tile;
        public Timer timer;
    }

    private class Player
    {
        // ID_num is the Maptile.owner ID
        public Player(int ID_num, Text textObj)
        {
            ID = ID_num;
            m_scoreText = textObj;
        }

        public int score { get { return m_score; } set { m_score = value; m_scoreText.text = string.Format("{0:000}", value); } }
        public int placementCount { get { return placements.Count; } }

        public void enqueuePlacement(Placement p)
        {
            placements.Enqueue(p);
        }

        public Placement dequeuePlacement()
        {
            Placement p = placements.Dequeue();

            p.timer.stopTimer();
            if (p.tile.improvement == MapTile.TileImprovement.Direction && p.tile.owner == ID)
            {
                p.tile.improvement = MapTile.TileImprovement.None;
            }

            return p;
        }

        public void clearPlacements()
        {
            for (int i = placements.Count; i > 0; --i)
            {
                dequeuePlacement();
            }
        }

        public int ID;
        public string playerName = "Player";

        private int m_score = 0;
        private Queue<Placement> placements = new Queue<Placement>();
        private Text m_scoreText = null;
    }


    // Spawns a cat at a random spawner
    private void spawnCat(MapTile spawn)
    {
        // We create a new timer to clear the lambda subscriber for now
        m_catSpawnTimer = new Timer();

        m_catSpawnTimer.timerCompleted += () => {
            m_gameMap.placeCat(spawn.transform.localPosition, spawn.improvementDirection);
            m_catSpawnTimer = null;
        };
        m_catSpawnTimer.startTimer(2.0f);

        ++m_catCounter;
    }


    // Spawns a mouse at a random spawner
    private void spawnMouse(MapTile spawn)
    {
        m_gameMap.placeMouse(spawn.transform.localPosition, spawn.improvementDirection);
    }


    // Spawns a 50 point mouse at a random spawner
    private void spawnBigMouse(MapTile spawn)
    {
        m_gameMap.placeBigMouse(spawn.transform.localPosition, spawn.improvementDirection);
    }


    // Spawns a special game modifier mouse at a random spawner
    private void spawnSpecialMouse(MapTile spawn)
    {
        m_gameMap.placeSpecialMouse(spawn.transform.localPosition, spawn.improvementDirection);
    }


    // Set each spawner to begin spawning mice and spawn in a cat, the normal case without special mice effects
    // I don't know how frequently mice are supposed to spawn, so it will be random averaging 3 seconds per spawner for now
    private void beginNormalSpawn()
    {
        // We always start with one cat? Might need a delayed entrance?
        spawnCat(m_spawnTiles[m_rng.Next(m_spawnTiles.Count)]);

        m_spawnFrequencyTimer = new Timer();
        m_spawnFrequencyTimer.timerUpdate += () => {
            // 1/18 chance 6 times per second for EV of 3 seconds, per spawner
            for (int i = m_spawnTiles.Count - 1; i >= 0; --i)
            {
                if (m_rng.Next(18) == 0)
                {
                    // 1 in 80 chance to spawn a 50 point or special mouse for now
                    //if (m_rng.Next(80) == 0)
                    if (m_rng.Next(8) == 0)
                    {
                        if (m_specialMouseCounter == 0 && m_rng.Next(2) == 0)
                        {
                            spawnSpecialMouse(m_spawnTiles[i]);
                            ++m_specialMouseCounter;
                        }
                        else
                        {
                            spawnBigMouse(m_spawnTiles[i]);
                        }
                    }
                    else
                    {
                        spawnMouse(m_spawnTiles[i]);
                    }
                }
            }

            // We create a new cat whenever one dies while game is running
            if (m_catCounter == 0 && m_gameState.mainState == GameState.State.Started_Unpaused)
            {
                spawnCat(m_spawnTiles[m_rng.Next(m_spawnTiles.Count)]);
            }
        };
        m_spawnFrequencyTimer.startTimerWithUpdate((float)m_remainingTime, 0.1667f);
    }


    // Randomly select a game modifier to apply
    private void addRandomGameModifier()
    {
        // For now, we have only a subset of options
        int[] availableEvents = { 0, 2, 4, 7 };
        int modifierSelection = availableEvents[m_rng.Next(availableEvents.Length)];
        GameObject modifierDisplay = m_display.Find("Event Popup").gameObject;

        modifierDisplay.GetComponentInChildren<Text>().text = m_modifierText[modifierSelection];
        modifierDisplay.SetActive(true);

        PauseInstance pause = TimeManager.addTimePause();

        m_modifierTimer = new Timer(false);
        m_modifierTimer.timerCompleted += () => {
            modifierDisplay.SetActive(false);
            TimeManager.removeTimePause(pause);

            m_modifierFunctions[modifierSelection]();
        };
        m_modifierTimer.startTimer(2.0f);
    }


    // Mouse Mania - Rapid mouse spawning for some time
    private void beginMouseMania()
    {
        m_spawnFrequencyTimer.stopTimer();

        // Remove the normal-spawn cat
        // There is no gamemap function for removing all cats, so just search the map
        Cat cat = m_gameMap.GetComponentInChildren<Cat>();
        m_gameMap.destroyMover(cat);
        m_catCounter = 0;

        m_spawnFrequencyTimer = new Timer();
        m_spawnFrequencyTimer.timerUpdate += () => {
            for (int i = m_spawnTiles.Count - 1; i >= 0; --i)
            {
                // How often do we spawn a mouse? For now, 1/2 chance per spawner
                // 1 in 80 chance to spawn a 50 point mouse for now
                if (m_rng.Next(2) == 0)
                {
                    if (m_rng.Next(80) == 0)
                    {
                        spawnBigMouse(m_spawnTiles[i]);
                    }
                    else
                    {
                        spawnMouse(m_spawnTiles[i]);
                    }
                }
            }
        };
        m_spawnFrequencyTimer.timerCompleted += () => {
            m_spawnFrequencyTimer.stopTimer();
            beginNormalSpawn();
        };
        m_spawnFrequencyTimer.startTimerWithUpdate(Mathf.Min(10.0f, (float)m_remainingTime), 0.1667f);
    }


    // Cat Mania - multiple cats and no mice for some time
    private void beginCatMania()
    {
        m_spawnFrequencyTimer.stopTimer();
        m_gameMap.destroyAllMovers();

        // We have a cat for each spawn tile
        m_catCounter = 0;
        for (int i = m_catCounter; i < m_spawnTiles.Count; ++i)
        {
            spawnCat(m_spawnTiles[i]);
        }

        // We spawn a cat each time one has been removed, on a random spawner
        m_spawnFrequencyTimer = new Timer();
        m_spawnFrequencyTimer.timerUpdate += () => {
            while (m_catCounter < m_spawnTiles.Count)
            {
                spawnCat(m_spawnTiles[m_rng.Next(m_spawnTiles.Count)]);
            }
        };
        m_spawnFrequencyTimer.timerCompleted += () => {
            m_spawnFrequencyTimer.stopTimer();
            m_gameMap.destroyAllMovers();
            m_catCounter = 0;
            m_specialMouseCounter = 0;
            beginNormalSpawn();
        };
        m_spawnFrequencyTimer.startTimerWithUpdate(Mathf.Min(10.0f, (float)m_remainingTime), 0.1667f);
    }


    // Place Arrows Again - players arrows are removed
    private void removePlacements()
    {
        for (int i = 0; i < m_players.Length; ++i)
        {
            m_players[i].clearPlacements();
        }

        PauseInstance pause = TimeManager.addTimePause();

        // We give the player some time to re-make placements
        m_modifierTimer = new Timer(false);
        m_modifierTimer.timerCompleted += () => {
            TimeManager.removeTimePause(pause);
        };
        m_modifierTimer.startTimer(4.0f);
    }


    // Slow Down - Grid movers move more slowly
    private void beginSlowMode()
    {
        GridMovement.speedMultiplier = 0.5f;

        m_modifierTimer = new Timer(false);
        m_modifierTimer.timerCompleted += () => {
            GridMovement.speedMultiplier = 1.0f;
        };
        m_modifierTimer.startTimer(10.0f);
    }


    // Passes a second and updates the timer display
    private void tickGameTimer()
    {
        if (m_remainingTime > 0)
        {
            --m_remainingTime;
        }
        m_timerText.text = string.Format("{0}:{1:00}", m_remainingTime / 60, m_remainingTime % 60);
    }


    // Game finishes with a victor
    private void finishGame()
    {
        // We want to update the timer when it finishes as well
        tickGameTimer();

        int victor = 0;
        for (int i = 1; i < m_players.Length; ++i)
        {
            if (m_players[i].score >= m_players[victor].score)
            {
                victor = i;
            }
        }

        // Detect for game draw. If there is draw, use Ended_Failure state
        Transform completeMenu = m_display.Find("CompleteMenu");
        if (victor != 0 && m_players[0].score == m_players[victor].score)
        {
            m_gameState.mainState = GameState.State.Ended_Failure;
            completeMenu.Find("Text").GetComponent<Text>().text = "The game ended in a draw.";
        }
        else
        {
            m_gameState.mainState = GameState.State.Ended_Victory;
            completeMenu.Find("Text").GetComponent<Text>().text = m_players[victor].playerName + " wins!";
        }
        completeMenu.gameObject.SetActive(true);
    }


    private void onTagStateAdd(GameState.TagState state)
    {
        if (state == GameState.TagState.Suspended)
        {
            if (m_modifierTimer != null && m_modifierTimer.isRunning)
            {
                m_modifierTimer.pauseTimer();
            }
        }
    }


    private void onTagStateRemove(GameState.TagState state)
    {
        if (state == GameState.TagState.Suspended)
        {
            if (m_modifierTimer != null && !m_modifierTimer.isRunning)
            {
                m_modifierTimer.resumeTimer();
            }
        }
    }


    public delegate void ModifierFunction();
    private static readonly string[] m_modifierText = {
        "Mouse Mania!", "Everybody Move!", "Cat Mania!", "Cat Attack!", "Place Arrows Again!", "Mouse Monopoly!", "Speed Up!", "Slow Down!"
    };

    private Transform m_display;
    private Transform m_audioParent;
    private GameState m_gameState;
    private GameMap m_gameMap;
    private Text m_timerText;

    private ModifierFunction[] m_modifierFunctions;
    private System.Random m_rng = new System.Random();
    private Timer m_catSpawnTimer = null;
    private Timer m_mouseSpawnTimer = null;
    private Timer m_spawnFrequencyTimer = null;
    private Timer m_modifierTimer = null;
    private Timer m_gameTimer = new Timer();
    private Timer m_startCountdown = new Timer();

    private List<MapTile> m_spawnTiles = new List<MapTile>();
    private Player[] m_players = new Player[4];
    private int m_remainingTime;
    private int m_catCounter = 0;
    private int m_specialMouseCounter = 0;
}
