﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu_Puzzle : MonoBehaviour, IPointerClickHandler
{ 
    public AudioSource audioData;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            audioData.Play(0);

            GlobalData.mode = GameController.GameMode.Puzzle;

            GameObject main = transform.parent.gameObject;
            GameObject typeSelect = GameObject.FindWithTag("Menu").transform.Find("TypeFolder").gameObject;

            main.SetActive(false);
            typeSelect.SetActive(true);
        }
    }
}
