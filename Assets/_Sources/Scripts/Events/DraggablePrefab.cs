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

    public List<SlotDraggable> TargetSlots { get; set; } = null;
    public MiniGameController MiniGameController {get; set;}

    private IItemHolder _itemHolder;

    public event Action OnBeginDragEvent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        _itemHolder = GetComponent<IItemHolder>();

        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
            Debug.LogError("DraggablePrefab precisa estar dentro de um Canvas!");

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    // --- INTERFACE DE DRAG ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.95f;

        if (pickSfx) audioSource.PlayOneShot(pickSfx);

        transform.DOScale(Vector2.one * 2.1f, 0.25f);

        OnBeginDragEvent?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        transform.position = Mouse.current.position.ReadValue();
        
        var nearSlot = IsNearSlot();
        if (nearSlot)
        {
            nearSlot.HighlightSlot(true);
        }
        else
        {
            foreach (var slot in TargetSlots)
            {
                slot.HighlightSlot(false);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
{
    isDragging = false;
    canvasGroup.blocksRaycasts = true;
    canvasGroup.alpha = 1f;

    var nearSlot = IsNearSlot();
    if (nearSlot)
    {
        Debug.Log($"[DraggablePrefab] Slot detectado: {nearSlot.name}.");

        Destroy(gameObject);
        nearSlot.HighlightSlot(false);

        ItemsSO[] items = Array.Empty<ItemsSO>();
        if (_itemHolder != null)
        {
            items = _itemHolder.Items;
        }

        if (MiniGameController != null)
        {
            MiniGameController.OnObjectDroppedInSlot(nearSlot, items);
        }
        else
        {
            if (items.Length > 0)
            {
                CharactersManager.Instance.ApplyPointsByTrait(items);
            }

        }

        return;
    }
    else
    {
        Debug.Log($"[DraggablePrefab] Nenhum slot detectado perto de {gameObject.name}.");
    }

    transform.DOScale(Vector2.one, 0.25f);
}
    public SlotDraggable IsNearSlot()
{
    if (TargetSlots == null)
    {
        return null;
    }

    foreach (var slot in TargetSlots)
    {
        if (slot == null) continue;

        float dist = Vector2.Distance(transform.position, slot.transform.position);

        if (dist <= slot.acceptDistance)
        {
            return slot;
        }
    }
    return null;
}
}