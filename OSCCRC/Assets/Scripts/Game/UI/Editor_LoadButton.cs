using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Editor_LoadButton : MonoBehaviour, IPointerClickHandler
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
            string loadPath = transform.parent.GetComponentInChildren<Text>().text;

            m_editor.loadSave(loadPath);

            InputField field = transform.parent.Find("Stage").GetComponent<InputField>();
            field.text = m_gameStage.stageName;
        }
    }

    private Editor m_editor;
    private GameStage m_gameStage;
}