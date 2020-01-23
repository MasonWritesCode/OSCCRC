using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pause_No : MonoBehaviour, IPointerClickHandler
{
    public AudioSource audioData;
    public Canvas pauseDisplay; // Editor Set

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            if (audioData != null)
            {
                audioData.Play(0);
            }

            GameController m_gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

            pauseDisplay.enabled = false;
            m_gameController.gameState.removeState(GameState.TagState.Suspended);
        }
    }
}