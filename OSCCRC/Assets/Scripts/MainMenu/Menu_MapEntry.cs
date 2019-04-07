using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Menu_MapEntry : MonoBehaviour, IPointerClickHandler {

    public int entryID;
    public AudioSource audioData;

    public void setThumbnail(string fileName)
    {
        //
    }

    public void setName(string name)
    {
        Text t = transform.Find("LevelName").GetComponent<Text>();
        t.text = name;
    }

    public void setCompleted(bool completed)
    {
        //
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            audioData = GameObject.Find("ClickSound").GetComponent<AudioSource>();
            audioData.Play(0);

            transform.parent.GetComponent<Menu_Panel>().load(entryID);
        }
    }
}
