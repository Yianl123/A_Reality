using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Pokemon GO style spawning menu with scrollable creature list.
/// Players tap creatures to spawn them in AR.
/// </summary>
public class SpawnMenuUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CreatureDatabase database;
    [SerializeField] private ARSpawner spawner;
    
    [Header("UI References")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private GameObject creatureButtonPrefab;
    [SerializeField] private Button toggleMenuButton;
    [SerializeField] private TextMeshProUGUI toggleButtonText;
    
    [Header("UI Settings")]
    [SerializeField] private bool startOpen = false;
    [SerializeField] private string openText = "Close Menu";
    [SerializeField] private string closeText = "Open Spawn Menu";
    
    private List<CreatureButton> creatureButtons = new List<CreatureButton>();
    private bool isMenuOpen = false;
    
    private void Start()
    {
        if (database == null)
        {
            Debug.LogError("CreatureDatabase not assigned!");
            return;
        }
        
        database.Initialize();
        PopulateMenu();
        
        // Setup toggle button
        if (toggleMenuButton != null)
        {
            toggleMenuButton.onClick.AddListener(ToggleMenu);
        }
        
        // Set initial state
        if (startOpen)
        {
            OpenMenu();
        }
        else
        {
            CloseMenu();
        }
    }
    
    /// <summary>
    /// Populate menu with all available creatures
    /// </summary>
    private void PopulateMenu()
    {
        if (creatureButtonPrefab == null)
        {
            Debug.LogError("Creature button prefab not assigned!");
            return;
        }
        
        // Get all creatures from database
        List<CreatureData> creatures = database.GetAllCreatures();
        
        foreach (var creature in creatures)
        {
            if (creature == null) continue;
            
            // Instantiate button
            GameObject buttonObj = Instantiate(creatureButtonPrefab, contentContainer);
            CreatureButton creatureButton = buttonObj.GetComponent<CreatureButton>();
            
            if (creatureButton != null)
            {
                creatureButton.Initialize(creature, this);
                creatureButtons.Add(creatureButton);
            }
            else
            {
                Debug.LogWarning("CreatureButton component not found on prefab!");
            }
        }
        
        Debug.Log($"✓ Spawn menu populated with {creatureButtons.Count} creatures");
    }
    
    /// <summary>
    /// Called when player taps a creature button
    /// </summary>
    public void OnCreatureButtonClicked(CreatureData creature)
    {
        if (spawner == null)
        {
            Debug.LogError("ARSpawner not assigned!");
            return;
        }
        
        // Spawn creature at screen center
        GameObject spawned = spawner.SpawnCreatureAtCenter(creature.creatureID);
        
        if (spawned != null)
        {
            Debug.Log($"✓ Spawned from menu: {creature.displayName}");
            
            // Optional: Close menu after spawning
            // CloseMenu();
        }
    }
    
    /// <summary>
    /// Toggle menu open/closed
    /// </summary>
    public void ToggleMenu()
    {
        if (isMenuOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }
    
    public void OpenMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }
        
        if (toggleButtonText != null)
        {
            toggleButtonText.text = openText;
        }
        
        isMenuOpen = true;
    }
    
    public void CloseMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
        
        if (toggleButtonText != null)
        {
            toggleButtonText.text = closeText;
        }
        
        isMenuOpen = false;
    }
    
    /// <summary>
    /// Filter menu by rarity (for advanced UI)
    /// </summary>
    public void FilterByRarity(CreatureRarity rarity)
    {
        foreach (var button in creatureButtons)
        {
            if (button.GetCreatureData().rarity == rarity)
            {
                button.gameObject.SetActive(true);
            }
            else
            {
                button.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Show all creatures
    /// </summary>
    public void ShowAllCreatures()
    {
        foreach (var button in creatureButtons)
        {
            button.gameObject.SetActive(true);
        }
    }
}