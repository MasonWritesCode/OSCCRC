using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class inherits from IDisposable, make sure to use a using statement or call Dispose

// This class is used to create pools of cloned GameObjects to be reused to prevent performance issues from frequent destruction and instantiation.
// It also is used to know how many objects of a specific group are still alive.
// Objects are destroyed when despawned if the number of allocated objects is above the desired minimum and large relative to active objects, or above desired maximum.
// There is no guarantee whether a spawned object will call it's Start or not when spawned, so be sure to handle any necessary (re)initialization after spawning.
//   However, if you have to do initialization beyond the arguments to Instantiate/spawn, it probably isn't an object that you should be pooling.

public class ObjectPool : System.IDisposable
{
    // The number of active items from the pool
    public int count { get { return m_activeObjects.Count; } }

    private int m_maxDesiredSize;           // # of objects at highest desired quantity to be allocated; 256 if unspecified
    private int m_minDesiredSize;           // # of allocated objects at minimum; 16 if unspecified
    private float m_percentToAlloc = 0.05f; // percent of allocated that are active when there are few enough active that memory should be deallocated
    private GameObject m_prefab;
    private List<GameObject> m_activeObjects;
    private List<GameObject> m_inactiveObjects;

    private bool m_disposed = false;

    // Creates an object pool using the specified prefab as the object with default bounds
    public ObjectPool(GameObject prefab)
        : this(prefab, 16, 256) {}

    public ObjectPool(GameObject prefab, int minDesiredSize, int maxDesiredSize)
        : this(prefab, minDesiredSize, minDesiredSize, maxDesiredSize) {}

    // Creates an object pool using the specified prefab as the object, allowing control over how much memory should be available in the pool
    public ObjectPool(GameObject prefab, int initialSize, int minDesiredSize, int maxDesiredSize)
    {
        if (minDesiredSize > maxDesiredSize)
        {
            throw new System.ArgumentException("Max desired size was smaller than min desired size. Try checking your argument order.");
        }

        m_prefab = prefab;
        m_maxDesiredSize = maxDesiredSize;
        m_minDesiredSize = minDesiredSize;

        m_activeObjects = new List<GameObject>(initialSize);
        m_inactiveObjects = new List<GameObject>(initialSize);
        for (int i = 0; i < initialSize; ++i)
        {
            m_inactiveObjects.Add(createObject());
        }
    }

    // Spawns a new instance of object for use out of this pool
    public GameObject spawn()
    {
        GameObject obj;

        if (m_inactiveObjects.Count > 0)
        {
            obj = m_inactiveObjects[m_inactiveObjects.Count-1];
            m_inactiveObjects.RemoveAt(m_inactiveObjects.Count-1);
        }
        else
        {
            obj = createObject();
        }

        obj.SetActive(true);
        m_activeObjects.Add(obj);

        return obj;
    }

    // Spawns a new instance of object for use out of this pool, with arguments you would give to instantiate
    public GameObject spawn(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject obj;

        if (m_inactiveObjects.Count > 0)
        {
            obj = m_inactiveObjects[m_inactiveObjects.Count-1];
            m_inactiveObjects.RemoveAt(m_inactiveObjects.Count-1);

            obj.transform.SetParent(parent);
            obj.transform.SetPositionAndRotation(position, rotation);
        }
        else
        {
            obj = createObject(position, rotation, parent);
        }

        obj.SetActive(true);
        m_activeObjects.Add(obj);

        return obj;
    }

    // Removes an instance of object from this pool from use
    public void despawn(GameObject obj)
    {
        m_activeObjects.Remove(obj);

        int allocatedObjects = m_activeObjects.Count + m_inactiveObjects.Count;
        // Destroy instead of disable if over max, or active is less than m_percentToAlloc of allocated while over min
        if (   (allocatedObjects > m_maxDesiredSize)
            || (allocatedObjects > m_minDesiredSize && (m_activeObjects.Count < allocatedObjects * m_percentToAlloc))
           )
        {
            destroyObject(obj);
        }
        else
        {
            obj.SetActive(false);
            m_inactiveObjects.Add(obj);
        }
    }

    // Instantiates a new inactive object, but does not place it in a pool
    private GameObject createObject()
    {
        GameObject obj = GameObject.Instantiate(m_prefab);
        obj.SetActive(false);
        return obj;
    }

    // Instantiates a new inactive object, passing parameters to instantiate, but does not place in a pool
    private GameObject createObject(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject obj = GameObject.Instantiate(m_prefab, position, rotation, parent);
        obj.SetActive(false);
        return obj;
    }

    // Destroys an Instantiated object
    private void destroyObject(GameObject obj)
    {
        GameObject.Destroy(obj);
    }

    public void Dispose()
    {
        Dispose(true);
        System.GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (m_disposed)
        {
            return;
        }

        if (m_activeObjects != null)
        {
            for (int i = 0; i < m_activeObjects.Count; ++i)
            {
                destroyObject(m_activeObjects[i]);
            }
            m_activeObjects = null;
        }
        if (m_inactiveObjects != null)
        {
            for (int i = 0; i < m_inactiveObjects.Count; ++i)
            {
                destroyObject(m_inactiveObjects[i]);
            }
            m_inactiveObjects = null;
        }

        m_prefab = null;

        m_disposed = true;
    }

    ~ObjectPool()
    {
        Dispose(false);
    }
}
