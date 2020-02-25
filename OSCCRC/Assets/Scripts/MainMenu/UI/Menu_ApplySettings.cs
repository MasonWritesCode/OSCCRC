using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Menu_ApplySettings : MonoBehaviour, IPointerClickHandler
{
    public Menu_AudioSettings audioSettings; // Editor Set
    public Menu_DisplaySettings displaySettings; // Editor Set

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            // We apply the settings for whichever group is active
            if (audioSettings.isActiveAndEnabled)
            {
                audioSettings.apply();
            }
            if (displaySettings.isActiveAndEnabled)
            {
                displaySettings.apply();
            }
        }
    }
}
