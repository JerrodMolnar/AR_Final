using UnityEngine;
using UnityEngine.UI;

public class ExamineEvent : MonoBehaviour
{

    public delegate void ExamineButton();
    public static event ExamineButton Examine;
    public static Color _examinedColor = new Color(200, 200, 200, 128);
    public static Color _unexaminedColor = new Color(163, 255, 143, 255);

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
        if (Examine != null)
            Examine();
        Debug.Log("*** Examine Event Triggered!");

        
    }

    public static void ChangeColor(bool isExamined)
    {
        if (isExamined)
        {
            ColorBlock cb = examineButton.colors;
            cb.normalColor = _examinedColor;
            examineButton.colors = cb;
        }
        else
        {
            ColorBlock cb = examineButton.colors;
            cb.normalColor = _unexaminedColor;
            examineButton.colors = cb;
        }
    }
}
