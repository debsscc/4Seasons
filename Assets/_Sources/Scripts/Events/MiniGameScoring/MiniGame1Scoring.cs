using System.Collections;
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

        if (feedbackCorajoso != null) feedbackCorajoso.SetActive(false);
        if (feedbackNaoCorajoso != null) feedbackNaoCorajoso.SetActive(false);

        if (confirmButton != null)
        {
            confirmButton.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[MiniGame1] confirmButton não foi atribuído no Inspector.");
        }
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
    }

    public void OnConfirmButtonClicked()
    {
        if (_pendingItems == null || _pendingItems.Length == 0)
        {
            Debug.LogWarning("[MiniGame1] Não há itens pendentes para confirmar.");
            return;
        }

        if (confirmButton != null)
            confirmButton.SetActive(false);

        Debug.Log("[MiniGame1] Confirmando escolha. Aplicando pontuações...");

        bool escolheuCorajoso = ApplyScores(_pendingItems);

        if (miniGameController != null)
        {
            miniGameController.ShowNPCReactions(_pendingItems);
        }

        ShowFeedbackModal(escolheuCorajoso);
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
}

    private bool ApplyScores(ItemsSO[] items)
    {
        var charsManager = CharactersManager.Instance;
        if (charsManager == null)
        {
            Debug.LogError("[MiniGame1] CharactersManager.Instance é nulo.");
            return false;
        }

        bool escolheuTerrorOuSuspense = items.Any(i => terrorOuSuspenseItems.Contains(i));

        // ------------- NPCs -------------
        foreach (var npc in charsManager.npcs)
        {
            if (npc == null) continue;

            int delta = 0;

            // Favoritos
            bool escolheuFavoritoNPC = items.Any(i => npc.favoriteItems.Contains(i));
            delta += escolheuFavoritoNPC ? 2 : -1;

            // Traços vs gênero
            if (escolheuTerrorOuSuspense)
            {
                if (npc.traits.isFearful) delta += -2;   // medroso com terror/suspense
                //
                if (npc.traits.isBrave)   delta += +3;   // corajoso com terror/suspense
            }
            else
            {
                if (npc.traits.isFearful) delta += +3;   // medroso sem terror/suspense
                if (npc.traits.isBrave)   delta += -2;   // corajoso sem terror/suspense
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
        else
        {
            Debug.LogWarning("[MiniGame1] playerCharacter não configurado em CharactersManager.");
        }

        return escolheuTerrorOuSuspense;
    }



    private void ShowFeedbackModal(bool escolheuCorajoso)
    {
        if (feedbackCorajoso != null) feedbackCorajoso.SetActive(false);
        if (feedbackNaoCorajoso != null) feedbackNaoCorajoso.SetActive(false);

        if (escolheuCorajoso)
        {
            Debug.Log("[MiniGame1] Feedback: CORAJOSO.");
            if (feedbackCorajoso != null)
                feedbackCorajoso.SetActive(true);
        }
        else
        {
            Debug.Log("[MiniGame1] Feedback: NÃO CORAJOSO.");
            if (feedbackNaoCorajoso != null)
                feedbackNaoCorajoso.SetActive(true);
        }
    }
}