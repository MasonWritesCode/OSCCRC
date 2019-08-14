using System.Collections;
using UnityEngine;

// This class is used to determine when a length of time has passed. It is used similarly to a C# Timing.Timer, throwing an event upon the time fully elapsing.
// This is because we cannot reasonably use C# timing methods due to Unity's old .NET version and the restriction of Unity functions running on the main thread,
//   causing us to need to utilize a monobehavior, and we want to be able to have this functionality in classes that are not monobehaviors.
//
// Currently, each instance can only run a single timer at once and it cannot be canceled. The timer does not repeat, it has to be invoked again to run again.
// Basic functionality is to use startTimer(float timeInSeconds) on an instance to have that instance start its timer.
//
// We should enhance this class with pauseTimer/unpauseTimer functions and remainingTime, shouldRepeat properties
// Some of these may require the class to be changed in implementation

public class Timer {

    // We can't make Timer a monobehavior, because monobehaviors needs to be added to GameObjects.
    // So Timer creates a static reference to a GameObject with a monobehavior to run its coroutine, even though it is messy and annoying.
    // We want to create a dedicated GameObject so that we can guarantee the object will persist as long as the timer

    public delegate void voidEvent();
    public event voidEvent timerCompleted;
    public event voidEvent timerUpdate;


    // Whether the timer should use scaled time or not. Must be set before starting the timer. Defaults to true.
    public bool isScaledTime { get { return m_isScaledTime; } set { m_isScaledTime = value; } }

    // Whether the timer is currently running.
    public bool isRunning { get { return m_timerRunning; } }


    // Starts the timer with the specified duration
    public void startTimer(float durationInSeconds)
    {
        if (m_timerRunning)
        {
            Debug.LogWarning("Attempted to start a timer that was already running");
            return;
        }

        // The timer object can be destroyed on scene change (which we want so it stops the timer)
        // So we have to make sure a new one is created in case it does, so check each timer start
        if (m_timerObj == null)
        {
            m_timerObj = new GameObject("Timer").AddComponent<TimerComponent>();
        }

        coroutineInstance = m_timerObj.StartCoroutine(runTimer(durationInSeconds));
    }


    // Starts the timer with the specified duration, with an update callback called after each update delay
    public void startTimerWithUpdate(float durationInSeconds, float updateInSeconds)
    {
        if (m_timerRunning)
        {
            Debug.LogWarning("Attempted to start a timer that was already running");
            return;
        }

        // The timer object can be destroyed on scene change (which we want so it stops the timer)
        // So we have to make sure a new one is created in case it does, so check each timer start
        if (m_timerObj == null)
        {
            m_timerObj = new GameObject("Timer").AddComponent<TimerComponent>();
        }

        coroutineInstance = m_timerObj.StartCoroutine(runUpdateTimer(durationInSeconds, updateInSeconds));
    }


    // Stops the timer if it is started
    public void stopTimer()
    {
        if (m_timerRunning)
        {
            m_timerObj.StopCoroutine(coroutineInstance);
            m_timerRunning = false;
        }
    }


    private IEnumerator runTimer(float timeInSeconds)
    {
        m_timerRunning = true;

        if (m_isScaledTime)
        {
            yield return new WaitForSeconds(timeInSeconds);
        }
        else
        {
            yield return new WaitForSecondsRealtime(timeInSeconds);
        }

        m_timerRunning = false;
        if (timerCompleted != null)
        {
            timerCompleted();
        }
    }


    private IEnumerator runUpdateTimer(float timeInSeconds, float updateDelay)
    {
        float remainingTime = timeInSeconds;
        float delayOffset = 0.0f;
        float timeScale = m_isScaledTime ? Time.timeScale : 1.0f;
        float startTime = Time.time;
        m_timerRunning = true;

        while (remainingTime > updateDelay)
        {
            if (m_isScaledTime)
            {
                yield return new WaitForSeconds(updateDelay + delayOffset);
            }
            else
            {
                yield return new WaitForSecondsRealtime(updateDelay + delayOffset);
            }

            // We need to manually keep track of elapsed time to prevent drift from WaitForSeconds finishing at the end of frame instead of end of time
            // TODO: This probably will break if time scale is changed during the timer run
            float newRemainingTime = timeInSeconds - ((Time.time - startTime) * timeScale);
            delayOffset = newRemainingTime + updateDelay + delayOffset - remainingTime;
            remainingTime = newRemainingTime;

            if (timerUpdate != null)
            {
                timerUpdate();
            }
        }

        if (m_isScaledTime)
        {
            yield return new WaitForSeconds(remainingTime);
        }
        else
        {
            yield return new WaitForSecondsRealtime(remainingTime);
        }

        m_timerRunning = false;
        if (timerCompleted != null)
        {
            timerCompleted();
        }
    }


    private bool m_timerRunning = false;
    private bool m_isScaledTime = true;
    private Coroutine coroutineInstance;
    private static TimerComponent m_timerObj = null;

    // We can't add a Monobehavior directly, so we create an empty wrapper
    private class TimerComponent : MonoBehaviour { }
}
