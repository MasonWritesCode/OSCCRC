using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMovement : MonoBehaviour {

	public float speed;
	public GameMap map;
	public Directions direction;

	MapTile mouseTile;
	MapTile destinationTile;

	GameController gameController;

	public enum Directions {north, south, east, west};

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
			float degrees;
			bool skip = false;

			mouseTile = map.tileAt (transform.position);

			//TODO: check for other types of tiles: pits, goals, etc.

			if (transform.position == mouseTile.transform.position) { // we hit our destination, so get a new tile
				transform.position = mouseTile.transform.position;

				//check for goals and holes
				if (mouseTile.improvement == MapTile.TileImprovement.Goal) {
					// TODO: additional handling of goals
					Destroy (gameObject);
				}
				else if (mouseTile.improvement == MapTile.TileImprovement.Hole) {
					// TODO: additional handlinng of holes (if needed)
					Destroy (gameObject);
				}

				//checking for arrows
				if (mouseTile.improvement == MapTile.TileImprovement.Up) {
					direction = Directions.north;
					transform.eulerAngles = new Vector3 (0.0f, 0.0f, 0.0f);
				} 
				else if (mouseTile.improvement == MapTile.TileImprovement.Down) {
					direction = Directions.south;
					transform.eulerAngles = new Vector3 (0.0f, 180.0f, 0.0f);
				} 
				else if (mouseTile.improvement == MapTile.TileImprovement.Left) {
					direction = Directions.west;
					transform.eulerAngles = new Vector3 (0.0f, 270.0f, 0.0f);
				} 
				else if (mouseTile.improvement == MapTile.TileImprovement.Right) {
					direction = Directions.east;
					transform.eulerAngles = new Vector3 (0.0f, 90.0f, 0.0f);
				}

				//Checking for walls
				if (mouseTile.walls.north && direction == GridMovement.Directions.north) {
					if (mouseTile.walls.east) {
						if (mouseTile.walls.west) {
							degrees = 180;
							direction = GridMovement.Directions.south;
						} else {
							degrees = -90;
							direction = GridMovement.Directions.west;
						}
					} else {
						degrees = 90;
						direction = GridMovement.Directions.east;
					}

					transform.Rotate (new Vector3 (0, degrees, 0));

					if (transform.position == mouseTile.transform.position) { // we hit our destination, so get a new tile
						destinationTile = map.tileAt (transform.position + transform.forward * map.tileSize); // after rotating, so facing the desired direction
					}
					skip = true;
				} else if (mouseTile.walls.south && direction == GridMovement.Directions.south) {
					if (mouseTile.walls.west) {
						if (mouseTile.walls.east) {
							degrees = 180;
							direction = GridMovement.Directions.north;
						} else {
							degrees = -90;
							direction = GridMovement.Directions.east;
						}

					} else {
						degrees = 90;
						direction = GridMovement.Directions.west;
					}

					transform.Rotate (new Vector3 (0, degrees, 0));

					if (transform.position == mouseTile.transform.position) { // we hit our destination, so get a new tile
						destinationTile = map.tileAt (transform.position + transform.forward * map.tileSize); // after rotating, so facing the desired direction
					}
					skip = true;
				} else if (mouseTile.walls.east && direction == GridMovement.Directions.east) {
					if (mouseTile.walls.south) {
						if (mouseTile.walls.north) {
							degrees = 180;
							direction = GridMovement.Directions.west;
						} else {
							degrees = -90;
							direction = GridMovement.Directions.north;
						}
					} else {
						degrees = 90;
						direction = GridMovement.Directions.south;
					}
					transform.Rotate (new Vector3 (0, degrees, 0));

					if (transform.position == mouseTile.transform.position) { // we hit our destination, so get a new tile
						destinationTile = map.tileAt (transform.position + transform.forward * map.tileSize); // after rotating, so facing the desired direction
					}
					skip = true;
				} else if (mouseTile.walls.west && direction == GridMovement.Directions.west) {
					if (mouseTile.walls.north) {
						if (mouseTile.walls.south) {
							degrees = 180;
							direction = GridMovement.Directions.east;
						} else {
							degrees = -90;
							direction = GridMovement.Directions.south;
						}

					} else {
						degrees = 90;
						direction = GridMovement.Directions.north;
					}
					transform.Rotate (new Vector3 (0, degrees, 0));

					if (transform.position == mouseTile.transform.position) { // we hit our destination, so get a new tile
						destinationTile = map.tileAt (transform.position + transform.forward * map.tileSize); // after rotating, so facing the desired direction
					}
					skip = true;
				}
				if (!skip) {
					destinationTile = map.tileAt (transform.position + transform.forward * map.tileSize); // after rotating, so facing the desired direction
				}
			}

			transform.Translate (Vector3.ClampMagnitude (Vector3.forward * speed, Vector3.Distance (destinationTile.transform.position, transform.position)));
		}
	}
}
