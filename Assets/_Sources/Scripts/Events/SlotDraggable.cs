using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class SlotDraggable : MonoBehaviour, IPointerClickHandler
{
    public event Action<SlotDraggable> OnObjectRemovedFromSlot;
    
    [Header("Visual")]
    public Image outlineImage;

    [Header("Close Animation")]
    public Animator slotAnimator;
    public string closeAnimationTrigger = "Close";
    public float closeAnimationDuration = 0.6f;

    [Header("MiniGames Data")]
    public UIOutline npcOutline;
    public CharacterData associatedCharacter;
    public int specialId;

    [Header("Audio")]
    public AudioClip successSfx;
    public float acceptDistance = 80f;
    
    public GameObject lastDroppedObject;
    
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        if (outlineImage) outlineImage.enabled = false;
    }

    public void OnSuccessfulDrop(GameObject droppedObject)
    {
        lastDroppedObject = droppedObject;
        PlaySuccessSound();
        FlashOutlineEffect();
    }

    public void HighlightSlot(bool highlight)
    {
        if (npcOutline == null) return;
        npcOutline.enabled = true;
        Color c = npcOutline.effectColor;
        c.a = highlight ? 1f : 0f;
        npcOutline.effectColor = c;
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

    public void PlayCloseAnimation(Action onComplete)
    {
        if (slotAnimator != null)
        {
            slotAnimator.SetTrigger(closeAnimationTrigger);
            StartCoroutine(WaitThenCallback(closeAnimationDuration, onComplete));
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    private IEnumerator WaitThenCallback(float duration, Action onComplete)
    {
        yield return new WaitForSeconds(duration);
        onComplete?.Invoke();
    }

    private void PlaySuccessSound()
    {
        if (successSfx) audioSource.PlayOneShot(successSfx);
    }

    private void FlashOutlineEffect()
    {
        if (outlineImage) StartCoroutine(FlashOutline());
    }

    private IEnumerator FlashOutline()
    {
        outlineImage.enabled = true;
        yield return new WaitForSeconds(0.35f);
        outlineImage.enabled = false;
    }
}
