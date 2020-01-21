﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class Menu_Panel : MonoBehaviour {

    public enum Folder { Unset, Competitive, Retro, New, Custom };

    public RectTransform mapEntryPrefab;
    public RectTransform mapsList;
    public SceneLoader sceneLoader;

    public Folder folder { get { return m_folder; } set { m_folder = value; getFiles(value); page = 0; } }
    public int page { get { return m_pageNum; } set { setPage(value); } }

    void Awake()
    {
        m_folderNames.Add(Folder.Competitive, "Competitive/");
        m_folderNames.Add(Folder.Retro, "Retro/");
        m_folderNames.Add(Folder.New, "New/");
        m_folderNames.Add(Folder.Custom, "Custom/");

        m_tempStageInfo = gameObject.AddComponent<GameStage>();
    }


    public void load(int place)
    {
        FileInfo selectedFile = m_fileList[place + m_startIndex];
        GlobalData.currentStagePath = m_folderNames[m_folder] + selectedFile.Name;
        sceneLoader.loadGameScene();
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
        int m_YAxis = 160 - ((myHeight + 5) * place);

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


    private const int m_numEntries = 10; // Number of entries displayed at any given time
    private int m_pageNum = 0;
    private int m_startIndex = 0;
    private Folder m_folder = Folder.Unset;
    private Dictionary<Folder, string> m_folderNames = new Dictionary<Folder, string>(4);
    private FileInfo[] m_fileList;
    private GameStage m_tempStageInfo;
}
