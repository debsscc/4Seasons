using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "NPCData", menuName = "Game/NPC Data")]
public class CharacterData : ScriptableObject
{
    [SerializeField] private int _initialRelationshipScore = 10;
    public List<ItemsSO> favoriteItems;

    public TraitsData traits;

    [ShowInInspector, ReadOnly] public int RelationshipScore { get; set; }

    private void OnEnable()
    {
        RelationshipScore = _initialRelationshipScore;
    }
}


[Serializable]
public class TraitsData
{
    public bool isRebel;
    public bool isBrave;
    public bool isCautious;
    public bool isLeader;
}

