using System.Collections;
using System.Collections.Generic;

// This class handles a list of directional improvement placements that are allowed for a stage

public class AvailablePlacements
{
    public AvailablePlacements()
    {
        counts = new Dictionary<Directions.Direction, int>(4, new Directions.DirectionComparer()){
                { Directions.Direction.North, 0 },
                { Directions.Direction.East, 0 },
                { Directions.Direction.South, 0 },
                { Directions.Direction.West, 0 }
            };
    }

    public AvailablePlacements(AvailablePlacements other)
    {
        counts = new Dictionary<Directions.Direction, int>(other.counts);
    }

    // Adds an additional count of directional tile of direction "dir" to be placed
    public void add(Directions.Direction dir)
    {
        // Shouldn't have to worry about overflow here
        ++counts[dir];
    }

    // Removes a single count of directional tile of direction "dir" from being an available placement
    public void remove(Directions.Direction dir)
    {
        if (counts[dir] > 0)
        {
            --counts[dir];
        }
    }

    // Sets the number of of available placements of directional tiles of the direction specified by "dir"
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void set(Directions.Direction dir, int count)
    {
        counts[dir] = count;
    }

    // Returns the number of of available placements of directional tiles of the direction specified by "dir"
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int get(Directions.Direction dir)
    {
        return counts[dir];
    }

    private Dictionary<Directions.Direction, int> counts;
}
