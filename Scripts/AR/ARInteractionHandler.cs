using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ARInteractionHandler : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ARRaycastManager raycastManager;
    [Header("Interaction Settings")]
    [SerializeField] private bool enableInteraction = true;
    [SerializeField] private float minScale = 0.1f;
    [SerializeField] private float maxScale = 5f;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float scaleSpeed = 3f;
    [SerializeField] private float movementSmoothing = 0.5f;
    [SerializeField] private bool snapToPlanes = true;
    [SerializeField] private GameObject selectionRingPrefab;
    
    // Current selection
    private GameObject selectedCreature = null;
    private Vector3 dragOffset;
    private float initialPinchDistance = 0f;
    private Vector3 initialScale;

    private GameObject currentSelectionRing;
    
    // Events
    public System.Action<GameObject> OnCreatureSelected;
    public System.Action OnCreatureDeselected;
    
    private void Update()
    {
        if (!enableInteraction) return;
        
        #if UNITY_EDITOR
        HandleMouseInteraction();
        #else
        HandleTouchInteraction();
        #endif
    }
    private void HandleTouchInteraction()
    {
        if (Camera.main == null) return;
        
        // No touches = do nothing (keep creature selected)
        if (Input.touchCount == 0)
        {
            return;
        }
        
        // TWO FINGERS = Pinch to scale
        if (Input.touchCount == 2)
        {
            HandlePinchGesture();
            return;
        }
        
        // ONE FINGER = Select, Move, or Rotate
        if (Input.touchCount == 1)
        {
            HandleSingleTouchGesture();
        }
    }
    private void HandlePinchGesture()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);
        
        float currentDistance = Vector2.Distance(touch0.position, touch1.position);
        
        if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
        {
            // Start pinch
            initialPinchDistance = currentDistance;
            Vector2 centerPoint = (touch0.position + touch1.position) / 2f;
            TrySelectCreature(centerPoint);
            
            if (selectedCreature != null)
            {
                initialScale = selectedCreature.transform.localScale;
            }
        }
        else if (selectedCreature != null && (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved))
        {
            // Scale creature
            float scaleFactor = currentDistance / initialPinchDistance;
            Vector3 newScale = initialScale * scaleFactor;
            
            float scaleValue = newScale.x;
            if (scaleValue >= minScale && scaleValue <= maxScale)
            {
                selectedCreature.transform.localScale = newScale;
            }
        }
    }
    
    private void HighlightCreature(GameObject creature, bool highlight)
    {
        if (creature == null) return;
        Renderer[] renderers = creature.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            if (highlight)
            {
                foreach (Material mat in renderer.materials)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", Color.yellow * 0.2f);
                }
            }
            else
            {
                foreach (Material mat in renderer.materials)
                {
                    mat.DisableKeyword("_EMISSION");
                }
            }
        }
    }

    private void RemoveHighlight()
    {
        if (selectedCreature != null)
        {
            HighlightCreature(selectedCreature, false);
        }
    }

    private void HandleSingleTouchGesture()
    {
        Touch touch = Input.GetTouch(0);
        
        if (touch.phase == TouchPhase.Began)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                Debug.Log("Touch on UI - ignoring");
                return;
            }
            
            TrySelectCreature(touch.position);
        }
        else if (touch.phase == TouchPhase.Moved && selectedCreature != null)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }
            
            Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
            float distanceFromCenter = Vector2.Distance(touch.position, screenCenter);
            float screenRadius = Mathf.Min(Screen.width, Screen.height) / 2f;
            
            // Near edges (outer 40%) = ROTATE
            if (distanceFromCenter > screenRadius * 0.6f)
            {
                RotateCreature(touch.deltaPosition.x);
            }
            // Center area = MOVE
            else
            {
                MoveCreature(touch.position);
            }
        }
    }
    private void TrySelectCreature(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100f))
        {
            CreatureInstance creature = hit.collider.GetComponentInParent<CreatureInstance>();
            
            if (creature != null)
            {
                if (selectedCreature == creature.gameObject)
                {
                    Debug.Log(">>> Removed old selection");
                    
                    RemoveHighlight();
                    
                    LookAtCamera LookBehavior = selectedCreature.GetComponent<LookAtCamera>();
                    if (LookBehavior != null)
                    {
                        LookBehavior.OnDeselected();
                    }
                    
                    OnCreatureDeselected?.Invoke();
                    selectedCreature = null;
                    return;
                }
                
                if (selectedCreature != null)
                {
                    Debug.Log(">>> Removed old selection");
                    
                    RemoveHighlight();
                    
                    LookAtCamera oldLook = selectedCreature.GetComponent<LookAtCamera>();
                    if (oldLook != null)
                    {
                        oldLook.OnDeselected();
                    }
                }
                
                selectedCreature = creature.gameObject;
                
                Plane groundPlane = new Plane(Vector3.up, selectedCreature.transform.position);
                float distance;
                if (groundPlane.Raycast(ray, out distance))
                {
                    Vector3 hitPoint = ray.GetPoint(distance);
                    dragOffset = selectedCreature.transform.position - hitPoint;
                }
                
                initialScale = selectedCreature.transform.localScale;
                
                HighlightCreature(selectedCreature, true);

                LookAtCamera lookBehavior = selectedCreature.GetComponent<LookAtCamera>();
                if (lookBehavior != null)
                {
                    lookBehavior.OnSelected();
                }
                
                OnCreatureSelected?.Invoke(selectedCreature);
                Debug.Log($"✓ Selected: {creature.data.displayName}");
            }
        }
    }

    private void MoveCreature(Vector2 touchPosition)
    {
        // Try to snap to detected planes
        if (snapToPlanes && raycastManager != null)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            
            if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                Vector3 targetPosition = hits[0].pose.position;
                selectedCreature.transform.position = Vector3.Lerp(
                    selectedCreature.transform.position,
                    targetPosition,
                    movementSmoothing
                );
                return;
            }
        }
        
        // Fallback: move on virtual ground plane
        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        Plane groundPlane = new Plane(Vector3.up, selectedCreature.transform.position);
        float distance;
        
        if (groundPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 targetPosition = hitPoint + dragOffset;
            
            selectedCreature.transform.position = Vector3.Lerp(
                selectedCreature.transform.position,
                targetPosition,
                movementSmoothing
            );
        }
    }
    
    /// <summary>
    /// Rotate creature around Y axis
    /// </summary>
    private void RotateCreature(float deltaX)
    {
        float rotationAmount = deltaX * rotationSpeed * Time.deltaTime;
        selectedCreature.transform.Rotate(Vector3.up, rotationAmount, Space.World);
    }
    
    /// <summary>
    /// Editor mouse controls for testing
    /// </summary>
    private void HandleMouseInteraction()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            TrySelectCreature(Input.mousePosition);
        }
        
        if (Input.GetMouseButton(0) && selectedCreature != null)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                RotateCreature(Input.GetAxis("Mouse X") * 10f);
            }
            else
            {
                MoveCreature(Input.mousePosition);
            }
        }
        
        if (Input.GetMouseButtonUp(0) && selectedCreature != null)
        {
            OnCreatureDeselected?.Invoke();
        }
        
        // Mouse scroll to scale
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0 && selectedCreature != null)
        {
            Vector3 newScale = selectedCreature.transform.localScale * (1f + scroll * scaleSpeed);
            float scaleValue = newScale.x;
            
            if (scaleValue >= minScale && scaleValue <= maxScale)
            {
                selectedCreature.transform.localScale = newScale;
            }
        }
    }

    public void SetInteractionEnabled(bool enabled)
    {
        enableInteraction = enabled;
        
        if (!enabled && selectedCreature != null)
        {
            OnCreatureDeselected?.Invoke();
            selectedCreature = null;
        }
    }

    public GameObject GetSelectedCreature()
    {
        return selectedCreature;
    }
}