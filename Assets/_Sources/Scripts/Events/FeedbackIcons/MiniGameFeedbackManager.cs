using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;



public enum FeedbackType { Neutral, Positive, Negative }

[Serializable]
public class NPCFeedbackUI
{
    [Tooltip("ID do personagem (ex: Ian, Arabella)")]
    public string characterId;

    [Tooltip("A imagem do ícone que vai trocar")]
    public Image iconImage;

    [Header("Sprites Específicas deste NPC")]
    public Sprite neutralSprite;
    public Sprite positiveSprite;
    public Sprite negativeSprite;
}

[Serializable]
public class CharacterFeedbackEntry
{
    public string characterId;
    public FeedbackType type;
}

[Serializable]
public class SlotFeedbackRule
{
    [Tooltip("ID do slot (ex: specialId do SlotDraggable)")]
    public int specialId;

    [Tooltip("Mudanças de feedback para cada NPC quando este slot for escolhido")]
    public List<CharacterFeedbackEntry> changes = new List<CharacterFeedbackEntry>();
}

public class MiniGameFeedbackManager : MonoBehaviour
{
    public static MiniGameFeedbackManager Instance;

    [Header("NPCs na Cena")]
    public List<NPCFeedbackUI> npcFeedbacks = new List<NPCFeedbackUI>();

    [Header("Animação")]
    public float animDuration = 0.2f;

    private Dictionary<string, NPCFeedbackUI> _feedbackLookup = new Dictionary<string, NPCFeedbackUI>();

    [ContextMenu("PrintRegisteredCharacters")]
    public void PrintRegisteredCharacters()
    {
        Debug.Log("[FeedbackManager] Registered characters:");
        foreach (var k in _feedbackLookup.Keys)
            Debug.Log($" - {k}");
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[FeedbackManager] Instância criada.");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("[FeedbackManager] Instância destruída.");
        }

        Debug.Log("[FeedbackManager] Construindo lookup de NPCs...");
        BuildLookup();
        // NÃO inicializamos sprites no Awake aqui caso os balões sejam instanciados depois.
        // Vamos inicializar no Start para ter mais chance de as refs já estarem prontas.
    }

    private void Start()
    {
        InitializeAllToNeutral();
    }

    private void InitializeAllToNeutral()
    {
        Debug.Log("[FeedbackManager] Inicializando todos os ícones para neutral...");
        foreach (var ui in npcFeedbacks)
        {
            if (ui == null)
            {
                Debug.LogWarning("[FeedbackManager] npcFeedbacks element null");
                continue;
            }

            if (ui.iconImage == null)
            {
                Debug.LogWarning($"[FeedbackManager] iconImage NÃO atribuído para '{ui.characterId}'");
                continue;
            }

            if (ui.neutralSprite == null)
                Debug.LogWarning($"[FeedbackManager] neutralSprite NÃO atribuído para '{ui.characterId}'");

            ui.iconImage.sprite = ui.neutralSprite;
            ui.iconImage.transform.localScale = Vector3.one;

            // se quiser esconder balões neutros, faça aqui (opcional)
            // if (ui.bubbleObject != null && hideNeutralBubbles) ui.bubbleObject.SetActive(false);
        }
    }

    private void BuildLookup()
    {
        Debug.Log("[FeedbackManager] Construindo dicionário de feedbacks...");
        _feedbackLookup.Clear();

        foreach (var fb in npcFeedbacks)
        {
            if (fb == null)
            {
                Debug.LogWarning("[FeedbackManager] entrada npcFeedbacks contém null - verifique Inspector");
                continue;
            }

            if (string.IsNullOrEmpty(fb.characterId))
            {
                Debug.LogWarning("[FeedbackManager] NPCFeedbackUI com characterId vazio (verifique Inspector)", this);
                continue;
            }

            if (_feedbackLookup.ContainsKey(fb.characterId))
            {
                Debug.LogWarning($"[FeedbackManager] characterId duplicado: {fb.characterId}", this);
                continue;
            }

            _feedbackLookup[fb.characterId] = fb;
            Debug.Log($"[FeedbackManager] Adicionado NPC '{fb.characterId}' ao lookup.");
        }



    }
    public void UpdatePreview(string characterId, FeedbackType type)
    {
        Debug.Log($"[FeedbackManager] Atualizando preview do NPC '{characterId}' para '{type}'.");
        if (!_feedbackLookup.TryGetValue(characterId, out var ui))
        {
            Debug.LogWarning($"[FeedbackManager] NPC '{characterId}' não configurado!");
            return;
        }

        Sprite targetSprite = type switch
        {
            FeedbackType.Positive => ui.positiveSprite,
            FeedbackType.Negative => ui.negativeSprite,
            _ => ui.neutralSprite
        };

        if (ui.iconImage.sprite == targetSprite) return;

        ui.iconImage.sprite = targetSprite;

        // Animação de "mudança"
        ui.iconImage.transform.DOKill();
        ui.iconImage.transform.localScale = Vector3.one;
        ui.iconImage.transform.DOPunchScale(Vector3.one * 0.2f, animDuration);
    }

    public List<UICharacterOrder> uiCharacterOrders = new();

    public void ApplyPreview(ItemsSO[] items)
    {
        // Debug.Log("[FeedbackManager] Aplicando preview múltiplo...");
        // foreach (var kv in statesByCharacterId)
        // {
        //     Debug.Log($"[FeedbackManager] Aplicando preview para '{kv.Key}' como '{kv.Value}'.");
        //     UpdatePreview(kv.Key, kv.Value);
        // }

        foreach (var ui in uiCharacterOrders)
        {
            foreach(var item in items)
            {
                ui.UpdateExpresionBasedOnItem(item);
                if (ui.CharacterLikesItem(item)) break;
            }
        }
    }

    public void ApplySlotRule(SlotFeedbackRule rule)
    {
        if (rule == null || rule.changes == null) return;

        foreach (var change in rule.changes)
        {
            Debug.Log($"[FeedbackManager] Aplicando mudança para '{change.characterId}' como '{change.type}'.");
            UpdatePreview(change.characterId, change.type);
        }
    }

    public void ResetAll()
    {
        Debug.Log("[FeedbackManager] Resetando todos os feedbacks para neutro...");
        foreach (var ui in npcFeedbacks)
        {
            if (ui.iconImage != null)
                ui.iconImage.sprite = ui.neutralSprite;
        }
    }
}