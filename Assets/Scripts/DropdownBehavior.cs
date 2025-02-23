using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class DropdownBehavior : MonoBehaviour
{
    private TMP_Dropdown _dropdown;
    [SerializeField] private ARPlacementInteractable _PlacementInteractor;
    [SerializeField] private GameObject[] _placementPrefabs;

    private void Start()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
        if (_dropdown == null)
            Debug.LogError("*** Cannot find Dropdown on DropdownBehavior Script on " + name);

        _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        if (_PlacementInteractor == null)
        {
            try
            {
                GameObject.Find("*** AR Placement Interactable").GetComponent<ARPlacementInteractable>();
            }
            catch
            {
                Debug.LogError("*** Cannot find Placement Interactor on DropdownBehavior Script on " + name);
            }
        }
        if (_placementPrefabs.Length != _dropdown.options.Count - 1)
        {
            Debug.LogError("*** Prefabs or options not enough on DropdownBehavior script on " + name);
        }
    }

    private void OnDropdownValueChanged(int index)
    {
        if (index == 0)
        {
            GameManager.ViewPlanes(false);
        }
        else
        {
            int indexForArray = index - 1;
            GameObject[] activePlaceables = GameObject.FindGameObjectsWithTag("Placeable");
            foreach (GameObject gameObject in activePlaceables)
            {
                if (gameObject.activeInHierarchy && _placementPrefabs[indexForArray].name + "(Clone)" == gameObject.name)
                {
                    ResetValue();
                    return;
                }
            }
            _PlacementInteractor.gameObject.SetActive(true);
            _PlacementInteractor.placementPrefab = _placementPrefabs[indexForArray];
            Debug.Log("*** " + _placementPrefabs[indexForArray].name + " selected");
            GameManager.ViewPlanes(true);
        }
    }

    public void ResetValue()
    {
        _dropdown.value = 0;
        _dropdown.RefreshShownValue();
        GameManager.ViewPlanes(false);
        Debug.Log("*** Last value reset");
    }
}
