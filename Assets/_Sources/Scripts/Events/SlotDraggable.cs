using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;

public class SlotDraggable : MonoBehaviour
{
    public Image outlineImage;    
    public AudioClip successSfx;
    public float acceptDistance = 200f;
    private AudioSource audioSource;

    [HideInInspector]
    public GameObject lastDroppedObject;

    [Header("Visual Feedback (Minigame 5)")]
    [Tooltip("Outline do NPC (hover e seleção)")]
    public UnityEngine.UI.Outline npcOutline;

    [Header("Dados opcionais")]
    public int specialId;
    public CharacterData associatedCharacter;
    
    // Referência para o botão de remoção (opcional)
    public GameObject removeButton;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        if (outlineImage) outlineImage.enabled = false;
        
        // Inicializa o botão de remoção como invisível
        if (removeButton != null)
        {
            removeButton.SetActive(false);
        }
    }
    
    public void OnSuccessfulDrop(GameObject droppedObject)
    {
        lastDroppedObject = droppedObject;
        OnSuccessfulDrop();
        
        // Mostra o botão de remoção se houver um objeto
        if (removeButton != null && droppedObject != null)
        {
            removeButton.SetActive(true);
        }
    }

    public void OnSuccessfulDrop()
    {
        Debug.Log($"[SlotDraggable] OnSuccessfulDrop em '{name}'");
        if (successSfx) audioSource.PlayOneShot(successSfx);
        if (outlineImage) StartCoroutine(FlashOutline());
    }

    System.Collections.IEnumerator FlashOutline()
    {
        if (outlineImage == null) yield break;
        outlineImage.enabled = true;
        yield return new WaitForSeconds(0.35f); //GameDesigner pode mudar
        outlineImage.enabled = false;
    }

    public void HighlightSlot(bool highlight)
    {
        Vector2 scale = highlight ? Vector2.one * 1.3f : Vector2.one;
        transform.DOScale(scale, 0.25f);
    }
    
    public void ClearSlot()
    {
        lastDroppedObject = null;
    }

    // Método para remover o objeto do slot (mais intuitivo)
    public void RemoveObjectFromSlot()
    {
        if (lastDroppedObject != null)
        {
            var draggable = lastDroppedObject.GetComponent<DraggablePrefab>();
            if (draggable != null)
            {
                draggable.ResetPosition();
            }
            else
            {
                Destroy(lastDroppedObject);
            }
            
            lastDroppedObject = null;
            
            // Esconde o botão de remoção
            if (removeButton != null)
            {
                removeButton.SetActive(false);
            }
            
            // Notifica o MiniGame1 se existir
            var miniGame1 = FindObjectOfType<MiniGame1Scoring>();
            if (miniGame1 != null)
            {
                miniGame1.OnItemRemovedFromSlot();
            }
        }
    }
}