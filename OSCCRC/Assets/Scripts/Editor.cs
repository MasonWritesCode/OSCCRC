using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Editor : MonoBehaviour {

    // TODO: Make controllable via UI instead of arbitrary keys
    //       Add abilty to change map size (even if it never makes it to editor UI)
    //       Allow map saving and loading with input name (needs ui)

    private enum ObjectType { None, Wall, Improvement, MovingObject }

    private GameObject m_placeholderObject;
    private MapTile.TileImprovement m_selectedImprovement;
    private ObjectType m_placeholderType;
    private GridMovement.Directions m_direction;
    private GameMap m_gameMap;

	// Use this for initialization
	void Start () {
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();
        m_placeholderType = ObjectType.None;
	}
	
	// Update is called once per frame
	void Update () {
        // I'm not familiar with UI in Unity, so select what you want to place with buttons for now until UI gets set up.
        ObjectType newType = ObjectType.None;
        MapTile.TileImprovement newImprovement = MapTile.TileImprovement.None;

        // Keys to select which improvement
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            //
        }

        // If reselecting same object "put it away" instead so that no object is selected for placement
        if (m_placeholderType == newType && m_selectedImprovement == newImprovement)
        {
            Destroy(m_placeholderObject);
            m_placeholderType = ObjectType.None;
            m_selectedImprovement = MapTile.TileImprovement.None;
        }
        else
        {
            m_placeholderType = newType;
            m_selectedImprovement = newImprovement;
            // Instantiate placeholder and set its material
        }

        // Snap to selected tile, otherwise center on mouse

        // Place object.

        // Save map
        if (Input.GetKeyDown(KeyCode.F6))
        {
            m_gameMap.exportMap("dev");
        }
        else if (Input.GetKeyDown(KeyCode.F7))
        {
            m_gameMap.importMap("dev");
        }
    }
}
