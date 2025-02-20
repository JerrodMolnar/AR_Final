using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class DropdownBehavior : MonoBehaviour
{
    private int _lastValue = Mathf.RoundToInt(Mathf.Infinity);
    private TMP_Dropdown _dropdown;
    [SerializeField] private ARPlacementInteractable _PlacementInteractor;
    [SerializeField] private GameObject[] _placementPrefabs;

    private void Start()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
        if (_dropdown == null)
            Debug.LogError("Cannot find Dropdown on DropdownBehavior Script on " + name);

        _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        if (_PlacementInteractor == null)
        {
            try
            {
                GameObject.Find("AR Placement Interactable").GetComponent<ARPlacementInteractable>();
            }
            catch
            {
                Debug.LogError("Cannot find Placement Interactor on DropdownBehavior Script on " + name);
            }
        }
        if (_placementPrefabs.Length != _dropdown.options.Count + 1)
        {
            Debug.LogError("Prefabs or options not enough on DropdownBehavior script on " + name);
        }
    }

    private void OnDropdownValueChanged(int index)
    {
        GameObject[] activePlaceables = GameObject.FindGameObjectsWithTag("Placeable");
        foreach (GameObject gameObject in activePlaceables)
        {
            if (_placementPrefabs[index + 1].name == gameObject.name)
            {
                ResetValue();
                return;
            }
        }


        _lastValue = index;
        _PlacementInteractor.placementPrefab = _placementPrefabs[index - 1];
        Debug.Log("*** " + _placementPrefabs[index].name + " selected");
        GameManager.ViewPlanes(true);

    }

    public void ResetValue()
    {
        _lastValue = Mathf.RoundToInt(Mathf.Infinity);
        _dropdown.value = 0;
        _dropdown.RefreshShownValue();
        Debug.Log("*** Last value reset");
    }
}
