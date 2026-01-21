using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
public class ARSpawner : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private CreatureDatabase database;
    
    [Header("Spawn Settings")]
    [Tooltip("Spawn on detected planes instead of marker position")]
    [SerializeField] private bool snapToPlanes = true;
    
    [Tooltip("Rotation applied to spawned models")]
    [SerializeField] private Vector3 spawnRotation = new Vector3(-90, 0, 0);
    
    // Tracking spawned instances
    private Dictionary<string, GameObject> activeCreatures = new Dictionary<string, GameObject>();
    
    // Events for other systems to listen to
    public System.Action<GameObject, CreatureData> OnCreatureSpawned;
    public System.Action<string> OnCreatureDespawned;
    
    private void Awake()
    {
        if (database != null)
        {
            database.Initialize();
        }
        
        ValidateComponents();
    }
    
    private void ValidateComponents()
    {
        if (raycastManager == null)
            Debug.LogWarning("ARRaycastManager not assigned - spawn position may be inaccurate");
        
        if (planeManager == null)
            Debug.LogWarning("ARPlaneManager not assigned - plane detection disabled");
        
        if (database == null)
            Debug.LogError("CreatureDatabase not assigned!");
    }
    
    public GameObject SpawnCreatureAtCenter(string creatureID)
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        return SpawnCreatureAtScreenPoint(creatureID, screenCenter);
    }

    public GameObject SpawnCreatureAtScreenPoint(string creatureID, Vector2 screenPoint)
    {
        Debug.Log($"🎯 SPAWN ATTEMPT: {creatureID} at screen position {screenPoint}");
        
        CreatureData data = database.GetCreature(creatureID);
        
        if (data == null)
        {
            Debug.LogError($"Creature not found: {creatureID}");
            return null;
        }
        
        if (data.prefab == null)
        {
            Debug.LogError($"Prefab missing for creature: {creatureID}");
            return null;
        }
        
        // Try to find a plane to spawn on
        Vector3 spawnPosition;
        
        if (snapToPlanes && raycastManager != null)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            
            if (raycastManager.Raycast(screenPoint, hits, TrackableType.PlaneWithinPolygon))
            {
                spawnPosition = hits[0].pose.position;
                Debug.Log($"✓ Spawning on detected plane");
            }
            else
            {
                spawnPosition = GetFallbackPosition(screenPoint);
                Debug.LogWarning("⚠ No plane found, using fallback position");
            }
        }
        else
        {
            spawnPosition = GetFallbackPosition(screenPoint);
        }
        
        // Instantiate the creature
        GameObject creature = Instantiate(data.prefab, spawnPosition, Quaternion.Euler(data.baseRotation));
        creature.transform.localScale = Vector3.one * data.defaultScale;
        creature.name = $"{data.displayName} (Instance)";
        
        // Store reference with unique ID
        string instanceID = $"{creatureID}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
        activeCreatures[instanceID] = creature;
        
        // Attach metadata component
        var metadata = creature.AddComponent<CreatureInstance>();
        metadata.Initialize(instanceID, data);
        
        // Add spawn animation
        var spawnAnim = creature.AddComponent<SpawnAnimation>();
        spawnAnim.PlaySpawnAnimation();
        
        // Notify listeners
        OnCreatureSpawned?.Invoke(creature, data);
        
        Debug.Log($"✓✓ SPAWNED: {data.displayName} at {spawnPosition}");
        
        return creature;
    }
    public GameObject SpawnCreatureAtMarker(string creatureID, Transform markerTransform)
    {
        CreatureData data = database.GetCreature(creatureID);
        
        if (data == null || data.prefab == null)
        {
            Debug.LogError($"Cannot spawn creature: {creatureID}");
            return null;
        }
        
        GameObject creature = Instantiate(data.prefab, markerTransform);
        creature.transform.localPosition = Vector3.zero;
        creature.transform.localRotation = Quaternion.Euler(data.baseRotation);
        creature.transform.localScale = Vector3.one * data.defaultScale;
        creature.name = $"{data.displayName} (Marker)";
        
        string instanceID = $"{creatureID}_marker";
        activeCreatures[instanceID] = creature;
        
        var metadata = creature.AddComponent<CreatureInstance>();
        metadata.Initialize(instanceID, data);
        
        OnCreatureSpawned?.Invoke(creature, data);
        
        return creature;
    }
    
    public void DespawnCreature(string instanceID)
    {
        if (activeCreatures.TryGetValue(instanceID, out GameObject creature))
        {
            activeCreatures.Remove(instanceID);
            Destroy(creature);
            OnCreatureDespawned?.Invoke(instanceID);
            Debug.Log($"Despawned: {instanceID}");
        }
    }

    public void DespawnAll()
    {
        foreach (var creature in activeCreatures.Values)
        {
            if (creature != null)
            {
                Destroy(creature);
            }
        }
        
        activeCreatures.Clear();
        Debug.Log("All creatures despawned");
    }

    public Dictionary<string, GameObject> GetActiveCreatures()
    {
        return activeCreatures;
    }
    private Vector3 GetFallbackPosition(Vector2 screenPoint)
    {
        if (Camera.main == null) return Vector3.zero;
        
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        return ray.GetPoint(1f); // 1 meter forward
    }
}

public class CreatureInstance : MonoBehaviour
{
    public string instanceID;
    public CreatureData data;
    
    public void Initialize(string id, CreatureData creatureData)
    {
        instanceID = id;
        data = creatureData;
    }
}