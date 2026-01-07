// using UnityEngine;
// using Yarn.Unity;
// using System.Collections.Generic;

// public class YarnRelationshipSync : MonoBehaviour
// {{
//     public DialogueRunner dialogueRunner;
//     public CharacterData characterData;
    
//     private Dictionary<string, int> initialScores = new Dictionary<string, int>();

//     public void SyncGameStateToYarn()
//     {
//         initialScores.Clear();
//         foreach (var mapping in characters)
//         {
//             string varName = "relationshipScore_" + mapping.characterName;
//             int currentScore = mapping.characterData.RelationshipScore;
//             initialScores[varName] = currentScore;
//             dialogueRunner.VariableStorage.SetValue(varName, currentScore);
//         }
//     }

//     public void ApplyDialogueChangesToGame()
//     {
//         foreach (var mapping in characters)
//         {
//             string varName = "relationshipScore_" + mapping.characterName;
//             int initialScore = initialScores.ContainsKey(varName) ? initialScores[varName] : 0;
//             int finalScore = dialogueRunner.VariableStorage.GetValue(varName).AsInt;

//             int delta = finalScore - initialScore;
//             if (delta != 0)
//             {
//                 mapping.characterData.RelationshipScore += delta;
//             }
//         }
//     }
// }
// }