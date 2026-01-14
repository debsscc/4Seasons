using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class SlotDraggable : MonoBehaviour, IPointerClickHandler
{
    public event Action<SlotDraggable> OnObjectRemovedFromSlot;
    
    [Header("Visual")]
    public Image outlineImage;

    [Header("MiniGames Data")]
    public Outline npcOutline;              // Usado em MiniGame 5
    public CharacterData associatedCharacter; // Usado em MiniGame 4 e 5
    public int specialId;                    // Usado em MiniGame 3, 4 e 5

    [Header("Audio")]
    public AudioClip successSfx;
    public float acceptDistance = 80f;
    
    public GameObject lastDroppedObject;
    
    private AudioSource audioSource;
    private void Awake()
    {
        InitializeComponents();
        HideOutline();
    }
        private void InitializeComponents()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    private void HideOutline()
    {
        if (outlineImage)
            outlineImage.enabled = false;
    }
        public void OnSuccessfulDrop(GameObject droppedObject)
    {
        lastDroppedObject = droppedObject;

        PlaySuccessSound();
        FlashOutlineEffect();
    }
    public void HighlightSlot(bool highlight)
    {
        Vector2 targetScale = highlight ? Vector2.one * 1.3f : Vector2.one;
        transform.DOScale(targetScale, 0.25f);
    }

    public void ClearSlot()
{
    if (lastDroppedObject != null)
    {
        lastDroppedObject = null;
        OnObjectRemovedFromSlot?.Invoke(this);
    }
}
public void OnPointerClick(PointerEventData eventData)
{
    if (lastDroppedObject != null)
    {
        var draggable = lastDroppedObject.GetComponent<DraggablePrefab>();
        if (draggable != null)
        {
            draggable.ReleaseSlot();
            draggable.ResetPosition();
        }

        OnObjectRemovedFromSlot?.Invoke(this);
    }
}

    private void PlaySuccessSound()
    {
        if (successSfx)
            audioSource.PlayOneShot(successSfx);
    }

    private void FlashOutlineEffect()
    {
        if (outlineImage)
            StartCoroutine(FlashOutline());
    }

    private System.Collections.IEnumerator FlashOutline()
    {
        outlineImage.enabled = true;
        yield return new WaitForSeconds(0.35f);
        outlineImage.enabled = false;
    }
    
}