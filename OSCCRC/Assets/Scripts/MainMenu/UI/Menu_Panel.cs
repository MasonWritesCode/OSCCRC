using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Menu_Panel : MonoBehaviour {

    public enum Folder { Unset, Competitive, Retro, New, Custom };

    public RectTransform mapEntryPrefab;
    public RectTransform mapsList;
    public SceneLoader sceneLoader;

    public Folder folder { get { return m_folder; } set { setFolder(value); } }
    public int page { get { return m_pageNum; } set { setPage(value); } }

    void Awake()
    {
        m_tempStageInfo = gameObject.AddComponent<GameStage>();

        // We might have wanted to set folder before the gameobject became active
        // So now that we are active we can open a page for the folder if it was set
        if (folder != Folder.Unset)
        {
            page = 0;
        }
    }


    // Loads the stage referenced by the entry ID
    public void loadStage(int place)
    {
        FileInfo selectedFile = m_fileList[place + m_startIndex];
        GlobalData.currentStagePath = m_folderNames[m_folder] + selectedFile.Name;
        sceneLoader.loadGameScene();
    }


    // Retrieves the stage files for the currently selected folder
    private void getFiles(Folder folder)
    {
        DirectoryInfo di = new DirectoryInfo(Application.streamingAssetsPath + "/Maps/" + m_folderNames[folder]);

        if (di.Exists)
        {
            m_fileList = di.GetFiles("*.stage");
        }
        else
        {
            m_fileList = new FileInfo[0];
        }
    }


    // Creates and places a new GameObjects for listing stages
    private void placeEntry(FileInfo file, int place)
    {
        Transform newEntryObj = Instantiate(mapEntryPrefab, mapsList);
        Menu_MapEntry newEntry = newEntryObj.GetComponent<Menu_MapEntry>();
        newEntry.entryID = place;
        newEntry.parentPanel = this;

        m_tempStageInfo.loadStageMetadata(m_folderNames[m_folder] + file.Name.Remove(file.Name.Length - file.Extension.Length));

        string entryName = m_tempStageInfo.stageName;
        if (CompletionTracker.isCompleted(m_folderNames[m_folder] + file.Name))
        {
            entryName = entryName + " (Completed)";
        }
        newEntry.setName(entryName);

        RectTransform rt = newEntry.GetComponent<RectTransform>();

        int myHeight = 26;
        int myWidth = 560;
        int m_XAxis = 0;
        int m_YAxis = 145 - ((myHeight + 10) * place);

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, myHeight);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, myWidth);
        rt.anchoredPosition = new Vector2(m_XAxis, m_YAxis);
    }


    // Removes an entry from the GameObjects used for listing stages
    private void removeEntry(Menu_MapEntry entry)
    {
        Destroy(entry.gameObject);
    }


    // Loads files from a folder
    private void setFolder(Folder folder)
    {
        if (folder == Folder.Unset)
        {
            return;
        }

        // For now setFolder will act as a request to update the file list even if already set to this folder
        // This also resets the page to page 0
        getFiles(folder);
        m_folder = folder;

        // We use m_temStageInfo to verify the gameobject became active since folder might be set before then
        if (m_tempStageInfo != null)
        {
            page = 0;
        }
    }


    // Displays a page of the file list
    private void setPage(int pageNum)
    {
        Menu_MapEntry[] entries = GetComponentsInChildren<Menu_MapEntry>();
        for (int i = 0; i < entries.Length; ++i)
        {
            removeEntry(entries[i]);
        }

        // If we don't have more than a page's worth of entries, we force the page to be 0
        if (m_fileList.Length <= m_numEntries)
        {
            m_pageNum = 0;
            m_startIndex = 0;
        }
        else
        {
            m_pageNum = System.Math.Max(pageNum, 0);
            m_startIndex = m_pageNum * m_numEntries;

            // If we navigate past all the entries, we want to stay filled with the last entries
            if (m_startIndex > (m_fileList.Length - m_numEntries) && m_fileList.Length > m_numEntries)
            {
                m_startIndex = m_fileList.Length - m_numEntries;
                m_pageNum--;
            }
        }

        for (int i = 0; i < m_numEntries && i < m_fileList.Length; ++i)
        {
            placeEntry(m_fileList[i + m_startIndex], i);
        }
    }


    private const int m_numEntries = 8; // Number of entries displayed at any given time
    private int m_pageNum = 0;
    private int m_startIndex = 0;
    private Folder m_folder = Folder.Unset;
    private FileInfo[] m_fileList;
    private GameStage m_tempStageInfo = null;

    private Dictionary<Folder, string> m_folderNames = new Dictionary<Folder, string>(4) {
        { Folder.Competitive, "Competitive/" },
        { Folder.Retro,       "Retro/"       },
        { Folder.New,         "New/"         },
        { Folder.Custom,      "Custom/"      }
    };
}
