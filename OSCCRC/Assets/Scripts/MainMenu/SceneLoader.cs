using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (GlobalData.d_loadSceneAsync && currentLoadOp != null)
        {
            currentLoadOp.allowSceneActivation = true;
        }
        else
        {
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
    }

    // Begins asychronously loading a scene after a frame has passed
    // We want to wait a frame because calling LoadSceneAsync is actually extremely slow (like the entire time it would take to load a scene...)
    //   This makes sure the the Main Menu scene can appear quickly, as the stutter from the call wont matter as much in a loaded main menu
    //   However, don't be fooled by the "async" in the name; loadSceneAsync can cause stutter and input drops when it is called.
    private IEnumerator loadSceneAsyncDelayed(string sceneName)
    {
        yield return new WaitForFixedUpdate();
        currentLoadOp = SceneManager.LoadSceneAsync(sceneName);
        currentLoadOp.allowSceneActivation = false;
    }

    private AsyncOperation currentLoadOp = null;
}
