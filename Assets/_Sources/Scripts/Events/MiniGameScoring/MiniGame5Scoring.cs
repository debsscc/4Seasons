    using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

[System.Serializable]
public struct MiniGame5SlotFeedback
{
    public SlotDraggable slot;
    public Image characterImage;
    public Sprite happySprite;
    public Sprite sadSprite;
    public Image heartImage;
    public Sprite positiveHeartSprite;
    public Sprite negativeHeartSprite;
}

public class MiniGame5Scoring : MonoBehaviour, IMiniGameScoring
{
    [Header("Feedback Visual dos Slots")]
    public List<MiniGame5SlotFeedback> slotFeedbacks = new List<MiniGame5SlotFeedback>();

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

        foreach (var slot in npcSlots)
        {
            if (slot != null && slot.npcOutline != null)
                slot.npcOutline.enabled = false;
        }

        foreach (var modal in modalsByID)
        {
            if (modal != null) modal.SetActive(false);
        }

        foreach (var slot in npcSlots)
        {
            if (slot == null) continue;
            slot.OnObjectRemovedFromSlot += OnNPCSlotObjectRemoved;

            // Garante que o slot possa receber cliques mesmo sem Graphic próprio
            if (slot.GetComponent<Image>() == null)
            {
                var img = slot.gameObject.AddComponent<Image>();
                img.color = Color.clear;
                img.raycastTarget = true;
            }
        }
    }

    public void OnDragOverSlot(DraggablePrefab draggable, SlotDraggable nearSlot)
    {
        if (_confirmed) return;

        if (nearSlot == null)
        {
            if (_hoveredSlot != null && _hoveredSlot != _selectedSlot)
                SetOutline(_hoveredSlot, false, hoverOutlineColor);
            _hoveredSlot = null;
            return;
        }

        if (_hoveredSlot != null && _hoveredSlot != nearSlot && _hoveredSlot != _selectedSlot)
            SetOutline(_hoveredSlot, false, hoverOutlineColor);

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
            SetAllSlotsNeutral();

        _selectedSlot = slot;
        SetOutline(_selectedSlot, true, selectedOutlineColor);

        foreach (var fb in slotFeedbacks)
        {
            if (fb.slot == slot)
                SetSlotFeedback(fb, true, true);
            else
                SetSlotFeedback(fb, false, false);
        }

        if (ticketObject != null)
            ticketObject.SetActive(false);

        if (confirmButton) confirmButton.SetActive(true);

        Debug.Log($"[MiniGame5] NPC selecionado: {slot.name} (char = {slot.associatedCharacter?.name ?? "null"}, specialId = {slot.specialId})");
    }

    public void OnSelectedNPCClicked()
    {
        if (_confirmed) return;
        if (_selectedSlot == null) return;

        Debug.Log($"[MiniGame5] NPC desselecionado: {_selectedSlot.name}");

        SetOutline(_selectedSlot, false, selectedOutlineColor);
        if (_selectedSlot != null) _selectedSlot.lastDroppedObject = null;
        SetAllSlotsNeutral();
        _selectedSlot = null;

        ShowTicket();

        if (confirmButton) confirmButton.SetActive(false);
    }

    private void OnNPCSlotObjectRemoved(SlotDraggable slot)
    {
        if (_confirmed) return;

        Debug.Log($"[MiniGame5] NPC desselecionado via clique: {slot.name}");

        SetOutline(slot, false, selectedOutlineColor);
        slot.lastDroppedObject = null;
        SetAllSlotsNeutral();
        _selectedSlot = null;

        ShowTicket();

        if (confirmButton) confirmButton.SetActive(false);
    }

    private void ShowTicket()
    {
        if (ticketObject == null) return;

        var draggable = ticketObject.GetComponent<DraggablePrefab>();
        if (draggable != null)
        {
            draggable.rectTransform.DOKill();
            draggable.rectTransform.anchoredPosition = draggable.InitialAnchoredPosition;
        }

        ticketObject.SetActive(true);
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

        foreach (var fb in slotFeedbacks)
        {
            if (fb.slot == _selectedSlot)
                ShowHeart(fb, true);
            else
                ShowHeart(fb, false);
        }

        if (confirmButton) confirmButton.SetActive(false);

        ApplyScoring(_selectedSlot);
        StartCoroutine(ShowModalAfterDelay(_selectedSlot.specialId));
    }

    public void OnObjectDropped(SlotDraggable slot, ItemsSO[] items)
    {
        OnSlotDropped(slot);
    }

    public void OnItemRemovedFromSlot() { }

    // --- FEEDBACK VISUAL ---

    private void SetSlotFeedback(MiniGame5SlotFeedback fb, bool isSelected, bool isPositive)
    {
        if (fb.characterImage != null)
            fb.characterImage.sprite = isPositive ? fb.happySprite : fb.sadSprite;
        if (fb.heartImage != null)
            fb.heartImage.gameObject.SetActive(false);
    }

    private void SetAllSlotsNeutral()
    {
        foreach (var fb in slotFeedbacks)
            SetSlotFeedback(fb, false, false);
    }

    private void ShowHeart(MiniGame5SlotFeedback fb, bool positive)
    {
        if (fb.heartImage == null) return;
        fb.heartImage.sprite = positive ? fb.positiveHeartSprite : fb.negativeHeartSprite;
        fb.heartImage.gameObject.SetActive(true);
        var color = fb.heartImage.color;
        color.a = 1f;
        fb.heartImage.color = color;
        fb.heartImage.DOKill();
        fb.heartImage.DOFade(0f, 2f).SetEase(Ease.Linear).OnComplete(() =>
        {
            if (fb.heartImage != null) fb.heartImage.gameObject.SetActive(false);
        });
    }

    private void SetOutline(SlotDraggable slot, bool enabled, Color color)
    {
        if (slot == null || slot.npcOutline == null) return;
        slot.npcOutline.enabled = enabled;
        if (enabled)
            slot.npcOutline.effectColor = color;
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
}