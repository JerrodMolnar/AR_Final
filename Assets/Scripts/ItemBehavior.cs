using System;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ItemBehavior : MonoBehaviour
{
    private Material _material;
    private Transform _originalTransform;
    private GameObject _selectedObject;
    public static bool isSelected;
    public static bool isExamined;
    private GameObject _examineTarget;
    private Vector3 _initialScale;
    private float _rotationThreshold = 10f;
    private Vector2 _startTouch0, _startTouch1;
    private float _startDistance;
    private bool _isRotating = false;
    [SerializeField] private float _examineScaleOffset = 1f;
    [SerializeField][Range(0.0f, 1f)] private float _rotationSpeed = 0.2f;
    [SerializeField] private Color _emissionColor = new Color(1.94339621f, 0.504182994f, 0.504182994f, 1);

    private void Start()
    {
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
        if (Input.touchCount < 2 && Input.touchCount > 0)
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
                        SelectionExitButtonBehavior.EnableButton();
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
            if (_material.GetColor("_EmissionColor") != _emissionColor)
            {
                _material.SetColor("_EmissionColor", _emissionColor);
            }
            ExamineEvent.examineButton.gameObject.SetActive(true);
            Debug.Log("*** Selecting");
        }
        else
        {
            _selectedObject = null;
            ItemBehavior.isSelected = false;
            _material.DisableKeyword("_EMISSION");
            isExamined = false;
            ExamineEvent.examineButton.gameObject.SetActive(false);
            Debug.Log("*** Deselecting " + ExamineEvent.examineButton.name);
        }
    }

    public void ExamineObject()
    {
        if (!isExamined && isSelected)
        {
            try
            {
                //Parent to target
                _originalTransform = _selectedObject.transform;
                _originalTransform.position = _selectedObject.transform.position;
                _originalTransform.rotation = _selectedObject.transform.localRotation;
                _originalTransform.localScale = _selectedObject.transform.localScale;
                _selectedObject.transform.parent = _examineTarget.transform;
                _selectedObject.transform.localPosition = Vector3.zero;
                _selectedObject.transform.localScale = _initialScale * _examineScaleOffset;
            }
            catch (NullReferenceException e)
            {
                if (_originalTransform == null)
                {
                    Debug.LogError("*** Original transform on ItemBehavior is null on " + name);
                }
                if (_selectedObject == null)
                {
                    Debug.LogError("*** Selected object on ItemBehavior is null on " + name);
                }
                Debug.LogError("***** " + e.Message + " \n" + e.StackTrace);
            }
            isExamined = true;
            Debug.Log("*** Examining");
        }
        else
        {
            _selectedObject.transform.position = _originalTransform.position;
            _selectedObject.transform.localScale = _originalTransform.localScale;
            _selectedObject.transform.rotation = _originalTransform.rotation;
            _selectedObject.transform.parent = null;
            _originalTransform = null;
            isExamined = false;
            Debug.Log("*** Unexamining");
        }
    }

    private void HandleScalingAndRotation()
    {
        Vector3 scaleNow = Vector3.one;
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
                _isRotating = false;
            }
            else
            {
                float newDistance = Vector2.Distance(touch1.position, touch2.position);
                float scaleFactor = newDistance / _startDistance;
                Vector3 newScale = scaleNow * scaleFactor;

                float minScale = 0.5f;
                float maxScale = 2.0f;

                // Clamp each axis of the scale
                newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
                newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
                newScale.z = Mathf.Clamp(newScale.z, minScale, maxScale);

                _selectedObject.transform.localScale = newScale;

                // Rotation: Check if fingers are twisting
                Vector2 currentDir = (touch2.position - touch1.position).normalized;
                Vector2 startDir = (_startTouch1 - _startTouch0).normalized;
                float angle = Vector2.SignedAngle(startDir, currentDir) * _rotationSpeed;

                if (Mathf.Abs(angle) > _rotationThreshold)  // Threshold to avoid unwanted rotation
                {
                    _selectedObject.transform.Rotate(Vector3.up, angle);
                    _isRotating = true;
                }
            }
        }
        else if (_isRotating)
        {
            _isRotating = false;
        }
    }
}
