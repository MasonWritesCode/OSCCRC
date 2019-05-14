using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu_BackLevel : MonoBehaviour, IPointerClickHandler
{
    public GameObject typeMenu;
    public GameObject levelMenu;
    public AudioSource audioData;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            audioData.Play(0);

            levelMenu.SetActive(false);
            typeMenu.SetActive(true);
        }
    }
}
