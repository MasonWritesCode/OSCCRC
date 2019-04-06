using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu_New : MonoBehaviour, IPointerClickHandler
{
    public AudioSource audioData;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            audioData.Play(0);

            GameObject typeSelect = transform.parent.gameObject;
            GameObject levelSelect = GameObject.FindWithTag("Menu").transform.Find("LevelFolder").gameObject;

            typeSelect.SetActive(false);
            levelSelect.SetActive(true);

            Menu_Panel panelScript = levelSelect.transform.Find("MapsList").GetComponent<Menu_Panel>();
            panelScript.folder = Menu_Panel.Folder.New;
        }
    }
}
