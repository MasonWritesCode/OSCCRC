using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Editor_SaveButton : MonoBehaviour, IPointerClickHandler
{
    public InputField pathInput;
    public InputField stageNameInput;

    void Start()
    {
        m_gameStage = GameObject.FindWithTag("GameController").GetComponent<GameStage>();
        m_editor = GameObject.FindWithTag("GameController").GetComponent<Editor>();
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            string savePath = pathInput.text;
            string stageName = stageNameInput.text;

            Debug.Log(savePath);
            Debug.Log(stageName);
            m_gameStage.stageName = stageName;
            m_editor.createSave(savePath);
        }
    }

    private Editor m_editor;
    private GameStage m_gameStage;
}