using UnityEngine;
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
    private bool m_ignoreUpdates = true;

    private void OnEnable()
    {
        // We don't want to show an very old frame's info (especially when isAdvanced doesn't match)
        textDisplay.text = "FPS: ";
    }

    void Start () {
        textDisplay.text = "FPS: ";
	}
	
	void LateUpdate () {
        m_timeRemaining -= Time.unscaledDeltaTime;
        m_timeAccum += Time.unscaledDeltaTime;
        ++m_frameAccum;

        if (m_timeRemaining <= 0.0f)
        {
            // Ignore the first few seconds where framerate will be unstable
            if (m_ignoreUpdates)
            {
                if (Time.timeSinceLevelLoad > 3.0f)
                {
                    m_ignoreUpdates = false;
                }

                m_frameAccum = 0;
                m_timeAccum = 0;
                return;
            }

            ++m_updateNumber;
            if (m_updateNumber < 0)
            {
                // overflow happened (is it reasonable this would ever happen? If so, make it a long), so reset the average framerate counter
                Debug.LogWarning("Framerate counter had an overflow!");
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
            // We will target 60 fps as green, 30 fps as yellow, 0 as red, and 240 as white and try to set the color to have a smooth gradient between them
            float blue = Mathf.Clamp01( (currentFramerate - 60.0f) / (240.0f - 60.0f) );
            float green = Mathf.Clamp01( currentFramerate / 60.0f );
            float red = Mathf.Clamp01( ((60.0f - currentFramerate) / 60.0f) + blue );
            textDisplay.color = new Color(red, green, blue);

            m_frameAccum = 0;
            m_timeAccum = 0;
        }
    }
}
