using UnityEngine;

// Allows operations relating to cardinal absolute directions.

public static class Directions {

    // All Game Objects must be designed so that they face north when not rotated!
    // Otherwise, we have no idea what angle to set to make it face a direction

    public enum Direction { North = 1, East = 2, South = 4, West = 8 };


    // Rotates a transform into the specified cardinal direction, local to relativeTo or its parent if not specified
    public static void rotate(Transform transform, Direction dir, Transform relativeTo = null)
    {
        if (transform == null)
        {
            return;
        }

        float angle;
        switch (dir)
        {
            case Direction.East:
                angle = 90.0f;
                break;
            case Direction.South:
                angle = 180.0f;
                break;
            case Direction.West:
                angle = 270.0f;
                break;
            default:
                angle = 0.0f;
                break;
        }

        // We rotate the vector so that eulerAngles.y is the rotation about a fixed global axis, then we assign our value and then undo our previous rotation
        // We use eulerAngles so that we can assign a degree value without having to worry about the current rotation in that direction and avoid touching rotation about other axes
        if (relativeTo == null)
        {
            // We can just do localRotation in this case instead for performance

            Vector3 newAngles = transform.localEulerAngles;
            newAngles.y = angle;

            transform.localRotation = Quaternion.Euler(newAngles);
        }
        else
        {
            Vector3 relVec = relativeTo.forward;
            Quaternion rot = Quaternion.FromToRotation(relVec, Vector3.forward);

            Vector3 newAngles = (rot * transform.rotation).eulerAngles;
            newAngles.y = angle;

            transform.rotation = Quaternion.Inverse(rot) * Quaternion.Euler(newAngles);
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


    // Returns the vector representing this direction
    public static Vector3 toDirectionVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.North:
                return Vector3.forward;
            case Direction.East:
                return Vector3.right;
            case Direction.South:
                return Vector3.back;
            case Direction.West:
                return Vector3.left;
            default:
                return Vector3.forward;
        }

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
