using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMovement : MonoBehaviour {

	public float speed;
	public GameMap map;
	public Directions direction;

	MapTile mouseTile;
	Vector3 destinationPos;

	GameController gameController;

	public enum Directions {north, south, east, west};

    void updateDirection()
    {
        float degrees = 0;

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
            direction = Directions.north;
            transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        }
        else if (mouseTile.improvement == MapTile.TileImprovement.Down)
        {
            direction = Directions.south;
            transform.eulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
        }
        else if (mouseTile.improvement == MapTile.TileImprovement.Left)
        {
            direction = Directions.west;
            transform.eulerAngles = new Vector3(0.0f, 270.0f, 0.0f);
        }
        else if (mouseTile.improvement == MapTile.TileImprovement.Right)
        {
            direction = Directions.east;
            transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
        }

        //Checking for walls
        if (mouseTile.walls.north && direction == GridMovement.Directions.north)
        {
            if (mouseTile.walls.east)
            {
                if (mouseTile.walls.west)
                {
                    degrees = 180;
                    direction = GridMovement.Directions.south;
                }
                else
                {
                    degrees = -90;
                    direction = GridMovement.Directions.west;
                }
            }
            else
            {
                degrees = 90;
                direction = GridMovement.Directions.east;
            }

            transform.Rotate(new Vector3(0, degrees, 0));
        }
        else if (mouseTile.walls.south && direction == GridMovement.Directions.south)
        {
            if (mouseTile.walls.west)
            {
                if (mouseTile.walls.east)
                {
                    degrees = 180;
                    direction = GridMovement.Directions.north;
                }
                else
                {
                    degrees = -90;
                    direction = GridMovement.Directions.east;
                }

            }
            else
            {
                degrees = 90;
                direction = GridMovement.Directions.west;
            }

            transform.Rotate(new Vector3(0, degrees, 0));
        }
        else if (mouseTile.walls.east && direction == GridMovement.Directions.east)
        {
            if (mouseTile.walls.south)
            {
                if (mouseTile.walls.north)
                {
                    degrees = 180;
                    direction = GridMovement.Directions.west;
                }
                else
                {
                    degrees = -90;
                    direction = GridMovement.Directions.north;
                }
            }
            else
            {
                degrees = 90;
                direction = GridMovement.Directions.south;
            }
            transform.Rotate(new Vector3(0, degrees, 0));
        }
        else if (mouseTile.walls.west && direction == GridMovement.Directions.west)
        {
            if (mouseTile.walls.north)
            {
                if (mouseTile.walls.south)
                {
                    degrees = 180;
                    direction = GridMovement.Directions.east;
                }
                else
                {
                    degrees = -90;
                    direction = GridMovement.Directions.south;
                }

            }
            else
            {
                degrees = 90;
                direction = GridMovement.Directions.north;
            }
            transform.Rotate(new Vector3(0, degrees, 0));
        }

        destinationPos = transform.position + transform.forward * map.tileSize; // after rotating, so facing the desired direction
    }

	// Use this for initialization
	void Start () {
		map = GameObject.FindWithTag("Map").GetComponent<GameMap>();

		gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

		// assign the initial rotation
		if(direction == GridMovement.Directions.south) {
			transform.Rotate (new Vector3 (0, 180, 0));
		}
		else if(direction == GridMovement.Directions.east) {
			transform.Rotate (new Vector3 (0, 90, 0));
		}
		else if(direction == GridMovement.Directions.west) {
			transform.Rotate (new Vector3 (0, -90, 0));
		}
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
