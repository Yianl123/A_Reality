using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CreatureInfoPanel : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ARInteractionHandler interactionHandler;
    [SerializeField] private ARSpawner spawner;
    [SerializeField] private QuizManager quizManager;
    
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button quizButton;

    [Header("Settings")]
    [SerializeField] private bool hideWhenNoSelection = true;
    
    private GameObject currentCreature = null;
    private CreatureData currentCreatureData = null;
    private void StartQuiz()
    {
        if (currentCreatureData == null)
        {
            Debug.LogWarning("No creature data available for quiz!");
            return;
        }
        
        if (quizManager == null)
        {
            Debug.LogError("QuizManager not assigned to CreatureInfoPanel!");
            return;
        }
        
        quizManager.StartQuiz(currentCreatureData);
        
        Debug.Log($"✓ Starting quiz for {currentCreatureData.displayName}");
    }
    private void Start()
    {
        if (interactionHandler != null)
        {
            interactionHandler.OnCreatureSelected += OnCreatureSelected;
            interactionHandler.OnCreatureDeselected += OnCreatureDeselected;
        }
        
        if (spawner != null)
        {
            spawner.OnCreatureSpawned += OnCreatureSpawned;
        }
        
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(DeleteCurrentCreature);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }

        if (hideWhenNoSelection && panel != null)
        {
            panel.SetActive(false);
        }

        if (quizButton != null)
        {
            quizButton.onClick.AddListener(StartQuiz);
        }
        else
        {
            Debug.LogWarning("Quiz button is NULL in CreatureInfoPanel!");
        }
        }
    
    private void OnDestroy()
    {
        if (interactionHandler != null)
        {
            interactionHandler.OnCreatureSelected -= OnCreatureSelected;
            interactionHandler.OnCreatureDeselected -= OnCreatureDeselected;
        }
        
        if (spawner != null)
        {
            spawner.OnCreatureSpawned -= OnCreatureSpawned;
        }
    }
    
    private void OnCreatureSelected(GameObject creature)
    {
        currentCreature = creature;
        UpdatePanel(creature);
        
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }
    
    private void OnCreatureDeselected()
    {
        // Don't hide panel anymore - let user close it manually
        // currentCreature = null;
    }
    
    private void ClosePanel()
    {
        currentCreature = null;
        currentCreatureData = null;
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
    
    private void OnCreatureSpawned(GameObject creature, CreatureData data)
    {
        // Optionally show info when creature spawns
        // UpdatePanel(creature);
    }
    
    private void UpdatePanel(GameObject creature)
    {
        if (creature == null) return;
        
        // Get creature metadata
        CreatureInstance instance = creature.GetComponent<CreatureInstance>();
        
        if (instance == null || instance.data == null)
        {
            Debug.LogWarning("Creature has no metadata!");
            return;
        }
        
        CreatureData data = instance.data;
        currentCreatureData = data;
        
        // Update UI elements
        if (iconImage != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }
        
        if (nameText != null)
        {
            nameText.text = data.displayName;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = data.description;
        }

        if (quizButton != null)
        {
            bool hasQuestions = data.quizQuestions != null && data.quizQuestions.Count > 0;
            quizButton.gameObject.SetActive(hasQuestions);
            
            if (hasQuestions)
            {
                TextMeshProUGUI btnText = quizButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = $"Làm bài quiz ({data.quizQuestions.Count} câu)";
                }
            }
        }
    }

    private void DeleteCurrentCreature()
    {
        if (currentCreature == null) return;
        
        // Get instance ID
        CreatureInstance instance = currentCreature.GetComponent<CreatureInstance>();
        
        if (instance != null && spawner != null)
        {
            spawner.DespawnCreature(instance.instanceID);
            currentCreature = null;
            currentCreatureData = null;
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
        else
        {
            // Fallback: just destroy the GameObject
            Destroy(currentCreature);
            currentCreature = null;
            currentCreatureData = null;
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Manually show info for a specific creature
    /// </summary>
    public void ShowCreatureInfo(GameObject creature)
    {
        currentCreature = creature;
        UpdatePanel(creature);
        
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide the info panel
    /// </summary>
    public void Hide()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}