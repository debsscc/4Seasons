using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DG.Tweening;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class DraggablePrefab : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Title("Audio")]
    public AudioClip pickSfx;
    public AudioClip dropSfx;

    [HideInInspector] public RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas parentCanvas;
    private AudioSource audioSource;

    private bool isDragging;
    private SlotDraggable currentSlot; // Armazena o slot atual

    public List<SlotDraggable> TargetSlots { get; set; } = null;
    public MiniGameController MiniGameController { get; set; }

    private IItemHolder _itemHolder;

    public event Action OnBeginDragEvent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        _itemHolder = GetComponent<IItemHolder>();

        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
            Debug.LogError("DraggablePrefab precisa ta dentro de um componente canvas");

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    // --- INTERFACE DE DRAG ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        canvasGroup.blocksRaycasts = false;

        if (pickSfx) audioSource.PlayOneShot(pickSfx);

        // Verifica se o item está em um slot (para MiniGame2)
        var miniGame2 = MiniGameController?.GetComponent<MiniGame2Scoring>();
        if (miniGame2 != null)
        {
            var drink = GetComponent<DrinksINFO>();
            if (drink != null && drink.isInBasket)
            {
                miniGame2.OnDrinkRemovedFromBasket(drink);
            }
        }

        // Armazena o slot atual (caso esteja dentro de um)
        currentSlot = GetComponentInParent<SlotDraggable>();

        OnBeginDragEvent?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.WorldSpace)
        {
            rectTransform.position = eventData.position;
        }
        else
        {
            Camera cam = eventData.pressEventCamera ?? Camera.main;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                rectTransform,
                eventData.position,
                cam,
                out var worldPos))
            {
                rectTransform.position = worldPos;
            }
        }

        var nearSlot = IsNearSlot();

        // ----- MINIGAME 2 -----
        var miniGame2 = MiniGameController?.GetComponent<MiniGame2Scoring>();
        if (miniGame2 != null)
        {
            miniGame2.OnDragOverSlot(this, nearSlot);
        }

        // ----- MINIGAME 3 -----
        var miniGame3 = MiniGameController?.GetComponent<MiniGame3Scoring>();
        if (miniGame3 != null)
        {
            miniGame3.OnIsqueiroHover(true);
        }

        // ----- MINIGAME 4.1 -----
        var miniGame41 = MiniGameController?.GetComponent<MiniGame41Scoring>();
        if (miniGame41 != null)
        {
            miniGame41.OnDragOverSlot(this, nearSlot);
        }

        // ----- MINIGAME 5.1 -----
        var miniGame5 = MiniGameController?.GetComponent<MiniGame5Scoring>();
        if (miniGame5 != null)
        {
            miniGame5.OnDragOverSlot(this, nearSlot);
        }

        if (nearSlot != null)
        {
            nearSlot.HighlightSlot(true);

            if (miniGame3 != null)
            {
                miniGame3.OnSlotHover(nearSlot, true);
            }
        }
        else if (TargetSlots != null)
        {
            foreach (var slot in TargetSlots)
            {
                if (slot != null)
                {
                    slot.HighlightSlot(false);

                    if (miniGame3 != null)
                    {
                        miniGame3.OnSlotHover(slot, false);
                    }
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        var nearSlot = IsNearSlot();
        Debug.Log($"OnEndDrag - nearSlot: {(nearSlot != null ? nearSlot.name : "NONE")}");

        var miniGame1 = MiniGameController?.GetComponent<MiniGame1Scoring>();
        var miniGame2 = MiniGameController?.GetComponent<MiniGame2Scoring>();
        var miniGame3 = MiniGameController?.GetComponent<MiniGame3Scoring>();
        var miniGame41 = MiniGameController?.GetComponent<MiniGame41Scoring>();
        var miniGame5 = MiniGameController?.GetComponent<MiniGame5Scoring>();

        if (miniGame3 != null)
        {
            miniGame3.OnIsqueiroHover(false);
        }

        if (nearSlot)
        {
            nearSlot.HighlightSlot(false);

            // Remove hover do slot (minigame 3)
            if (miniGame3 != null)
            {
                miniGame3.OnSlotHover(nearSlot, false);
            }

            // Minigame 2
            if (miniGame2 != null && nearSlot == miniGame2.basketSlot)
            {
                nearSlot.lastDroppedObject = gameObject;
                if (MiniGameController != null && _itemHolder != null)
                {
                    MiniGameController.OnObjectDroppedInSlot(nearSlot, _itemHolder.Items);
                }
                return;
            }

            // Minigame 3
            if (miniGame3 != null)
            {
                miniGame3.OnSlotSelected(nearSlot);
                return;
            }

            // Minigame 4.1
            if (miniGame41 != null)
            {
                miniGame41.OnSlotDropped(nearSlot);
                return;
            }

            // Minigame 5.1
            if (miniGame5 != null)
            {
                miniGame5.OnSlotDropped(nearSlot);
                return;
            }

            // Minigame 1
            if (miniGame1 != null)
            {
                nearSlot.lastDroppedObject = gameObject; // Armazena o objeto no slot
                if (MiniGameController != null && _itemHolder != null)
                {
                    MiniGameController.OnObjectDroppedInSlot(nearSlot, _itemHolder.Items);
                }
                return; 
            }

            nearSlot.OnSuccessfulDrop();
            if (MiniGameController != null && _itemHolder != null)
            {
                MiniGameController.OnObjectDroppedInSlot(nearSlot, _itemHolder.Items);
            }
            else if (_itemHolder != null)
            {
                CharactersManager.Instance.ApplyPointsByTrait(_itemHolder.Items);
            }

            Destroy(gameObject);
        }
        else
        {
            if (currentSlot != null)
            {
                if (miniGame1 != null)
                {
                    miniGame1.OnItemRemovedFromSlot();
                }

                currentSlot.lastDroppedObject = null;
                currentSlot = null;
            }
        }
    }

    public SlotDraggable IsNearSlot()
    {
        if (TargetSlots == null || TargetSlots.Count == 0)
            return null;

        foreach (var slot in TargetSlots)
        {
            if (slot == null) continue;

            float dist = Vector2.Distance(transform.position, slot.transform.position);
            if (dist <= slot.acceptDistance)
                return slot;
        }
        return null;
    }
}