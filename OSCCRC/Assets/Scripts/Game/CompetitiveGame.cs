using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This is an interface between the game controller and the 4 player Competitive game mode.
// This mode requires being passed information about the game state.
// This mode requires being passed its related UI transform.

public class CompetitiveGame : IGameMode
{
    public CompetitiveGame(GameState gameStateRef, Transform modeUI)
    {
        m_gameState = gameStateRef;
        m_display = modeUI;

        m_modifierFunctions = new ModifierFunction[] {
            null, null, null, null, new ModifierFunction(removePlacements), null, null, null
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
        return;
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
    private void spawnCat()
    {
        // We create a new timer to clear the lambda subscriber for now
        m_catSpawnTimer = new Timer();

        m_catSpawnTimer.timerCompleted += () => {
            MapTile spawn = m_spawnTiles[m_rng.Next(m_spawnTiles.Count)];
            m_gameMap.placeCat(spawn.transform.localPosition, spawn.improvementDirection);
            m_catSpawnTimer = null;
        };
        m_catSpawnTimer.startTimer(2.0f);

        ++m_catCounter;
    }


    // Spawns a mouse at a random spawner
    private void spawnMouse()
    {
        // We create a new timer to clear the lambda subscriber for now
        m_mouseSpawnTimer = new Timer();

        m_mouseSpawnTimer.timerCompleted += () => {
            MapTile spawn = m_spawnTiles[m_rng.Next(m_spawnTiles.Count)];
            m_gameMap.placeMouse(spawn.transform.localPosition, spawn.improvementDirection);
            m_mouseSpawnTimer = null;
        };
        m_mouseSpawnTimer.startTimer(2.0f);
    }


    // Spawns a 50 point mouse at a random spawner
    private void spawnBigMouse()
    {
        MapTile spawn = m_spawnTiles[m_rng.Next(m_spawnTiles.Count)];
        m_gameMap.placeBigMouse(spawn.transform.localPosition, spawn.improvementDirection);
    }


    // Spawns a special game modifier mouse at a random spawner
    private void spawnSpecialMouse()
    {
        MapTile spawn = m_spawnTiles[m_rng.Next(m_spawnTiles.Count)];
        m_gameMap.placeSpecialMouse(spawn.transform.localPosition, spawn.improvementDirection);
    }


    // Set each spawner to begin spawning mice and spawn in a cat, the normal case without special mice effects
    // I don't know how frequently mice are supposed to spawn, so it will be random averaging 3 seconds per spawner for now
    private void beginNormalSpawn()
    {
        // We always start with one cat? Might need a delayed entrance?
        spawnCat();

        m_spawnFrequencyTimer = new Timer();
        m_spawnFrequencyTimer.timerUpdate += () => {
            // 1/18 chance 6 times per second for EV of 3 seconds, per spawner
            if (m_rng.Next(18) < m_spawnTiles.Count)
            {
                // 1 in 100 chance to spawn a 50 point mouse for now
                if (m_rng.Next(100) == 0)
                {
                    if (m_rng.Next(2) == 0)
                    {
                        spawnBigMouse();
                    }
                    else
                    {
                        spawnSpecialMouse();
                    }
                }
                else
                {
                    spawnMouse();
                }
            }

            // We create a new cat whenever one dies while game is running
            if (m_catCounter == 0 && m_gameState.mainState == GameState.State.Started_Unpaused)
            {
                spawnCat();
            }
        };
        m_spawnFrequencyTimer.startTimerWithUpdate((float)m_remainingTime, 0.1667f);
    }


    // Randomly select a game modifier to apply
    private void addRandomGameModifier()
    {
        // For now, we only have one implemented, so just do that
        int modifierSelection = 4;
        GameObject modifierDisplay = m_display.Find("Event Popup").gameObject;

        modifierDisplay.GetComponentInChildren<Text>().text = m_modifierText[modifierSelection];
        modifierDisplay.SetActive(true);

        PauseInstance pause = TimeManager.addTimePause();

        // TODO: We need to Pause unscaled timers on suspend state
        m_modifierTimer = new Timer();
        m_modifierTimer.timerCompleted += () => {
            modifierDisplay.SetActive(false);
            TimeManager.removeTimePause(pause);

            if (m_modifierFunctions[modifierSelection] != null)
            {
                m_modifierFunctions[modifierSelection]();
            }
        };
        m_modifierTimer.isScaledTime = false;
        m_modifierTimer.startTimer(2.0f);
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
        // TODO: We need to Pause unscaled timers on suspend state
        m_modifierTimer = new Timer();
        m_modifierTimer.timerCompleted += () => {
            TimeManager.removeTimePause(pause);
        };
        m_modifierTimer.isScaledTime = false;
        m_modifierTimer.startTimer(4.0f);
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
}
