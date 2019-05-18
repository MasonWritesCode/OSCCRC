using System.Collections.Generic;

// This class used for handling tracking and notification of the current state of the game.

public class GameState
{
    // The game can currently only have one main state. However, additional states can be applied on top of this.
    // The state enum shows valid main states, and the TagState enum shows additional states that can be tagged on top.
    /*
     * States are as follows:
     * State None             : Default value that can be used when no behavior is desired
     * State Unstarted        : The game level is getting ready to start, but not yet ready for user input
     * State Started_Paused   : The game level is started, but no movement occurs because we are waiting for user to finish their input
     * State Started_Unpaused : The game level is started and simulation is occuring
     * State Ended_Paused     : The game level is finished, but proceeding with new activities is on hold
     * State Ended_Unpaused   : The game level is finished
     * TagState Suspended     : The game is paused/suspended and activity should be halted until it is no longer suspended
     * TagState InputFocused  : A UI input field is set to recieve user input, and so game controls should avoid reacting to input
     */
    public enum State { None, Unstarted, Started_Paused, Started_Unpaused, Ended_Paused, Ended_Unpaused }
    public enum TagState { Suspended, InputFocused }

    public delegate void stateChangeEvent(State oldState, State newState);
    public delegate void stateListChangeEvent(TagState state);
    public event stateChangeEvent mainStateChange;
    public event stateListChangeEvent stateAdded;
    public event stateListChangeEvent stateRemoved;


    public GameState(State startState = State.None)
    {
        m_mainState = startState;

        m_additionalStates = new HashSet<TagState>();
    }


    // Sets or returns the main state of the GameState
    public State mainState
    {
        get
        {
            return m_mainState;
        }

        set
        {
            State oldState = m_mainState;
            m_mainState = value;
            if (mainStateChange != null)
            {
                mainStateChange(oldState, value);
            }
        }
    }


    // Returns whether or not the state is added to the GameState
    public bool hasState(TagState state)
    {
        return m_additionalStates.Contains(state);
    }


    // Adds a new state to the GameState
    public void addState(TagState state)
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
    public void removeState(TagState state)
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


    // Returns a list of the Tag States set at the time the getter was called
    public TagState[] tagStates
    {
        get
        {
            TagState[] stateList = new TagState[m_additionalStates.Count];
            m_additionalStates.CopyTo(stateList);
            return stateList;
        }
    }


    private State m_mainState;
    private HashSet<TagState> m_additionalStates;
}
