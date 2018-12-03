using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// This class is an interface to a game stage, being a combination of the map, music, or other assets that will be loaded and used together at the same time.

public class GameStage : MonoBehaviour {

    // This class handles a list of directional improvement placements that are allowed for a stage
    public class availablePlacements
    {
        public availablePlacements()
        {
            counts = new Dictionary<Directions.Direction, int>{
                { Directions.Direction.North, 0 },
                { Directions.Direction.East, 0 },
                { Directions.Direction.South, 0 },
                { Directions.Direction.West, 0 }
            };
        }

        public availablePlacements(availablePlacements other)
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


    public availablePlacements placements = new availablePlacements();
    // Not using file name as the stage name gives more flexibility in naming, should we choose to show a map name somewhere
    public string stageName { get { return m_stageName; } set { m_stageName = value; } }
    // Maybe add a game mode? Probably will later just in case it gets used.
    public string musicTrack { get { return m_musicTrack; } set { m_musicTrack = value; } }
    public string resourcePackName { get { return m_resourcePack; } set { m_resourcePack = value; } }

    private string m_stageName = "Stage";
    private string m_musicTrack = "Default";
    private string m_resourcePack = "Default";
    // Increment this whenever we change map or stage file layout after a release
    private const int m_currentFileVersion = 1;


    // Get information of a stage without actually loading it.
    public void loadStageMetadata(string fileName)
    {
        string stagePath = Application.dataPath + "/Maps/" + fileName + ".stage";
        if (!File.Exists(stagePath))
        {
            Debug.LogWarning("Tried to load a stage but it wasn't found! " + stagePath);
            return;
        }
        using (StreamReader fin = new StreamReader(stagePath, Encoding.ASCII))
        {
            int versionNumber;
            bool recognizedVers = int.TryParse(fin.ReadLine(), out versionNumber);

            // For now, assume if this is good, everything else is good too
            if (!recognizedVers || versionNumber != m_currentFileVersion)
            {
                Debug.LogWarning("Failed to read stage file " + fileName);
                return;
            }

            stageName = fin.ReadLine();
            musicTrack = fin.ReadLine();
            resourcePackName = fin.ReadLine();
        }
    }
    // Loads a stage specified by "fileName" as the current stage
    public void loadStage(string fileName)
    {
        string stagePath = Application.dataPath + "/Maps/" + fileName + ".stage";
        if (!File.Exists(stagePath))
        {
            Debug.LogWarning("Tried to load a stage but it wasn't found! " + stagePath);
            return;
        }
        using (StreamReader fin = new StreamReader(stagePath, Encoding.ASCII))
        {
            int versionNumber;
            bool recognizedVers = int.TryParse(fin.ReadLine(), out versionNumber);

            // For now, assume if this is good, everything else is good too
            if (!recognizedVers || versionNumber != m_currentFileVersion)
            {
                Debug.LogWarning("Failed to read stage file " + fileName);
                return;
            }

            stageName = fin.ReadLine();
            musicTrack = fin.ReadLine();
            resourcePackName = fin.ReadLine();
            GameResources.loadResources(resourcePackName);
            placements.set( Directions.Direction.North, int.Parse(fin.ReadLine()) );
            placements.set( Directions.Direction.East, int.Parse(fin.ReadLine()) );
            placements.set( Directions.Direction.South, int.Parse(fin.ReadLine()) );
            placements.set( Directions.Direction.West, int.Parse(fin.ReadLine()) );

            // Now load the map itself
            GameMap gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
            bool wasLoaded = gameMap.importMap(fin);
            if (!wasLoaded)
            {
                Debug.LogWarning("Failed to read stage file " + fileName);
            }
        }
    }


    // Saves the currently loaded stage under a file specified by "fileName"
    public void saveStage(string fileName)
    {
        string mapPath = Application.dataPath + "/Maps/" + fileName + ".stage";
        using (StreamWriter fout = new StreamWriter(mapPath, false, Encoding.ASCII))
        {
            fout.WriteLine(m_currentFileVersion);

            fout.WriteLine(stageName);
            fout.WriteLine(musicTrack);
            fout.WriteLine(resourcePackName);
            fout.WriteLine(placements.get(Directions.Direction.North));
            fout.WriteLine(placements.get(Directions.Direction.East));
            fout.WriteLine(placements.get(Directions.Direction.South));
            fout.WriteLine(placements.get(Directions.Direction.West));

            // Now save the map itself
            GameMap gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
            gameMap.exportMap(fout);
        }
    }
}
