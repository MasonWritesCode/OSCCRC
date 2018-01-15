using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMovement : MonoBehaviour {

	public float speed;
	public GameMap map;
	public string direction;

	MapTile mouseTile;
	MapTile destinationTile;

	public enum Directions {north, south, east, west};

	// Use this for initialization
	void Start () {
		map = GameObject.FindWithTag("Map").GetComponent<GameMap>();
	}

	void Update() {
		//MapTile mouseTile;
		float degrees;
		bool skip = false;

		//TODO: check for arrow tiles, pits, goal, etc.

		//prevent stepping outside of array bounds for testing purposes.
		float x = transform.position.x;
		float z = transform.position.z;
		if(x < 0) {
			x = 0.0f;
		}
		if(z < 0) {
			z = 0.0f;
		}
		transform.position = new Vector3 (x,0.0f,z);

		// Using tileAt works fine when moving north or east. When moving south, it doesn't work as intended. 
		// if xPos is at .9927, the floor treats it as 0, obviously, and the mouse will turn before it has actually
		// fully entered the tile
		/*
		if (direction == "south" || direction == "west") {
			float xPos = Mathf.FloorToInt (transform.position.x);
			float zPos = Mathf.FloorToInt (transform.position.z);
			Debug.Log (xPos);
			Debug.Log (zPos);
			mouseTile = map.tileAt (new Vector3 (xPos, 0.0f, zPos));
		} 
		else {
			mouseTile = map.tileAt(transform.position);
		}
		*/

		mouseTile = map.tileAt(transform.position);

		//Debug.Log (Vector3.Distance(mouseTile.transform.position,transform.position));

		//yeah, this could be cleaner, but it works.
		if (mouseTile.walls.north && direction == "north") {
			Debug.Log (transform.position);
			if (mouseTile.walls.east) {
				if (mouseTile.walls.west) {
					degrees = 180;
					direction = "south";
				}
				degrees = -90;
				direction = "west";

			} 
			else {
				degrees = 90;
				direction = "east";
			}
			transform.Rotate (new Vector3 (0, degrees, 0));

			//the idea with this is to snap a mouse to a whole number value
			//since the transform.Translate() isn't exactly ensuring that happens
			//this is probably a dumb idea

			//float xPos = Mathf.CeilToInt(transform.position.x);
			float xPos = transform.position.x;
			float zPos = Mathf.Round(transform.position.z);
			transform.position = new Vector3 (xPos,0.0f,zPos);
			//
			if (transform.position == mouseTile.transform.position) // we hit our destination, so get a new tile
			{
				Debug.Log ("north");
				destinationTile = map.tileAt(transform.position + Vector3.forward * map.tileSize); // after rotating, so facing the desired direction
			}
			skip = true;
		} 
		else if (mouseTile.walls.south && direction == "south") {
			Debug.Log (transform.position);
			if (mouseTile.walls.west) {
				if (mouseTile.walls.east) {
					degrees = 180;
					direction = "north";
				}
				degrees = -90;
				direction = "east";

			} 
			else {
				degrees = 90;
				direction = "west";
			}
			transform.Rotate (new Vector3 (0, degrees, 0));
			//float xPos = Mathf.FloorToInt(transform.position.x);
			float xPos = transform.position.x;
			float zPos = Mathf.Round(transform.position.z);
			transform.position = new Vector3 (xPos,0.0f,zPos);

			if (transform.position == mouseTile.transform.position) // we hit our destination, so get a new tile
			{
				Debug.Log ("south");
				destinationTile = map.tileAt(transform.position + Vector3.forward * map.tileSize); // after rotating, so facing the desired direction
			}
			skip = true;
		}
		else if (mouseTile.walls.east && direction == "east") {
			Debug.Log (transform.position);
			if (mouseTile.walls.south) {
				if (mouseTile.walls.north) {
					degrees = 180;
					direction = "west";
				}
				degrees = -90;
				direction = "north";

			} 
			else {
				degrees = 90;
				direction = "south";
			}
			transform.Rotate (new Vector3 (0, degrees, 0));
			float xPos = Mathf.Round(transform.position.x);
			//float zPos = Mathf.CeilToInt(transform.position.z);
			float zPos = transform.position.z;
			transform.position = new Vector3 (xPos,0.0f,zPos);

			if (transform.position == mouseTile.transform.position) // we hit our destination, so get a new tile
			{
				Debug.Log ("east");
				destinationTile = map.tileAt(transform.position + Vector3.forward * map.tileSize); // after rotating, so facing the desired direction
			}
			skip = true;
		}
		else if (mouseTile.walls.west && direction == "west") {
			Debug.Log (transform.position);
			if (mouseTile.walls.north) {
				if (mouseTile.walls.south) {
					degrees = 180;
					direction = "east";
				}
				degrees = -90;
				direction = "south";

			} 
			else {
				degrees = 90;
				direction = "north";
			}
			transform.Rotate (new Vector3 (0, degrees, 0));
			float xPos = Mathf.Round(transform.position.x);
			//float zPos = Mathf.FloorToInt(transform.position.z);
			float zPos = transform.position.z;
			transform.position = new Vector3 (xPos,0.0f,zPos);

			if (transform.position == mouseTile.transform.position) // we hit our destination, so get a new tile
			{
				Debug.Log ("west");
				destinationTile = map.tileAt(transform.position + Vector3.forward * map.tileSize); // after rotating, so facing the desired direction
			}
			skip = true;
		}

		if (!skip) {
			if (transform.position == mouseTile.transform.position) { // we hit our destination, so get a new tile
				Debug.Log ("default");
				destinationTile = map.tileAt (transform.position + Vector3.forward * map.tileSize); // after rotating, so facing the desired direction
			}
		}
			
		//transform.position = Vector3.ClampMagnitude (Vector3.forward * speed,Vector3.Distance(map.transform.position,transform.position));
		transform.Translate(Vector3.ClampMagnitude(Vector3.forward * speed, Vector3.Distance(destinationTile.transform.position, transform.position)));

		//transform.Translate (new Vector3 (0, 0, 1 * speed) * Time.deltaTime);
		//transform.Translate(Vector3.forward * speed);
		//duration += (1 * speed) * Time.deltaTime;
	}
}
