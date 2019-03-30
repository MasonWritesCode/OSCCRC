﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// This class controls various parts of gameplay and manages the state of the game.

public class GameController : MonoBehaviour {

    public enum GameMode { None, Editor, Puzzle, Multiplayer };
    public GameMode mode { get { return m_mode; } }
    public GameState gameState { get { return m_gameState; } }

    // Checks if a player is allowed to place the desired improvement, and does so if they can
    public void requestPlacement(MapTile tile, MapTile.TileImprovement improvement = MapTile.TileImprovement.Direction, Directions.Direction dir = Directions.Direction.North)
    {
        if (m_gameState.hasState(GameState.TagState.Suspended))
        {
            return;
        }

        if (improvement == MapTile.TileImprovement.Direction)
        {
            if (tile.improvement == MapTile.TileImprovement.None || tile.improvement == MapTile.TileImprovement.Direction)
            {
                m_game.placeDirection(tile, dir);
            }
        }
    }

    // Begins a new game of the specified mode
    public void runGame(GameMode newMode)
    {
        if (m_game != null)
        {
            m_game.endGame();
        }

        if (newMode == GameMode.Puzzle)
        {
            m_game = new PuzzleGame(m_gameState);
        }
        else if (newMode == GameMode.Editor)
        {
            m_game = new EditorGame(m_gameState);
        }

        Editor editor = GetComponent<Editor>();
        if (editor)
        {
            if (newMode == GameMode.Editor && !editor.enabled)
            {
                editor.enabled = true;
            }
            else if (newMode != GameMode.Editor && editor.enabled)
            {
                editor.enabled = false;
            }
        }

        if (newMode != GameMode.None)
        {
            m_game.startGame();
        }

        m_mode = newMode;
    }


    void Start () {
        if (GlobalData.d_uncapFrames)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
        }

        m_eventSystem = EventSystem.current;
        m_wasInputFocused = false;

        m_gameState = new GameState();
        m_gameState.stateAdded += onTagStateAdd;
        m_gameState.stateRemoved += onTagStateRemove;

        GameStage stage = GetComponent<GameStage>();
        string currentStage = GlobalData.currentStageFile;
        if (currentStage != null)
        {
            stage.loadStage(currentStage);
        }
        else
        {
            stage.loadStage("Internal/default");
        }

        runGame(GlobalData.mode);
    }


    void Update()
    {
        // Unity does not have an event for a change in the currently selected game object, so we have to do a poll...
        bool isInputFocused = false;
        if (m_eventSystem.currentSelectedGameObject)
        {
            InputField field = m_eventSystem.currentSelectedGameObject.GetComponent<InputField>();
            isInputFocused = field != null && field.isFocused;
            
        }

        if (isInputFocused != m_wasInputFocused) // focus changed, so change input focus state
        {
            m_wasInputFocused = isInputFocused;

            if (isInputFocused && !m_gameState.hasState(GameState.TagState.InputFocused))
            {
                m_gameState.addState(GameState.TagState.InputFocused);
            }
            else if (!isInputFocused && m_gameState.hasState(GameState.TagState.InputFocused))
            {
                m_gameState.removeState(GameState.TagState.InputFocused);
            }
        }
    }


    void FixedUpdate()
    {
        // Here we manually simulate physics, so that we can avoid physics processing while paused (or possibly other scenarios)
        if (Physics.autoSimulation)
        {
            return;
        }
        // One scenario we could consider adding is to disable physics when there are no cats to collide with.
        if (m_gameState.mainState != GameState.State.Started_Unpaused || m_gameState.hasState(GameState.TagState.Suspended))
        {
            return;
        }

        Physics.Simulate(Time.fixedDeltaTime);
    }

    // We have to make sure we restore the time scale when the scene changes
    void OnDestroy()
    {
        Time.timeScale = m_timeScaleHolder;
    }

    // Set timescale to 0 for suspending the game
    // Animators wont animate, but will still use the same resources, so scripts attached to an animated object should consider disabling the animation on this event
    private void onTagStateAdd(GameState.TagState state)
    {
        if (state == GameState.TagState.Suspended)
        {
            m_timeScaleHolder = Time.timeScale;
            Time.timeScale = 0.0f;
        }
    }

    // Removes the actions of suspending the game
    // Scripts should avoid doing things while the game is suspended, so that their actions aren't undone by the unsuspend
    private void onTagStateRemove(GameState.TagState state)
    {
        if (state == GameState.TagState.Suspended)
        {
            Time.timeScale = m_timeScaleHolder;
        }
    }

    private GameMode m_mode = GameMode.None;
    private GameState m_gameState;
    private IGameMode m_game;
    private float m_timeScaleHolder = 1.0f;
    private EventSystem m_eventSystem;
    private bool m_wasInputFocused;
}
