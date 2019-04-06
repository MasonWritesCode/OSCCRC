using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu_BackLevel : MonoBehaviour, IPointerClickHandler
{
    public AudioSource audioData;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            audioData.Play(0);

            GameObject levelSelect = transform.parent.gameObject;
            GameObject typeSelect = GameObject.FindWithTag("Menu").transform.Find("TypeFolder").gameObject;

            levelSelect.SetActive(false);
            typeSelect.SetActive(true);
        }
    }
}
