using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MiniGame3Scoring : MonoBehaviour, IMiniGameScoring, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Regras de Preview por Slot")]
    public List<SlotFeedbackRule> previewRules = new List<SlotFeedbackRule>();

    [Header("IDs dos Slots")]
    public int acceptId = 1;    
    public int rejectId = 2; 
    
    [Header("Visual")]
    public Color hoverOutlineColor = Color.white;
    private Outline outline;


    [Header("Objetos da Cena")]
    public GameObject isqueiroObject;

    [Tooltip("Outline do ícone das drogas (mesmo GameObject da imagem, mas componente Outline)")]
    public Outline drogasOutline;
    [Tooltip("Outline do ícone do bolso")]
    public Outline bolsoOutline;
    [Tooltip("Outline do ícone do isqueiro")]
    public Outline isqueiroOutline;

    [Header("Modals Feedbacks")]
    [Tooltip("Modal Feedback Bolso")]
    public GameObject feedbackBolso;
    [Tooltip("Modal Feedback Drogas")]
    public GameObject feedbackDrogas;

    [Header("Pontuação")]
    public int npcPointsOnAccept = 3;
    public int selfPointsOnAccept = 3;

    [Header("Personagens do Evento")]
    public CharacterData npcDoEvento; 
    public CharacterData playerCharacter;

    [Header("UI de Confirmação")]
    public GameObject confirmButton;

    [Header("Configuração")]
    public float feedbackDuration = 3f;

    private SlotDraggable selectedSlot;
    private bool isConfirmed = false;

    private void Awake()
    {
        outline = GetComponent<Outline>();

        if (outline)
        {
            outline.effectColor = hoverOutlineColor;
            Color startColor = outline.effectColor;
            startColor.a = 0f;
            outline.effectColor = startColor;
        }
    }

    private void Start()
    {
        if (confirmButton) confirmButton.SetActive(false);
        if (feedbackBolso) feedbackBolso.SetActive(false);
        if (feedbackDrogas) feedbackDrogas.SetActive(false);
        
        // Outlines começam desativados
        SetOutlineEnabled(drogasOutline, false);
        SetOutlineEnabled(bolsoOutline, false);
        SetOutlineEnabled(isqueiroOutline, false);
    }

    public void OnSlotHover(SlotDraggable slot, bool isHovering)
    {
        if (isConfirmed || slot == null) return;

        Outline targetOutline = GetOutlineForSlot(slot);
        if (targetOutline != null)
        {
            SetOutlineEnabled(targetOutline, isHovering);
        }
    }

    public void OnIsqueiroHover(bool isHovering)
    {
        if (isConfirmed) return;
        SetOutlineEnabled(isqueiroOutline, isHovering);
    }

    public void OnSlotSelected(SlotDraggable slot)
    {
        selectedSlot = slot;
        if (confirmButton) confirmButton.SetActive(true);

        //Icons Feedback:
        if (npcDoEvento != null)
    {
        string id = npcDoEvento.name; 

        // Busca a regra correspondente ao slot
        var rule = previewRules.Find(r => r.specialId == slot.specialId);
        if (rule != null)
        {
            MiniGameFeedbackManager.Instance.ApplySlotRule(rule);
        }
    }

    }
    public void OnItemRemovedFromSlot()
    {
        if (selectedSlot != null)
        {
            if (selectedSlot.specialId == rejectId && isqueiroObject != null)
            {
                isqueiroObject.SetActive(true);
                isqueiroObject.transform.localScale = Vector3.zero;
                isqueiroObject.transform.DOScale(Vector3.one, 0.2f);
            }
        }

        selectedSlot = null;                // limpa a seleção atual
        isConfirmed = false;                
        if (confirmButton) confirmButton.SetActive(false); // esconder botão de confirmar até nova seleção

        SetOutlineEnabled(drogasOutline, false);
        SetOutlineEnabled(bolsoOutline, false);
        SetOutlineEnabled(isqueiroOutline, false);

        if (feedbackBolso) feedbackBolso.SetActive(false);
        if (feedbackDrogas) feedbackDrogas.SetActive(false);

        Debug.Log("[MiniGame3] Item removido do slot — estado resetado para permitir nova escolha.");

        //Limpar FeedbackIcon
        if (npcDoEvento != null)
        {
            MiniGameFeedbackManager.Instance.ResetAll();    
        }
    }

    public void OnConfirmButtonClicked()
    {
        if (selectedSlot == null) return;
        
        isConfirmed = true;
        if (confirmButton) confirmButton.SetActive(false);
        
        OnObjectDropped(selectedSlot, System.Array.Empty<ItemsSO>());
        StartCoroutine(ShowFeedbackSequence(selectedSlot.specialId));
    }

    public void OnObjectDropped(SlotDraggable slot, ItemsSO[] items)
    {        
        if (slot == null)
        {
            Debug.LogWarning("[MiniGame3] Slot é nulo!");
            return;
        }

        Debug.Log($"[MiniGame3] Slot recebido: {slot.name}, specialId = {slot.specialId}");

        if (slot.specialId == acceptId)
        {
            Debug.Log("[MiniGame3] ACEITOU a maconha!");
            
            if (npcDoEvento != null)
            {
                int scoreAntes = npcDoEvento.RelationshipScore;
                npcDoEvento.RelationshipScore += npcPointsOnAccept;
                Debug.Log($"[MiniGame3] {npcDoEvento.name}: {scoreAntes} → {npcDoEvento.RelationshipScore} (+{npcPointsOnAccept})");
            }
            else
            {
                Debug.LogWarning("[MiniGame3] npcDoEvento não foi atribuído no Inspector!");
            }

            if (playerCharacter != null)
            {
                int scoreAntes = playerCharacter.RelationshipScore;
                playerCharacter.RelationshipScore += selfPointsOnAccept;
                Debug.Log($"[MiniGame3] Player: {scoreAntes} → {playerCharacter.RelationshipScore} (+{selfPointsOnAccept})");
            }
            else
            {
            }
        }
        else if (slot.specialId == rejectId)
        {
            Debug.Log("[MiniGame3] RECUSOU a maconha. Nenhum ponto aplicado.");
        }
        else
        {
            Debug.Log($"[MiniGame3] specialId={slot.specialId} não reconhecido. Usando lógica padrão.");
            CharactersManager.Instance.ApplyPointsByTrait(items);
        }
    }

    private IEnumerator ShowFeedbackSequence(int specialId)
    {
        yield return new WaitForSeconds(feedbackDuration);
        
        if (specialId == acceptId)
        {
            if (feedbackDrogas) feedbackDrogas.SetActive(true);
        }
        else if (specialId == rejectId)
        {
            if (feedbackBolso) feedbackBolso.SetActive(true);
        }
    }

    // Helpers
    private Outline GetOutlineForSlot(SlotDraggable slot)
    {
        if (slot.specialId == acceptId) return drogasOutline;
        if (slot.specialId == rejectId) return bolsoOutline;
        return null;
    }

    private void SetOutlineEnabled(Outline outline, bool enabled)
    {
        if (outline == null) return;
        outline.enabled = enabled;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isConfirmed) return;
        ShowOutline(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isConfirmed) return;
        ShowOutline(false);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isConfirmed) return;
    }

    private void ShowOutline(bool show)
    {
        if (outline == null) return;

        Color targetColor = hoverOutlineColor;
        targetColor.a = show ? 1f : 0f;

        DOTween.To(() => outline.effectColor, x => outline.effectColor = x, targetColor, 0.2f);
    }
}