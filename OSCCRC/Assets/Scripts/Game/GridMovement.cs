﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class controls the behavior of moving objects such as Cats and Mice.
// As an abstract class, one should Instantiate a Mouse or Cat object directly instead.

public abstract class GridMovement : MonoBehaviour {

    [Range(0, 255)] public float speed;

    public Directions.Direction direction;
    public MapTile tile { get { return m_tile; } }


    // Runs interactions with the specified tile improvement
    protected abstract void interactWithImprovement(MapTile.TileImprovement improvement);


    void Start () {
        m_map = GameObject.FindWithTag("Map").GetComponent<GameMap>();
		m_gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        m_transform = GetComponent<Transform>();
        m_rigidbody = GetComponent<Rigidbody>();
        m_interpolationMode = m_rigidbody.interpolation;

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
            setToTile(m_map.tileAt(m_transform.localPosition));
        }

        // Now move the distance we need to move
        m_transform.Translate(Vector3.forward * distance);
        m_remainingDistance -= distance;

        // Wrap around to tile on opposite side of map if necessary
        if (m_isOnEdgeTile)
        {
            applyMapWrap();
        }
    }


    // Wraps the transform to the other side of the map if necessary
    private void applyMapWrap()
    {
        bool willWrap = false;
        Vector3 pos = m_transform.localPosition;

        if (pos.x <= -(m_map.tileSize / 2)) // West -> East
        {
            pos.x += m_map.mapWidth * m_map.tileSize;
            willWrap = true;
        }
        else if (pos.x >= (m_map.mapWidth - 0.5f) * m_map.tileSize) // East -> West
        {
            pos.x -= m_map.mapWidth * m_map.tileSize;
            willWrap = true;
        }
        if (pos.z <= -(m_map.tileSize / 2)) // South -> North
        {
            pos.z += m_map.mapHeight * m_map.tileSize;
            willWrap = true;
        }
        else if (pos.z >= (m_map.mapHeight - 0.5f) * m_map.tileSize) // North -> South
        {
            pos.z -= m_map.mapHeight * m_map.tileSize;
            willWrap = true;
        }

        if (willWrap)
        {
            // We don't want to interpolate a "warp", so temporarily disable it
            m_rigidbody.interpolation = RigidbodyInterpolation.None;
            m_transform.localPosition = pos;
            m_rigidbody.interpolation = m_interpolationMode;
        }
    }


    // Sets the gameobject's tile, places it onto there, and resets its behavior to start moving across that tile
    private void setToTile(MapTile tile)
    {
        m_tile = tile;

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
        Directions.rotate(ref m_transform, direction);

        m_remainingDistance = m_map.tileSize;

        // We only want to check for wrapping per update when we are on a tile that is at the edge
        m_isOnEdgeTile = m_transform.localPosition.x < (m_map.tileSize / 2)
                         || m_transform.localPosition.x > (m_map.mapWidth - 1.5f) * m_map.tileSize
                         || m_transform.localPosition.z < (m_map.tileSize / 2)
                         || m_transform.localPosition.z > (m_map.mapHeight - 1.5f) * m_map.tileSize;
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


    protected GameController m_gameController;
    protected GameMap m_map;
    protected Transform m_transform;
    protected Rigidbody m_rigidbody;
    protected Animator m_animator;
    private RigidbodyInterpolation m_interpolationMode;
    private MapTile m_tile;
    private float m_remainingDistance;
    private bool m_isOnEdgeTile;
}
