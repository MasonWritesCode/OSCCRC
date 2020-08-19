using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu_Competitive : MonoBehaviour, IPointerClickHandler
{
    public Menu_Panel levelMenu;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            GlobalData.mode = GameController.GameMode.Competitive; 
            levelMenu.folder = Menu_Panel.Folder.Competitive;
        }
    }
}
