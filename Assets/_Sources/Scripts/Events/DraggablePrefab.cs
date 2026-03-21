using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class DraggablePrefab : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    #region Serialized Fields
    
    [Title("Audio")]
    public AudioClip pickSfx;
    public AudioClip dropSfx;
    
    [HideInInspector] public RectTransform rectTransform;
    public List<SlotDraggable> TargetSlots { get; set; } = null;
    public MiniGameController MiniGameController { get; set; }
    public event Action OnBeginDragEvent;
    public event Action OnEndDragEvent;

    private CanvasGroup canvasGroup;
    private Canvas parentCanvas;
    private AudioSource audioSource;
    private IItemHolder _itemHolder;
    private UIOutline _draggableOutline;

    private bool isDragging;
    private SlotDraggable currentSlot;
    private Vector2 initialAnchoredPosition;
    public Vector2 InitialAnchoredPosition => initialAnchoredPosition;
    
    public ItemsSO[] Items => _itemHolder != null ? _itemHolder.Items : null;
    
    private void Awake()
    {
        InitializeComponents();
        ValidateCanvas();
        StoreInitialPosition();
    }

    private void InitializeComponents()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        _itemHolder = GetComponent<IItemHolder>();
        parentCanvas = GetComponentInParent<Canvas>();
        audioSource = gameObject.AddComponent<AudioSource>();
        _draggableOutline = GetComponent<UIOutline>();
        if (_draggableOutline != null)
        {
            _draggableOutline.enabled = true;
            Color c = _draggableOutline.effectColor;
            c.a = 0f;
            _draggableOutline.effectColor = c;
        }
    }

    private void ValidateCanvas()
    {
        if (parentCanvas == null)
            Debug.LogError($"[DraggablePrefab] {name} precisa estar dentro de um Canvas!");
    }

    private void StoreInitialPosition()
    {
        initialAnchoredPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        StartDrag();
        PlayPickSound();
        NotifyMiniGamesOnBeginDrag();
        OnBeginDragEvent?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        UpdateDragPosition(eventData);
        HandleSlotHighlighting();
        NotifyMiniGamesOnDrag();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDrag();
        
        var nearSlot = IsNearSlot();
        Debug.Log($"[DraggablePrefab] OnEndDrag - nearSlot: {(nearSlot != null ? nearSlot.name : "NONE")}");

        OnEndDragEvent?.Invoke();
        NotifyMiniGamesOnEndDrag();

        if (nearSlot != null)
        {
            Debug.Log($"[DraggablePrefab] Dropped on slot: {nearSlot.name}");
            HandleDropOnSlot(nearSlot);
        }
        else
        {
            Debug.Log("[DraggablePrefab] Dropped outside any slot.");
            HandleDropOutsideSlot();
        }
    }
    
    private void StartDrag()
    {
        isDragging = true;
        canvasGroup.blocksRaycasts = false;
        SetDraggableOutline(true);
    }

    private void EndDrag()
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
    }

    private void PlayPickSound()
    {
        if (pickSfx)
            audioSource.PlayOneShot(pickSfx);
    }

    private void UpdateDragPosition(PointerEventData eventData)
    {
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
    }

    private void HandleSlotHighlighting()
    {
        var nearSlot = IsNearSlot();

        if (nearSlot != null)
        {
            SetDraggableOutline(false);
            nearSlot.HighlightSlot(true);
            NotifyMiniGame3OnSlotHover(nearSlot, true);
        }
        else
        {
            SetDraggableOutline(true);
            if (TargetSlots != null)
            {
                foreach (var slot in TargetSlots)
                {
                    if (slot != null)
                    {
                        slot.HighlightSlot(false);
                        NotifyMiniGame3OnSlotHover(slot, false);
                    }
                }
            }
        }
    }
    
    private void HandleDropOnSlot(SlotDraggable nearSlot)
    {
        bool slotOcupado = nearSlot.lastDroppedObject != null;

        nearSlot.HighlightSlot(false);
        NotifyMiniGame3OnSlotHover(nearSlot, false);

        if (TryDropInMiniGame2(nearSlot, slotOcupado)) return;
        if (TryDropInMiniGame3(nearSlot, slotOcupado)) return;
        if (TryDropInMiniGame41(nearSlot, slotOcupado)) return;
        if (TryDropInMiniGame5(nearSlot, slotOcupado)) return;
        if (TryDropInMiniGame1(nearSlot, slotOcupado)) return;

        HandleDefaultDrop(nearSlot, slotOcupado);
    }

    private void HandleDropOutsideSlot()
    {
        SetDraggableOutline(false);
        if (currentSlot != null)
        {
            currentSlot.lastDroppedObject = null;
            currentSlot = null;
        }
    }

    private void HandleDefaultDrop(SlotDraggable nearSlot, bool slotOcupado)
    {
        nearSlot.OnSuccessfulDrop(gameObject);

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
    
    private bool TryDropInMiniGame1(SlotDraggable nearSlot, bool slotOcupado)
    {
        var miniGame1 = MiniGameController?.GetComponent<MiniGame1Scoring>();
        if (miniGame1 == null) return false;

        if (slotOcupado)
        {
            SetDraggableOutline(false);
            ResetPosition();
            return true;
        }

        nearSlot.lastDroppedObject = gameObject;
        currentSlot = nearSlot;
        SetDraggableOutline(true);

        if (MiniGameController != null && _itemHolder != null)
        {
            MiniGameController.OnObjectDroppedInSlot(nearSlot, _itemHolder.Items);
        }

        return true;
    }

    private bool TryDropInMiniGame2(SlotDraggable nearSlot, bool slotOcupado)
    {
        var miniGame2 = MiniGameController?.GetComponent<MiniGame2Scoring>();
        if (miniGame2 == null || nearSlot != miniGame2.basketSlot) return false;

        nearSlot.lastDroppedObject = gameObject;
        currentSlot = nearSlot;

        if (MiniGameController != null && _itemHolder != null)
        {
            miniGame2.OnDroppedInBasket(this, _itemHolder.Items);
            MiniGameController.OnObjectDroppedInSlot(nearSlot, _itemHolder.Items);
        }

        return true;
    }

    private bool TryDropInMiniGame3(SlotDraggable nearSlot, bool slotOcupado)
    {
        var miniGame3 = MiniGameController?.GetComponent<MiniGame3Scoring>();
        if (miniGame3 == null) return false;

        if (slotOcupado)
        {
            ResetPosition();
            return true;
        }

        nearSlot.lastDroppedObject = gameObject;
        currentSlot = nearSlot;

        miniGame3.OnSlotSelected(nearSlot);
        return true;
    }

    private bool TryDropInMiniGame41(SlotDraggable nearSlot, bool slotOcupado)
    {
        var miniGame41 = MiniGameController?.GetComponent<MiniGame41Scoring>();
        if (miniGame41 == null) return false;

        if (slotOcupado)
        {
            ResetPosition();
            return true;
        }

        nearSlot.lastDroppedObject = gameObject;
        currentSlot = nearSlot;
        miniGame41.OnSlotDropped(nearSlot);
        return true;
    }

    private bool TryDropInMiniGame5(SlotDraggable nearSlot, bool slotOcupado)
    {
        var miniGame5 = MiniGameController?.GetComponent<MiniGame5Scoring>();
        if (miniGame5 == null) return false;

        if (slotOcupado)
        {
            ResetPosition();
            return true;
        }

        nearSlot.lastDroppedObject = gameObject;
        currentSlot = nearSlot;
        miniGame5.OnSlotDropped(nearSlot);
        return true;
    }
    
    private void NotifyMiniGamesOnBeginDrag()
    {
        var miniGame2 = MiniGameController?.GetComponent<MiniGame2Scoring>();
        if (miniGame2 != null)
        {
            var drink = GetComponent<DrinksINFO>();
            if (drink != null && drink.isInBasket)
            {
                miniGame2.OnDrinkRemovedFromBasket(drink);
            }
        }

        var miniGame3 = MiniGameController?.GetComponent<MiniGame3Scoring>();
        if (currentSlot != null)
        {
            currentSlot.lastDroppedObject = null;
            miniGame3?.OnItemRemovedFromSlot();
        }
    }

    private void NotifyMiniGamesOnDrag()
    {
        var nearSlot = IsNearSlot();

        var miniGame2 = MiniGameController?.GetComponent<MiniGame2Scoring>();
        if (miniGame2 != null && nearSlot == miniGame2.basketSlot && _itemHolder != null)
            miniGame2.OnDragOverBasket(_itemHolder.Items);

        var miniGame3 = MiniGameController?.GetComponent<MiniGame3Scoring>();
        miniGame3?.OnIsqueiroHover(true);

        var miniGame41 = MiniGameController?.GetComponent<MiniGame41Scoring>();
        miniGame41?.OnDragOverSlot(this, nearSlot);

        var miniGame5 = MiniGameController?.GetComponent<MiniGame5Scoring>();
        miniGame5?.OnDragOverSlot(this, nearSlot);
    }

    private void NotifyMiniGamesOnEndDrag()
    {
        var miniGame3 = MiniGameController?.GetComponent<MiniGame3Scoring>();
        miniGame3?.OnIsqueiroHover(false);
    }

    private void NotifyMiniGame3OnSlotHover(SlotDraggable slot, bool isHovering)
    {
        var miniGame3 = MiniGameController?.GetComponent<MiniGame3Scoring>();
        miniGame3?.OnSlotHover(slot, isHovering);
    }
    
    public void ResetPosition()
    {
        SetDraggableOutline(false);
        rectTransform.DOAnchorPos(initialAnchoredPosition, 0.5f);

        if (currentSlot != null)
        {
            currentSlot.lastDroppedObject = null;
            currentSlot = null;
        }
    }

    public void ReleaseSlot()
    {
        if (currentSlot != null)
        {
            currentSlot.lastDroppedObject = null;
            currentSlot = null;
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

    private void SetDraggableOutline(bool show)
    {
        if (_draggableOutline == null) return;
        Color c = _draggableOutline.effectColor;
        c.a = show ? 1f : 0f;
        _draggableOutline.effectColor = c;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[DraggablePrefab] OnPointerClick - {name} clicked.");
        if (MiniGameController != null && GetComponent<IgnoreClickReset>() == null)
        {
            ResetPosition();
            PlayPickSound();
            NotifyMiniGamesOnBeginDrag();
            MiniGameController.OnDVDRemoved(this);
        }
    }
    
    #endregion
}
