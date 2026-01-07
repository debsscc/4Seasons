using UnityEngine;
using Yarn.Unity;
using System; // <-- Adicione esta linha

public class YarnScoreCommands : MonoBehaviour
{
    public ScoreRulesDialogue scoreManager;

    // Registra comando Yarn: <<command ApplyEventPart "Evento2.0_Parte1">>
    [YarnCommand("ApplyEventPart")]
    public void ApplyEventPart(string ruleId)
    {
        if (scoreManager == null)
        {
            Debug.LogWarning("[YarnScoreCommands] scoreManager not assigned");
            return;
        }

        scoreManager.ApplyRuleById(ruleId);
    }

    // Alternativa: mais flexível, permite passar CSV e delta:
    // <<command ApplyPoints "Sabrina,Melissa" 1>>
    [YarnCommand("ApplyPoints")]
    public void ApplyPoints(string csvIds, int delta)
    {
        if (scoreManager == null) return;

        var ids = csvIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        var tempRule = new EventScoreRule
        {
            gain = new System.Collections.Generic.List<string>(),
            lose = new System.Collections.Generic.List<string>(),
            amount = Math.Abs(delta) // Agora vai funcionar
        };

        if (delta > 0)
        {
            foreach (var id in ids) tempRule.gain.Add(id.Trim());
        }
        else if (delta < 0)
        {
            foreach (var id in ids) tempRule.lose.Add(id.Trim());
        }

        scoreManager.ApplyRule(tempRule);
    }

    [YarnCommand("DebugLog")]
    public void DebugLog(string message)
    {
        Debug.Log("[Yarn DebugLog] " + message);
    }
}