using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Complete_Next : MonoBehaviour, IPointerClickHandler
{
    public AudioSource audioData;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            if (audioData != null)
            {
                audioData.Play(0);
            }

            GlobalData.mode = GameController.GameMode.None;

            // Get the path of our current stage, because we only want the same game mode
            string [] splitFilePath = GlobalData.currentStagePath.Split(new char[] {'/'});
            string curStageName = splitFilePath[splitFilePath.Length - 1];
            string curStagePath = "";
            for(int i = 0; i < splitFilePath.Length - 1; i++)
            {
                curStagePath += splitFilePath[i] + "/";
            }

            try
            {
                DirectoryInfo di = new DirectoryInfo(Application.streamingAssetsPath + "/Maps/" + curStagePath);
                m_fileList = di.GetFiles("*.stage");
            }
            catch
            {
                SceneManager.LoadScene("MainMenu");
            }

            // Search for the current stage and open the next one if there is one
            for (int i = 0; i < m_fileList.Length - 1; i++)
            {
                if (m_fileList[i].Name == curStageName)
                {
                    GameController gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
                    gameController.GetComponent<GameStage>().loadStage(curStagePath + m_fileList[i + 1].Name.Remove(m_fileList[i + 1].Name.Length - ".stage".Length));
                    gameController.runGame(gameController.mode);
                    transform.parent.gameObject.SetActive(false);

                    return;
                }
            }

            SceneManager.LoadScene("MainMenu");
        }
    }

    private FileInfo[] m_fileList;
}
