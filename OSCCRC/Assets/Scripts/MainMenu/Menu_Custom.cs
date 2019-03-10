﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu_Custom : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            GameObject typeSelect = transform.parent.gameObject;
            GameObject levelSelect = GameObject.FindWithTag("Menu").transform.Find("LevelFolder").gameObject;

            typeSelect.SetActive(false);
            levelSelect.SetActive(true);

            Menu_Panel panelScript = levelSelect.transform.Find("MapsList").GetComponent<Menu_Panel>();
            panelScript.folder = Menu_Panel.Folder.Custom;
        }
    }
}
