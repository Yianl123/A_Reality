using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class DataList
{
    public Datas[] datas;
}

[System.Serializable]
public class Datas
{
    public string imageName;
    public string modelName;
    public string title;
    public string description;
    public float scale = 1f;
}

public class ARTracker : MonoBehaviour
{
    [Header("AR Components")]
    public ARTrackedImageManager trackedImageManager;
    public ARRaycastManager raycastManager;
    
    [Header("UI (Optional)")]
    public TextMeshProUGUI infoText;
    
    [Header("Tracking Settings")]
    public float hideDelay = 2f;
    public bool followImage = false;
    public bool enableRaycastPlacement = true;
    public bool hideWhenLost = true;
    
    [Header("Interaction Settings")]
    public bool enableInteraction = true;
    public float minScale = 0.5f;
    public float maxScale = 3f;
    public float rotationSpeed = 100f;
    public float moveSpeed = 0.01f;
    
    private Dictionary<string, GameObject> spawnedModels = new Dictionary<string, GameObject>();
    private Dictionary<string, Datas> dataDatabase = new Dictionary<string, Datas>();
    private Dictionary<string, float> lastSeenTime = new Dictionary<string, float>();
    private Dictionary<string, Vector3> modelWorldPositions = new Dictionary<string, Vector3>();
    
    private GameObject selectedModel = null;
    private Vector3 dragOffset;
    
    void Awake()
    {
        LoadData();
        
        // TEMP: Test spawning without AR (remove after testing)
        Invoke("TestSpawnWithoutAR", 2f);
    }
    
    // TEMP DEBUG FUNCTION
    void TestSpawnWithoutAR()
    {
        Debug.Log("Testing spawn without AR...");
        
        // Try to spawn first model in database
        if (dataDatabase.Count > 0)
        {
            var firstData = dataDatabase.Values.GetEnumerator();
            firstData.MoveNext();
            Datas data = firstData.Current;
            
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/{data.modelName}");
            if (prefab != null)
            {
                GameObject model = Instantiate(prefab);
                model.transform.position = new Vector3(0, 0, 2); // 2m in front
                model.transform.rotation = Quaternion.identity;
                model.transform.localScale = Vector3.one * data.scale;
                
                // Add collider
                if (model.GetComponent<Collider>() == null)
                {
                    BoxCollider col = model.AddComponent<BoxCollider>();
                    Debug.Log($"Test model spawned with collider size: {col.size}");
                }
                
                spawnedModels["test"] = model;
                Debug.Log("Test model spawned! Try tapping it.");
            }
            else
            {
                Debug.LogError($"Could not load prefab: {data.modelName}");
            }
        }
    }
    
    void OnEnable() { }
    void OnDisable() { }

    void LoadData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Datas");
        
        if (jsonFile == null)
        {
            Debug.LogError("Datas.json not found in Resources folder!");
            return;
        }
        
        DataList dataList = JsonUtility.FromJson<DataList>(jsonFile.text);
        
        if (dataList == null || dataList.datas == null)
        {
            Debug.LogError("Failed to parse JSON or datas array is null!");
            return;
        }
        
