using UnityEngine;

// Allows operations relating to cardinal absolute directions.

public static class Directions {

    // All Game Objects must be designed so that they face north when not rotated!
    // Otherwise, we have no idea what angle to set to make it face a direction

    public enum Direction { North = 1, East = 2, South = 4, West = 8 };

    // Rotates a transform into the specified cardinal direction
    public static void rotate (ref Transform transform, Direction dir)
    {
        if (transform == null)
        {
            return;
        }

        if (dir == Direction.North)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0.0f, transform.eulerAngles.z);
        }
        else if (dir == Direction.South)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, 180.0f, transform.eulerAngles.z);
        }
        else if (dir == Direction.West)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, 270.0f, transform.eulerAngles.z);
        }
        else if (dir == Direction.East)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, 90.0f, transform.eulerAngles.z);
        }
    }

    // Gets the cardinal direction opposite of the one specified by "dir"
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

    // Returns the closest cardinal direction moving clockwise from "dir"
    public static Direction nextClockwiseDir(Direction dir)
    {
        int dirNum = (int)dir;

        int newNum = (dirNum << 1) % 15;

        return (Direction)(newNum);
    }

    // Returns the closest cardinal direction moving counter-clockwise from "dir"
    public static Direction nextCounterClockwiseDir(Direction dir)
    {
        int dirNum = (int)dir;

        int newNum = dirNum >> 1;
        if (newNum == 0)
        {
            newNum = 8;
        }

        return (Direction)(newNum);
    }
}
