using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [Range(0, 3)] public int playerID;
    public Transform highlighter;
    public MapTile currentTile { get { return m_currentTile; } }


    void Start()
    {
        m_gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        m_gameMap = GameObject.FindWithTag("Map").GetComponent<GameMap>();

        m_rng = new System.Random(playerID);
        m_cursorPos = Vector3.zero;

        // We want to immediately update the highlighter position
        selectTile();

        // We can add selection of different AI behaviors later
        StartCoroutine(AIBehavior_Dummy());
    }


    // Updates the current tile and positions the highlighter accordingly
    private void selectTile()
    {
        m_currentTile = m_gameMap.tileAt(m_cursorPos);

        // Move tile highlighter to the mouse position
        if (m_currentTile == null)
        {
            if (highlighter.gameObject.activeSelf)
            {
                highlighter.gameObject.SetActive(false);
            }
        }
        else
        {
            if (!highlighter.gameObject.activeSelf)
            {
                highlighter.gameObject.SetActive(true);
            }
            highlighter.position = m_currentTile.transform.position + Vector3.up * 2;
        }
    }


    // An AI behavior for testing. Just randomly places tiles around
    private IEnumerator AIBehavior_Dummy()
    {
        const float speed = 2.0f;
        bool active = true;

        while (active)
        {
            // Choose next tile
            MapTile chosenTile = m_gameMap.tileAt(m_rng.Next(m_gameMap.mapHeight), m_rng.Next(m_gameMap.mapWidth));
            Vector3 targetPos = chosenTile.transform.position;

            // Move towards the tile over time
            float prevTime = Time.time;
            while (m_cursorPos != targetPos)
            {
                m_cursorPos = Vector3.MoveTowards(m_cursorPos, targetPos, speed * (Time.time - prevTime) * Time.timeScale);
                prevTime = Time.time;
                selectTile();
                yield return null;
            }

            // Select the tile and make a placement
            selectTile();
            Directions.Direction direction = Directions.Direction.North;
            int directionRNG = m_rng.Next(4);
            switch (directionRNG)
            {
                case 1:
                    direction = Directions.Direction.East;
                    break;
                case 2:
                    direction = Directions.Direction.South;
                    break;
                case 3:
                    direction = Directions.Direction.West;
                    break;
            }
            m_gameController.requestPlacement(m_currentTile, MapTile.TileImprovement.Direction, direction, playerID);

            // We wait before starting again
            yield return new WaitForSeconds(3.0f);
        }
    }


    private GameController m_gameController;
    private GameMap m_gameMap;
    private MapTile m_currentTile = null;
    private Vector3 m_cursorPos;

    private System.Random m_rng;
}
