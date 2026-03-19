using UnityEngine;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "NPCData", menuName = "Game/NPC Data")]
public class CharacterData : ScriptableObject
{
    [SerializeField] public int _initialRelationshipScore = 10;
    [SerializeField] public int _maxRelationshipScore;
    [ShowInInspector, ReadOnly] private int _relationshipScore = 0;
    [SerializeField] private ExpressionFeedbackSprite _expressionFeedbackSprite;

    public List<ItemsSO> favoriteItems;
    public MapData favoritePlace; 
    public TraitsData traits;
    public List<int> _relationshipTresholds;

    public event Action<int> OnRelationshipChanged;

    public int RelationshipScore
    {
        get => _relationshipScore;
        set
        {
            _relationshipScore = value;
            OnRelationshipChanged?.Invoke(value);
        }
    }
    
    public ExpressionFeedbackSprite ExpressionFeedbackSprite => _expressionFeedbackSprite;

    public void OnEnable()
    {
        _relationshipScore = _initialRelationshipScore;
    }

    public bool LikesItem(ItemsSO item)
    {
        if (item == null) return false;

        Debug.Log($"[CharacterData] Checking if '{item.name}' is liked by character '{name}'");

        if (favoriteItems == null || favoriteItems.Count == 0)
            return false;

        if (favoriteItems.Contains(item))
            return true;

        // Fallback: compare by name in case different asset instances are used
        foreach (var fav in favoriteItems)
        {
            if (fav != null && fav.name == item.name)
                return true;
        }

        return false; 
    }
}

[Serializable]
public class TraitsData
{
    public bool isRebel;
    public bool isCorrect;
    public bool isReckless;
    public bool isBrave;
    public bool isFearful;
    public bool isCautious;
    public bool isLeader;
    public bool isFollower;
}

[Serializable]
public struct ExpressionFeedbackSprite
{
    [PreviewField] public Sprite neutralSprite;
    [PreviewField] public Sprite happySprite;
    [PreviewField] public Sprite sadSprite;
}
