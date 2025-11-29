using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DG.Tweening;

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

    public SlotDraggable TargetSlot { get; set; } = null;

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

        if (IsNearSlot())
        {
            TargetSlot.HighlightSlot(true);
        }
        else
        {
            TargetSlot.HighlightSlot(false);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (IsNearSlot())
        {
            Destroy(gameObject);
            TargetSlot.HighlightSlot(false);
            CharactersManager.Instance.ApplyPointsByTrait(_itemHolder.Items);
            return;
        }

        transform.DOScale(Vector2.one, 0.25f);
    }

    public bool IsNearSlot()
    {
        // transformar o centro do slot para coordenada anc
        float dist = Vector2.Distance(transform.position, TargetSlot.transform.position);
        return dist <= TargetSlot.acceptDistance;
    }
}