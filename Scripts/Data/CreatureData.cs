using UnityEngine;
using System.Collections.Generic; // ← ADD THIS LINE!

/// <summary>
/// ScriptableObject that defines a spawnable AR creature/object.
/// Replaces JSON data with editor-friendly, performant data storage.
/// </summary>

[System.Serializable]
public class QuizQuestion
{
    public string questionText;
    public QuestionType type;
    public string correctAnswer;
    public List<string> options; // For multiple choice
    public string explanation; // Educational feedback after answer
}

public enum QuestionType
{
    TrueFalse,
    MultipleChoice,
    Numerical
}

[CreateAssetMenu(fileName = "New Creature", menuName = "AR App/Creature Data", order = 1)]
public class CreatureData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique identifier for this creature")]
    public string creatureID = "creature_001";
    
    [Tooltip("Display name shown in UI")]
    public string displayName = "Mysterious Creature";
    
    [Tooltip("Description shown in info panel")]
    [TextArea(3, 6)]
    public string description = "A fascinating creature waiting to be discovered...";
    
    [Header("Visuals")]
    [Tooltip("Icon shown in spawn menu")]
    public Sprite icon;
    
    [Tooltip("3D model prefab to spawn")]
    public GameObject prefab;
    
    [Header("AR Settings")]
    [Tooltip("Default scale when spawned")]
    [Range(0.001f, 10f)]
    public float defaultScale = 1f;
    
    [Tooltip("Base rotation (Euler angles) when spawned")]
    public Vector3 baseRotation = new Vector3(0, 0, 0);
    
    [Tooltip("Optional: Image target name for marker-based AR")]
    public string imageTargetName = "";
    
    [Header("Gameplay (Optional)")]
    [Tooltip("Rarity: Common, Rare, Epic, Legendary")]
    public CreatureRarity rarity = CreatureRarity.Common;
    
    [Tooltip("Unlock condition (leave empty if always available)")]
    public string unlockCondition = "";
    
    [Tooltip("Spawn cost (for currency systems)")]
    public int spawnCost = 0;

    [Header("STEM Education")]
    [Tooltip("Quiz questions about this animal")]
    public List<QuizQuestion> quizQuestions = new List<QuizQuestion>();

    [Tooltip("Fun facts for kids")]
    [TextArea(2, 4)]
    public List<string> funFacts = new List<string>();

    [Header("Scientific Facts")]
    [Tooltip("Scientific classification")]
    public string scientificName = "";
    
    [Tooltip("Where does this animal live?")]
    public string habitat = "";
    
    [Tooltip("What does this animal eat?")]
    public string diet = "";
    
    [Tooltip("Maximum speed in km/h")]
    public float maxSpeed = 0f;
    
    [Tooltip("Average lifespan in years")]
    public float lifespan = 0f;
}

public enum CreatureRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}