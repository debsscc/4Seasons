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
            var key = b.characterId?.Trim();
            if (string.IsNullOrEmpty(key) || b.characterData == null) continue;
            if (!_idToCharacterData.ContainsKey(key))
                _idToCharacterData[key] = b.characterData;
            else
                Debug.LogWarning($"ScoreRulesManager: binding duplicate for '{key}'");
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
            var trimId = id?.Trim();
            if (string.IsNullOrEmpty(trimId)) continue;
            if (_idToCharacterData.TryGetValue(trimId, out var data) && data != null)
            {
                data.RelationshipScore += gainAmount;
                MiniGameFeedbackManager.Instance?.ShowHeart(trimId, true);
                MiniGameFeedbackManager.Instance?.UpdatePreviewTemp(trimId, 1);
            }
            else
            {
                Debug.LogWarning($"[ScoreRulesManager] gain: character '{trimId}' not bound");
            }
        }

        // Apply loses (use -amount)
        foreach (var id in rule.lose)
        {
            var trimId = id?.Trim();
            if (string.IsNullOrEmpty(trimId)) continue;
            if (_idToCharacterData.TryGetValue(trimId, out var data) && data != null)
            {
                data.RelationshipScore -= loseAmount;
                MiniGameFeedbackManager.Instance?.ShowHeart(trimId, false);
                MiniGameFeedbackManager.Instance?.UpdatePreviewTemp(trimId, -1);
            }
            else
            {
                Debug.LogWarning($"[ScoreRulesManager] lose: character '{trimId}' not bound");
            }
        }
    }
}