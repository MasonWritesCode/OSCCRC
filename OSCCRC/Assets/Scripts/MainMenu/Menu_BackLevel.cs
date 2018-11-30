﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Menu_BackLevel : MonoBehaviour, IPointerClickHandler
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

            GameObject levelSelect = transform.parent.gameObject;
            GameObject typeSelect = GameObject.FindWithTag("Menu").transform.Find("TypeFolder").gameObject;

            levelSelect.SetActive(false);
            typeSelect.SetActive(true);
        }
    }
}