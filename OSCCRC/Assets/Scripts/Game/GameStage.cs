using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// This class is an interface to a game stage, being a combination of the map, music, or other assets that will be loaded and used together at the same time.

public class GameStage : MonoBehaviour {

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

            // Now save the map itself
            GameMap gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
            gameMap.exportMap(fout);
        }
    }
}
