using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class controls the behavior of moving objects such as Cats and Mice.
// As an abstract class, one should Instantiate a Mouse or Cat object directly instead.
// Currently assumes that the map isn't rotated after GridMovement's start is called, and that all GridMovement are children of game map as are MapTiles.

public abstract class GridMovement : MonoBehaviour {

    public static float speedMultiplier { get { return m_speedMultiplier; } set { setSpeedMultiplier(value); } }
    public static bool moonwalkEnabled { get { return m_moonwalkEnabled; } set { m_moonwalkEnabled = value; } }
    [Range(0.0f, 255.0f)] public float speed; // Editor set ; Make sure to call updateSpeed if changed during runtime since annoyingly we can't make it a property and be editor set I think

    public Directions.Direction direction;
    public MapTile tile { get { return m_tile; } }


    // Runs interactions with the specified tile improvement
    protected abstract void interactWithImprovement(MapTile.TileImprovement improvement);


    // Updates a grid mover's speed and animation based on its speed variable and the current global multiplier
    public void updateSpeed()
    {
        // We require a positive speed for now
        m_scaledSpeed = Mathf.Abs(speed * m_speedMultiplier);

        if (m_animator)
        {
            m_animator.speed = m_scaledSpeed / 2;
        }
    }


    void Start () {
        m_map = GameObject.FindWithTag("Map").GetComponent<GameMap>();
		m_gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        m_transform = GetComponent<Transform>();
        m_rigidbody = GetComponent<Rigidbody>();
        if (m_rigidbody)
        {
            m_interpolationMode = m_rigidbody.interpolation;
        }

        m_animator = GetComponent<Animator>();

        m_tile = m_map.tileAt(m_transform.localPosition);
        m_dirVec = m_map.transform.TransformVector(Directions.toVector(direction));
        updateSpeed();

        multiplierChange += updateSpeed;
        m_gameController.gameState.stateAdded += onTagStateAdd;
        m_gameController.gameState.stateRemoved += onTagStateRemove;
	}


	void FixedUpdate() {
        if (   !(
                    m_gameController.gameState.mainState == GameState.State.Started_Unpaused
                 || m_gameController.gameState.mainState == GameState.State.Ended_Victory
                )
            || m_gameController.gameState.hasState(GameState.TagState.Suspended)
           )
        {
            return;
        }

        if(m_tile.walls.north &&
           m_tile.walls.south &&
           m_tile.walls.east &&
           m_tile.walls.west
           )
        {
            return;
        }

        float distance = m_scaledSpeed * Time.smoothDeltaTime;
        while (distance >= m_remainingDistance)
        {
            // We should reach the center of destination tile. So re-center, get new remaining movement distance, and get new heading
            // Assumes tile and grid movers are children of game map.
            distance -= m_remainingDistance;
            Vector3 newTilePos = m_transform.localPosition + (m_dirVec * m_remainingDistance);
            if (m_isOnEdgeTile)
            {
                setToTile(m_map.tileAt(m_map.wrapCoord(newTilePos)));
            }
            else
            {
                setToTile(m_map.tileAt(newTilePos));
            }
        }

        // Now move the distance we need to move
        m_transform.Translate(m_dirVec * distance, Space.World);
        m_remainingDistance -= distance;

        // Wrap around to tile on opposite side of map if necessary
        if (m_isOnEdgeTile)
        {
            applyMapWrap();
        }
    }


    // Sets a new global speed multiplier for grid movers
    private static void setSpeedMultiplier(float newVal)
    {
        m_speedMultiplier = newVal;

        if (multiplierChange != null)
        {
            multiplierChange();
        }
    }


    // Wraps the transform to the other side of the map if necessary
    private void applyMapWrap()
    {
        Vector3 wrappedPos = m_map.wrapCoord(m_transform.localPosition);

        if (m_rigidbody && wrappedPos != m_transform.localPosition)
        {
            // We don't want to interpolate a "warp", so temporarily disable it
            m_rigidbody.interpolation = RigidbodyInterpolation.None;
            m_transform.localPosition = wrappedPos;
            m_rigidbody.interpolation = m_interpolationMode;
        }
    }


