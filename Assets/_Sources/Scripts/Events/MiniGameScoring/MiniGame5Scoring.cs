using UnityEngine;
using System.Collections.Generic;

public class MiniGame5Scoring : MonoBehaviour, IMiniGameScoring
{
    [Header("Amigos presentes no evento")]
    public List<CharacterData> eventFriends = new List<CharacterData>(); 

    [Header("Pontuação")]
    public int chosenFriendPoints = 4;
    public int notChosenFriendPoints = -2;

    public void OnObjectDropped(SlotDraggable slot, ItemsSO[] items)
    {
        if (slot == null)
        {
            Debug.LogWarning("[MiniGame5] Slot nulo.");
            return;
        }

        var chosen = slot.associatedCharacter;
        Debug.Log($"[MiniGame5] Slot {slot.name}, chosen={(chosen != null ? chosen.name : "null")}");

        if (chosen == null)
        {
            Debug.LogWarning("[MiniGame5] Slot sem CharacterData associado.");
            return;
        }

        foreach (var friend in eventFriends)
        {
            if (friend == null) continue;

            if (friend == chosen)
            {
                int scoreAntes = friend.RelationshipScore;
                friend.RelationshipScore += chosenFriendPoints;
                Debug.Log($"[MiniGame5] {friend.name} foi escolhido: {scoreAntes} → {friend.RelationshipScore} (+{chosenFriendPoints})");
            }
            else
            {
                int scoreAntes = friend.RelationshipScore;
                friend.RelationshipScore += notChosenFriendPoints;
                Debug.Log($"[MiniGame5] {friend.name} não foi escolhido: {scoreAntes} → {friend.RelationshipScore} ({notChosenFriendPoints})");
            }
        }

        Debug.Log("[MiniGame5] Pontos próprios não alterados.");
    }
}