using UnityEngine;
using Yarn.Unity;
using System.Collections.Generic;

public class YarnRelationshipController : MonoBehaviour
{
    [Header("Yarn References")]
    public DialogueRunner dialogueRunner;

    [Header("Score Rules")]
    public EventScoreRules eventScoreRules;

    [Header("Character Relationships")]
    public List<CharacterRelationshipBinding> characterBindings = new();

    private Dictionary<string, CharacterData> _idToCharacterData = new();
    private string _lastRule = string.Empty;

    void Awake()
    {
        _idToCharacterData.Clear();

        foreach (var binding in characterBindings)
        {
            if (binding == null) continue;
            if (string.IsNullOrEmpty(binding.characterId)) continue;
            if (binding.characterData == null) continue;

            _idToCharacterData[binding.characterId] = binding.characterData;
        }
    }

    void Update()
    {
        if (dialogueRunner == null) return;
        if (dialogueRunner.VariableStorage == null) return;

        string currentRule = GetYarnStringRobust("current_rule");

        if (string.IsNullOrEmpty(currentRule)) return;
        if (currentRule == _lastRule) return;

        _lastRule = currentRule;

        ApplyRelationshipChange(currentRule);
    }

    private void ApplyRelationshipChange(string ruleId)
    {
        if (eventScoreRules == null) return;

        var rule = eventScoreRules.GetRule(ruleId);
        if (rule == null) return;

        foreach (var charId in rule.gain)
        {
            if (_idToCharacterData.TryGetValue(charId, out var data))
            {
                data.RelationshipScore += rule.amount;
            }
        }

        foreach (var charId in rule.lose)
        {
            if (_idToCharacterData.TryGetValue(charId, out var data))
            {
                data.RelationshipScore -= rule.amount;
            }
        }
    }

    private string GetYarnStringRobust(string varName)
    {
        string key = varName.StartsWith("$") ? varName : "$" + varName;

        if (dialogueRunner.VariableStorage.TryGetValue<string>(key, out var str))
            return str ?? "";

        if (dialogueRunner.VariableStorage.TryGetValue<object>(key, out var obj) && obj != null)
            return obj.ToString();

        return "";
    }
}