    // Sets the gameobject's tile, places it onto there, and resets its behavior to start moving across that tile
    private void setToTile(MapTile tile)
    {
        m_tile = tile;
        Directions.Direction prevDir = direction;

        // May modify our direction
        interactWithImprovement(tile.improvement);

        //Checking for walls
        if (tile.walls.north && direction == Directions.Direction.North)
        {
            if (tile.walls.east)
            {
                if (tile.walls.west)
                {
                    direction = Directions.Direction.South;
                }
                else
                {
                    direction = Directions.Direction.West;
                }
            }
            else
            {
                direction = Directions.Direction.East;
            }
        }
        else if (tile.walls.south && direction == Directions.Direction.South)
        {
            if (tile.walls.west)
            {
                if (tile.walls.east)
                {
                    direction = Directions.Direction.North;
                }
                else
                {
                    direction = Directions.Direction.East;
                }

            }
            else
            {
                direction = Directions.Direction.West;
            }
        }
        else if (tile.walls.east && direction == Directions.Direction.East)
        {
            if (tile.walls.south)
            {
                if (tile.walls.north)
                {
                    direction = Directions.Direction.West;
                }
                else
                {
                    direction = Directions.Direction.North;
                }
            }
            else
            {
                direction = Directions.Direction.South;
            }
        }
        else if (tile.walls.west && direction == Directions.Direction.West)
        {
            if (tile.walls.north)
            {
                if (tile.walls.south)
                {
                    direction = Directions.Direction.East;
                }
                else
                {
                    direction = Directions.Direction.South;
                }

            }
            else
            {
                direction = Directions.Direction.North;
            }
        }

        m_transform.localPosition = tile.transform.localPosition;
        // This if statement assumes that the game map isn't rotated after grid mover is placed causing rotation to not update until direction changes
        if (direction != prevDir)
        {
            if (m_moonwalkEnabled)
            {
                Directions.rotate(m_transform, Directions.getOppositeDir(direction));
            }
            else
            {
                Directions.rotate(m_transform, direction);
            }

            // The direction we choose to move in world space is the relative direction vector of parent (which we will assume is the gamemap for now)
            // We can't use the transform's forward because we want to allow for a rotated transform to work as a grid mover
            m_dirVec = m_map.transform.TransformVector(Directions.toVector(direction));
        }

        m_remainingDistance = m_map.tileSize;

        // We only want to check for wrapping per update when we are on a tile that is at the edge
        m_isOnEdgeTile = m_map.isEdgeTile(tile);
    }


    // Disables the animator while suspended purely to not be wasteful
    private void onTagStateAdd(GameState.TagState state)
    {
        if (state == GameState.TagState.Suspended)
        {
            if (m_animator)
            {
                m_animator.enabled = false;
            }
        }
    }


    // Re-enables the animator if onTagStateAdd disabled it
    private void onTagStateRemove(GameState.TagState state)
    {
        if (state == GameState.TagState.Suspended)
        {
            if (m_animator)
            {
                m_animator.enabled = true;
            }
        }
    }

    private delegate void voidEvent();
    private static event voidEvent multiplierChange;

    private static float m_speedMultiplier = 1.0f;
    private static bool m_moonwalkEnabled = false;

    protected GameController m_gameController;
    protected GameMap m_map;
    protected Transform m_transform;
    protected Rigidbody m_rigidbody;
    protected Animator m_animator;
    private RigidbodyInterpolation m_interpolationMode;
    private MapTile m_tile;
    private float m_scaledSpeed;
    private float m_remainingDistance;
    private bool m_isOnEdgeTile;
    private Vector3 m_dirVec;
}
