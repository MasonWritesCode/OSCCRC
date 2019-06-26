using System.Collections;
using UnityEngine;

// This class is used to determine when a length of time has passed. It is used similarly to a C# Timing.Timer, throwing an event upon the time fully elapsing.
// This is because we cannot reasonably use C# timing methods due to Unity's old .NET version and the restriction of Unity functions running on the main thread,
//   causing us to need to utilize a monobehavior, and we want to be able to have this functionality in classes that are not monobehaviors.
//
// Currently, each instance can only run a single timer at once and it cannot be canceled. The timer does not repeat, it has to be invoked again to run again.
// Basic functionality is to use startTimer(float timeInSeconds) on an instance to have that instance start its timer.
//
// We should enhance this class with pauseTimer, getRemainingTime functions and shouldRepeat, isScaledTime, isRunning properties
// Some of these may require the class to be changed in implementation

public class Timer {

    // We can't make Timer a monobehavior, because monobehaviors needs to be added to GameObjects.
    // So Timer creates a static reference to a GameObject with a monobehavior to run its coroutine, even though it is messy and annoying.
    // We want to create a dedicated GameObject so that we can guarantee the object will persist as long as the timer

    public delegate void voidEvent();
    public event voidEvent timerCompleted;


    // Starts the timer with the specified length
    public void startTimer(float timeInSeconds)
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

        coroutineInstance = m_timerObj.StartCoroutine(runTimer(timeInSeconds));
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
        yield return new WaitForSeconds(timeInSeconds);

        m_timerRunning = false;
        if (timerCompleted != null)
        {
            timerCompleted();
        }
    }


    private bool m_timerRunning = false;
    private Coroutine coroutineInstance;
    private static TimerComponent m_timerObj = null;

    // We can't add a Monobehavior directly, so we create an empty wrapper
    private class TimerComponent : MonoBehaviour { }
}
