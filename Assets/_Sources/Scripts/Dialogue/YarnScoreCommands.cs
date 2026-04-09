using UnityEngine;
using Yarn.Unity;
using System;

public class YarnScoreCommands : MonoBehaviour
{
    public ScoreRulesDialogue scoreManager;
    public DialogueEmotionController emotionController;

    public void ApplyEventPart(string ruleId)
    {

        if (scoreManager == null)
        {
            Debug.LogError("[YarnScoreCommands] scoreManager NÃO atribuído");
            return;
        }

        emotionController?.ForceApplyCurrentEmotion();

        scoreManager.ApplyRuleById(ruleId);
    }

    public System.Collections.IEnumerator ApplyEventPartRoutine(string ruleId)
    {

        if (scoreManager == null)
        {
            Debug.LogError("[YarnScoreCommands] scoreManager NÃO atribuído");
            yield break;
        }

        emotionController?.ForceApplyCurrentEmotion();
        scoreManager.ApplyRuleById(ruleId);

        bool isMinigame = MiniGameFeedbackManager.Instance != null && MiniGameFeedbackManager.Instance.isMinigame;

        if (scoreManager.rulesAsset != null)
        {
            var rule = scoreManager.rulesAsset.GetRule(ruleId);
            if (rule != null && (rule.gain == null || rule.gain.Count == 0) && (rule.lose == null || rule.lose.Count == 0))
            {
                yield break;
            }
        }

        if (!isMinigame)
            yield return new UnityEngine.WaitForSeconds(3f);
    }

    public void ApplyPoints(string csvIds, int delta)
    {
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
        }
        else if (delta < 0)
        {
            foreach (var id in ids) tempRule.lose.Add(id.Trim());
        }

        scoreManager.ApplyRule(tempRule);
    }

    public void DebugLog(string message)
    {
        Debug.Log("[Yarn DebugLog] " + message);
    }
}
