using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Central registry of all creatures. Replaces JSON loading with ScriptableObject references.
/// Singleton pattern for easy access from anywhere.
/// </summary>
[CreateAssetMenu(fileName = "CreatureDatabase", menuName = "AR App/Creature Database", order = 0)]
public class CreatureDatabase : ScriptableObject
{
    [Header("All Available Creatures")]
    [SerializeField] private List<CreatureData> creatures = new List<CreatureData>();
    
    // Fast lookup cache (built at runtime)
    private Dictionary<string, CreatureData> creatureCache;
    
    /// <summary>
    /// Initialize the database - call this on app start
    /// </summary>
    public void Initialize()
    {
        creatureCache = new Dictionary<string, CreatureData>();
        
        foreach (var creature in creatures)
        {
            if (creature != null)
            {
                if (creatureCache.ContainsKey(creature.creatureID))
                {
                    Debug.LogWarning($"Duplicate creature ID: {creature.creatureID}");
                }
                else
                {
                    creatureCache[creature.creatureID] = creature;
                }
            }
        }
        
        Debug.Log($"✓ CreatureDatabase initialized with {creatureCache.Count} creatures");
    }
    
    /// <summary>
    /// Get creature by ID (fast dictionary lookup)
    /// </summary>
    public CreatureData GetCreature(string id)
    {
        if (creatureCache == null) Initialize();
        
        creatureCache.TryGetValue(id, out CreatureData creature);
        return creature;
    }
    
    /// <summary>
    /// Get creature by image target name (for marker-based AR)
    /// </summary>
    public CreatureData GetCreatureByImageTarget(string imageName)
    {
        if (creatureCache == null) Initialize();
        
        return creatures.FirstOrDefault(c => c.imageTargetName == imageName);
    }
    
    /// <summary>
    /// Get all creatures (for UI spawning menu)
    /// </summary>
    public List<CreatureData> GetAllCreatures()
    {
        return new List<CreatureData>(creatures);
    }
    
    /// <summary>
    /// Get unlocked creatures only (for progression systems)
    /// </summary>
    public List<CreatureData> GetUnlockedCreatures()
    {
        // TODO: Implement unlock logic based on player progress
        // For now, return all creatures with empty unlock conditions
        return creatures.Where(c => string.IsNullOrEmpty(c.unlockCondition)).ToList();
    }
    
    /// <summary>
    /// Get creatures by rarity (for filtering UI)
    /// </summary>
    public List<CreatureData> GetCreaturesByRarity(CreatureRarity rarity)
    {
        return creatures.Where(c => c.rarity == rarity).ToList();
    }
}