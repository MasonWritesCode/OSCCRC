using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Editor_SaveButton : MonoBehaviour, IPointerClickHandler
{
    void Start()
    {
        m_editor = GameObject.FindWithTag("GameController").GetComponent<Editor>();
        m_gameStage = GameObject.FindWithTag("GameController").GetComponent<GameStage>();
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            string savePath = transform.parent.GetComponentInChildren<Text>().text;
            string stageName = transform.parent.Find("Stage").GetComponentInChildren<Text>().text;

            Debug.Log(savePath);
            Debug.Log(stageName);
            m_gameStage.stageName = stageName;
            m_editor.createSave(savePath);
        }
    }

    private Editor m_editor;
    private GameStage m_gameStage;
}