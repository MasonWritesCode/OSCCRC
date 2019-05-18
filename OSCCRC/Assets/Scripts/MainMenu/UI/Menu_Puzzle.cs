using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu_Puzzle : MonoBehaviour, IPointerClickHandler
{
    public GameObject mainMenu;
    public GameObject typeMenu;
    public AudioSource audioData;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            audioData.Play(0);

            GlobalData.mode = GameController.GameMode.Puzzle;

            mainMenu.SetActive(false);
            typeMenu.SetActive(true);
        }
    }
}
