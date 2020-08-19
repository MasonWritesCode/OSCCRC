using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;

public class Menu_UpdateMaps : MonoBehaviour, IPointerClickHandler
{
    public GameObject gameDataObj; // Editor Set

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            // Currently map saving and loading requires the unity game objects to write and read the data
            // We could modify GameMap to support a "virtual" game map that doesn't do instantiations and would be
            //  vastly more efficient for this purpose, but I'm not going to bother doing that just for this rarely used function.
            // For now, we just create a GameObject that will be used for those instantiations

            // We need to pass in prefabs to the component, so the components are just loaded with the scene now.
            // Actually, I think the only thing we need to pass in was set to be passed in by mistake, so review this later.
            /*
            if (GameDataObj == null)
            {
                GameDataObj = new GameObject();
                GameDataObj.AddComponent<GameStage>();
                GameDataObj.AddComponent<GameResources>();
                GameDataObj.AddComponent<GameMap>();
                GameDataObj.tag = "Map";
            }
            */

            updateAll();
        }
    }

    private void updateAll()
    {
        Debug.Log("Beginning Update to latest map format (may take some time)...");

        gameDataObj.SetActive(true);
        GameMap mapData = gameDataObj.GetComponent<GameMap>();
        GameStage stageData = gameDataObj.GetComponent<GameStage>();

        DirectoryInfo mapsDir = new DirectoryInfo(Application.streamingAssetsPath + "/Maps/");
        DirectoryInfo[] mapCollections = mapsDir.GetDirectories();

        for (int i = 0; i < mapCollections.Length; ++i)
        {
            FileInfo[] mapFiles = mapCollections[i].GetFiles("*.map");
            for (int j = 0; j < mapFiles.Length; ++j)
            {
                string path = mapCollections[i].Name + '/' + mapFiles[j].Name.Remove(mapFiles[j].Name.Length - mapFiles[j].Extension.Length);
                mapData.loadMap(path);
                mapData.saveMap(path);
            }

            FileInfo[] stageFiles = mapCollections[i].GetFiles("*.stage");
            for (int j = 0; j < stageFiles.Length; ++j)
            {
                string path = mapCollections[i].Name + '/' + stageFiles[j].Name.Remove(stageFiles[j].Name.Length - stageFiles[j].Extension.Length);
                stageData.loadStage(path);
                stageData.saveStage(path);
            }
        }

        gameDataObj.SetActive(false);

        Debug.Log("Map format update complete.");
    }
}
