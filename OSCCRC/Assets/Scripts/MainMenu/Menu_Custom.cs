using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Menu_Custom : MonoBehaviour, IPointerClickHandler
{

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

            GlobalData.folder = Menu_Panel.Folder.Custom;
            GlobalData.curPage = 1;

            GameObject typeSelect = transform.parent.gameObject;
            GameObject levelSelect = GameObject.FindWithTag("Menu").transform.Find("LevelFolder").gameObject;

            typeSelect.SetActive(false);
            levelSelect.SetActive(true);

            // Show page 0 to start with
            Menu_Panel panelScript = levelSelect.transform.Find("MapsList").GetComponent<Menu_Panel>();
            panelScript.getFiles();
            panelScript.showPage(0);
        }
    }
}
