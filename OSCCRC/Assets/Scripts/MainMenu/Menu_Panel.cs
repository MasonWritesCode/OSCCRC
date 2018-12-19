using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class Menu_Panel : MonoBehaviour {

    public enum Folder { Retro, New, Custom };

    private Dictionary<Folder, string> m_folderNames = new Dictionary<Folder, string>(3);

    private const int m_numEntries = 10;
    private FileInfo[] m_fileList;
    private Transform m_entryPrefab;

    private GameStage m_tempStageInfo;

    void Awake()
    {
        m_folderNames.Add(Folder.Retro, "Retro/");
        m_folderNames.Add(Folder.New, "New/");
        m_folderNames.Add(Folder.Custom, "Custom/");

        m_entryPrefab = GameResources.objects["MapEntry"];
        m_tempStageInfo = gameObject.AddComponent<GameStage>();
    }

    // Retrieves file list for the globally selected folder
    public void getFiles()
    {
        DirectoryInfo di = new DirectoryInfo(Application.streamingAssetsPath + "/Maps/" + m_folderNames[GlobalData.folder]);
        m_fileList = di.GetFiles("*.stage");
    }

    // Displays a page of the file list
    public void showPage(int page)
    {
        int startIndex = page * m_numEntries;
        // If we navigate past all the entries, we want to stay filled with the last entries
        if (startIndex > (m_fileList.Length - m_numEntries) && m_fileList.Length > m_numEntries)
        {
            startIndex = m_fileList.Length - m_numEntries;
            GlobalData.curPage--;
        }

        // Destroy the old entries
        Menu_MapEntry[] entries = GetComponentsInChildren<Menu_MapEntry>();
        for (int i = 0; i < entries.Length; ++i)
        {
            removeEntry(entries[i]);
        }

        for (int i = 0; i < m_numEntries && i < m_fileList.Length; ++i)
        {
            placeEntry(m_fileList[i+startIndex], i);
        }
    }

    private void placeEntry(FileInfo file, int place)
    {
        Transform newEntryObj = Instantiate(m_entryPrefab, transform);
        Menu_MapEntry newEntry = newEntryObj.GetComponent<Menu_MapEntry>();
        newEntry.entryID = place;

        m_tempStageInfo.loadStageMetadata(m_folderNames[GlobalData.folder] + file.Name.Replace(".stage", ""));

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

    private void removeEntry(Menu_MapEntry entry)
    {
        Destroy(entry.gameObject);
    }

    public void load(int place)
    {
        FileInfo selectedFile = m_fileList[place];
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
        //m_tempStageInfo.loadStage(m_folderNames[GlobalData.folder] + selectedFile.Name.Replace(".stage", ""));
        GlobalData.currentStageFile = m_folderNames[GlobalData.folder] + selectedFile.Name.Replace(".stage", "");
    }
}
