using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Editor_LoadButton : MonoBehaviour, IPointerClickHandler
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
            m_editor.loadSave(pathInput.text);

            stageNameInput.text = m_gameStage.stageName;
        }
    }

    private Editor m_editor;
    private GameStage m_gameStage;
}