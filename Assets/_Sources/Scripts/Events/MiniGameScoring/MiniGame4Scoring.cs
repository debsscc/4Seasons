using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame41Scoring : MonoBehaviour, IMiniGameScoring
{
    [Header("Config dos Fogos")]
    [Tooltip("Slots dos fogos")]
    public List<SlotDraggable> fireworkSlots = new List<SlotDraggable>();

    [Tooltip("Button confirm")]
    public GameObject confirmButton;

    [Tooltip("Distância centralizada max")]
    public float centerDistanceThreshold = 40f;

    [Header("Characters")]
    [Tooltip("Todas pessoas nesse evento")]
    public List<CharacterData> friendCharacters = new List<CharacterData>();

    [Tooltip("CharacterData da protagonista")]
    public CharacterData playerCharacter;

    [Header("Pontuação")]
    [Tooltip("Amigo dono da foto escolhida")]
    public int chosenFriendPoints = 2;

    [Tooltip("Amigos que não foram escolhidos")]
    public int notChosenFriendPoints = -1;

    [Tooltip("Satisfação própria se escolher a própria foto")]
    public int selfChosenPoints = 2;

    [Tooltip("Satisfação própria se NÃO escolher a própria foto")]
    public int selfNotChosenPoints = -1;

    [Header("Modals por ID")]
    public List<GameObject> modalsByID = new List<GameObject>();

    private SlotDraggable _centeredSlot;    
    private SlotDraggable _selectedSlot;    
    private bool _confirmed = false;

    private void Start()
    {
        if (confirmButton) confirmButton.SetActive(false);

        foreach (var modal in modalsByID)
        {
            if (modal != null) modal.SetActive(false);
        }
    }

    public void OnDragOverSlot(DraggablePrefab draggable, SlotDraggable nearSlot)
    {
        if (_confirmed) return;

        if (nearSlot == null)
        {
            _centeredSlot = null;
            if (confirmButton) confirmButton.SetActive(false);
            return;
        }

        float dist = Vector2.Distance(draggable.transform.position, nearSlot.transform.position);

        if (dist <= centerDistanceThreshold)
        {
            _centeredSlot = nearSlot;
            if (confirmButton) confirmButton.SetActive(true);
        }
        else
        {
            _centeredSlot = null;
            if (confirmButton) confirmButton.SetActive(false);
        }
    }

    public void OnSlotDropped(SlotDraggable slot)
    {
        if (_confirmed) return;
        if (slot == null) return;

        _selectedSlot = slot;

        Debug.Log($"[MiniGame4.1] Fogo selecionado: {slot.name} (char = {slot.associatedCharacter?.name ?? "null"}, specialId = {slot.specialId})");
    }

    public void OnConfirmButtonClicked()
    {
        if (_confirmed) return;
        if (_selectedSlot == null)
        {
            Debug.LogWarning("[MiniGame4.1] Tentou confirmar sem nenhum fogo selecionado.");
            return;
        }
        if (_centeredSlot != _selectedSlot)
        {
            return;
        }

        _confirmed = true;

        if (confirmButton) confirmButton.SetActive(false);

        ApplyScoring(_selectedSlot);

        // Ativa o modal baseado no specialId do slot
        ShowModalByID(_selectedSlot.specialId);

        Debug.Log("[MiniGame4.1] Fim do minigame 4.1.");
    }

    private void ApplyScoring(SlotDraggable chosenSlot)
    {
        CharacterData chosenChar = chosenSlot.associatedCharacter;

        foreach (var friend in friendCharacters)
        {
            if (friend == null) continue;

            int before = friend.RelationshipScore;

            if (friend == chosenChar)
            {
                friend.RelationshipScore += chosenFriendPoints;
                Debug.Log($"[MiniGame4.1] {friend.name} FOI escolhido: {before} -> {friend.RelationshipScore} (+{chosenFriendPoints})");
            }
            else
            {
                friend.RelationshipScore += notChosenFriendPoints;
                Debug.Log($"[MiniGame4.1] {friend.name} NÃO foi escolhido: {before} -> {friend.RelationshipScore} ({notChosenFriendPoints})");
            }
        }

        if (playerCharacter != null)
        {
            int before = playerCharacter.RelationshipScore;

            if (chosenChar == playerCharacter)
            {
                playerCharacter.RelationshipScore += selfChosenPoints;
                Debug.Log($"[MiniGame4.1] Player ESCOLHEU a própria foto: {before} -> {playerCharacter.RelationshipScore} (+{selfChosenPoints})");
            }
            else
            {
                playerCharacter.RelationshipScore += selfNotChosenPoints;
                Debug.Log($"[MiniGame4.1] Player NÃO escolheu a própria foto: {before} -> {playerCharacter.RelationshipScore} ({selfNotChosenPoints})");
            }
        }
        else
        {
            Debug.LogWarning("[MiniGame4.1] playerCharacter não atribuído.");
        }
    }

    private void ShowModalByID(int id)
    {
        int index = id ;

        if (index >= 0 && index < modalsByID.Count && modalsByID[index] != null)
        {
            modalsByID[index].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[MiniGame4.1] Nenhum modal configurado para ID {id}.");
        }
    }

    public void OnItemRemovedFromSlot(){
    }

    public void OnObjectDropped(SlotDraggable slot, ItemsSO[] items)
    {
        OnSlotDropped(slot);
    }
} 