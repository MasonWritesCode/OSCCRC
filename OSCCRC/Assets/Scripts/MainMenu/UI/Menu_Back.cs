using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu_Back : MonoBehaviour, IPointerClickHandler
{
    public AudioSource audioData;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            audioData.Play(0);

            GameObject typeSelect = transform.parent.gameObject;
            GameObject main = GameObject.FindWithTag("Menu").transform.Find("Main").gameObject;

            typeSelect.SetActive(false);
            main.SetActive(true);
        }
    }
}
