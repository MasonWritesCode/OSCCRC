﻿using UnityEngine;
using UnityEngine.UI;

public class FramerateDisplay : MonoBehaviour {

    public float updateInterval; // number of updates per second
    public int precision; // number of decimal places to use
    public bool isAdvanced = false; // Whether to include more information than just current framerate
    public Text textDisplay;

    private int m_updateNumber = 0;
    private int m_frameAccum = 0;
    private float m_timeAccum = 0.0f;
    private float m_timeRemaining = 0.0f;
    private float m_averageFramerate = 0.0f;
    private float m_minFramerate = -1.0f;

	void Start () {
		
	}
	
	void Update () {
        m_timeRemaining -= Time.unscaledDeltaTime;
        m_timeAccum += Time.unscaledDeltaTime;
        ++m_frameAccum;

        if (m_timeRemaining <= 0.0f)
        {
            if (Time.timeSinceLevelLoad < 3.0f)
            {
                // Ignore the first few seconds where framerate will be unstable
                m_frameAccum = 0;
                m_timeAccum = 0;
                return;
            }

            ++m_updateNumber;
            if (m_updateNumber < 0)
            {
                // overflow happened (is it reasonable this would ever happen? If so, make it a long), so reset the average framerate counter
                m_averageFramerate = 0.0f;
                m_updateNumber = 1;
            }
            m_timeRemaining += 1.0f / updateInterval;

            float currentFramerate = m_frameAccum / m_timeAccum;
            m_averageFramerate = (m_averageFramerate * ((m_updateNumber - 1) / (float)m_updateNumber)) + (currentFramerate / m_updateNumber);
            if (currentFramerate < m_minFramerate || m_minFramerate < 0)
            {
                m_minFramerate = currentFramerate;
            }

            if (!isAdvanced)
            {
                // Here we will just show the current framerate
                textDisplay.text = "FPS: " + currentFramerate.ToString("F" + precision);
            }
            else
            {
                // We want more info, so show the current framerate, the average framerate, and the minimum framerate, in that order
                textDisplay.text = "FPS: " + currentFramerate.ToString("F" + precision) + " / " + m_averageFramerate.ToString("F" + precision) + " / " + m_minFramerate.ToString("F" + precision);
            }
            // We will use current FPS for entire text color for now.
            // We will target 60 fps as green, 30 fps as yellow, and 0 as red, and try to set the color to have a smooth gradient between them
            textDisplay.color = new Color(Mathf.Max((60.0f-currentFramerate) / 60.0f, 0.0f), Mathf.Max(currentFramerate / 60.0f, 1.0f), 0.0f);

            m_frameAccum = 0;
            m_timeAccum = 0;
        }
    }
}
