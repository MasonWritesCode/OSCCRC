using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// "Navigates" a menu by enabling an object and disabling others.

public class Menu_Navigate : MonoBehaviour, IPointerClickHandler
{
    public GameObject targetMenu;
    public GameObject[] closableMenus;
    public AudioSource audioData;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            if (audioData)
            {
                audioData.Play(0);
            }

            for (int i = 0; i < closableMenus.Length; ++i)
            {
                closableMenus[i].SetActive(false);
            }

            targetMenu.SetActive(true);
        }
    }
}
