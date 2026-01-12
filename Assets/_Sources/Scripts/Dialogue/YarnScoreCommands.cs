using UnityEngine;
using Yarn.Unity;
using System;

public class YarnScoreCommands : MonoBehaviour
{
    public ScoreRulesDialogue scoreManager;

    //<<command ApplyEventPart "Evento2.0_Parte1">>
  //  [YarnCommand("ApplyEventPart")]
    public void ApplyEventPart(string ruleId)
    {
        Debug.Log($"[YarnScoreCommands] ApplyEventPart chamado: {ruleId}");

        if (scoreManager == null)
        {
            Debug.LogError("[YarnScoreCommands] scoreManager NÃO atribuído");
            return;
        }

        scoreManager.ApplyRuleById(ruleId);
    }

    // <<command ApplyPoints "Sabrina,Melissa" 1>>
 //   [YarnCommand("ApplyPoints")]
    public void ApplyPoints(string csvIds, int delta)
    {
        Debug.Log($"[YarnScoreCommands] ApplyPoints called with ids='{csvIds}', delta={delta}");
        if (scoreManager == null) return;

        var ids = csvIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        var tempRule = new EventScoreRule
        {
            gain = new System.Collections.Generic.List<string>(),
            lose = new System.Collections.Generic.List<string>(),
            amount = Math.Abs(delta) 
        };

        if (delta > 0)
        {
            foreach (var id in ids) tempRule.gain.Add(id.Trim());
            Debug.Log("[Yarn DebugLog] Applying +points via ApplyPoints");
        }
        else if (delta < 0)
        {
            foreach (var id in ids) tempRule.lose.Add(id.Trim());
            Debug.Log("[Yarn DebugLog] Applying -points via ApplyPoints");
        }

        scoreManager.ApplyRule(tempRule);
    }

  //  [YarnCommand("DebugLog")]
    public void DebugLog(string message)
    {
        Debug.Log("[Yarn DebugLog] " + message);
    }
}