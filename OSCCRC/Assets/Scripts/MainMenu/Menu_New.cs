using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Menu_New : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            GlobalData.folder = Menu_Panel.Folder.New;

            GameObject typeSelect = transform.parent.gameObject;
            GameObject levelSelect = GameObject.FindWithTag("Menu").transform.Find("LevelFolder").gameObject;

            typeSelect.SetActive(false);
            levelSelect.SetActive(true);

            // Show page 0 to start with
            Menu_Panel panelScript = levelSelect.transform.Find("MapsList").GetComponent<Menu_Panel>();
            panelScript.getFiles();
            panelScript.page = 0;
        }
    }
}
