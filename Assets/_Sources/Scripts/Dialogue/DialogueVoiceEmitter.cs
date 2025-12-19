using UnityEngine;
using Yarn.Unity;
using System.Collections.Generic;
using System.Threading;
using TMPro;

public class DialogueVoiceEmitter : MonoBehaviour, IActionMarkupHandler
{
    [Header("Configurações de Áudio")]
    public AudioSource audioSource;
    public List<AudioClip> voiceBeeps; // Lista de sons para randomizar
    [Range(1, 5)]
    public int charactersPerBeep = 3; // Tocar a cada 3 letras

    private int characterCount = 0;

    private void Start()
    {
        // Registra este script no LinePresenter do prefab
        var presenter = GetComponent<LinePresenter>();
        if (presenter != null)
        {
            presenter.ActionMarkupHandlers.Add(this);
        }
    }

    // Chamado toda vez que uma letra aparece no efeito typewriter
    public YarnTask OnCharacterWillAppear(int currentCharacterIndex, Yarn.Markup.MarkupParseResult line, CancellationToken cancellationToken)
    {
        characterCount++;

        if (characterCount >= charactersPerBeep)
        {
            PlayRandomBeep();
            characterCount = 0;
        }

        return YarnTask.CompletedTask;
    }

    private void PlayRandomBeep()
    {
        if (voiceBeeps.Count > 0 && audioSource != null)
        {
            int index = Random.Range(0, voiceBeeps.Count);
            audioSource.pitch = Random.Range(0.9f, 1.1f); // Pequena variação de tom para não cansar o ouvido
            audioSource.PlayOneShot(voiceBeeps[index]);
        }
    }

    //yarn precisa desses 4 p funfar
    public void OnPrepareForLine(Yarn.Markup.MarkupParseResult line, TMP_Text text) { characterCount = 0; }
    public void OnLineDisplayBegin(Yarn.Markup.MarkupParseResult line, TMP_Text text) { }
    public void OnLineDisplayComplete() { }
    public void OnLineWillDismiss() { }
}

//Preciso por esse componente ao mesmo objeto que tem o LinePresenter no prefab
// Ao criar um AudioSource no prefab, arrasto ele p AudioSource do script 
// Os beeps vão ir na listinha de VoiceBeeps
