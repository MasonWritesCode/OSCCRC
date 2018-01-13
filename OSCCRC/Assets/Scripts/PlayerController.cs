using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    // I heard Unity is going to be overhauling its input system soon. It would be nice to subscribe to event callbacks rather than poll every frame.

    public int playerID;

    private MapTile currentTile = null;
    private GameController gameController;

	void Start () {
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        // We will need to differentiate the inputs of players if we add multiplayer.
        // I don't know how that will work yet, but just assign a playerID of 1 to the player controls for now
        playerID = 1;
	}
	
	void Update () {
        // The mouse hovers over a tile to select it as the one where improvements will be placed
        Ray tileSelector = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitObject;
        if (Physics.Raycast(tileSelector, out hitObject, 1 << LayerMask.NameToLayer("Player Selectable")))
        {
            MapTile newTile = hitObject.transform.GetComponent<MapTile>();
            if (newTile != null && newTile != currentTile)
            {
                // Some form of highlighting should be applied to the current tile here to verify which tile is selected while the cursor is between tiles.
                currentTile = newTile;
            }
        }

        // Pretty sure the user can only place directional tiles in game
        if (Input.GetButtonDown("Up"))
        {
            gameController.requestPlacement(currentTile, MapTile.TileImprovement.Up);
        }
        else if (Input.GetButtonDown("Right"))
        {
            gameController.requestPlacement(currentTile, MapTile.TileImprovement.Right);
        }
        else if (Input.GetButtonDown("Down"))
        {
            gameController.requestPlacement(currentTile, MapTile.TileImprovement.Down);
        }
        else if (Input.GetButtonDown("Left"))
        {
            gameController.requestPlacement(currentTile, MapTile.TileImprovement.Left);
        }

    }
}
