using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// This class manages the loading of the game scene.

public class SceneLoader : MonoBehaviour {

	void Start () {
		if (GlobalData.d_loadSceneAsync)
        {
            StartCoroutine(loadSceneAsyncDelayed("Game"));
        }
	}

    // Loads the game scene
    public void loadGameScene()
    {
        if (GlobalData.d_loadSceneAsync && m_currentLoadOp != null)
        {
            m_currentLoadOp.allowSceneActivation = true;
        }
        else
        {
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
    }

    // Begins asychronously loading a scene after a frame has passed
    // We want to wait a frame in case calling LoadSceneAsync is slow
    //   This makes sure the the Main Menu scene can appear or change quickly, as the stutter from the call wont matter as much in a loaded main menu
    //   Previously, loadSceneAsync can cause stutter and input drops when it is called, but that doesn't seem to happen anymore.
    private IEnumerator loadSceneAsyncDelayed(string sceneName)
    {
        yield return new WaitForFixedUpdate();
        m_currentLoadOp = SceneManager.LoadSceneAsync(sceneName);
        m_currentLoadOp.allowSceneActivation = false;
    }

    private AsyncOperation m_currentLoadOp = null;
}
