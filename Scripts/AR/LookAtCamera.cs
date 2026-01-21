using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lookChance = 0.02f;
    [SerializeField] private float turnSpeed = 2f;
    [SerializeField] private float lookDuration = 3f;
    
    private bool isSelected = false;
    private bool isLooking = false;
    private float lookTimer = 0f;
    
    private void Update()
    {
        if (Camera.main == null) return;
        
        if (isSelected)
        {
            LookAtCameraFunc();
            return;
        }
        
        if (!isLooking && Random.value < lookChance)
        {
            isLooking = true;
            lookTimer = 0f;
        }
        
        if (isLooking)
        {
            lookTimer += Time.deltaTime;
            LookAtCameraFunc();
            
            if (lookTimer > lookDuration)
            {
                isLooking = false;
            }
        }
    }
    
    private void LookAtCameraFunc()
    {
        Vector3 directionToCamera = Camera.main.transform.position - transform.position;
        directionToCamera.y = 0; // Chỉ xoay trục Y
        
        if (directionToCamera != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }
    }

    public void OnSelected()
    {
        isSelected = true;
        isLooking = true;
        Debug.Log($"[LookAtCamera] {gameObject.name} - SELECTED, bắt đầu nhìn");
    }

    public void OnDeselected()
    {
        isSelected = false;
        isLooking = false;
        Debug.Log($"[LookAtCamera] {gameObject.name} - DESELECTED, ngừng nhìn");
    }
}