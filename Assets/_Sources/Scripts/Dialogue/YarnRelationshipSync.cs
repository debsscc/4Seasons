using UnityEngine;
using Yarn.Unity;
using System.Collections.Generic;

public class YarnRelationshipController : MonoBehaviour
{
    [Header("Yarn References")]
    public DialogueRunner dialogueRunner;

    [Header("Character Relationships")]
    public List<CharacterRelationshipBinding> characterBindings = new();

    // Internals
    private Dictionary<string, CharacterData> _idToCharacterData = new();
    private string _lastCharacter = string.Empty;
    private string _lastOption = string.Empty;

    void Awake()
    {
        if (this == null) return;

        _idToCharacterData.Clear();
        foreach (var binding in characterBindings)
        {
            if (binding == null || string.IsNullOrEmpty(binding.characterId) || binding.characterData == null) continue;
            _idToCharacterData[binding.characterId] = binding.characterData;
        }
    }

    void Update()
    {
        if (this == null || dialogueRunner == null || dialogueRunner.VariableStorage == null) return;

        string currentCharacter = GetYarnStringRobust("current_character");
        string currentOption = GetYarnStringRobust("current_option");

        // Detecta mudança nas variáveis
        if (currentCharacter != _lastCharacter || currentOption != _lastOption)
        {
            _lastCharacter = currentCharacter;
            _lastOption = currentOption;

            if (!string.IsNullOrEmpty(currentCharacter) && !string.IsNullOrEmpty(currentOption))
            {
                ApplyRelationshipChange(currentCharacter, currentOption);
            }
        }
    }

    private void ApplyRelationshipChange(string characterId, string option)
    {
        if (this == null) return;

        if (!_idToCharacterData.TryGetValue(characterId, out var characterData))
        {
            Debug.LogWarning($"[YarnRelationshipController] Personagem '{characterId}' não encontrado.");
            return;
        }

        int delta = 0;
        if (option == "Positive") delta = 1;
        else if (option == "Negative") delta = -1;

        if (delta != 0)
        {
            characterData.RelationshipScore += delta;
            Debug.Log($"[YarnRelationshipController] {characterId}: {characterData.RelationshipScore} ({(delta > 0 ? "+" : "")}{delta})");
        }
    }

    private string GetYarnStringRobust(string varNameWithoutOrWithDollar)
    {
        if (this == null || dialogueRunner == null || dialogueRunner.VariableStorage == null) return "";

        string key = varNameWithoutOrWithDollar.StartsWith("$") ? varNameWithoutOrWithDollar : "$" + varNameWithoutOrWithDollar;

        try
        {
            if (dialogueRunner.VariableStorage.TryGetValue<string>(key, out var strVal))
                return strVal ?? "";
        }
        catch
        {
            // Ignorar erros
        }

        try
        {
            if (dialogueRunner.VariableStorage.TryGetValue<object>(key, out var objVal) && objVal != null)
                return objVal.ToString();
        }
        catch
        {
            // Ignorar erros
        }

        return "";
    }

    void OnDisable()
    {
        // Se você adicionar listeners no futuro, remova-os aqui
    }
}