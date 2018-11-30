﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Pause_No : MonoBehaviour, IPointerClickHandler
{

    //Detect if a click occurs
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        //Use this to tell when the user right-clicks on the Button
        if (pointerEventData.button == PointerEventData.InputButton.Right)
        {
            //Output to console the clicked GameObject's name and the following message. You can replace this with your own actions for when clicking the GameObject.
            Debug.Log(name + " Game Object Right Clicked!");
        }

        //Use this to tell when the user left-clicks on the Button
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log(name + " Game Object Left Clicked!");

            Canvas pauseDisplay = GameObject.Find("PauseMenu").GetComponent<Canvas>();
            GameController m_gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
            PlayerController player1 = GameObject.FindWithTag("Player").GetComponentInChildren<PlayerController>();

            pauseDisplay.enabled = false;
            if (player1.menuPaused)
            {
                m_gameController.isPaused = false;
                player1.menuPaused = false;
            }

        }
    }
}