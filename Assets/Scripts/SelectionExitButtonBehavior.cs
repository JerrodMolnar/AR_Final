using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SelectionExitButtonBehavior : MonoBehaviour
{
    public delegate void SelectionExitButton(bool setSelection);
    public static event SelectionExitButton ExitSelection;

    public static Button exitSelectButton;

    void Start()
    {
        exitSelectButton = this.gameObject.GetComponent<Button>();
        if (exitSelectButton == null)
        {
            Debug.LogError("Exit selection Button on SelectionExitButtonBehavior is null on " + name);
        }
        this.gameObject.SetActive(false);
    }

    public void ButtonClick()
    {
        ExitSelection?.Invoke(false);
    }

    public static void EnableButton()
    {
        exitSelectButton.gameObject.SetActive(true);
    }
}
