using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu_Custom : MonoBehaviour, IPointerClickHandler
{
    public Menu_Panel levelMenu;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            levelMenu.folder = Menu_Panel.Folder.Custom;
        }
    }
}
