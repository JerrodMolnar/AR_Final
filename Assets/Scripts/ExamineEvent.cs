using UnityEngine;
using UnityEngine.UI;

public class ExamineEvent : MonoBehaviour
{

    public delegate void ExamineButton();
    public static event ExamineButton Examine;

    public static Button examineButton;

    private void Start()
    {
        examineButton = GetComponent<Button>();
        if (examineButton == null)
        {
            Debug.LogError("Examine Button on ExamineEvent is null on " + name);
        }
        else
        {
            examineButton.gameObject.SetActive(false);
        }
    }

    public void ButtonClick()
    {
        Debug.Log("*** Button Click Triggered!");
        if (Examine != null)
        {
            Examine();
            Debug.Log("*** Examine Event Triggered!");
        }
    }
}
