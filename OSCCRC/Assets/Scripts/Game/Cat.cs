using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A Cat implementation of a Grid-Mover.

public class Cat : GridMovement {

    // Cats can destroy mice, so they check for a collision
    void OnTriggerEnter(Collider other)
    {
        Mouse mouseComponent = other.GetComponent<Mouse>();
        if (mouseComponent != null)
        {
            m_gameController.destroyMover(mouseComponent);
        }
    }

    // Runs interactions with the specified tile improvement
    protected override void interactWithImprovement(MapTile.TileImprovement improvement)
    {
        if (tile.improvement == MapTile.TileImprovement.Goal)
        {
            m_gameController.destroyMover(this);

            return;
        }
        else if (tile.improvement == MapTile.TileImprovement.Hole)
        {
            m_gameController.destroyMover(this);

            return;
        }
        else if (tile.improvement == MapTile.TileImprovement.Direction)
        {
            if (direction == Directions.getOppositeDir(tile.improvementDirection))
            {
                tile.damageTile();
            }

            direction = tile.improvementDirection;
        }
    }
}
