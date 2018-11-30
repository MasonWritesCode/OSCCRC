﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Editor_SaveButton : MonoBehaviour, IPointerClickHandler
{
    Editor editor;
    GameStage gameStage;
    void Start()
    {
        editor = GameObject.FindWithTag("GameController").GetComponent<Editor>();
        gameStage = GameObject.FindWithTag("GameController").GetComponent<GameStage>();
    }

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
            string savePath = transform.parent.GetComponentInChildren<Text>().text;
            string stageName = transform.parent.Find("Stage").GetComponentInChildren<Text>().text;
            Debug.Log(savePath);
            Debug.Log(stageName);
            gameStage.stageName = stageName;
            editor.createSave(savePath);

        }
    }
}