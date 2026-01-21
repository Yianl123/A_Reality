using UnityEngine;

public class CreatureBehavior : MonoBehaviour
{
    [Header("Behavior Settings")]
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float stoppingDistance = 0.2f;
    [SerializeField] private float idleTime = 5f;
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private bool enableAI = true;
    
    private Animator animator;
    private CreatureBehaviorState currentState = CreatureBehaviorState.Idle;
    private float stateTimer = 0f;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float currentSpeed = 0f; // ✅ NEW - Current movement speed (for acceleration)
    
    // Animation hash IDs
    private int idleHash = Animator.StringToHash("Idle");
    private int walkHash = Animator.StringToHash("Walk");
    private int eatHash = Animator.StringToHash("Eat");
    private int sleepHash = Animator.StringToHash("Sleep");
    
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }
    
    private void Update()
    {
        if (!enableAI) return;
        
        // Handle movement
        if (isMoving)
        {
            MoveToTarget();
        }
        else
        {
            // Decelerate when stopped
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, acceleration * Time.deltaTime);
        }
        
        stateTimer += Time.deltaTime;
        
        switch (currentState)
        {
            case CreatureBehaviorState.Idle:
                UpdateIdleState();
                break;
            case CreatureBehaviorState.Walking:
                UpdateWalkingState();
                break;
            case CreatureBehaviorState.Eating:
                UpdateEatingState();
                break;
            case CreatureBehaviorState.Sleeping:
                UpdateSleepingState();
                break;
        }
    }
    
    private void MoveToTarget()
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0; // Keep on same height
        
        float distance = direction.magnitude;
        
        // ✅ Stop when close enough
        if (distance < stoppingDistance)
        {
            isMoving = false;
            currentSpeed = 0f;
            ChangeState(CreatureBehaviorState.Idle);
            Debug.Log("🐴 Reached destination");
            return;
        }
        
        // ✅ Smoothly accelerate
        currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed, acceleration * Time.deltaTime);
        
        // ✅ Slow down as we approach target
        float speedMultiplier = Mathf.Clamp01(distance / 1f); // Slow down in last 1 meter
        float finalSpeed = currentSpeed * speedMultiplier;
        
        // Rotate towards target
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Move forward
        transform.position += direction.normalized * finalSpeed * Time.deltaTime;
    }
    
    private void UpdateIdleState()
    {
        if (stateTimer > idleTime)
        {
            float rand = Random.value;
            
            if (rand < 0.5f)
            {
                StartWandering();
            }
            else if (rand < 0.75f)
            {
                StartEating();
            }
            else
            {
                StartSleeping();
            }
        }
    }
    
    private void UpdateWalkingState()
    {
        if (!isMoving)
        {
            ChangeState(CreatureBehaviorState.Idle);
        }
    }
    
    private void UpdateEatingState()
    {
        if (stateTimer > 3f)
        {
            ChangeState(CreatureBehaviorState.Idle);
        }
    }
    
    private void UpdateSleepingState()
    {
        if (stateTimer > 5f)
        {
            ChangeState(CreatureBehaviorState.Idle);
        }
    }
    
    // PUBLIC COMMANDS
    public void CommandWalkTo(Vector3 position)
    {
        targetPosition = position;
        targetPosition.y = transform.position.y; // Keep same height
        isMoving = true;
        currentSpeed = 0f; // Start from zero speed
        ChangeState(CreatureBehaviorState.Walking);
        Debug.Log($"🐴 Walking to {position}");
    }
    
    public void CommandEat()
    {
        StartEating();
    }
    
    public void CommandSleep()
    {
        StartSleeping();
    }
    
    public void CommandLookAtCamera()
    {
        LookAtCamera();
    }
    
    private void StartWandering()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection.y = 0;
        targetPosition = transform.position + randomDirection;
        isMoving = true;
        currentSpeed = 0f;
        ChangeState(CreatureBehaviorState.Walking);
    }
    
    private void StartEating()
    {
        isMoving = false;
        ChangeState(CreatureBehaviorState.Eating);
    }
    
    private void StartSleeping()
    {
        isMoving = false;
        ChangeState(CreatureBehaviorState.Sleeping);
    }
    
    private void LookAtCamera()
    {
        if (Camera.main == null) return;
        
        Vector3 directionToCamera = Camera.main.transform.position - transform.position;
        directionToCamera.y = 0;
        
        if (directionToCamera != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
        }
    }
    
    private void ChangeState(CreatureBehaviorState newState)
    {
        currentState = newState;
        stateTimer = 0f;
        
        if (animator != null)
        {
            animator.SetBool(idleHash, newState == CreatureBehaviorState.Idle);
            animator.SetBool(walkHash, newState == CreatureBehaviorState.Walking);
            animator.SetBool(eatHash, newState == CreatureBehaviorState.Eating);
            animator.SetBool(sleepHash, newState == CreatureBehaviorState.Sleeping);
        }
    }
    
    public void SetAIEnabled(bool enabled)
    {
        enableAI = enabled;
        if (!enabled)
        {
            isMoving = false;
            ChangeState(CreatureBehaviorState.Idle);
        }
    }
}

public enum CreatureBehaviorState
{
    Idle,
    Walking,
    Eating,
    Sleeping
}