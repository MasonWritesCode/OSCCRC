using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu_Competitive : MonoBehaviour, IPointerClickHandler
{
    public GameObject mainMenu;
    public GameObject levelMenu;
    public AudioSource audioData;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            audioData.Play(0);

            GlobalData.mode = GameController.GameMode.Competitive; 

            mainMenu.SetActive(false);
            levelMenu.SetActive(true);

            Menu_Panel panelScript = levelMenu.GetComponent<Menu_Panel>();
            panelScript.folder = Menu_Panel.Folder.Competitive;
        }
    }
}
