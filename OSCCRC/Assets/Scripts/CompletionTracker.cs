using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public static class CompletionTracker {

    // Marks the stage with path stagePath as completed
    public static void markCompleted(string stagePath)
    {
        updateTrackerData();

        if (!m_completedFiles.Contains(stagePath))
        {
            m_completedFiles.Add(stagePath);

            writeTrackerData();
        }
    }


    // Makes the stage with path stagePath no longer marked as completed if it has been
    public static void unmarkCompleted(string stagePath)
    {
        updateTrackerData();

        if (m_completedFiles.Contains(stagePath))
        {
            m_completedFiles.Remove(stagePath);

            writeTrackerData();
        }
    }


    // Removes all saved data related to completions
    public static void unmarkAll()
    {
        if (File.Exists(m_trackerFilePath))
        {
            File.Delete(m_trackerFilePath);
        }
    }


    // Returns true if the stage located at stagePath has been completed
    public static bool isCompleted(string stagePath)
    {
        updateTrackerData();

        return m_completedFiles.Contains(stagePath);
    }


    // Grabs the contents of the tracker data file
    private static void readTrackerData()
    {
        if (!File.Exists(m_trackerFilePath))
        {
            return;
        }

        m_completedFiles.Clear();

        using (StreamReader fin = new StreamReader(m_trackerFilePath, Encoding.ASCII))
        {
            while (!fin.EndOfStream)
            {
                m_completedFiles.Add(fin.ReadLine());
            }
        }

        m_shouldRead = false;
    }


    // Updates the tracker file with the current data
    private static void writeTrackerData()
    {
        using (StreamWriter fout = new StreamWriter(m_trackerFilePath, false, Encoding.ASCII))
        {
            for (int i = 0; i < m_completedFiles.Count; ++i)
            {
                fout.WriteLine(m_completedFiles[i]);
            }
        }
    }


    // Reads the data to be used from the tracker file if deemed appropriate
    private static void updateTrackerData()
    {
        if (m_shouldRead)
        {
            readTrackerData();
        }
    }


    private static string m_trackerFilePath = Application.persistentDataPath + "/completions.dat";
    private static List<string> m_completedFiles = new List<string>(); // List is chosen because we might regularly iterate over the collection for writing the data
    private static bool m_shouldRead = true; // We want to assume the file hasn't been externally changed during CompletionTracker's lifetime to avoid unnecessary IO
}
