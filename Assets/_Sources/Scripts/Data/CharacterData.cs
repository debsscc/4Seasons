using UnityEngine;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "NPCData", menuName = "Game/NPC Data")]
public class CharacterData : ScriptableObject
{
    [SerializeField] public int _initialRelationshipScore = 10;
    [SerializeField] public int _maxRelationshipScore;
    [ShowInInspector, ReadOnly]
    [SerializeField] private int _relationshipScore;

    public List<ItemsSO> favoriteItems;
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

