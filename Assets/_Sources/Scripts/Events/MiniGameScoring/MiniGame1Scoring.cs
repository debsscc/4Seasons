using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MiniGame1Scoring : MonoBehaviour, IMiniGameScoring
{
    [Header("Referências")]
    [Tooltip("MiniGameController dessa cena (pra chamar ShowNPCReactions, se quiser).")]
    public MiniGameController miniGameController;

    [Tooltip("Botão de confirmar (Button_Prefab). Deve começar DESATIVADO.")]
    public GameObject confirmButton;

    [Header("Feedbacks (Modais)")]
    public GameObject feedbackCorajoso;
    public GameObject feedbackNaoCorajoso;

    [Header("Config de Gênero")]
    [Tooltip("Quais DVDs contam como Terror ou Suspense (corajosos).")]
    public ItemsSO[] terrorOuSuspenseItems;

    private SlotDraggable _pendingSlot;
    private ItemsSO[] _pendingItems;

    private void Start()
    {
        if (miniGameController != null)
        {
            foreach (var slot in miniGameController.targetSlots)
            {
                if (slot != null)
                {
                    slot.OnObjectRemovedFromSlot += HandleObjectRemoved;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (miniGameController != null)
        {
            foreach (var slot in miniGameController.targetSlots)
            {
                if (slot != null)
                {
                    slot.OnObjectRemovedFromSlot -= HandleObjectRemoved;
                }
            }
        }
    }

    private void HandleObjectRemoved(SlotDraggable slot)
    {
        Debug.Log($"[MiniGame1] Objeto removido do slot '{slot.name}'");
        OnItemRemovedFromSlot();
    }

    // Chamado pelo MiniGameController quando um objeto é dropado no slot
    public void OnObjectDropped(SlotDraggable slot, ItemsSO[] items)
    {
        if (slot == null)
        {
            Debug.LogWarning("[MiniGame1] Slot é nulo em OnObjectDropped.");
            return;
        }

        _pendingSlot = slot;
        _pendingItems = items;

        Debug.Log($"[MiniGame1] Drop registrado no slot '{slot.name}'. Aguardando confirmação do jogador.");

        // esconde modais
        if (feedbackCorajoso != null) feedbackCorajoso.SetActive(false);
        if (feedbackNaoCorajoso != null) feedbackNaoCorajoso.SetActive(false);

        if (confirmButton != null)
            confirmButton.SetActive(true);
        else
            Debug.LogWarning("[MiniGame1] confirmButton não foi atribuído no Inspector.");

        ApplyPreviewForItems(items);
    }

    public void OnItemDraggedOutOfSlot()
    {
        _pendingSlot = null;
        _pendingItems = null;

        if (confirmButton != null)
        {
            confirmButton.SetActive(false);
        }

        Debug.Log("[MiniGame1] Item arrastado para fora do slot. Escolha cancelada.");

        // limpa preview
        if (MiniGameFeedbackManager.Instance != null)
            MiniGameFeedbackManager.Instance.ResetAll();
    }

    public void OnConfirmButtonClicked()
    {
        if (_pendingSlot == null || _pendingSlot.lastDroppedObject == null)
        {
            Debug.LogWarning("[MiniGame1] Nenhum DVD no slot para confirmar!");
            if (confirmButton != null) confirmButton.SetActive(false);
            return;
        }

        if (_pendingItems == null || _pendingItems.Length == 0)
        {
            Debug.LogWarning("[MiniGame1] Não há itens pendentes para confirmar.");
            if (confirmButton != null) confirmButton.SetActive(false);
            return;
        }

        Debug.Log($"[MiniGame1] Confirmando escolha do slot '{_pendingSlot.name}'.");

        if (confirmButton != null)
            confirmButton.SetActive(false);

        bool escolheuCorajoso = ApplyScores(_pendingItems);

        if (miniGameController != null)
        {
            miniGameController.ShowNPCReactions(_pendingItems);
        }

        ShowFeedbackModal(escolheuCorajoso);

        if (_pendingSlot.lastDroppedObject != null)
        {
            Destroy(_pendingSlot.lastDroppedObject);
        }
        _pendingSlot.ClearSlot();

        _pendingSlot = null;
        _pendingItems = null;

    }

    public void OnItemRemovedFromSlot()
    {
        _pendingSlot = null;
        _pendingItems = null;

        if (confirmButton != null)
        {
            confirmButton.SetActive(false);
        }

        Debug.Log("[MiniGame1] Item removido do slot. Escolha cancelada.");

        // Reseta preview visual imediatamente
        MiniGameFeedbackManager.Instance?.ResetAll();
    }

    private bool ApplyScores(ItemsSO[] items)
    {
        var charsManager = CharactersManager.Instance;

        bool escolheuTerrorOuSuspense = items.Any(i => terrorOuSuspenseItems.Contains(i));

        foreach (var npc in charsManager.npcs)
        {
            if (npc == null) continue;

            int delta = 0;

            bool escolheuFavoritoNPC = items.Any(i => npc.favoriteItems.Contains(i));
            delta += escolheuFavoritoNPC ? 2 : -1;

            if (escolheuTerrorOuSuspense)
            {
                if (npc.traits.isFearful) delta += -2;
                if (npc.traits.isBrave)   delta += +3;
            }
            else
            {
                if (npc.traits.isFearful) delta += +3;
                if (npc.traits.isBrave)   delta += -2;
            }

            int antes = npc.RelationshipScore;
            npc.RelationshipScore = antes + delta;
            Debug.Log($"[MiniGame1][NPC] {npc.name}: {antes} → {npc.RelationshipScore} (Δ {delta})");
        }

        var player = charsManager.playerCharacter;
        if (player != null)
        {
            int deltaSelf = 0;

            bool escolheuFavoritoSelf = items.Any(i => player.favoriteItems.Contains(i));
            deltaSelf += escolheuFavoritoSelf ? 2 : -1;

            if (escolheuTerrorOuSuspense && player.traits.isFearful)
            {
                deltaSelf += -2;
            }
            else if (!escolheuTerrorOuSuspense && player.traits.isFearful)
            {
                deltaSelf += +3;
            }

            int antes = player.RelationshipScore;
            player.RelationshipScore = antes + deltaSelf;
            Debug.Log($"[MiniGame1][SELF] {player.name}: {antes} → {player.RelationshipScore} (Δ {deltaSelf})");
        }

        return escolheuTerrorOuSuspense;
    }

    private void ShowFeedbackModal(bool escolheuCorajoso)
    {
        if (feedbackCorajoso != null) feedbackCorajoso.SetActive(false);
        if (feedbackNaoCorajoso != null) feedbackNaoCorajoso.SetActive(false);

        if (escolheuCorajoso)
        {
            if (feedbackCorajoso != null)
                feedbackCorajoso.SetActive(true);
        }
        else
        {
            if (feedbackNaoCorajoso != null)
                feedbackNaoCorajoso.SetActive(true);
        }
    }

    private void ApplyPreviewForItems(ItemsSO[] items)
{
    if (MiniGameFeedbackManager.Instance == null) return;

    var charsManager = CharactersManager.Instance;
    Dictionary<string, int> deltas = new Dictionary<string, int>();

    bool escolheuTerrorOuSuspense = items.Any(i => terrorOuSuspenseItems.Contains(i));

    foreach (var npc in charsManager.npcs)
    {
        if (npc == null) continue;

        string cleanId = npc.name.Replace("NPCData_", "").Trim();
        
        int delta = 0;
        bool escolheuFavoritoNPC = items.Any(i => npc.favoriteItems.Contains(i));
        delta += escolheuFavoritoNPC ? 2 : -1;

        if (escolheuTerrorOuSuspense)
        {
            if (npc.traits.isFearful) delta += -2;
            if (npc.traits.isBrave)   delta += +3;
        }
        else
        {
            if (npc.traits.isFearful) delta += +3;
            if (npc.traits.isBrave)   delta += -2;
        }

        deltas[cleanId] = delta;
    }

    // Player
    var player = charsManager.playerCharacter;
    if (player != null)
    {
        int deltaSelf = 0;
        bool escolheuFavoritoSelf = items.Any(i => player.favoriteItems.Contains(i));
        deltaSelf += escolheuFavoritoSelf ? 2 : -1;

        if (escolheuTerrorOuSuspense && player.traits.isFearful) deltaSelf += -2;
        else if (!escolheuTerrorOuSuspense && player.traits.isFearful) deltaSelf += +3;

        deltas["Player"] = deltaSelf; 
    }

    var preview = new Dictionary<string, FeedbackType>();
    foreach (var kv in deltas)
    {
        FeedbackType t = FeedbackType.Neutral;
        if (kv.Value > 0) t = FeedbackType.Positive;
        else if (kv.Value < 0) t = FeedbackType.Negative;

        preview[kv.Key] = t;
    }

    MiniGameFeedbackManager.Instance.ApplyPreview(preview);
}
}