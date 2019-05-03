using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is used to determine when a length of time has passed. It is used similarly to a C# Timing.Timer, throwing an event upon the time fully elapsing.
// This is because we cannot reasonably use C# timing methods due to Unity's old .NET version and the restriction of Unity functions running on the main thread,
//   causing us to need to utilize a monobehavior, and we want to be able to have this functionality in classes that are not monobehaviors.
//
// Currently, each instance can only run a single timer at once and it cannot be canceled. The timer does not repeat, it has to be invoked again to run again.
// Basic functionality is to use startTimer(float timeInSeconds) on an instance to have that instance start its timer.

public class Timer {

    // We can't make Timer a monobehavior, because monobehaviors needs to be added to GameObjects.
    // So Timer is a wrapper around a sub-class that actually does the work. This is ugly, but is necessary afaik.

    public delegate void voidEvent();
    public event voidEvent timerCompleted;

    public Timer()
    {
        m_timerObj = new GameObject().AddComponent<TimerComponent>();
    }

    public void startTimer(float timeInSeconds)
    {
        m_timerObj.startTimer(timeInSeconds, setTimerComplete);
    }

    private void setTimerComplete()
    {
        if (timerCompleted != null)
        {
            timerCompleted();
        }
    }

    private TimerComponent m_timerObj = null;



    private class TimerComponent : MonoBehaviour
    {
        public void startTimer(float timeInSeconds, voidEvent completionCallback)
        {
            if (m_timerRunning)
            {
                Debug.LogWarning("Attempted to start a timer that was already running");
                return;
            }

            m_completionCallback = completionCallback;
            m_timerLengthSeconds = timeInSeconds;
            StartCoroutine(runTimer());
        }

        private IEnumerator runTimer()
        {
            m_timerRunning = true;
            yield return new WaitForSeconds(m_timerLengthSeconds);

            m_timerRunning = false;
            m_completionCallback();
        }

        private voidEvent m_completionCallback;
        private bool m_timerRunning = false;
        private float m_timerLengthSeconds = 0.0f;
    }
}
