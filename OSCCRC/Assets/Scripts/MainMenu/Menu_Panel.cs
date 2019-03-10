using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class Menu_Panel : MonoBehaviour {

    public enum Folder { Unset, Retro, New, Custom };

    public Folder folder { get { return m_folder; } set { m_folder = value; getFiles(value); page = 0; } }
    public int page { get { return m_pageNum; } set { setPage(value); } }

    void Awake()
    {
        m_folderNames.Add(Folder.Retro, "Retro/");
        m_folderNames.Add(Folder.New, "New/");
        m_folderNames.Add(Folder.Custom, "Custom/");

        m_entryPrefab = GameResources.objects["MapEntry"];
        m_tempStageInfo = gameObject.AddComponent<GameStage>();
    }


    public void load(int place)
    {
        FileInfo selectedFile = m_fileList[place + m_startIndex];
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
        GlobalData.currentStageFile = m_folderNames[m_folder] + selectedFile.Name.Remove(selectedFile.Name.Length - selectedFile.Extension.Length);
    }


    // Retrieves the stage files for the currently selected folder
    private void getFiles(Folder folder)
    {
        DirectoryInfo di = new DirectoryInfo(Application.streamingAssetsPath + "/Maps/" + m_folderNames[folder]);
        m_fileList = di.GetFiles("*.stage");
    }


    // Creates and places a new GameObjects for listing stages
    private void placeEntry(FileInfo file, int place)
    {
        Transform newEntryObj = Instantiate(m_entryPrefab, transform);
        Menu_MapEntry newEntry = newEntryObj.GetComponent<Menu_MapEntry>();
        newEntry.entryID = place;

        m_tempStageInfo.loadStageMetadata(m_folderNames[m_folder] + file.Name.Remove(file.Name.Length - file.Extension.Length));

        newEntry.setName(m_tempStageInfo.stageName);

        RectTransform rt = newEntry.GetComponent<RectTransform>();

        int myHeight = 25;
        int myWidth = 560;
        int m_XAxis = 0;
        int m_YAxis = 165 - ((myHeight + 5) * place);

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, myHeight);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, myWidth);
        rt.anchoredPosition = new Vector2(m_XAxis, m_YAxis);
    }


    // Removes an entry from the GameObjects used for listing stages
    private void removeEntry(Menu_MapEntry entry)
    {
        Destroy(entry.gameObject);
    }


    // Displays a page of the file list
    private void setPage(int pageNum)
    {
        if (pageNum < 0)
        {
            m_pageNum = 0;
        }
        else
        {
            m_pageNum = pageNum;
        }

        m_startIndex = m_pageNum * m_numEntries;
        // If we navigate past all the entries, we want to stay filled with the last entries
        if (m_startIndex > (m_fileList.Length - m_numEntries) && m_fileList.Length > m_numEntries)
        {
            m_startIndex = m_fileList.Length - m_numEntries;
            m_pageNum--;
        }

        Menu_MapEntry[] entries = GetComponentsInChildren<Menu_MapEntry>();
        for (int i = 0; i < entries.Length; ++i)
        {
            removeEntry(entries[i]);
        }

        for (int i = 0; i < m_numEntries && i < m_fileList.Length; ++i)
        {
            placeEntry(m_fileList[i + m_startIndex], i);
        }
    }


    private const int m_numEntries = 10;
    private int m_pageNum = 0;
    private int m_startIndex = 0;
    private Folder m_folder = Folder.Unset;
    private Dictionary<Folder, string> m_folderNames = new Dictionary<Folder, string>(3);
    private FileInfo[] m_fileList;
    private Transform m_entryPrefab;
    private GameStage m_tempStageInfo;
}
