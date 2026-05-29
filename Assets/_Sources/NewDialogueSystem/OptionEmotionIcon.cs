using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//componente no prefab "Option Item" junto com a Image do ícone. 
//recebe a string da tag "emotion:X" (ex: "Happy", "Angry") e aplica o sprite ao ícone.
public class OptionEmotionIcon : MonoBehaviour
{
    [Serializable]

    //estrutura para mapear cada emoção a um sprite específico, configurável no inspector.
    public class EmotionIconEntry
    {
        public EmotionType emotion;
        public Sprite sprite;
    }

    [SerializeField] private Image _iconImage;
    [SerializeField] private List<EmotionIconEntry> _emotionIcons = new();

    private Dictionary<EmotionType, Sprite> _lookup;

    private void Awake()
    {
        BuildLookup();
    }

    //le no dicionário de enums, faz o parse da string da tag pra enum, e aplica o sprite correspondente no ícone.
    //P fallback: Se não achar a emoção ou se a tag for inválida, esconde o ícone. 
    private void BuildLookup()
    {
        _lookup = new Dictionary<EmotionType, Sprite>();
        foreach (var entry in _emotionIcons)
        {
            if (entry != null && entry.sprite != null)
                _lookup[entry.emotion] = entry.sprite;
        }
    }

    public void Apply(string emotionTag)
    {
        if (_iconImage == null) return;

        if (_lookup == null || _lookup.Count == 0)
            BuildLookup();

        if (!Enum.TryParse(emotionTag, true, out EmotionType emotion))
        {
            _iconImage.gameObject.SetActive(false);
            return;
        }

        if (_lookup.TryGetValue(emotion, out var sprite) && sprite != null)
        {
            _iconImage.sprite = sprite;
            _iconImage.gameObject.SetActive(true);
        }
        else
        {
            _iconImage.gameObject.SetActive(false);
        }
    }

    public void Hide()
    {
        if (_iconImage != null)
            _iconImage.gameObject.SetActive(false);
    }
}
