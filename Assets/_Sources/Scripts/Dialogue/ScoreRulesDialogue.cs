using UnityEngine;
using Yarn.Unity;
using System;
using System.Collections.Generic;

public class ScoreRulesDialogue : MonoBehaviour
{
    [Header("Rules (ScriptableObject)")]
    public EventScoreRules rulesAsset;

    [Header("Character Bindings")]
    public List<CharacterRelationshipBinding> characterBindings = new();

    // Internals
    private Dictionary<string, CharacterData> _idToCharacterData = new(StringComparer.OrdinalIgnoreCase);

    void Awake()
    {
        BuildLookup();
    }

    void OnValidate()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        _idToCharacterData.Clear();
        if (characterBindings == null) return;
        foreach (var b in characterBindings)
        {
            if (b == null) continue;
            if (string.IsNullOrEmpty(b.characterId) || b.characterData == null) continue;
            if (!_idToCharacterData.ContainsKey(b.characterId))
                _idToCharacterData[b.characterId] = b.characterData;
            else
                Debug.LogWarning($"ScoreRulesManager: binding duplicate for '{b.characterId}'");
        }
    }

    public void ApplyRuleById(string ruleId)
    {
        if (string.IsNullOrEmpty(ruleId))
        {
            Debug.LogWarning("[ScoreRulesManager] ApplyRuleById called with null/empty id");
            return;
        }

        if (rulesAsset == null)
        {
            Debug.LogWarning("[ScoreRulesManager] No rules asset assigned");
            return;
        }

        var rule = rulesAsset.GetRule(ruleId);
        if (rule == null)
        {
            Debug.LogWarning($"[ScoreRulesManager] Rule not found: '{ruleId}'");
            return;
        }
        Debug.Log($"[ScoreRulesManager] Applying rule '{ruleId}'");
        ApplyRule(rule);
    }

    public void ApplyRule(EventScoreRule rule)
    {
        if (rule == null) return;

        int gainAmount = Math.Abs(rule.amount);
        int loseAmount = Math.Abs(rule.amount);

        // Apply gains (use +amount)
        foreach (var id in rule.gain)
        {
            if (string.IsNullOrEmpty(id)) continue;
            if (_idToCharacterData.TryGetValue(id, out var data) && data != null)
            {
                data.RelationshipScore += gainAmount;
                Debug.Log($"[ScoreRulesManager] +{gainAmount} -> {id} (now {data.RelationshipScore})");
            }
            else
            {
                Debug.LogWarning($"[ScoreRulesManager] gain: character '{id}' not bound");
            }
        }

        // Apply loses (use -amount)
        foreach (var id in rule.lose)
        {
            if (string.IsNullOrEmpty(id)) continue;
            if (_idToCharacterData.TryGetValue(id, out var data) && data != null)
            {
                data.RelationshipScore -= loseAmount;
                Debug.Log($"[ScoreRulesManager] -{loseAmount} -> {id} (now {data.RelationshipScore})");
            }
            else
            {
                Debug.LogWarning($"[ScoreRulesManager] lose: character '{id}' not bound");
            }
        }
    }
}