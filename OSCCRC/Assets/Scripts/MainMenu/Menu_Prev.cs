using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu_Prev : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            Menu_Panel mapList = transform.parent.Find("MapsList").GetComponent<Menu_Panel>();

            mapList.page = mapList.page - 1;
        }
    }
}