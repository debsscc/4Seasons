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
    // currentSlot agora representa o slot onde o objeto foi DROPPED (se houver).
    // Não vamos parentear o objeto ao slot — só guardamos a referência do slot aqui.
    private SlotDraggable currentSlot;

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

        // MiniGame2: notifica remoção da cesta se aplicável
        var miniGame2 = MiniGameController?.GetComponent<MiniGame2Scoring>();
        if (miniGame2 != null)
        {
            var drink = GetComponent<DrinksINFO>();
            if (drink != null && drink.isInBasket)
            {
                miniGame2.OnDrinkRemovedFromBasket(drink);
            }
        }

        // Se esse draggable estava marcado como vindo de um slot (currentSlot != null),
        // limpamos a referência naquele slot e notificamos MiniGame3 para resetar a UI.
        // Observação: currentSlot só será != null se ele tiver sido dropado antes em um slot.
        var miniGame3 = MiniGameController?.GetComponent<MiniGame3Scoring>();
        if (currentSlot != null)
        {
            // limpa a referência armazenada no slot (evita dangling refs)
            currentSlot.lastDroppedObject = null; // exige que SlotDraggable tenha public GameObject lastDroppedObject
            // notifica MiniGame3 para reverter UI (reaparecer isqueiro, esconder botão)
            miniGame3?.OnItemRemovedFromSlot();

            // não zera currentSlot aqui — zeraremos no OnEndDrag, após o drop final
            // (mas já limpamos a referência no slot para indicar que o objeto está sendo arrastado)
        }

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

        // Desliga hover do isqueiro caso esteja ativo
        if (miniGame3 != null)
        {
            miniGame3.OnIsqueiroHover(false);
        }

        if (nearSlot != null)
        {
            // Desativa highlight do slot
            nearSlot.HighlightSlot(false);

            // Remove hover do slot (minigame 3)
            if (miniGame3 != null)
            {
                miniGame3.OnSlotHover(nearSlot, false);
            }

            // --- Minigame 2: cesta ---
            if (miniGame2 != null && nearSlot == miniGame2.basketSlot)
            {
                // Armazena referência do objeto no slot (mas NÃO parentear nem mover o objeto).
                nearSlot.lastDroppedObject = gameObject;
                // Também marcamos localmente qual slot o objeto está (para poder ser removido depois)
                currentSlot = nearSlot;

                if (MiniGameController != null && _itemHolder != null)
                {
                    MiniGameController.OnObjectDroppedInSlot(nearSlot, _itemHolder.Items);
                }
                return;
            }

            // --- Minigame 3: bolso/aceita ---
            if (miniGame3 != null)
            {
                // Registrar referência no slot para permitir remoção futura (mas NÃO parentear)
                nearSlot.lastDroppedObject = gameObject;
                currentSlot = nearSlot; // guarda que este objeto está naquele slot agora

                // Notifica o minigame (vai exibir confirm button, esconder isqueiro se for reject, etc.)
                miniGame3.OnSlotSelected(nearSlot);
                return;
            }

            // --- Minigame 4.1 ---
            if (miniGame41 != null)
            {
                nearSlot.lastDroppedObject = gameObject;
                currentSlot = nearSlot;
                miniGame41.OnSlotDropped(nearSlot);
                return;
            }

            // --- Minigame 5.1 ---
            if (miniGame5 != null)
            {
                nearSlot.lastDroppedObject = gameObject;
                currentSlot = nearSlot;
                miniGame5.OnSlotDropped(nearSlot);
                return;
            }

            // --- Minigame 1 ---
            if (miniGame1 != null)
            {
                nearSlot.lastDroppedObject = gameObject; // Armazena o objeto no slot
                currentSlot = nearSlot;
                if (MiniGameController != null && _itemHolder != null)
                {
                    MiniGameController.OnObjectDroppedInSlot(nearSlot, _itemHolder.Items);
                }
                return; 
            }

            // Padrão para outros drops
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
                currentSlot = null;
            }
            else
            {
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