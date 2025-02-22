using UnityEngine;

public class ExamineEvent : MonoBehaviour
{

    public delegate void ExamineButton();
    public static event ExamineButton Examine;

    public void ButtonClick()
    {
        if (Examine != null)
        {
            Examine();
            Debug.Log("*** Examine Event Triggered!");
        }        
    }
}
