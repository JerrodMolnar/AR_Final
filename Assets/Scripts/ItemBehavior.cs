using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Touch = UnityEngine.Touch;

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
    [SerializeField] private float _examineScaleOffset = 1f;
    [SerializeField] private Color _emissionColor = new Color(1.94339621f, 0.504182994f, 0.504182994f, 1);
    private Vector2 startTouchPosition;
    private Vector2 currentTouchPosition;
    private bool isDragging = false;

    private static GameObject _selectedObject;
    public static bool isSelected;
    public static bool isExamined;

    private ARRaycastManager _raycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

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
        else if (!_material.IsKeywordEnabled("_EMISSION"))
            _material.EnableKeyword("_EMISSION");

        try
        {
            _examineTarget = GameObject.Find("Examine Point");
        }
        catch
        {
            Debug.LogError("*** Examine Target cannot be found on ItemBehavior on " + name);
        }

        try
        {
            _raycastManager = GameObject.FindFirstObjectByType<ARRaycastManager>();
        }
        catch
        {
            Debug.LogError("AR Raycast Manager is null on ItemBehavior on " + name);
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
            HandleMoveAndRotation();
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
                    if (obj != _selectedObject)
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
            if (_material.GetColor("_EmissionColor") != _emissionColor)
            {
                _material.SetColor("_EmissionColor", _emissionColor);
            }
            ExamineEvent.examineButton.gameObject.SetActive(isSelected);
            ExamineEvent.ChangeColor(isSelected);
            SelectionExitButtonBehavior.EnableButton();
            GameManager.ViewPlanes(isSelected);
            Debug.Log("*** Selecting");
        }
        else
        {
            if (isExamined)
            {
                isExamined = isSelected;
                ExamineObject();
            }
            ExamineEvent.examineButton.gameObject.SetActive(isSelected);
            GameManager.ViewPlanes(isSelected);
            _material.SetColor("_EmissionColor", Color.black);
            ItemBehavior.isSelected = isSelected;
            _selectedObject = null;
            Debug.Log("*** Deselecting ");
        }
    }

    public void ExamineObject()
    {
        if (!isExamined && isSelected && _selectedObject != null)
        {
            _originalPosition = _selectedObject.transform.position;
            _originalRotation = _selectedObject.transform.rotation;
            _originalScale = _selectedObject.transform.localScale;
            _selectedObject.transform.parent = _examineTarget.transform;
            _selectedObject.transform.localPosition = Vector3.zero;
            _selectedObject.transform.localScale = _initialScale * _examineScaleOffset;
            ExamineEvent.ChangeColor(false);
            isExamined = true;
            Debug.Log("*** Examining");
        }
        else
        {
            _selectedObject.transform.position = _originalPosition;
            _selectedObject.transform.localScale = _originalScale;
            _selectedObject.transform.rotation = _originalRotation;
            _selectedObject.transform.parent = _placementParent.transform;
            isExamined = false;
            ExamineEvent.ChangeColor(true);
            Debug.Log("*** Unexamining");
        }
    }

    private void HandleMoveAndRotation()
    {
        Vector3 scaleNow = Vector3.one;
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPosition = touch.position;
                    isDragging = _selectedObject != null;
                    break;

                case TouchPhase.Moved:
                    if (isDragging && _selectedObject != null)
                    {
                        currentTouchPosition = touch.position;
                        if (_raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                        {
                            Pose hitPose = hits[0].pose;
                            _selectedObject.transform.position = hitPose.position;
                        }
                    }
                    break;

                case TouchPhase.Ended:
                    isDragging = false;
                    break;
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
