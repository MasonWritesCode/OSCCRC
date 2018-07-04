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

	private Vector3 m_destinationPos;
	private GameController m_gameController;
    private Transform m_transform; // We have to store transform in a variable to pass it out as ref
    private Vector3 m_oldPos;

	// Use this for initialization
	void Start () {
		map = GameObject.FindWithTag("Map").GetComponent<GameMap>();
		m_gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        m_transform = GetComponent<Transform>();

        Animator anim = GetComponent<Animator>();
        if (anim)
        {
            anim.speed = speed / 2;
        }

        Directions.rotate(ref m_transform, direction);
	}

	void FixedUpdate() {
		if (!m_gameController.isPaused) {
            tile = map.tileAt (m_transform.position);

			//TODO: check for other types of tiles: pits, goals, etc.

			if (m_transform.position == tile.transform.position) { // we hit our destination, so get a new tile
                updateDirection();
            }

            m_oldPos = m_transform.position;
            m_transform.Translate (Vector3.ClampMagnitude (Vector3.forward * speed * Time.smoothDeltaTime, Vector3.Distance (m_destinationPos, m_transform.position)));
            if (m_transform.position == m_oldPos)
            {
                // Our distance to destination is not small enough to match, but not big enough for translate to do anything, so prevent from getting stuck
                m_transform.position = tile.transform.position;
            }

            // Wrap around to opposite side of map if necessary
            Vector3 pos = m_transform.position;
            if (pos.x <= -(map.tileSize / 2))
            {
                pos.x = map.mapWidth - (map.tileSize / 2) - 0.1f;
                m_transform.position = pos;
                updateDirection();
            }
            else if ( pos.x >= (map.mapWidth - (map.tileSize / 2)) )
            {
                pos.x = 0;
                m_transform.position = pos;
                updateDirection();
            }
            if (pos.z <= -(map.tileSize / 2))
            {
                pos.z = map.mapHeight - (map.tileSize / 2) - 0.1f;
                m_transform.position = pos;
                updateDirection();
            }
            else if ( pos.z >= (map.mapHeight - (map.tileSize / 2)) )
            {
                pos.z = 0;
                m_transform.position = pos;
                updateDirection();
            }
        }
	}

    // Allows the parent object to decide which way to turn based on the tile it is located on, and do so
    private void updateDirection()
    {
        //check for goals and holes
        if (tile.improvement == MapTile.TileImprovement.Goal)
        {
            // TODO: additional handling of goals
            if (isCat)
            {
                direction = Directions.getOppositeDir(direction);
            }
            else
            {
                map.destroyMouse(transform);
            }
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

        Directions.rotate(ref m_transform, direction);

        m_destinationPos = m_transform.position + m_transform.forward * map.tileSize; // after rotating, so facing the desired direction
    }

    void OnTriggerEnter(Collider other)
    {
        if (m_gameController && !m_gameController.isPaused)
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
}
