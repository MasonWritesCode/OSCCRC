using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu_GraphicsSettings : MonoBehaviour
{
    public Dropdown qualitySelect;  // Editor Set

    // Updates UI to currently applied settings
    public void read()
    {
        qualitySelect.ClearOptions();
        qualitySelect.AddOptions(new List<string>(QualitySettings.names));
        qualitySelect.value = QualitySettings.GetQualityLevel();
        qualitySelect.RefreshShownValue();
    }


    // Applies user input settings
    public void apply()
    {
        QualitySettings.SetQualityLevel(qualitySelect.value, true);
    }


    // We need to make sure to update our settings display when re-enabled (in case of unapplied changes)
    void OnEnable()
    {
        read();
    }
}
