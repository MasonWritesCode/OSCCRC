using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        // TODO: Countdown delay to start

        // We always start with one cat?
        spawnCat();

        // Begin mice spawn
        activateMiceSpawn();
    }


    public void resetGame()
    {
        // TODO
        return;
    }


    public void endGame()
    {
        // TODO
        return;
    }


    public void placeDirection(MapTile tile, Directions.Direction dir, int playerID)
    {
        if (tile.improvement != MapTile.TileImprovement.None)
        {
            // Players cannot change an already placed tile
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
            // TODO: Start blinking or whatever at 1 second remaining. Would have the timer start a new timer?
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
        // TODO: Score

        // We create a new cat whenever one dies
        if (deadMeat is Cat)
        {
            // TODO: This needs to be delayed by death animation once we have a death animation
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
        public Queue<Placement> placements = new Queue<Placement>();
        public int score = 0;
    }


    // Spawns a cat at a random spawner
    private void spawnCat()
    {
        MapTile spawn = m_spawnTiles[m_rng.Next(m_spawnTiles.Count)];
        m_gameMap.placeCat(spawn.transform.localPosition, spawn.improvementDirection);
    }


    // Set each spawner to begin spawning mice
    // I don't know how frequently mice are supposed to spawn, so it will be random averaging 3 seconds for now
    private void activateMiceSpawn()
    {
        // TODO
        return;
    }


    private GameState m_gameState;
    private GameMap m_gameMap;
    private List<MapTile> m_spawnTiles = new List<MapTile>();
    private Player[] m_players = new Player[4];
    private System.Random m_rng = new System.Random();
    private Timer m_miceSpawnTimer = new Timer();
}
