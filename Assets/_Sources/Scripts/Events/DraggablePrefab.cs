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

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        // Movimento usando o PointerEventData (funciona com input antigo e novo)
        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.WorldSpace)
        {
            // Canvas em Screen Space (Overlay ou Camera)
            rectTransform.position = eventData.position;
        }
        else
        {
            // World Space ou fallback
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

        // Tenta achar o MiniGame3Scoring, se existir
        var miniGame3 = MiniGameController != null
            ? MiniGameController.GetComponent<MiniGame3Scoring>()
            : null;

        // Hover do isqueiro (só para minigame 3)
        if (miniGame3 != null)
        {
            miniGame3.OnIsqueiroHover(true);
        }

        var nearSlot = IsNearSlot();
        if (nearSlot != null)
        {
            nearSlot.HighlightSlot(true);

            // Hover do slot (só para minigame 3)
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

                    // Remove hover dos slots (só para minigame 3)
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
        canvasGroup.alpha = 1f;

        var nearSlot = IsNearSlot();
        Debug.Log($"[DraggablePrefab] OnEndDrag - nearSlot: {(nearSlot != null ? nearSlot.name : "NENHUM")}");

        var miniGame3 = MiniGameController != null
            ? MiniGameController.GetComponent<MiniGame3Scoring>()
            : null;

        // Remove hover do isqueiro (só minigame 3)
        if (miniGame3 != null)
        {
            miniGame3.OnIsqueiroHover(false);
        }

        if (nearSlot)
        {
            nearSlot.HighlightSlot(false);

            // Remove hover do slot (só minigame 3)
            if (miniGame3 != null)
            {
                miniGame3.OnSlotHover(nearSlot, false);
            }

            // CASO ESPECIAL: Minigame 3
            if (miniGame3 != null)
            {
                // Deixa o fluxo visual/pontuação/confirmar na mão do MiniGame3Scoring
                miniGame3.OnSlotSelected(nearSlot);
                return; // NÃO destrói o draggable aqui
            }

            // CASO PADRÃO: qualquer outro minigame
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
            return;
        }

        transform.DOScale(Vector2.one, 0.25f);
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