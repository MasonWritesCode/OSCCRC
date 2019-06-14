using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A Mouse implementation of a Grid-Mover

public class Mouse : GridMovement {

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
            direction = tile.improvementDirection;
        }
    }
}
