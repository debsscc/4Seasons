using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class DialogueEmotionController : MonoBehaviour
{
    [Header("Referências")]
    public DialogueRunner dialogueRunner;
    public List<CharacterEmotionProfile> characterProfiles;
    public Image emotionDisplay;
    public Animator emotionAnimator;

    [Header("Config")]
    public EmotionType defaultEmotion = EmotionType.Normal;

    private Dictionary<string, CharacterEmotionProfile> _profiles;
    private string _lastCharacter = "";
    private EmotionType _lastEmotion = EmotionType.Normal;

    void Awake()
    {
        _profiles = new Dictionary<string, CharacterEmotionProfile>();
        foreach (var p in characterProfiles)
        {
            if (p != null && !string.IsNullOrEmpty(p.characterId))
                _profiles[p.characterId] = p;
        }
    }

    void Update()
{
    if (dialogueRunner == null)
    {
        Debug.LogWarning("[DialogueEmotionController] DialogueRunner é null!");
        return;
    }

    if (!dialogueRunner.IsDialogueRunning)
    {
        // Não loga nada aqui pra não spammar quando não tem diálogo
        return;
    }

    // Lê as variáveis do Yarn
    string currentChar = GetYarnString("$current_character");
    string emotionStr = GetYarnString("$current_emotion");

    Debug.Log($"[Update] Lendo Yarn: character='{currentChar}', emotion='{emotionStr}'");

    // Se mudou, aplica
    if (currentChar != _lastCharacter || emotionStr != _lastEmotion.ToString())
    {
        Debug.Log($"[Update] MUDANÇA DETECTADA! Antes: {_lastCharacter}/{_lastEmotion}, Agora: {currentChar}/{emotionStr}");

        _lastCharacter = currentChar;

        if (System.Enum.TryParse(emotionStr, true, out EmotionType emotion))
        {
            _lastEmotion = emotion;
            Debug.Log($"[Update] Emoção parseada com sucesso: {emotion}");
        }
        else
        {
            _lastEmotion = defaultEmotion;
            Debug.LogWarning($"[Update] Falha ao parsear '{emotionStr}', usando default: {defaultEmotion}");
        }

        ApplyEmotion(_lastCharacter, _lastEmotion);
    }
}

    private string GetYarnString(string varName)
{
    if (dialogueRunner.VariableStorage.TryGetValue<string>(varName, out var value))
    {
        Debug.Log($"[GetYarnString] '{varName}' = '{value}'");
        return value;
    }
    
    Debug.LogWarning($"[GetYarnString] Variável '{varName}' não encontrada!");
    return "";
}

    private void ApplyEmotion(string characterId, EmotionType emotion)
    {
        if (string.IsNullOrEmpty(characterId))
            return;

        if (!_profiles.TryGetValue(characterId, out var profile))
        {
            Debug.LogWarning($"[DialogueEmotionController] Perfil não encontrado: '{characterId}'");
            return;
        }

        var entry = profile.GetEmotion(emotion);
        if (entry == null)
        {
            Debug.LogWarning($"[DialogueEmotionController] Emoção '{emotion}' não configurada para '{characterId}'");
            return;
        }

        Debug.Log($"Aplicando: {characterId} - {emotion}");

        if (entry.useAnimation && entry.animationClip != null && emotionAnimator != null)
        {
            emotionAnimator.enabled = true;
            emotionAnimator.Play(entry.animationClip.name);
        }
        else if (entry.sprite != null && emotionDisplay != null)
        {
            emotionDisplay.sprite = entry.sprite;
            emotionDisplay.enabled = true;

            if (emotionAnimator != null)
                emotionAnimator.enabled = false;
        }
    }
}