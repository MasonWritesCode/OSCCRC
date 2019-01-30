using System.Collections.Generic;

// This class used for handling tracking and notification of the current state of the game.

public class GameState
{
    // The game can currently only have one main state. However, additional states can be applied on top of this.
    // The state enum shows valid main states, and the TaggableState enum shows additional states that can be tagged on top.
    public enum State { None, Unstarted, Started_Paused, Started_Unpaused, Ended }
    public enum TaggableState { Suspended }

    public delegate void stateChangeEvent(State oldState, State newState);
    public delegate void stateListChangeEvent(TaggableState state);
    public event stateChangeEvent mainStateChange;
    public event stateListChangeEvent stateAdded;
    public event stateListChangeEvent stateRemoved;

    public GameState(State startState = State.None)
    {
        m_mainState = startState;

        m_additionalStates = new HashSet<TaggableState>();
    }

    // Returns the current main state of the GameState
    public State getMainState()
    {
        return m_mainState;
    }

    // Sets the current main state of the GameState
    public void setMainState(State newState)
    {
        State oldState = m_mainState;
        m_mainState = newState;
        if (mainStateChange != null)
        {
            mainStateChange(oldState, newState);
        }
    }

    // Adds a new state to the GameState
    public void addState(TaggableState state)
    {
        if (!m_additionalStates.Contains(state))
        {
            m_additionalStates.Add(state);
            if (stateAdded != null)
            {
                stateAdded(state);
            }
        }
    }

    // Removes a state from the GameState
    public void removeState(TaggableState state)
    {
        if (m_additionalStates.Contains(state))
        {
            m_additionalStates.Remove(state);
            if (stateRemoved != null)
            {
                stateRemoved(state);
            }
        }
    }

    private State m_mainState;
    private HashSet<TaggableState> m_additionalStates;
}
