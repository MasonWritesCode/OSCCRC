using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Menu_AudioSettings : MonoBehaviour
{
    public AudioMixer mixer;        // Editor Set

    public Slider masterVolume;     // Editor Set
    public Slider foregroundVolume; // Editor Set
    public Slider backgroundVolume; // Editor Set

    public Toggle masterMuted;      // Editor Set
    public Toggle foregroundMuted;  // Editor Set
    public Toggle backgroundMuted;  // Editor Set


    // Updates UI to currently applied settings
    public void read()
    {
        float curVolume;

        mixer.GetFloat("masterVolume", out curVolume);
        masterMuted.isOn = curVolume < -99.9f;
        masterVolume.value = Mathf.Pow(10, curVolume / 20.0f);

        mixer.GetFloat("foregroundVolume", out curVolume);
        foregroundMuted.isOn = curVolume < -99.9f;
        foregroundVolume.value = Mathf.Pow(10, curVolume / 20.0f);

        mixer.GetFloat("backgroundVolume", out curVolume);
        backgroundMuted.isOn = curVolume < -99.9f;
        backgroundVolume.value = Mathf.Pow(10, curVolume / 20.0f);
    }


    // Applies user input settings
    public void apply()
    {
        // Sound pressure level = 20 * log10(sound pressure)
        // Unity doesn't provide muting functionality, so we set below the mixer threshold volume (-80dB) which will suspend audio processing work

        if (masterMuted.isOn)
        {
            mixer.SetFloat("masterVolume", -100.0f);
        }
        else
        {
            mixer.SetFloat("masterVolume", Mathf.Log10(masterVolume.value) * 20.0f);
        }

        if (foregroundMuted.isOn)
        {
            mixer.SetFloat("foregroundVolume", -100.0f);
        }
        else
        {
            mixer.SetFloat("foregroundVolume", Mathf.Log10(foregroundVolume.value) * 20.0f);
        }

        if (backgroundMuted.isOn)
        {
            mixer.SetFloat("backgroundVolume", -100.0f);
        }
        else
        {
            mixer.SetFloat("backgroundVolume", Mathf.Log10(backgroundVolume.value) * 20.0f);
        }
    }


    // We need to make sure to update our settings display when re-enabled (in case of unapplied changes)
    void OnEnable()
    {
        read();
    }
}
