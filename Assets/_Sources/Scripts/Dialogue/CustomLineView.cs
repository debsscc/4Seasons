using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;
using Yarn.Unity.Legacy;

public class CustomLineView : DialogueViewBase
{
    [Header("Referências UI")]
    public GameObject bubbleRoot;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI characterNameText;
    public Button continueButton;

    [Header("Configuração")]
    public float charactersPerSecond = 40f;

    private Action _onLineFinished;
    private Coroutine _typingRoutine;
    private string _fullLineText;
    private bool _isTyping;

    private void Awake()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
            continueButton.gameObject.SetActive(false);
        }
    }

    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
    {
        _onLineFinished = onDialogueLineFinished;

        if (bubbleRoot != null)
            bubbleRoot.SetActive(true);

        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        // Nome do personagem
        if (characterNameText != null)
            characterNameText.text = dialogueLine.CharacterName ?? "";

        // Texto sem o nome
        _fullLineText = dialogueLine.TextWithoutCharacterName.Text;

        if (_typingRoutine != null)
            StopCoroutine(_typingRoutine);

        _typingRoutine = StartCoroutine(TypewriterCoroutine(_fullLineText));
    }

    private IEnumerator TypewriterCoroutine(string text)
    {
        _isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(1f / charactersPerSecond);
        }

        _isTyping = false;

        // Mostra botão de continuar quando terminar
        if (continueButton != null)
            continueButton.gameObject.SetActive(true);
    }

    public void OnContinueClicked()
    {
        if (_isTyping)
        {
            // Se ainda estiver digitando, pula pro texto completo
            if (_typingRoutine != null)
                StopCoroutine(_typingRoutine);

            _typingRoutine = null;
            _isTyping = false;

            dialogueText.text = _fullLineText;

            if (continueButton != null)
                continueButton.gameObject.SetActive(true);

            return;
        }

        // Linha terminou + jogador confirmou → avisa o Yarn
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        _onLineFinished?.Invoke();
    }

    public override void DismissLine(Action onDismissalComplete)
    {
        // Se quiser esconder o balão entre falas, descomente:
        // if (bubbleRoot != null)
        //     bubbleRoot.SetActive(false);

        onDismissalComplete();
    }

    public override void DialogueStarted()
    {
        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
    }

    public override void DialogueComplete()
    {
        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
    }
}