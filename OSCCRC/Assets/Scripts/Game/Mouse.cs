using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A Mouse implementation of a Grid-Mover

public class Mouse : GridMovement {

    /*
    void Awake()
    {
        speed = 6.6666f;
        direction = Directions.Direction.North;
    }
    */

    // Runs interactions with the specified tile improvement
    protected override void interactWithImprovement(MapTile.TileImprovement improvement)
    {
        //check for goals and holes
        if (tile.improvement == MapTile.TileImprovement.Goal)
        {
            m_map.destroyMouse(transform);

            return;
        }
        else if (tile.improvement == MapTile.TileImprovement.Hole)
        {
            m_map.destroyMouse(transform);

            return;
        }
        //checking for arrows
        else if (tile.improvement == MapTile.TileImprovement.Direction)
        {
            direction = tile.improvementDirection;
        }
    }
}
