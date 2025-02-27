using System;
using UnityEngine;

public class ItemBehavior : MonoBehaviour
{
    private Material _material;
    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private Quaternion _originalRotation;
    private GameObject _examineTarget;
    private GameObject _placementParent;
    private Vector3 _initialScale;
    private Vector2 _startTouch0, _startTouch1;
    private float _startDistance;
    private Vector2 _lastTouchPosition;
    private bool _isRotating = false;
    [SerializeField] private float _examineScaleOffset = 1f;
    [SerializeField][Range(0.0f, 1f)] private float _rotationSpeed = 0.2f;
    [SerializeField] private Color _emissionColor = new Color(1.94339621f, 0.504182994f, 0.504182994f, 1);

    private static GameObject _selectedObject;
    public static bool isSelected;
    public static bool isExamined;

    private void Start()
    {
        _placementParent = GameObject.Find("Placed GameObjects");
        if (_placementParent == null)
            Debug.LogError("Placement parent on ItemBehavior is null");
        else
            transform.parent = _placementParent.transform;

        _material = gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material;
        if (_material == null)
            Debug.LogError("*** Original Material is null on ItemBehavior on " + name);

        try
        {
            _examineTarget = GameObject.Find("Examine Point");
        }
        catch
        {
            Debug.LogError("*** Examine Target cannot be found on ItemBehavior on " + name);
        }

        _initialScale = transform.localScale;
    }

    private void OnEnable()
    {
        ExamineEvent.Examine += ExamineObject;
        SelectionExitButtonBehavior.ExitSelection += SetSelect;
    }

    private void OnDisable()
    {
        ExamineEvent.Examine -= ExamineObject;
        SelectionExitButtonBehavior.ExitSelection -= SetSelect;
    }

    private void Update()
    {
        if (Input.touchCount == 1 && !isSelected)
            SelectObject();

        if (isSelected)
        {
            HandleScalingAndRotation();
        }
    }

    private void SelectObject()
    {
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject obj = hit.collider.gameObject;
                if (obj.CompareTag("Placeable"))
                {
                    if (!isSelected)
                    {
                        _selectedObject = obj;
                        SetSelect(true);
                    }
                }
            }
        }
    }

    private void SetSelect(bool isSelected)
    {
        if (isSelected)
        {
            ItemBehavior.isSelected = isSelected;
            _material.EnableKeyword("_EMISSION");
            _material.SetFloat("_EmissionEnabled", 1.0f);
            if (_material.GetColor("_EmissionColor") != _emissionColor)
            {
                _material.SetColor("_EmissionColor", _emissionColor);
            }
            ExamineEvent.examineButton.gameObject.SetActive(isSelected);
            ExamineEvent.ChangeColor(isSelected);
            SelectionExitButtonBehavior.EnableButton();
            Debug.Log("*** Selecting");
        }
        else
        {
            _selectedObject = null;
            if (isExamined)
            {
                isExamined = isSelected;
                ExamineObject();
            }
            ExamineEvent.examineButton.gameObject.SetActive(isSelected);
            _material.SetColor("_EmissionColor", Color.black);
            ItemBehavior.isSelected = isSelected;
            _material.DisableKeyword("_EMISSION");
            Debug.Log("*** Deselecting ");
        }
    }

    public void ExamineObject()
    {
        if (!isExamined && isSelected)
        {
            try
            {
                _originalPosition = _selectedObject.transform.position;
                _originalRotation = _selectedObject.transform.rotation;
                _originalScale = _selectedObject.transform.localScale;
                _selectedObject.transform.parent = _examineTarget.transform;
                _selectedObject.transform.localPosition = Vector3.zero;
                _selectedObject.transform.localScale = _initialScale * _examineScaleOffset;
            }
            catch (Exception e)
            {
                Debug.LogError("***** " + e.Source + "\n" + e.Message + " \n" + e.StackTrace);
            }
            ExamineEvent.ChangeColor(false);
            isExamined = true;
            Debug.Log("*** Examining");
        }
        else
        {
            GameManager.ViewPlanes(true);
            _selectedObject.transform.position = _originalPosition;
            _selectedObject.transform.localScale = _originalScale;
            _selectedObject.transform.rotation = _originalRotation;
            _selectedObject.transform.parent = _placementParent.transform;
            GameManager.ViewPlanes(false);
            _originalPosition = Vector3.zero;
            _originalRotation = Quaternion.identity;
            _originalScale = Vector3.zero;
            isExamined = false;
            ExamineEvent.ChangeColor(true);
            Debug.Log("*** Unexamining");
        }
    }

    private void HandleScalingAndRotation()
    {
        Vector3 scaleNow = Vector3.one;

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                _lastTouchPosition = touch.position;
                _isRotating = true;
            }
            else if (touch.phase == TouchPhase.Moved && _isRotating)
            {
                Vector2 delta = touch.position - _lastTouchPosition;
                _selectedObject.transform.Rotate(Vector3.up, -delta.x * _rotationSpeed, Space.World);
                _lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                _isRotating = false;
            }
        }

        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                _startTouch0 = touch1.position;
                _startTouch1 = touch2.position;
                _startDistance = Vector2.Distance(_startTouch0, _startTouch1);
                scaleNow = _selectedObject.transform.localScale;
            }
            else
            {
                float newDistance = Vector2.Distance(touch1.position, touch2.position);
                float scaleFactor = newDistance / _startDistance;
                float minScale = 0.5f;
                float maxScale = 2.0f;
                Vector3 newScale = scaleNow * scaleFactor;
                newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
                newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
                newScale.z = Mathf.Clamp(newScale.z, minScale, maxScale);

                _selectedObject.transform.localScale = newScale;
            }
        }
    }
}
