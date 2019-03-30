using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Complete_Next : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            GlobalData.mode = GameController.GameMode.None;

            string [] splitFilePath = GlobalData.currentStagePath.Split(new char[] {'/'});
            string curStageName = splitFilePath[splitFilePath.Length - 1];
            string curStagePath = "";
            for(int i = 0; i < splitFilePath.Length - 1; i++)
            {
                curStagePath += splitFilePath[i] + "/";
            }

            DirectoryInfo di = new DirectoryInfo(Application.streamingAssetsPath + "/Maps/" + curStagePath);
            m_fileList = di.GetFiles("*.stage");

            for (int i = 0; i < m_fileList.Length - 1; i++)
            {
                if (m_fileList[i].Name == curStageName)
                {
                    GameController gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
                    gameController.GetComponent<GameStage>().loadStage(curStagePath + m_fileList[i + 1].Name.Remove(m_fileList[i + 1].Name.Length - ".stage".Length));
                    gameController.runGame(gameController.mode);
                    GetComponentInParent<Canvas>().enabled = false;
                    return;
                }
            }

            SceneManager.LoadScene("MainMenu");
        }
    }

    private FileInfo[] m_fileList;
}
