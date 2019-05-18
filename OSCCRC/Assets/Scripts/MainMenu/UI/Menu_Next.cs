using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu_Next : MonoBehaviour, IPointerClickHandler
{
    public GameObject levelMenu;
    public AudioSource audioData;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            audioData.Play(0);

            Menu_Panel mapList = levelMenu.GetComponent<Menu_Panel>();

            mapList.page = mapList.page + 1;
        }
    }
}