using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CharacterEmotionProfile",
    menuName = "Dialogue/Character Emotion Profile")]
public class CharacterEmotionProfile : ScriptableObject
{
    [Header("ID character (same as Yarn)")]
    [Tooltip("Nome exato como aparece no Yarn")]
    public string characterId;

    [Serializable]
    public class EmotionEntry
    {
        [Tooltip("Emotion Type")]
        public EmotionType type;

        [Tooltip("Se marcado, usa a animação em vez da sprite estática")]
        public bool useAnimation;

        [Tooltip("Sprite estática para essa emoção")]
        public Sprite sprite;

        [Tooltip("AnimationClip para essa emoção (se useAnimation = true)")]
        public AnimationClip animationClip;
    }

    [Header("Character Emotions")]
    public List<EmotionEntry> emotions = new();

    private Dictionary<EmotionType, EmotionEntry> _lookup;

    private void OnEnable()
    {
        BuildLookup();
    }

    private void OnValidate()
    {
        // p n ter emoções duplicadas
        var seen = new HashSet<EmotionType>();
        foreach (var e in emotions)
        {
            if (e != null && seen.Contains(e.type))
            {
                Debug.LogWarning($"[{name}] Emoção duplicada: {e.type}. Remova as duplicadas", this);
            }
            if (e != null)
                seen.Add(e.type);
        }
    }

    private void BuildLookup()
    {
        _lookup = new Dictionary<EmotionType, EmotionEntry>();

        foreach (var e in emotions)
        {
            if (e != null)
                _lookup[e.type] = e;
        }
    }
    public EmotionEntry GetEmotion(EmotionType type)
    {
        if (_lookup == null || _lookup.Count == 0)
            BuildLookup();

        // tenta achar a emoção
        if (_lookup.TryGetValue(type, out var entry))
            return entry;

        // Fallback 1: Normal
        if (_lookup.TryGetValue(EmotionType.Normal, out var normal))
        {
            Debug.LogWarning($"[{characterId}] Emoção '{type}' não encontrada. Usando 'Normal'.", this);
            return normal;
        }

        // Fallback 2: Primeira disponível
        foreach (var e in _lookup.Values)
        {
            Debug.LogWarning($"[{characterId}] Emoção '{type}' e 'Normal' não encontradas. Usando primeira disponível.", this);
            return e;
        }

        Debug.LogError($"[{characterId}] Nenhuma emoção configurada!", this);
        return null;
    }
    public bool HasEmotion(EmotionType type)
    {
        if (_lookup == null || _lookup.Count == 0)
            BuildLookup();

        return _lookup.ContainsKey(type);
    }
}