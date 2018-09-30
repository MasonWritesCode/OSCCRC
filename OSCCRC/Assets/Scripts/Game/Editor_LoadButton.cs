using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Editor_LoadButton : MonoBehaviour, IPointerClickHandler
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
            string loadPath = transform.parent.GetComponentInChildren<Text>().text;

            editor.loadSave(loadPath);

            InputField field = transform.parent.Find("Stage").GetComponent<InputField>();
            field.text = gameStage.stageName;
        }
    }
}