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
    [Header("Dados opcionais p/ minigames específicos")]
    public int specialId;
    public CharacterData associatedCharacter;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        if (outlineImage) outlineImage.enabled = false;
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
}
