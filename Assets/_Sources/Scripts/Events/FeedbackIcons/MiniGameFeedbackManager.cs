using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum FeedbackType { Neutral=0, Positive=1, Negative=-1 }

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

    [Header("Heart Feedback (mostrado ao confirmar)")]
    [Tooltip("Imagem do coração que aparecerá ao confirmar")]
    public Image heartImage;

    [Tooltip("Sprite do coração positivo")]
    public Sprite positiveHeartSprite;

    [Tooltip("Sprite do coração negativo")]
    public Sprite negativeHeartSprite;
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
    public float animDuration = 0.3f;

    [Header("Heart Feedback")]
    public float heartDisplayDuration = 3f;
    [Tooltip("Marque em cenas de minigame. Desmarcado (padrão): o diálogo pausa 3s após escolha para mostrar os corações.")]
    public bool isMinigame = false;

    private Dictionary<string, NPCFeedbackUI> _feedbackLookup = new Dictionary<string, NPCFeedbackUI>();
    private Dictionary<string, Tween> _heartTweens = new Dictionary<string, Tween>();
    private Dictionary<string, Vector3> _heartOriginalScales = new Dictionary<string, Vector3>();

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
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("[FeedbackManager] Instância destruída.");
        }

        BuildLookup();
        CacheHeartScales();
        // NÃO inicializamos sprites no Awake aqui caso os balões sejam instanciados depois.
        // Vamos inicializar no Start para ter mais chance de as refs já estarem prontas.
    }

    private void Start()
    {
        InitializeAllToNeutral();
    }

    private void InitializeAllToNeutral()
    {
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

            if (ui.heartImage != null)
            {
                ui.heartImage.enabled = false;
                ui.heartImage.sprite = null;
            }

            // se quiser esconder balões neutros, faça aqui (opcional)
            // if (ui.bubbleObject != null && hideNeutralBubbles) ui.bubbleObject.SetActive(false);
        }
    }

    private void CacheHeartScales()
    {
        foreach (var ui in npcFeedbacks)
        {
            if (ui == null || string.IsNullOrEmpty(ui.characterId) || ui.heartImage == null) continue;
            _heartOriginalScales[ui.characterId] = ui.heartImage.transform.localScale;
        }
    }

    private void BuildLookup()
    {
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
        }



    }

    public void UpdatePreviewTemp(CharacterData character, int expressionID) 
    {
        EnsureUICharacterOrdersDiscovered();
        foreach (var ui in uiCharacterOrders)
        {
            if (ui == null) continue;
            bool match = ui.Character != null &&
                         (ui.Character == character || ui.Character.name == character.name);

            if (!match)
            {
                var identity = ui.GetComponent<CharacterIdentity>();
                if (identity != null)
                    match = string.Equals(identity.characterId, character.name, System.StringComparison.OrdinalIgnoreCase);
            }

            if (match)
            {
                ui.UpdateExpressionBasedOnCharacter(expressionID);
                break;
            }
        }
    }

    public void UpdatePreviewTemp(string characterId, int expressionID)
    {
        EnsureUICharacterOrdersDiscovered();
        foreach (var ui in uiCharacterOrders)
        {
            if (ui == null) continue;
            var identity = ui.GetComponent<CharacterIdentity>();
            if (identity == null) continue;
            if (string.Equals(identity.characterId, characterId, System.StringComparison.OrdinalIgnoreCase))
            {
                ui.UpdateExpressionBasedOnCharacter(expressionID);
                break;
            }
        }
    }

    public void UpdatePreview(string characterId, FeedbackType type)
    {
        if (!_feedbackLookup.TryGetValue(characterId, out var ui))
        {
            Debug.LogWarning($"[FeedbackManager] NPC '{characterId}' não configurado!");
            return;
        }

        // Determina o sprite alvo com base no tipo de feedback
        Sprite targetSprite = type switch
        {
            FeedbackType.Positive => ui.positiveSprite,
            FeedbackType.Negative => ui.negativeSprite,
            _ => ui.neutralSprite
        };
        // Se o sprite já estiver correto, não faz nada
        if (ui.iconImage.sprite == targetSprite) return;
        // Atualiza o sprite do ícone
        ui.iconImage.sprite = targetSprite;

        // Animação de "mudança"
        ui.iconImage.transform.DOKill();
        ui.iconImage.transform.localScale = Vector3.one;
        ui.iconImage.transform.DOPunchScale(Vector3.one * 0.2f, animDuration);
    }

    public List<UICharacterOrder> uiCharacterOrders = new();

    private void EnsureUICharacterOrdersDiscovered()
    {
        // Remove referências nulas (caso algum UICharacterOrder tenha sido destruído)
        uiCharacterOrders.RemoveAll(x => x == null);
        if (uiCharacterOrders.Count > 0) return;

        var found = FindObjectsByType<UICharacterOrder>(FindObjectsSortMode.None);
        if (found == null || found.Length == 0) return;

        uiCharacterOrders.Clear();
        uiCharacterOrders.AddRange(found);
    }

    public void ApplyPreview(ItemsSO[] items) { }

    public void ApplyConfirmedReactions(ItemsSO[] items)
    {
        // Aplica as reações de coração baseadas nos itens confirmados para cada NPC
        EnsureUICharacterOrdersDiscovered();

        if (items == null || items.Length == 0) return;

        foreach (var ui in uiCharacterOrders)
        {
            if (ui == null) continue;
            bool liked = false;
            foreach (var item in items)
            {
                ui.UpdateExpresionBasedOnItem(item);
                if (ui.CharacterLikesItem(item)) { liked = true; break; }
            }
            if (liked) ui.PunchScale();
        }
    }

    public void ApplySlotRule(SlotFeedbackRule rule)
    {
        // Aplica as mudanças de feedback definidas na regra para os NPCs correspondentes
        if (rule == null || rule.changes == null) return;

        foreach (var change in rule.changes)
        {
            UpdatePreview(change.characterId, change.type);
        }
    }

    public void ResetAll()
    {

        // Para garantir que não fiquem tweens pendentes exibindo hearts depois do reset
        foreach (var tween in _heartTweens.Values)
            tween.Kill();
        _heartTweens.Clear();

        foreach (var ui in npcFeedbacks)
        {
            if (ui.iconImage != null)
                ui.iconImage.sprite = ui.neutralSprite;

            if (ui.heartImage != null)
            {
                ui.heartImage.enabled = false;
                ui.heartImage.sprite = null;
            }
        }
    }

    public void SetHeart(string characterId, bool positive)
    {
        // Configura o sprite do coração para positivo ou negativo
        if (!_feedbackLookup.TryGetValue(characterId, out var ui))
        {
            Debug.LogWarning($"[FeedbackManager] NPC '{characterId}' não configurado para hearts!");
            return;
        }

        if (ui.heartImage == null)
        {
            Debug.LogWarning($"[FeedbackManager] heartImage NÃO atribuído para '{characterId}'");
            return;
        }

        // Cancel any in-flight fade tweens
        if (_heartTweens.TryGetValue(characterId, out var existingTween))
        {
            existingTween.Kill();
            _heartTweens.Remove(characterId);
        }

        ui.heartImage.sprite = positive ? ui.positiveHeartSprite : ui.negativeHeartSprite;
        ui.heartImage.enabled = true;

        // Reset scale para o original e alpha para 1
        ui.heartImage.transform.DOKill();
        ui.heartImage.transform.localScale = _heartOriginalScales.TryGetValue(characterId, out var origScale) ? origScale : ui.heartImage.transform.localScale;
        var color = ui.heartImage.color;
        color.a = 1f;
        ui.heartImage.color = color;
    }

    public void ShowHeart(string characterId, bool positive)
    {
        // Configura o sprite do coração e inicia a animação de fade-out
        SetHeart(characterId, positive);

        if (!_feedbackLookup.TryGetValue(characterId, out var ui) || ui.heartImage == null)
            return;

        var tween = ui.heartImage.DOFade(0f, heartDisplayDuration).SetDelay(1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            if (ui.heartImage != null)
                ui.heartImage.enabled = false;
            _heartTweens.Remove(characterId);
        });

        _heartTweens[characterId] = tween;
    }
}

