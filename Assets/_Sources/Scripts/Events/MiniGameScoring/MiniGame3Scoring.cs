using UnityEngine;

public class MiniGame3Scoring : MonoBehaviour, IMiniGameScoring
{
    [Header("Config Minigame 3 - Maconha")]
    public int acceptId = 1; 
    public int rejectId = 2; 
    
    [Header("Pontuação")]
    public int npcPointsOnAccept = 3;
    public int selfPointsOnAccept = 3;

    [Header("Personagens do Evento")]
    public CharacterData npcDoEvento; 
    public CharacterData playerCharacter; // Arraste o CharacterData da protagonista aqui

    public void OnObjectDropped(SlotDraggable slot, ItemsSO[] items)
    {        
        if (slot == null)
        {
            Debug.LogWarning("[MiniGame3] Slot é nulo!");
            return;
        }

        Debug.Log($"[MiniGame3] Slot recebido: {slot.name}, specialId = {slot.specialId}");

        if (slot.specialId == acceptId)
        {
            Debug.Log("[MiniGame3] ACEITOU a maconha!");
            
            if (npcDoEvento != null)
            {
                int scoreAntes = npcDoEvento.RelationshipScore;
                npcDoEvento.RelationshipScore += npcPointsOnAccept;
                Debug.Log($"[MiniGame3] {npcDoEvento.name}: {scoreAntes} → {npcDoEvento.RelationshipScore} (+{npcPointsOnAccept})");
            }
            else
            {
                Debug.LogWarning("[MiniGame3] npcDoEvento não foi atribuído no Inspector!");
            }

            if (playerCharacter != null)
            {
                int scoreAntes = playerCharacter.RelationshipScore;
                playerCharacter.RelationshipScore += selfPointsOnAccept;
                Debug.Log($"[MiniGame3] Player: {scoreAntes} → {playerCharacter.RelationshipScore} (+{selfPointsOnAccept})");
            }
            else
            {
                Debug.LogWarning("[MiniGame3] playerCharacter não foi atribuído no Inspector!");
            }
        }
        else if (slot.specialId == rejectId)
        {
            Debug.Log("[MiniGame3] RECUSOU a maconha. Nenhum ponto aplicado.");
        }
        else
        {
            Debug.Log($"[MiniGame3] specialId={slot.specialId} não reconhecido. Usando lógica padrão.");
            CharactersManager.Instance.ApplyPointsByTrait(items);
        }
    }
}