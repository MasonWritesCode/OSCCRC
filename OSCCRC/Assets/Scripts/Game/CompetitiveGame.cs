using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// This is an interface between the game controller and the 4 player Competitive game mode.
// This mode requires being passed information about the game state.

public class CompetitiveGame : IGameMode
{
    public CompetitiveGame(GameState gameStateRef)
    {
        m_gameState = gameStateRef;
    }

    // Begins a puzzle game
    public void startGame()
    {
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();

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
            Debug.LogError("No spawn tiles found. Competitive mode opened on non-competitive map");
        }

        m_compDisplay = GameObject.Find("CompetitiveDisplay (UI)");
        m_compDisplay.GetComponent<Canvas>().enabled = true;
        m_timerText = m_compDisplay.transform.Find("Timer").GetComponentInChildren<Text>();

        for (int i = 0; i < m_players.Length; ++i)
        {
            m_players[i] = new Player(m_compDisplay.transform.Find("Score" + i).GetComponentInChildren<Text>());
            m_players[i].playerName = "Player " + i;
        }

        m_gameState.mainState = GameState.State.Started_Paused;

        // Go ahead and set the time
        m_remainingTime = 181;
        tickGameTimer();

        // TODO: Show stuff right before game starts
        m_startCountdown.timerCompleted += () => {
            m_gameState.mainState = GameState.State.Started_Unpaused;

            // TODO: Maybe flashy animation on timer to show it has started?

            // TODO: Ending, figure out player with highest score and show their player name
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
        if (player.placements.Count >= maxPlacements)
        {
            Placement tileToClear = player.placements.Dequeue();
            tileToClear.tile.improvement = MapTile.TileImprovement.None;
        }

        tile.owner = playerID;
        tile.improvementDirection = dir;
        tile.improvement = MapTile.TileImprovement.Direction;

        Timer ptimer = new Timer();
        ptimer.timerCompleted += () =>
        {
            // TODO: Start blinking or whatever at 1 second remaining. Make this an update timer for this.
            tile.improvement = MapTile.TileImprovement.None;
        };
        ptimer.startTimer(10.0f);

        Placement newPlacement;
        newPlacement.tile = tile;
        newPlacement.timer = ptimer;

        player.placements.Enqueue(newPlacement);
    }


    public void destroyMover(GridMovement deadMeat)
    {
        if (deadMeat is Cat)
        {
            if (deadMeat.tile.improvement == MapTile.TileImprovement.Goal)
            {
                int owner = deadMeat.tile.owner;
                // Here we don't want to use integer division, because we don't want to round down.
                m_players[owner].score = m_players[owner].score * 2 / 3;
            }

            m_gameMap.destroyCat(deadMeat.transform);
        }
        else
        {
            if (deadMeat.tile.improvement == MapTile.TileImprovement.Goal)
            {
                int owner = deadMeat.tile.owner;
                if (m_players[owner].score < 999)
                {
                    m_players[owner].score += 1;
                }
            }

            m_gameMap.destroyMouse(deadMeat.transform);
        }

        // We create a new cat whenever one dies
        if (deadMeat is Cat)
        {
            spawnCat();
        }
    }



    private struct Placement
    {
        public MapTile tile;
        public Timer timer;
    }

    private class Player
    {
        public Player(Text textObj)
        {
            scoreText = textObj;
        }

        public Queue<Placement> placements = new Queue<Placement>();
        public int score { get { return m_score; } set { m_score = value; scoreText.text = string.Format("{0:000}", value); } }
        public Text scoreText = null;
        public string playerName = "Player";

        private int m_score = 0;
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
                spawnMouse();
            }
        };
        m_spawnFrequencyTimer.startTimerWithUpdate((float)m_remainingTime, 0.1667f);
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
        int victor = 0;
        for (int i = 1; i < m_players.Length; ++i)
        {
            if (m_players[i].score >= m_players[victor].score)
            {
                victor = i;
            }
        }

        // Detect for game draw. If there is draw, use Ended_Failure state
        // TODO: UI instead of debug log
        if (victor != 0 && m_players[0].score == m_players[victor].score)
        {
            m_gameState.mainState = GameState.State.Ended_Failure;
            Debug.Log("The game ended in a draw.");
        }
        else
        {
            m_gameState.mainState = GameState.State.Ended_Victory;
            Debug.Log(m_players[victor].playerName + " wins!");
        }
    }


    private GameState m_gameState;
    private GameMap m_gameMap;
    private GameObject m_compDisplay;
    private Text m_timerText;
    private List<MapTile> m_spawnTiles = new List<MapTile>();
    private Player[] m_players = new Player[4];
    private System.Random m_rng = new System.Random();
    private Timer m_catSpawnTimer = null;
    private Timer m_mouseSpawnTimer = null;
    private Timer m_spawnFrequencyTimer = null;
    private Timer m_gameTimer = new Timer();
    private Timer m_startCountdown = new Timer();
    private int m_remainingTime;
}
