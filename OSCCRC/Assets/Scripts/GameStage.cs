using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameStage : MonoBehaviour {

    public LinkedList<Directions.Direction> availablePlacements = new LinkedList<Directions.Direction>();
    // Not using file name as the stage name gives more flexibility in naming, should we choose to show a map name somewhere
    public string stageName = "Stage";
    public string musicTrack = "Default";
    public string resourcePackName = "Default";

    // Increment this whenever we change map or stage file layout after a release
    private const int m_currentFileVersion = 1;

    public void loadStage(string fileName)
    {
        string stagePath = Application.dataPath + "/Maps/" + fileName + ".stage";
        if (!File.Exists(stagePath))
        {
            Debug.LogWarning("Tried to load a stage but it wasn't found! " + stagePath);
            return;
        }
        using (StreamReader fin = new StreamReader(stagePath))
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
            int numPlacements = int.Parse(fin.ReadLine());
            for (int i = 0; i <  numPlacements; ++i)
            {
                availablePlacements.AddLast((Directions.Direction)int.Parse(fin.ReadLine()));
            }

            // Now load the map itself
            GameMap gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
            bool wasLoaded = gameMap.importMap(fin);
            if (!wasLoaded)
            {
                Debug.LogWarning("Failed to read stage file " + fileName);
            }
        }
    }

    public void saveStage(string fileName)
    {
        string mapPath = Application.dataPath + "/Maps/" + fileName + ".stage";
        using (StreamWriter fout = new StreamWriter(mapPath, false))
        {
            fout.WriteLine(m_currentFileVersion);

            fout.WriteLine(stageName);
            fout.WriteLine(musicTrack);
            fout.WriteLine(resourcePackName);
            fout.WriteLine(availablePlacements.Count);
            foreach (Directions.Direction d in availablePlacements)
            {
                fout.WriteLine((int)d);
            }

            // Now save the map itself
            GameMap gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
            gameMap.exportMap(fout);
        }
    }
}
