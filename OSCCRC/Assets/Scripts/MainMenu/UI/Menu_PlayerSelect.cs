using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu_PlayerSelect : MonoBehaviour
{
    public int Player_Index; // Editor Set, 0 is first player

    void Start()
    {
        m_selector = GetComponent<Dropdown>();
        GlobalData.isHumanPlayer[Player_Index] = m_selector.value == 0;

        m_selector.onValueChanged.AddListener(delegate {
            OnDropdownValueChange();
        });
    }

    void OnDropdownValueChange()
    {
        GlobalData.isHumanPlayer[Player_Index] = m_selector.value == 0;
    }

    private Dropdown m_selector;
}
