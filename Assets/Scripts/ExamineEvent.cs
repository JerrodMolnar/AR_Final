using UnityEngine;
using UnityEngine.UI;

public class ExamineEvent : MonoBehaviour
{

    public delegate void ExamineButton();
    public static event ExamineButton Examine;
    [SerializeField] private Color ExaminedColor;
    [SerializeField] private Color UnexaminedColor;

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

        if (ItemBehavior.isSelected && !ItemBehavior.isExamined)
        {
            ColorBlock cb = examineButton.colors;
            cb.normalColor = ExaminedColor;
            examineButton.colors = cb;
        }
        else if (ItemBehavior.isSelected && ItemBehavior.isExamined)
        {
            ColorBlock cb = examineButton.colors;
            cb.normalColor = UnexaminedColor;
            examineButton.colors = cb;
        }
    }
}
