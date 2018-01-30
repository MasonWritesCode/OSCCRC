using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Directions : MonoBehaviour {

    // All Game Objects must be designed so that they face north when not rotated!
    // Otherwise, we have no idea what angle to set to make it face a direction

    public enum Direction { North = 1, East = 2, South = 4, West = 8 };

    public static void rotate (ref Transform transform, Direction dir)
    {
        if (dir == Direction.North)
        {
            transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        }
        else if (dir == Direction.South)
        {
            transform.eulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
        }
        else if (dir == Direction.West)
        {
            transform.eulerAngles = new Vector3(0.0f, 270.0f, 0.0f);
        }
        else if (dir == Direction.East)
        {
            transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
        }
    }

    public static Direction getOppositeDir(Direction dir)
    {
        int dirNum = (int)dir;

        int newNum = dirNum >> 2;
        if (newNum == 0)
        {
            newNum = dirNum << 2;
        }

        return (Direction)(newNum);
    }
}
