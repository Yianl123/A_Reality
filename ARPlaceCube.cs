using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARPlaceCube : MonoBehaviour
{
    [SerializeField] private     ARRaycastManager raycastManager;
    bool isPlaced = false;

    // Update is called once per frame
    void Update()
    {
        if (!raycastManager)
            return;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began || Input.GetMouseButtonDown(0) && !isPlaced)
        {
            isPlaced = true;

            if (Input.touchCount > 0)
            {
                Place_Object(Input.GetTouch(0).position);
            }
            else
            {
                Place_Object(Input.mousePosition);
            }
        }
       
    }

    void Place_Object(Vector2 touchPosition)
    {
        var hits = new List<ARRaycastHit>();
        raycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.AllTypes);
        if (hits.Count > 0)
        {
            Vector3 hitpose = hits[0].pose.position;
            Quaternion hitrotation = hits[0].pose.rotation;
            Instantiate(raycastManager.raycastPrefab, hitpose, hitrotation);
        }
        StartCoroutine(ResetPlacement());
    }

    IEnumerator ResetPlacement()
    {
        yield return new WaitForSeconds(0.25f);
        isPlaced = false;
    }
}
