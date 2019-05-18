using System.Collections;
using UnityEngine;

// This class is used to determine when a length of time has passed. It is used similarly to a C# Timing.Timer, throwing an event upon the time fully elapsing.
// This is because we cannot reasonably use C# timing methods due to Unity's old .NET version and the restriction of Unity functions running on the main thread,
//   causing us to need to utilize a monobehavior, and we want to be able to have this functionality in classes that are not monobehaviors.
//
// Currently, each instance can only run a single timer at once and it cannot be canceled. The timer does not repeat, it has to be invoked again to run again.
// Basic functionality is to use startTimer(float timeInSeconds) on an instance to have that instance start its timer.

public class Timer {

    // We can't make Timer a monobehavior, because monobehaviors needs to be added to GameObjects.
    // So Timer creates a static reference to a GameObject with a monobehavior to run its coroutine, even though it is messy and annoying.
    // We want to create a dedicated GameObject so that we can guarantee the object will persist as long as the timer

    public delegate void voidEvent();
    public event voidEvent timerCompleted;

    public void startTimer(float timeInSeconds)
    {
        if (m_timerRunning)
        {
            Debug.LogWarning("Attempted to start a timer that was already running");
            return;
        }

        m_timerObj.StartCoroutine(runTimer(timeInSeconds));
    }

    static Timer()
    {
        m_timerObj = new GameObject("Timer").AddComponent<TimerComponent>();
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
    private static TimerComponent m_timerObj = null;

    // We can't add a Monobehavior directly, so we create an empty wrapper
    private class TimerComponent : MonoBehaviour { }
}
