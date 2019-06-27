using UnityEngine;
using UnityEngine.EventSystems;

public class Menu_ClearCompletions : MonoBehaviour, IPointerClickHandler
{

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        CompletionTracker.unmarkAll();
        Debug.Log("Completion Data cleared");
    }
}
