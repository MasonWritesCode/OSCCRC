using UnityEngine;

// Allows operations relating to cardinal absolute directions.

public static class Directions {

    // All Game Objects must be designed so that they face north when not rotated!
    // Otherwise, we have no idea what angle to set to make it face a direction

    public enum Direction { North = 1, East = 2, South = 4, West = 8 };

    // Locally rotates a transform into the specified cardinal direction
    public static void rotate(Transform transform, Direction dir)
    {
        if (transform == null)
        {
            return;
        }

        // We want the rotation amount to be relative, but we want to use absolute axes so we can't use localRotation
        // So unfortunately we have to grab data from the parent to make the rotation amount local which is slow because eulerAngles access is slow
        Vector3 newAngles = transform.eulerAngles;
        newAngles.y = transform.parent ? transform.parent.eulerAngles.y : 0.0f;
        if (dir == Direction.East)
        {
            newAngles.y += 90.0f;
        }
        else if (dir == Direction.South)
        {
            newAngles.y += 180.0f;
        }
        else if (dir == Direction.West)
        {
            newAngles.y += 270.0f;
        }
        transform.rotation = Quaternion.Euler(newAngles);
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

    // An IEqualityComparer that avoids boxing to improve performance with a direction as a key
    public struct DirectionComparer : System.Collections.Generic.IEqualityComparer<Direction>
    {
        public bool Equals(Direction a, Direction b)
        {
            return a == b;
        }

        public int GetHashCode(Direction dir)
        {
            return (int)dir;
        }
    }
}
