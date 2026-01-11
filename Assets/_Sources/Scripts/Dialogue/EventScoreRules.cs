using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EventScoreRule
{
    [Tooltip("ID único do evento/parte. Ex: 'Evento2.0_Parte1'")]
    public string ruleId;

    [Tooltip("Personagens que GANHAM pontos")]
    public List<string> gain = new List<string>();

    [Tooltip("Personagens que PERDEM pontos")]
    public List<string> lose = new List<string>();

    [Tooltip("Quantidade de pontos a ganhar/perder")]
    public int amount = 1;
}

[CreateAssetMenu(fileName = "EventScoreRules", menuName = "Game/Score/EventScoreRules")]
public class EventScoreRules : ScriptableObject
{
    public List<EventScoreRule> rules = new List<EventScoreRule>();

    public EventScoreRule GetRule(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return rules.Find(r => string.Equals(r.ruleId, id, StringComparison.OrdinalIgnoreCase));
    }
}