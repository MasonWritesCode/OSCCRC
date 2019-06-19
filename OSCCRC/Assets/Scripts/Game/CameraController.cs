using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public bool isAttached { get { return m_isAttached; } }


	void Awake () {
        m_camera = GetComponent<Camera>();
        m_res = new Vector2Int(Screen.width, Screen.height);
    }


    void Update()
    {
        if (m_res.x != Screen.width || m_res.y != Screen.height)
        {
            GameMap map = GameObject.FindWithTag("Map").GetComponent<GameMap>();
            setCameraView(map);
            m_res.x = Screen.width;
            m_res.y = Screen.height;
        }
    }


    // Sets the camera to be in overhead orthographic mode
    public void setCameraOrthographic()
    {
        m_camera.orthographic = true;
        m_camera.transform.SetParent(null);
        m_camera.nearClipPlane = 40.99f;
        m_isAttached = false;
    }


    // Sets the camera to follow the target transform (in perspective mode)
    // Note: make sure to detach from the object before it is destroyed to avoid destroying the camera
    public void setCameraFollow(Transform obj)
    {
        m_camera.orthographic = false;
        m_camera.transform.SetParent(obj, false);
        m_camera.transform.localPosition = new Vector3(0.0f, 1.0f, 0.0f);
        m_camera.transform.localRotation = Quaternion.identity;
        m_camera.nearClipPlane = 0.5f;
        m_isAttached = true;
    }


    // Sets the position of camera to be able to see the entire map in orthographic view
    // I think this currently assumes the map is located at world origin and isn't rotated
    public void setCameraView(GameMap map)
    {
        // Currently this is set up for an orthographic camera.
        // We want to center the camera within non-ui space. This space is currently the rightmost 80% of the screen.

        // Increasing adds more empty space around the map, effectively controlling how zoomed in it is.
        const float mapPaddingRatio = 0.05f;

        // We want to get the camera width and height necessary for fitting the map. Then we decide on an orthographic size that fits.
        float neededHeight = map.mapHeight * map.tileSize * (1.0f + mapPaddingRatio);
        float neededWidth = map.mapWidth * map.tileSize * (1.0f + mapPaddingRatio);
        float newOrthographicSize = Mathf.Max(neededHeight / 2.0f, neededWidth / (m_camera.aspect * 2.0f * 0.8f));
        m_camera.orthographicSize = newOrthographicSize;

        // Now we position the resized map. The width must also subtract half of the screen space devoted to UI
        // (I'm not sure why, but our UI offset calculation seems to be slightly off, so I am adding a -0.5f to it which seems to work)
        float UIOffset = (newOrthographicSize * m_camera.aspect * 0.2f) - 0.5f;
        Vector3 newPos = new Vector3(((map.mapWidth - 1) / 2) - UIOffset, 50.0f, (map.mapHeight - 1) / 2);
        Quaternion newRot = new Quaternion(0.7071068f, 0.0f, 0.0f, 0.7071068f); // 90 degrees on X avoiding euler angles to quaternion conversion overhead
        m_camera.transform.SetPositionAndRotation(newPos, newRot);
    }


    private Camera m_camera;
    private Vector2Int m_res;
    private bool m_isAttached = false;
}
