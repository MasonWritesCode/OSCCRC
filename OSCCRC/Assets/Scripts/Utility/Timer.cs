using System.Collections;
using UnityEngine;

// This class is used to determine when a length of time has passed. It is used similarly to a C# Timing.Timer, throwing an event upon the time fully elapsing.
// This exists because we cannot reasonably use C# timing methods due to Unity's old .NET version and the restriction of Unity functions running on the main thread,
//   causing us to need to utilize a monobehavior, and we want to be able to have this functionality in classes that are not monobehaviors.
//
// Currently, each instance can only run a single timer at once. The timer does not repeat, it has to be invoked again to run again.
// Basic functionality is to use startTimer(float timeInSeconds) on an instance to have that instance start its timer.
//
// We could enhance this class with a shouldRepeat property for a true looping timer.

public class Timer {

    public delegate void voidEvent();
    public event voidEvent timerCompleted;
    public event voidEvent timerUpdate;


    // Whether the timer should use scaled time or not. Must be set before starting the timer. Defaults to true.
    public bool isScaledTime { get { return m_isScaledTime; } set { m_isScaledTime = value; } }

    // Whether the timer is currently running. Is false if the timer is unstarted, paused,or stopped.
    public bool isRunning { get { return m_timerRunning; } }

    // The remaining time on the timer. Returns 0.0 if the timer is not running.
    public float remainingTime { get { return getRemainingTime(); } }


    // Starts the timer with the specified duration
    public void startTimer(float durationInSeconds)
    {
        initializeAndStartTimer(durationInSeconds, -1.0f);
    }


    // Starts the timer with the specified duration, with an update callback called after each update delay
    public void startTimerWithUpdate(float durationInSeconds, float updateInSeconds)
    {
        initializeAndStartTimer(durationInSeconds, updateInSeconds);
    }


    // Pauses a started timer
    public void pauseTimer()
    {
        if (!m_timerRunning)
        {
            Debug.LogWarning("Attempted to pause a stopped timer");
            return;
        }

        m_timerObj.StopCoroutine(m_coroutineInstance);
        m_timerRunning = false;
        m_prevUsedTime = getCurrentTimeStamp() - m_coroutineStartTime;
    }


    // Resumes a paused timer
    public void resumeTimer()
    {
        m_timerRunning = true;

        m_coroutineStartTime = getCurrentTimeStamp();
        if (m_updateDuration > 0.0f)
        {
            m_coroutineInstance = m_timerObj.StartCoroutine(runUpdateTimer(getRemainingTime(), m_updateDuration));
        }
        else
        {
            m_coroutineInstance = m_timerObj.StartCoroutine(runTimer(getRemainingTime()));
        }
    }


    // Stops the timer if it is started
    public void stopTimer()
    {
        if (m_timerRunning)
        {
            m_timerObj.StopCoroutine(m_coroutineInstance);
            m_timerRunning = false;
        }
    }




    // We can't make Timer a monobehavior, because monobehaviors needs to be added to GameObjects.
    // So Timer creates a static reference to a GameObject with a monobehavior to run its coroutine, even though it is messy and annoying.
    // We want to create a dedicated GameObject so that we can guarantee the object will persist as long as the timer.

    private IEnumerator runTimer(float timeInSeconds)
    {
        if (timeInSeconds > 0.0f)
        {
            if (m_isScaledTime)
            {
                yield return new WaitForSeconds(timeInSeconds);
            }
            else
            {
                yield return new WaitForSecondsRealtime(timeInSeconds);
            }
        }

        m_timerRunning = false;
        if (timerCompleted != null)
        {
            timerCompleted();
        }
    }


    private IEnumerator runUpdateTimer(float timeInSeconds, float updateDelay)
    {
        float remainingTime = getRemainingTime();
        float prevRemainingTime = remainingTime;
        float delayOffset = 0.0f;

        while (remainingTime > updateDelay)
        {
            if (m_isScaledTime)
            {
                yield return new WaitForSeconds(Mathf.Max(updateDelay - delayOffset, 0.0f));
            }
            else
            {
                yield return new WaitForSecondsRealtime(Mathf.Max(updateDelay - delayOffset, 0.0f));
            }

            // We can't just pass our update delay to WaitForSeconds every time becayse WaitForSeconds will be rounded to a frame, causing drift.
            // We want to thus adjust our update delay with an offset to try to call our update as accurately as possible.
            // Our offset is the difference of the actual time spent from the desired time spent ((prevRemainingTime - remainingTime) - (updateDelay - delayOffset))
            remainingTime = getRemainingTime();
            delayOffset = prevRemainingTime - remainingTime - updateDelay + delayOffset;
            prevRemainingTime = remainingTime;

            if (timerUpdate != null)
            {
                timerUpdate();
            }
        }

        if (remainingTime > 0.0f)
        {
            if (m_isScaledTime)
            {
                yield return new WaitForSeconds(remainingTime);
            }
            else
            {
                yield return new WaitForSecondsRealtime(remainingTime);
            }
        }

        m_timerRunning = false;
        if (timerCompleted != null)
        {
            timerCompleted();
        }
    }


    private void initializeAndStartTimer(float timerDuration, float updateDuration)
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

        m_coroutineStartTime = getCurrentTimeStamp();
        m_prevUsedTime = 0.0f;
        m_targetDuration = timerDuration;
        m_updateDuration = updateDuration;

        m_timerRunning = true;

        if (updateDuration > 0.0f)
        {
            m_coroutineInstance = m_timerObj.StartCoroutine(runUpdateTimer(timerDuration, updateDuration));
        }
        else
        {
            m_coroutineInstance = m_timerObj.StartCoroutine(runTimer(timerDuration));
        }
    }


    private float getRemainingTime()
    {
        if (!m_timerRunning)
        {
            return 0.0f;
        }

        return m_targetDuration - (m_prevUsedTime + (getCurrentTimeStamp() - m_coroutineStartTime));
    }


    private float getCurrentTimeStamp()
    {
        return m_isScaledTime ? Time.time : Time.realtimeSinceStartup;
    }


    // We can't add a Monobehavior directly, so we create an empty wrapper
    private class TimerComponent : MonoBehaviour { }

    private static TimerComponent m_timerObj = null;

    private bool m_timerRunning = false;
    private bool m_isScaledTime = true;
    private float m_updateDuration;
    private float m_targetDuration = 0.0f;
    private float m_coroutineStartTime;
    private float m_prevUsedTime = 0.0f;
    private Coroutine m_coroutineInstance;
}
