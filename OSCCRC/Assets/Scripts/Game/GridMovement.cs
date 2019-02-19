using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class controls the behavior of moving objects such as Cats and Mice.

public class GridMovement : MonoBehaviour {

    [Range(0, 255)] public float speed;
	public GameMap map;
	public Directions.Direction direction;
    public bool isCat;
	public MapTile tile;

    private float m_remainingDistance;
	private GameController m_gameController;
    private Transform m_transform; // We have to store transform in a variable to pass it out as ref for Directions.rotate
    private Animator m_animator;

	// Use this for initialization
	void Start () {
		map = GameObject.FindWithTag("Map").GetComponent<GameMap>();
		m_gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        m_transform = GetComponent<Transform>();

        m_animator = GetComponent<Animator>();
        if (m_animator)
        {
            m_animator.speed = speed / 2;
        }

        Directions.rotate(ref m_transform, direction);

        m_gameController.gameState.stateAdded += onTagStateAdd;
        m_gameController.gameState.stateRemoved += onTagStateRemove;
	}

	void FixedUpdate() {
        if (   m_gameController.gameState.mainState != GameState.State.Started_Unpaused
            || m_gameController.gameState.hasState(GameState.TagState.Suspended)
           )
        {
            return;
        }

        float distance = speed * Time.smoothDeltaTime;
        if (distance >= m_remainingDistance)
        {
            // We should reach the center of destination tile. So re-center, get new remaining movement distance, and get new heading
            distance -= m_remainingDistance;
            setToTile(map.tileAt(m_transform.localPosition));
        }

        // Now move the distance we need to move
        m_transform.Translate(Vector3.forward * distance);
        m_remainingDistance -= distance;

        // Wrap around to tile on opposite side of map if necessary
        Vector3 pos = m_transform.localPosition;
        if (pos.x <= -(map.tileSize / 2)) // West -> East
        {
            pos.x += map.mapWidth * map.tileSize;
            m_transform.localPosition = pos;
        }
        else if (pos.x >= (map.mapWidth - (map.tileSize / 2))) // East -> West
        {
            pos.x -= map.mapWidth * map.tileSize;
            m_transform.localPosition = pos;
        }
        if (pos.z <= -(map.tileSize / 2)) // South -> North
        {
            pos.z += map.mapHeight * map.tileSize;
            m_transform.localPosition = pos;
        }
        else if (pos.z >= (map.mapHeight - (map.tileSize / 2))) // North -> South
        {
            pos.z -= map.mapHeight * map.tileSize;
            m_transform.localPosition = pos;
        }
    }

    // Sets the gameobject's tile, places it onto there, and resets its behavior to start moving across that tile
    private void setToTile(MapTile tile)
    {
        this.tile = tile;

        //check for goals and holes
        if (tile.improvement == MapTile.TileImprovement.Goal)
        {
            // TODO: additional handling of goals
            if (isCat)
            {
                map.destroyCat(transform);
            }
            else
            {
                map.destroyMouse(transform);
            }

            return;
        }
        else if (tile.improvement == MapTile.TileImprovement.Hole)
        {
            // TODO: additional handlinng of holes (if needed)
            if (isCat)
            {
                map.destroyCat(transform);
            }
            else
            {
                map.destroyMouse(transform);
            }

            return;
        }

        //checking for arrows
        if (tile.improvement == MapTile.TileImprovement.Direction)
        {
            if (isCat && direction == Directions.getOppositeDir(tile.improvementDirection))
            {
                direction = tile.improvementDirection;
                tile.damageTile();
            }
            else
            {
                direction = tile.improvementDirection;
            }
        }

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
        Directions.rotate(ref m_transform, direction);

        m_remainingDistance = map.tileSize;
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

    void OnTriggerEnter(Collider other)
    {
        if (isCat && other.name.Contains("Mouse"))
        {
            if (!other.GetComponent<GridMovement>().isCat)
            {
                map.destroyMouse(other.transform);
            }
        }
    }
}
