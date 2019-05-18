using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Menu_StageCreator : MonoBehaviour, IPointerClickHandler
{
    public AudioSource audioData;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            audioData.Play(0);

            GlobalData.currentStagePath = "Internal/default.stage";
            GlobalData.mode = GameController.GameMode.Editor;

            SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
    }
}
