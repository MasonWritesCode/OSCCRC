using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A Cat implementation of a Grid-Mover.

public class Cat : GridMovement {

    /*
    void Awake()
    {
        speed = 4.4444f;
        direction = Directions.Direction.North;
    }
    */

    // Cats can destroy mice, so they check for a collision
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Mouse>() != null)
        {
            m_map.destroyMouse(other.transform);
        }
    }

    // Runs interactions with the specified tile improvement
    protected override void interactWithImprovement(MapTile.TileImprovement improvement)
    {
        //check for goals and holes
        if (tile.improvement == MapTile.TileImprovement.Goal)
        {
            m_map.destroyCat(transform);

            return;
        }
        else if (tile.improvement == MapTile.TileImprovement.Hole)
        {
            m_map.destroyCat(transform);

            return;
        }
        //checking for arrows
        else if (tile.improvement == MapTile.TileImprovement.Direction)
        {
            direction = tile.improvementDirection;

            if (direction == Directions.getOppositeDir(tile.improvementDirection))
            {
                tile.damageTile();
            }
        }
    }
}
