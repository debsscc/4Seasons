using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;
using System.Linq;

[Serializable]
public class CharacterBinding
{
    public string characterId = string.Empty;
    public GameObject? characterGameObject;
}

public class DialogueEmotionController : MonoBehaviour
{
    [Header("References")]
    public DialogueRunner dialogueRunner;

    [Header("Scene characters")]
    public List<CharacterBinding> characterBindings = new();

    [Header("Profiles")]
    public List<CharacterEmotionProfile> characterProfiles = new();

    [Header("Speech bubble UI (single bubble)")]
    public RectTransform speechBubbleRect;
    public TextMeshProUGUI dialogueTextTMP;
    public Image bubbleEmotionImage;

    [Header("Config")]
    public Vector2 bubbleOffset = new Vector2(0, 120);
    public Camera uiCamera;

    private Dictionary<string, GameObject> _idToGO = new();
    private Dictionary<string, CharacterEmotionProfile> _idToProfile = new();
    private Dictionary<string, SpriteRenderer> _idToSpriteRenderer = new();
    private Dictionary<string, Animator> _idToAnimator = new();
    private Dictionary<string, Image> _idToUIImage = new();

    private string _lastCharacter = string.Empty;
    private EmotionType _lastEmotion = EmotionType.Normal;

    private List<CharacterAnimatorRunner> _animatorRunners = new List<CharacterAnimatorRunner>();

    public void AddAnimatorRunner(CharacterAnimatorRunner runner)
    {
        if (!_animatorRunners.Contains(runner))
            _animatorRunners.Add(runner);
    }

    void Awake()
    {
        _idToGO.Clear();
        _idToSpriteRenderer.Clear();
        _idToAnimator.Clear();
        _idToUIImage.Clear();

        foreach (var b in characterBindings)
        {
            if (b == null) continue;
            if (string.IsNullOrEmpty(b.characterId) || b.characterGameObject == null) continue;

            _idToGO[b.characterId] = b.characterGameObject;

            var sr = b.characterGameObject.GetComponent<SpriteRenderer>()
                     ?? b.characterGameObject.GetComponentInChildren<SpriteRenderer>(true);
            if (sr != null) _idToSpriteRenderer[b.characterId] = sr;

            var anim = b.characterGameObject.GetComponent<Animator>()
                       ?? b.characterGameObject.GetComponentInChildren<Animator>(true);
            if (anim != null) _idToAnimator[b.characterId] = anim;

            var img = b.characterGameObject.GetComponent<Image>()
                      ?? b.characterGameObject.GetComponentInChildren<Image>(true);
            if (img != null) _idToUIImage[b.characterId] = img;
        }

        _idToProfile.Clear();
        foreach (var p in characterProfiles)
        {
            if (p == null) continue;
            if (string.IsNullOrEmpty(p.characterId)) continue;
            _idToProfile[p.characterId] = p;
        }

        _lastCharacter = string.Empty;
    }

    void Update()
    {
        if (dialogueRunner == null || dialogueRunner.VariableStorage == null) return;

        string currentChar = GetYarnStringRobust("current_character");
        string emotionStr = GetYarnStringRobust("current_emotion");

        if (!Enum.TryParse(emotionStr, true, out EmotionType emotion))
            emotion = EmotionType.Normal;

        if (!string.Equals(_lastCharacter, currentChar, StringComparison.Ordinal) || _lastEmotion != emotion)
        {
            Debug.Log($"[DialogueEmotionController] Mudança detectada -> personagem='{currentChar}', emoção='{emotion}'");

            ApplyEmotionToSceneCharacter(currentChar, emotion);
            PositionSpeechBubbleToCharacter(currentChar);
            UpdateBubbleEmotionImage(currentChar, emotion);
        }
    }

    public void ForceApplyCurrentEmotion()
    {
        if (dialogueRunner == null || dialogueRunner.VariableStorage == null) return;

        string currentChar = GetYarnStringRobust("current_character");
        string emotionStr = GetYarnStringRobust("current_emotion");

        if (!Enum.TryParse(emotionStr, true, out EmotionType emotion))
            emotion = EmotionType.Normal;

        Debug.Log($"[DialogueEmotionController] ForceApply -> personagem='{currentChar}', emoção='{emotion}'");

        ApplyEmotionToSceneCharacter(currentChar, emotion);
        PositionSpeechBubbleToCharacter(currentChar);
        UpdateBubbleEmotionImage(currentChar, emotion);
    }

    private void ApplyEmotionToSceneCharacter(string characterId, EmotionType emotion)
    {
        _lastCharacter = characterId;
        _lastEmotion = emotion;

        if (_animatorRunners == null) return;

        foreach (CharacterAnimatorRunner runner in _animatorRunners)
        {
            if (runner.GetCharacterId() != characterId) continue;

            Debug.Log($"[DialogueEmotionController] Aplicando emoção '{emotion}' ao personagem '{characterId}'", runner.gameObject);
            runner.PlayAnimation(emotion);
        }
    }

    private string GetYarnStringRobust(string varNameWithoutOrWithDollar)
    {
        if (dialogueRunner == null || dialogueRunner.VariableStorage == null) return "";

        string key = varNameWithoutOrWithDollar.StartsWith("$") ? varNameWithoutOrWithDollar : "$" + varNameWithoutOrWithDollar;

        try
        {
            if (dialogueRunner.VariableStorage.TryGetValue<string>(key, out var strVal))
                return strVal ?? "";
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DialogueEmotionController] erro ao ler variável '{key}': {e.Message}");
        }

        try
        {
            if (dialogueRunner.VariableStorage.TryGetValue<object>(key, out var objVal) && objVal != null)
                return objVal.ToString();
        }
        catch (Exception e2)
        {
            Debug.LogWarning($"[DialogueEmotionController] fallback também falhou para '{key}': {e2.Message}");
        }

        return "";
    }

    private void PositionSpeechBubbleToCharacter(string characterId)
    {
        if (speechBubbleRect == null) return;

        if (!_idToGO.TryGetValue(characterId, out var characterGO)) return;

        if (characterGO == null) return;

        Vector3 bubblePos = speechBubbleRect.position;
        bubblePos.x = characterGO.transform.position.x;
        speechBubbleRect.position = bubblePos;
    }

    private void UpdateBubbleEmotionImage(string characterId, EmotionType emotion)
    {
        if (bubbleEmotionImage == null) return;

        if (_idToProfile.TryGetValue(characterId, out var profile))
        {
            var entry = profile.GetEmotion(emotion);
            if (entry != null && entry.sprite != null)
            {
                bubbleEmotionImage.sprite = entry.sprite;
                bubbleEmotionImage.enabled = true;
                return;
            }
        }
        bubbleEmotionImage.enabled = false;
    }

    public void UpdateDialogueText(string text)
    {
        if (dialogueTextTMP != null) dialogueTextTMP.text = text;
    }

    public void PreviewEmotion(string characterId, string emotionStr)
    {
        if (!Enum.TryParse(emotionStr, true, out EmotionType emotion))
            emotion = EmotionType.Normal;

        ApplyEmotionToSceneCharacter(characterId, emotion);
        PositionSpeechBubbleToCharacter(characterId);
        UpdateBubbleEmotionImage(characterId, emotion);
    }

    public void Debug_ForceApply(string characterId, EmotionType emotion)
    {
        Debug.Log($"[DEBUG] Forçando Apply: {characterId} / {emotion}");
        ApplyEmotionToSceneCharacter(characterId, emotion);
        PositionSpeechBubbleToCharacter(characterId);
        UpdateBubbleEmotionImage(characterId, emotion);
    }
}
