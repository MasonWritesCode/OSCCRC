using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu_DisplaySettings : MonoBehaviour
{
    public Dropdown resolutionSelect;  // Editor Set
    public InputField framerateSelect; // Editor Set
    public Toggle fullscreenSelect;    // Editor Set
    public Toggle vsyncSelect;         // Editor Set


    public void read()
    {
        Resolution currentRes = Screen.currentResolution;
        Resolution[] availableResolutions = Screen.resolutions;
        List<string> resolutionOptions = new List<string>(availableResolutions.Length);

        int currentResIndex = 0;
        for(int i = 0; i < availableResolutions.Length; ++i)
        {
            Resolution res = availableResolutions[i];
            resolutionOptions.Add(string.Format("{0}x{1} {2}Hz", res.width, res.height, res.refreshRate));

            if (res.width == currentRes.width && res.height == currentRes.height && res.refreshRate == currentRes.refreshRate)
            {
                currentResIndex = i;
            }
        }

        resolutionSelect.ClearOptions();
        resolutionSelect.AddOptions(resolutionOptions);
        resolutionSelect.value = currentResIndex;
        resolutionSelect.RefreshShownValue();

        if (Application.targetFrameRate > 0)
        {
            framerateSelect.text = Application.targetFrameRate.ToString();
        }
        else
        {
            framerateSelect.text = string.Empty;
        }

        fullscreenSelect.isOn = Screen.fullScreen;

        vsyncSelect.isOn = QualitySettings.vSyncCount > 0;
    }


    public void apply()
    {
        // It is probably safe to assume our dropdown and Screen.resolutions will not change and will match indecies
        Resolution res = Screen.resolutions[resolutionSelect.value];

        Screen.SetResolution(res.width, res.height, fullscreenSelect.isOn, res.refreshRate);

        int targetFramerate;
        if (framerateSelect.text.Length > 0 && int.TryParse(framerateSelect.text, out targetFramerate) && targetFramerate > 0)
        {
            Application.targetFrameRate = targetFramerate;
        }
        else
        {
            Application.targetFrameRate = -1;
        }

        QualitySettings.vSyncCount = vsyncSelect.isOn ? 1 : 0;
    }


    // We need to make sure to update our settings display when re-enabled (in case of unapplied changes)
    void OnEnable()
    {
        read();
    }
}
