using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual creature button in the spawn menu.
/// Displays creature info and handles tap events.
/// </summary>
public class CreatureButton : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private Button button;
    
    [Header("Rarity Colors (Optional)")]
    [SerializeField] private Color commonColor = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color uncommonColor = new Color(0.3f, 1f, 0.3f);
    [SerializeField] private Color rareColor = new Color(0.3f, 0.5f, 1f);
    [SerializeField] private Color epicColor = new Color(0.8f, 0.3f, 1f);
    [SerializeField] private Color legendaryColor = new Color(1f, 0.8f, 0.2f);
    
    private CreatureData data;
    private SpawnMenuUI menu;
    
    private void Awake()
    {
        // Get button if not assigned
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        
        // Setup button listener
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }
    
    /// <summary>
    /// Initialize button with creature data
    /// </summary>
    public void Initialize(CreatureData creatureData, SpawnMenuUI spawnMenu)
    {
        data = creatureData;
        menu = spawnMenu;
        
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (data == null) return;
        
        // Set icon
        if (iconImage != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }
        
        // Set name
        if (nameText != null)
        {
            nameText.text = data.displayName;
        }
        
        // Set description
        if (descriptionText != null)
        {
            descriptionText.text = data.description;
        }
        
        // Set rarity border color
        if (rarityBorder != null)
        {
            rarityBorder.color = GetRarityColor(data.rarity);
        }
    }
    
    private Color GetRarityColor(CreatureRarity rarity)
    {
        switch (rarity)
        {
            case CreatureRarity.Common:
                return commonColor;
            case CreatureRarity.Uncommon:
                return uncommonColor;
            case CreatureRarity.Rare:
                return rareColor;
            case CreatureRarity.Epic:
                return epicColor;
            case CreatureRarity.Legendary:
                return legendaryColor;
            default:
                return Color.white;
        }
    }
    
    private void OnButtonClicked()
    {
        if (menu != null && data != null)
        {
            menu.OnCreatureButtonClicked(data);
        }
    }
    
    /// <summary>
    /// Get the creature data this button represents
    /// </summary>
    public CreatureData GetCreatureData()
    {
        return data;
    }
}