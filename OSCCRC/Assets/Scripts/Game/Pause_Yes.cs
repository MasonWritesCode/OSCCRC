using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Pause_Yes : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            GlobalData.mode = GameController.GameMode.None;

            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
    }
}