        foreach (var data in dataList.datas)
        {
            dataDatabase[data.imageName] = data;
        }
    }
    
    void Update()
    {
        if (trackedImageManager == null)
        {
            return;
        }

        HashSet<string> currentlyTracking = new HashSet<string>();

        foreach (var trackedImage in trackedImageManager.trackables)
        {
            string imageName = trackedImage.referenceImage.name;
            currentlyTracking.Add(imageName);

            if (trackedImage.trackingState == TrackingState.Tracking || 
                trackedImage.trackingState == TrackingState.Limited)
            {
                lastSeenTime[imageName] = Time.time;
                
                if (!spawnedModels.ContainsKey(imageName))
                {
                    SpawnModel(trackedImage);
                }
                else if (!spawnedModels[imageName].activeSelf)
                {
                    spawnedModels[imageName].SetActive(true);
                    UpdateUI(imageName);
                }
                else
                {
                    if (followImage && modelWorldPositions.ContainsKey(imageName))
                    {
                        modelWorldPositions[imageName] = trackedImage.transform.position;
                        spawnedModels[imageName].transform.position = trackedImage.transform.position;
                    }
                    UpdateUI(imageName);
                }
            }
        }

        List<string> imagesToHide = new List<string>();

        foreach (var pair in spawnedModels)
        {
            string imageName = pair.Key;
            GameObject model = pair.Value;

            if (model.activeSelf && lastSeenTime.ContainsKey(imageName))
            {
                float timeSinceLastSeen = Time.time - lastSeenTime[imageName];
                
                if (!currentlyTracking.Contains(imageName) && timeSinceLastSeen > hideDelay)
                {
                    imagesToHide.Add(imageName);
                }
            }
        }

        foreach (string name in imagesToHide)
        {
            if (hideWhenLost)
            {
                spawnedModels[name].SetActive(false);
                Debug.Log($"Hiding model: {name} (not tracking for {hideDelay}s)");
            }
            // If hideWhenLost = false, model stays visible
            
            if (infoText != null)
            {
                infoText.text = "";
            }
        }
        
        if (enableInteraction)
        {
            HandleTouchInteraction();
        }
    }
    
    void SpawnModel(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        
        if (!dataDatabase.ContainsKey(imageName))
        {
            Debug.LogWarning($"No data found for image: {imageName}");
            return;
        }
        
        Datas data = dataDatabase[imageName];
        GameObject prefab = Resources.Load<GameObject>($"Prefabs/{data.modelName}");
        
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found: Resources/Prefabs/{data.modelName}");
            return;
        }
        
        GameObject model;
        Vector3 spawnPosition = trackedImage.transform.position;
        Quaternion spawnRotation = Quaternion.Euler(-90, 0, 0);
        
        if (enableRaycastPlacement && raycastManager != null)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
            
            // AR Foundation 6.x compatible raycast
            if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
            {
                if (hits.Count > 0)
                {
                    spawnPosition = hits[0].pose.position;
                    spawnRotation = Quaternion.Euler(-90, 0, 0);
                    Debug.Log($"Raycast hit plane at: {spawnPosition}");
                }
            }
            else
            {
                Debug.Log("Raycast didn't hit any planes, using image position");
            }
        }
        
        if (followImage)
        {
            model = Instantiate(prefab, trackedImage.transform);
            model.transform.localScale = Vector3.one * data.scale;
            model.transform.localRotation = spawnRotation;
        }
        else
        {
            model = Instantiate(prefab);
            model.transform.position = spawnPosition;
            model.transform.rotation = spawnRotation;
            model.transform.localScale = Vector3.one * data.scale;
            modelWorldPositions[imageName] = spawnPosition;
        }
        
        if (enableInteraction && model.GetComponent<Collider>() == null)
        {
            model.AddComponent<BoxCollider>();
        }
        
        spawnedModels[imageName] = model;
        UpdateUI(imageName);
        
        Debug.Log($"✓ Spawned: {data.title} at {spawnPosition}");
        Debug.Log($"✓ Model has collider: {model.GetComponent<Collider>() != null}");
        Debug.Log($"✓ Model active: {model.activeSelf}");
        Debug.Log($"✓ Model scale: {model.transform.localScale}");
    }
    
    void HandleTouchInteraction()
    {
        // Check if Camera.main exists
        if (Camera.main == null)
        {
            Debug.LogError("Camera.main is null! Make sure AR Camera has MainCamera tag!");
            return;
        }
        
        if (Input.touchCount == 0)
        {
            selectedModel = null;
            return;
        }
        
        // DEBUG: Check if raycasting works at all
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 100f))
            {
                Debug.Log($"HIT SOMETHING: {hit.collider.gameObject.name}");
            }
            else
            {
                Debug.Log("RAYCAST HIT NOTHING");
            }
        }
        
        // Pinch to zoom (two fingers)
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);
            
            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
            
            float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;
            float difference = currentMagnitude - prevMagnitude;
            
            Ray ray = Camera.main.ScreenPointToRay((touch0.position + touch1.position) / 2);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitObject = hit.collider.gameObject; // FIX 2: Declare hitObject here
                
                foreach (var pair in spawnedModels)
                {
                    if (pair.Value == hitObject || pair.Value.transform.IsChildOf(hitObject.transform) 
                        || hitObject.transform.IsChildOf(pair.Value.transform))
                    {
                        float scaleFactor = difference * 0.01f;
                        Vector3 newScale = pair.Value.transform.localScale + Vector3.one * scaleFactor;
                        float scaleValue = newScale.x;
                        
                        if (scaleValue >= minScale && scaleValue <= maxScale)
                        {
                            pair.Value.transform.localScale = newScale;
                        }
                        break;
                    }
                }
            }
        }
        // Single touch
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit))
                {
                    GameObject hitObject = hit.collider.gameObject; // FIX 2: Declare hitObject here too
                    
                    foreach (var pair in spawnedModels)
                    {
                        if (pair.Value == hitObject || pair.Value.transform.IsChildOf(hitObject.transform) 
                            || hitObject.transform.IsChildOf(pair.Value.transform))
                        {
                            selectedModel = pair.Value;
                            
                            Plane groundPlane = new Plane(Vector3.up, selectedModel.transform.position);
                            float distance;
                            if (groundPlane.Raycast(ray, out distance))
                            {
                                Vector3 hitPoint = ray.GetPoint(distance);
                                dragOffset = selectedModel.transform.position - hitPoint;
                            }
                            break;
                        }
                    }
                }
            }
            else if (touch.phase == TouchPhase.Moved && selectedModel != null)
            {
                Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
                float distanceFromCenter = Vector2.Distance(touch.position, screenCenter);
                float screenDiagonal = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
                
                if (distanceFromCenter > screenDiagonal * 0.35f)
                {
                    float rotationAmount = touch.deltaPosition.x * rotationSpeed * Time.deltaTime;
                    selectedModel.transform.Rotate(Vector3.up, rotationAmount, Space.World);
                }
                else
                {
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    Plane groundPlane = new Plane(Vector3.up, selectedModel.transform.position);
                    float distance;
                    
                    if (groundPlane.Raycast(ray, out distance))
                    {
                        Vector3 hitPoint = ray.GetPoint(distance);
                        selectedModel.transform.position = hitPoint + dragOffset;
                    }
                }
            }
        }
    }
    
    void UpdateUI(string imageName)
    {
        if (infoText != null && dataDatabase.ContainsKey(imageName))
        {
            Datas data = dataDatabase[imageName];
            infoText.text = $"<b>{data.title}</b>\n{data.description}";
        }
    }
}