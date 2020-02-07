using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is used to allow multiple different scripts to modify the time scale without interfering with each other.

public static class TimeManager
{
    // Sets the time scale to 0 and returns a PauseInstance used to remove the pause later.
    // restorationTimeScale is the desired restorationTimeScale property of the PauseInstance
    public static PauseInstance addTimePause(float restorationTimeScale = -1.0f)
    {
        if (desiredTimeScale < 0.0f)
        {
            desiredTimeScale = Time.timeScale;
        }

        Time.timeScale = 0;

        PauseInstance newInstance = new PauseInstance(restorationTimeScale);
        instances.Add(newInstance);

        return newInstance;
    }

    // Removes a pause instance, setting the desired timescale of the instance if it was the last one.
    public static void removeTimePause(PauseInstance instance)
    {
        Debug.Assert(instance != null);
        Debug.Assert(instances.Contains(instance));
        instances.Remove(instance);

        if (!(instance.restorationTimeScale < 0.0f))
        {
            desiredTimeScale = instance.restorationTimeScale;
        }

        if (instances.Count == 0)
        {
            Time.timeScale = desiredTimeScale;
            desiredTimeScale = -1.0f;
        }
    }

    private static List<PauseInstance> instances = new List<PauseInstance>();
    private static float desiredTimeScale = -1.0f;
}



// This struct stores an instance of a timescale pause.
// Different scripts may theoretically want to restore to different time scales. This allows us to keep track of which we want.

public class PauseInstance
{
    // The desired time scale when this pause is removed.
    //     A negative value means that the caller does not want to set restoration time scale and should use whatever was previously specified.
    public float restorationTimeScale { get; set; }

    public PauseInstance(float timeScale)
    {
        restorationTimeScale = timeScale;
    }
}
