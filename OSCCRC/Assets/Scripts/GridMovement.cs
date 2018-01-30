using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMovement : MonoBehaviour {

	public float speed;
	public GameMap map;
	public Directions.Direction direction;

	MapTile mouseTile;
	Vector3 destinationPos;

	GameController gameController;

    // We have to store transform in a variable to pass it out as ref
    Transform m_transform;

    void updateDirection()
    {
        //check for goals and holes
        if (mouseTile.improvement == MapTile.TileImprovement.Goal)
        {
            // TODO: additional handling of goals
            Destroy(gameObject);
        }
        else if (mouseTile.improvement == MapTile.TileImprovement.Hole)
        {
            // TODO: additional handlinng of holes (if needed)
            Destroy(gameObject);
        }

        //checking for arrows
        if (mouseTile.improvement == MapTile.TileImprovement.Up)
        {
            direction = Directions.Direction.North;
        }
        else if (mouseTile.improvement == MapTile.TileImprovement.Down)
        {
            direction = Directions.Direction.South;
        }
        else if (mouseTile.improvement == MapTile.TileImprovement.Left)
        {
            direction = Directions.Direction.West;
        }
        else if (mouseTile.improvement == MapTile.TileImprovement.Right)
        {
            direction = Directions.Direction.East;
        }

        //Checking for walls
        if (mouseTile.walls.north && direction == Directions.Direction.North)
        {
            if (mouseTile.walls.east)
            {
                if (mouseTile.walls.west)
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
        else if (mouseTile.walls.south && direction == Directions.Direction.South)
        {
            if (mouseTile.walls.west)
            {
                if (mouseTile.walls.east)
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
        else if (mouseTile.walls.east && direction == Directions.Direction.East)
        {
            if (mouseTile.walls.south)
            {
                if (mouseTile.walls.north)
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
        else if (mouseTile.walls.west && direction == Directions.Direction.West)
        {
            if (mouseTile.walls.north)
            {
                if (mouseTile.walls.south)
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

        destinationPos = transform.position + transform.forward * map.tileSize; // after rotating, so facing the desired direction
    }

	// Use this for initialization
	void Start () {
		map = GameObject.FindWithTag("Map").GetComponent<GameMap>();
		gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        m_transform = GetComponent<Transform>();

        Directions.rotate(ref m_transform, direction);
	}

	void FixedUpdate() {
		if (!gameController.isPaused) {
            mouseTile = map.tileAt (transform.position);

			//TODO: check for other types of tiles: pits, goals, etc.

			if (transform.position == mouseTile.transform.position) { // we hit our destination, so get a new tile
                updateDirection();
            }

			transform.Translate (Vector3.ClampMagnitude (Vector3.forward * speed, Vector3.Distance (destinationPos, transform.position)));

            // Wrap around to opposite side of map if necessary
            Vector3 pos = transform.position;
            if (pos.x <= -(map.tileSize / 2))
            {
                pos.x = map.mapWidth - (map.tileSize / 2) - 0.1f;
                transform.position = pos;
                updateDirection();
            }
            else if ( pos.x >= (map.mapWidth - (map.tileSize / 2)) )
            {
                pos.x = 0;
                transform.position = pos;
                updateDirection();
            }
            if (pos.z <= -(map.tileSize / 2))
            {
                pos.z = map.mapHeight - (map.tileSize / 2) - 0.1f;
                transform.position = pos;
                updateDirection();
            }
            else if ( pos.z >= (map.mapHeight - (map.tileSize / 2)) )
            {
                pos.z = 0;
                transform.position = pos;
                updateDirection();
            }
        }
	}
}
