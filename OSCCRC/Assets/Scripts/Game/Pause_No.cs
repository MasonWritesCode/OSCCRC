using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Pause_No : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            Canvas pauseDisplay = GameObject.Find("PauseMenu").GetComponent<Canvas>();
            GameController m_gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

            pauseDisplay.enabled = false;
            m_gameController.gameState.removeState(GameState.TagState.Suspended);
        }
    }
}