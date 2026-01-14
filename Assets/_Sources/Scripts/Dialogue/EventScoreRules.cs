using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EventScoreRule
{
    public string ruleId;
    public List<string> gain = new();
    public List<string> lose = new();
    public int amount = 1;
}

[CreateAssetMenu(fileName = "EventScoreRules", menuName = "Game/Score/EventScoreRules")]
public class EventScoreRules : ScriptableObject
{
    public List<EventScoreRule> rules = new();

    public EventScoreRule GetRule(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return rules.Find(r => string.Equals(r.ruleId, id, StringComparison.Ordinal));
    }
}


