using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MiniGame5Scoring : MonoBehaviour, IMiniGameScoring
{
    [Header("Amigos presentes no evento")]
    public List<CharacterData> eventFriends = new List<CharacterData>();

    [Header("Slots dos NPCs")]
    [Tooltip("Slots dos NPCs (cada um deve ter associatedCharacter e npcOutline)")]
    public List<SlotDraggable> npcSlots = new List<SlotDraggable>();

    [Header("Ticket")]
    [Tooltip("GameObject do ticket (draggable)")]
    public GameObject ticketObject;

    [Header("Botão de Confirmar")]
    [Tooltip("Botão que aparece quando um NPC é selecionado")]
    public GameObject confirmButton;

    [Header("Modals por ID")]
    [Tooltip("Lista de modais: índice 0 = ID 1, índice 1 = ID 2, etc.")]
    public List<GameObject> modalsByID = new List<GameObject>();

    [Header("Pontuação")]
    public int chosenFriendPoints = 4;
    public int notChosenFriendPoints = -2;

    [Header("Config")]
    [Tooltip("Cor do outline quando hover")]
    public Color hoverOutlineColor = Color.yellow;

    [Tooltip("Cor do outline quando selecionado")]
    public Color selectedOutlineColor = Color.white;

    [Tooltip("Tempo de espera antes de mostrar o modal (segundos)")]
    public float waitBeforeModal = 6f;

    private SlotDraggable _hoveredSlot;
    private SlotDraggable _selectedSlot;
    private bool _confirmed = false;

    private void Start()
    {
        if (confirmButton) confirmButton.SetActive(false);

        // Desativa todos os outlines no início
        foreach (var slot in npcSlots)
        {
            if (slot != null && slot.npcOutline != null)
            {
                slot.npcOutline.enabled = false;
            }
        }

        // Garante que todos os modais começam desativados
        foreach (var modal in modalsByID)
        {
            if (modal != null) modal.SetActive(false);
        }
    }

    public void OnDragOverSlot(DraggablePrefab draggable, SlotDraggable nearSlot)
    {
        if (_confirmed) return;

        // Se não está perto de nenhum slot, desativa hover
        if (nearSlot == null)
        {
            if (_hoveredSlot != null && _hoveredSlot != _selectedSlot)
            {
                SetOutline(_hoveredSlot, false, hoverOutlineColor);
            }
            _hoveredSlot = null;
            return;
        }

        // Se mudou de slot, desativa o hover do anterior
        if (_hoveredSlot != null && _hoveredSlot != nearSlot && _hoveredSlot != _selectedSlot)
        {
            SetOutline(_hoveredSlot, false, hoverOutlineColor);
        }

        // Ativa hover no slot atual (se não for o selecionado)
        if (nearSlot != _selectedSlot)
        {
            _hoveredSlot = nearSlot;
            SetOutline(_hoveredSlot, true, hoverOutlineColor);
        }
    }


    public void OnSlotDropped(SlotDraggable slot)
    {
        if (_confirmed) return;
        if (slot == null) return;

        if (_hoveredSlot != null)
        {
            SetOutline(_hoveredSlot, false, hoverOutlineColor);
            _hoveredSlot = null;
        }

        if (_selectedSlot != null)
        {
            SetOutline(_selectedSlot, false, selectedOutlineColor);
        }

        _selectedSlot = slot;
        SetOutline(_selectedSlot, true, selectedOutlineColor);

        // Esconde o ticket
        if (ticketObject != null)
        {
            ticketObject.SetActive(false);
        }

        // Mostra o botão de confirmar
        if (confirmButton) confirmButton.SetActive(true);

        Debug.Log($"[MiniGame5] NPC selecionado: {slot.name} (char = {slot.associatedCharacter?.name ?? "null"}, specialId = {slot.specialId})");
    }
    public void OnSelectedNPCClicked()
    {
        if (_confirmed) return;
        if (_selectedSlot == null) return;

        Debug.Log($"[MiniGame5] NPC desselecionado: {_selectedSlot.name}");

        SetOutline(_selectedSlot, false, selectedOutlineColor);
        _selectedSlot = null;

        if (ticketObject != null)
        {
            ticketObject.SetActive(true);
        }

        // Esconde o botão de confirmar
        if (confirmButton) confirmButton.SetActive(false);
    }

    public void OnConfirmButtonClicked()
    {
        if (_confirmed) return;
        if (_selectedSlot == null)
        {
            Debug.LogWarning("[MiniGame5] Tentou confirmar sem nenhum NPC selecionado.");
            return;
        }

        _confirmed = true;

        SetOutline(_selectedSlot, false, selectedOutlineColor);

        // Esconde o botão de confirmar
        if (confirmButton) confirmButton.SetActive(false);

        // Aplica pontos
        ApplyScoring(_selectedSlot);

        StartCoroutine(ShowModalAfterDelay(_selectedSlot.specialId));
    }

    private void ApplyScoring(SlotDraggable chosenSlot)
    {
        CharacterData chosenChar = chosenSlot.associatedCharacter;

        foreach (var friend in eventFriends)
        {
            if (friend == null) continue;

            int before = friend.RelationshipScore;

            if (friend == chosenChar)
            {
                friend.RelationshipScore += chosenFriendPoints;
                Debug.Log($"[MiniGame5] {friend.name} FOI escolhido: {before} -> {friend.RelationshipScore} (+{chosenFriendPoints})");
            }
            else
            {
                friend.RelationshipScore += notChosenFriendPoints;
                Debug.Log($"[MiniGame5] {friend.name} NÃO foi escolhido: {before} -> {friend.RelationshipScore} ({notChosenFriendPoints})");
            }
        }

        Debug.Log("[MiniGame5] Pontos próprios não alterados.");
    }

    private IEnumerator ShowModalAfterDelay(int id)
    {
        yield return new WaitForSeconds(waitBeforeModal);

        int index = id - 1;

        if (index >= 0 && index < modalsByID.Count && modalsByID[index] != null)
        {
            modalsByID[index].SetActive(true);
            Debug.Log($"[MiniGame5] Modal ID {id} ativado.");
        }
        else
        {
            Debug.LogWarning($"[MiniGame5] Nenhum modal configurado para ID {id}.");
        }
    }

    private void SetOutline(SlotDraggable slot, bool enabled, Color color)
    {
        if (slot == null || slot.npcOutline == null) return;

        slot.npcOutline.enabled = enabled;
        if (enabled)
        {
            slot.npcOutline.effectColor = color;
        }

    }

    public void OnObjectDropped(SlotDraggable slot, ItemsSO[] items)
    {
        OnSlotDropped(slot);
    }
}