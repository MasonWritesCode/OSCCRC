using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

        // We don't have a menu yet, so quit the game when escape is pressed just for now
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        // Toggle framerate display between Basic, Advanced, and Off
        // This should be a user configurable key binding, so move this to player controlls once player controls are implemented
        if (Input.GetKeyDown(KeyCode.F5)) // Is F5 generally reserved for something else?
        {
            FramerateDisplay fpsScript = GameObject.Find("GameController").GetComponent<FramerateDisplay>();
            Canvas fpsDisplay = GameObject.Find("FPSDisplay").GetComponent<Canvas>();

            if (!fpsScript.enabled)
            {
                fpsDisplay.enabled = true;
                fpsScript.enabled = true;
                fpsScript.isAdvanced = false;
            }
            else if (!fpsScript.isAdvanced)
            {
                fpsScript.isAdvanced = true;
            }
            else
            {
                fpsScript.enabled = false;
                fpsDisplay.enabled = false;
            }
        }
	}
}
