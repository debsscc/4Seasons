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
    private bool _isConfirming = false;

    private void Start()
    {
        if (miniGameController != null)
        {
            foreach (var slot in miniGameController.targetSlots)
            {
                if (slot != null)
                    slot.OnObjectRemovedFromSlot += HandleObjectRemoved;
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
                    slot.OnObjectRemovedFromSlot -= HandleObjectRemoved;
            }
        }
    }

    private void HandleObjectRemoved(SlotDraggable slot)
    {
        if (_isConfirming) return;
        Debug.Log($"[MiniGame1] Objeto removido do slot '{slot.name}'");
        OnItemRemovedFromSlot();
    }

    public void OnObjectDropped(SlotDraggable slot, ItemsSO[] items)
    {
        if (slot == null)
        {
            return;
        }

        _pendingSlot = slot;
        _pendingItems = items;

        if (feedbackCorajoso != null) feedbackCorajoso.SetActive(false);
        if (feedbackNaoCorajoso != null) feedbackNaoCorajoso.SetActive(false);

        if (confirmButton != null)
            confirmButton.SetActive(true);

        // Preview: troca expressão e anima ícone dos NPCs
        if (MiniGameFeedbackManager.Instance != null)
        {
            MiniGameFeedbackManager.Instance.ApplyPreview(items);
            foreach (var ui in MiniGameFeedbackManager.Instance.uiCharacterOrders)
            {
                if (ui == null) continue;
                foreach (var item in items)
                {
                    ui.UpdateExpresionBasedOnItem(item);
                    ui.PunchScale();
                    if (ui.CharacterLikesItem(item)) break;
                }
            }
        }
    }

    public void OnItemDraggedOutOfSlot()
    {
        _pendingSlot = null;
        _pendingItems = null;

        if (confirmButton != null)
            confirmButton.SetActive(false);


        if (MiniGameFeedbackManager.Instance != null)
            MiniGameFeedbackManager.Instance.ResetAll();
    }

    public void OnConfirmButtonClicked()
    {
        var slot = _pendingSlot;
        var items = _pendingItems;

        if (slot == null || slot.lastDroppedObject == null)
        {
            if (confirmButton != null) confirmButton.SetActive(false);
            return;
        }

        if (items == null || items.Length == 0)
        {
            if (confirmButton != null) confirmButton.SetActive(false);
            return;
        }


        if (confirmButton != null)
            confirmButton.SetActive(false);

        bool escolheuCorajoso = ApplyScores(items);

        if (miniGameController != null)
            miniGameController.ShowNPCReactions(items);

        foreach (var ui in MiniGameFeedbackManager.Instance.uiCharacterOrders)
        {
            foreach (var item in items)
            {
                bool positive = ui.CharacterLikesItem(item);
                ui.ShowHeart(positive);
                if (positive) break;
            }
        }

        _isConfirming = true;
        _pendingSlot = null;
        _pendingItems = null;

        if (slot.lastDroppedObject != null)
            Destroy(slot.lastDroppedObject);

        slot.ClearSlot();

        _isConfirming = false;

        slot.PlayCloseAnimation(() => ShowFeedbackModal(escolheuCorajoso));
    }

    public void OnItemRemovedFromSlot()
    {
        _pendingSlot = null;
        _pendingItems = null;

        if (confirmButton != null)
            confirmButton.SetActive(false);


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
                if (npc.traits.isBrave) delta += +3;
            }
            else
            {
                if (npc.traits.isFearful) delta += +3;
                if (npc.traits.isBrave) delta += -2;
            }

            int antes = npc.RelationshipScore;
            npc.RelationshipScore = antes + delta;
        }

        var player = charsManager.playerCharacter;
        if (player != null)
        {
            int deltaSelf = 0;

            bool escolheuFavoritoSelf = items.Any(i => player.favoriteItems.Contains(i));
            deltaSelf += escolheuFavoritoSelf ? 2 : -1;

            if (escolheuTerrorOuSuspense && player.traits.isFearful)
                deltaSelf += -2;
            else if (!escolheuTerrorOuSuspense && player.traits.isFearful)
                deltaSelf += +3;

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
}
