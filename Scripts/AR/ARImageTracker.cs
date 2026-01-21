using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARImageTracker : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private ARSpawner spawner;
    [SerializeField] private CreatureDatabase database;
    
    [Header("Settings")]
    [SerializeField] private bool autoSpawnOnDetect = true;
    [SerializeField] private bool followMarker = true;
    
    private Dictionary<string, GameObject> markerCreatures = new Dictionary<string, GameObject>();
    
    private void Awake()
    {
        if (trackedImageManager == null)
        {
            trackedImageManager = GetComponent<ARTrackedImageManager>();
            if (trackedImageManager == null)
            {
                Debug.LogError("❌ AR Tracked Image Manager Not founded.");
            }
        }
        
        if (database != null)
        {
            database.Initialize();
        }
        
        Debug.Log("✓ ARImageTracker initialized");
    }
    
    private void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
            Debug.Log("✓ Subscribed to tracked images");
        }
    }
    
    private void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        }
    }
    
    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var trackedImage in args.added)
        {
            Debug.Log($"🎯 Phát hiện marker mới: {trackedImage.referenceImage.name}");
            HandleMarkerDetected(trackedImage);
        }

        foreach (var trackedImage in args.updated)
        {
            HandleMarkerUpdated(trackedImage);
        }
        
        foreach (var trackedImage in args.removed)
        {
            HandleMarkerLost(trackedImage);
        }
    }
    
    private void HandleMarkerDetected(ARTrackedImage trackedImage)
    {
        string markerName = trackedImage.referenceImage.name;
        
        if (markerCreatures.ContainsKey(markerName))
        {
            Debug.Log($"⚠️ Marker {markerName} đã có động vật rồi");
            return;
        }
        
        CreatureData data = database.GetCreatureByImageTarget(markerName);
        
        if (data == null)
        {
            Debug.LogWarning($"❌ Không tìm thấy động vật cho marker: {markerName}");
            return;
        }
        
        if (!autoSpawnOnDetect) return;
        
        GameObject creature = spawner.SpawnCreatureAtMarker(data.creatureID, trackedImage.transform);
        
        if (creature != null)
        {
            markerCreatures[markerName] = creature;
            Debug.Log($"✓✓ Spawn thành công {data.displayName} tại marker {markerName}");
        }
    }
    
    private void HandleMarkerUpdated(ARTrackedImage trackedImage)
    {
        string markerName = trackedImage.referenceImage.name;
        
        if (!markerCreatures.ContainsKey(markerName)) return;
        
        GameObject creature = markerCreatures[markerName];
        if (creature == null) return;
        
        switch (trackedImage.trackingState)
        {
            case TrackingState.Tracking:
                if (!creature.activeSelf)
                {
                    creature.SetActive(true);
                    Debug.Log($"✓ Hiện {markerName}");
                }
                break;
                
            case TrackingState.Limited:
                if (!creature.activeSelf)
                {
                    creature.SetActive(true);
                }
                break;
                
            case TrackingState.None:
                if (creature.activeSelf)
                {
                    creature.SetActive(false);
                    Debug.Log($"⚠️ Ẩn {markerName} (tracking lost)");
                }
                break;
        }
    }
    
    private void HandleMarkerLost(ARTrackedImage trackedImage)
    {
        string markerName = trackedImage.referenceImage.name;
        Debug.Log($"❌ Marker {markerName} bị removed");

        if (markerCreatures.ContainsKey(markerName))
        {
            GameObject creature = markerCreatures[markerName];
            if (creature != null)
            {
                creature.SetActive(false);
            }
        }
    }
    
    public void ClearAllMarkers()
    {
        foreach (var creature in markerCreatures.Values)
        {
            if (creature != null) Destroy(creature);
        }
        markerCreatures.Clear();
        Debug.Log("🗑️ Đã xóa tất cả markers");
    }
}