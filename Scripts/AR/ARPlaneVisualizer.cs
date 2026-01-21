using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Manages AR plane detection and visualization.
/// Provides plane info to other systems without coupling logic.
/// </summary>
public class ARPlaneVisualizer : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Drag the XR Origin (with ARPlaneManager) here")]
    [SerializeField] private ARPlaneManager planeManager; 

    [Header("Visualization Settings")]
    [SerializeField] private bool showPlanes = true;
    [SerializeField] private Material planeMaterial;
    
    [Header("Detection Settings")]
    [SerializeField] private bool detectHorizontalPlanes = true;
    [SerializeField] private bool detectVerticalPlanes = false;
    
    private List<ARPlane> detectedPlanes = new List<ARPlane>();
    
    // Events
    public System.Action<ARPlane> OnPlaneAdded;
    public System.Action<ARPlane> OnPlaneUpdated;
    public System.Action<ARPlane> OnPlaneRemoved;
    
    private void Awake()
    {
        if (planeManager == null)
        {
            planeManager = GetComponent<ARPlaneManager>();
        }

        if (planeManager != null)
        {
            planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
            
            planeManager.enabled = true;
        }
    }
    private void OnEnable()
    {
        if (planeManager != null)
            planeManager.trackablesChanged.AddListener(OnPlanesChanged);
    }
    
    private void OnDisable()
    {
        if (planeManager != null)
            planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
    }
    
    private void ConfigurePlaneDetection()
    {
        PlaneDetectionMode mode = PlaneDetectionMode.None;
        
        if (detectHorizontalPlanes)
            mode |= PlaneDetectionMode.Horizontal;
        
        if (detectVerticalPlanes)
            mode |= PlaneDetectionMode.Vertical;
        
        planeManager.requestedDetectionMode = mode;
        Debug.Log($"Plane detection mode: {mode}");
    }
    
    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        foreach (var plane in args.added)
        {
            detectedPlanes.Add(plane);
            ApplyVisualization(plane);
            OnPlaneAdded?.Invoke(plane);
        }
        
        foreach (var plane in args.updated)
        {
            ApplyVisualization(plane);
            OnPlaneUpdated?.Invoke(plane);
        }
        
        foreach (var kvp in args.removed)
        {
            detectedPlanes.Remove(kvp.Value);
            OnPlaneRemoved?.Invoke(kvp.Value);
        }
    }
    
    private void ApplyVisualization(ARPlane plane)
    {
        MeshRenderer renderer = plane.GetComponent<MeshRenderer>();
        LineRenderer lineRenderer = plane.GetComponent<LineRenderer>();
        
        if (renderer != null)
        {
            renderer.enabled = showPlanes;
            if (showPlanes && planeMaterial != null)
            {
                renderer.material = planeMaterial;
            }
        }
        
        if (lineRenderer != null)
        {
            lineRenderer.enabled = showPlanes;
        }
    }
    
    public void SetPlaneVisualization(bool visible)
    {
        showPlanes = visible;
        foreach (var plane in detectedPlanes)
        {
            if (plane != null) ApplyVisualization(plane);
        }
    }

    public List<ARPlane> GetDetectedPlanes() => new List<ARPlane>(detectedPlanes);
    
    public ARPlane GetLargestHorizontalPlane()
    {
        ARPlane largest = null;
        float largestArea = 0f;
        foreach (var plane in detectedPlanes)
        {
            if (plane.alignment == PlaneAlignment.HorizontalUp || plane.alignment == PlaneAlignment.HorizontalDown)
            {
                float area = plane.size.x * plane.size.y;
                if (area > largestArea)
                {
                    largestArea = area;
                    largest = plane;
                }
            }
        }
        return largest;
    }
    
    public void ClearAllPlanes()
    {
        if (planeManager == null) return;
        planeManager.enabled = false;
        planeManager.enabled = true;
        detectedPlanes.Clear();
    }
}