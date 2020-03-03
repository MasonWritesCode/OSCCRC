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
        m_selector.value = GlobalData.isHumanPlayer[Player_Index] ? 0 : 1;

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
