using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

public class UnifiedDialoguePresenter : DialoguePresenterBase
{
    [Header("Referências UI")]
    public GameObject bubbleRoot;              
    public TextMeshProUGUI dialogueText;       
    public TextMeshProUGUI characterNameText;  
    public Button continueButton;              
    public Button nextOptionButton;            
    public Button confirmButton;               

    [Header("Configuração")]
    public float charactersPerSecond = 40f;

    private CancellationTokenSource typingCTS;

    private List<DialogueOption> _options = new List<DialogueOption>();
    private int _currentOptionIndex = 0;
    private YarnTaskCompletionSource<DialogueOption?> optionSelectionSource;
    private bool _isShowingOptions = false;

    private void Awake()
    {
        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        if (nextOptionButton != null)
        {
            nextOptionButton.gameObject.SetActive(false);
            nextOptionButton.onClick.AddListener(OnNextOptionClicked);
        }

        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(false);
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }
    }

    // ================== CICLO DE DIÁLOGO ==================

    public override YarnTask OnDialogueStartedAsync()
    {
        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
        return YarnTask.CompletedTask;
    }

    public override YarnTask OnDialogueCompleteAsync()
    {
        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);

        HideAllButtons();
        return YarnTask.CompletedTask;
    }

    // ================== LINHAS ==================

    public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        _isShowingOptions = false;
        HideOptionButtons();

        if (bubbleRoot != null)
            bubbleRoot.SetActive(true);

        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        if (characterNameText != null)
            characterNameText.text = line.CharacterName ?? "";

        string fullText = line.TextWithoutCharacterName.Text;
        dialogueText.text = "";

        typingCTS = new CancellationTokenSource();
        using (var linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(
            typingCTS.Token, token.HurryUpToken))
        {
            var ct = linkedCTS.Token;
            int delayMs = charactersPerSecond > 0 ? (int)(1000f / charactersPerSecond) : 0;

            foreach (char c in fullText)
            {
                if (ct.IsCancellationRequested)
                {
                    dialogueText.text = fullText;
                    break;
                }

                dialogueText.text += c;

                if (delayMs > 0)
                    await YarnTask.Delay(delayMs, ct).SuppressCancellationThrow();
            }
        }

        if (continueButton != null)
            continueButton.gameObject.SetActive(true);

        await YarnTask.WaitUntilCanceled(token.NextLineToken).SuppressCancellationThrow();

        if (continueButton != null)
            continueButton.gameObject.SetActive(false);
    }

    private void OnContinueClicked()
    {
        typingCTS?.Cancel();
    }

    // ================== OPÇÕES ==================

    public override async YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
    {
        _isShowingOptions = true;

        if (bubbleRoot != null)
            bubbleRoot.SetActive(true);

        _options.Clear();
        _options.AddRange(dialogueOptions);
        _currentOptionIndex = 0;

        if (_options.Count == 0)
            return await DialogueRunner.NoOptionSelected;

        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        if (nextOptionButton != null) nextOptionButton.gameObject.SetActive(true);
        if (confirmButton != null) confirmButton.gameObject.SetActive(true);

        ShowCurrentOption();

        optionSelectionSource = new YarnTaskCompletionSource<DialogueOption?>();

        using (var linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            var ct = linkedCTS.Token;

            _ = AwaitOptionCancellation(ct);

            var selected = await optionSelectionSource.Task;

            HideOptionButtons();

            if (ct.IsCancellationRequested)
                return await DialogueRunner.NoOptionSelected;

            return selected;
        }
    }

    private async YarnTask AwaitOptionCancellation(CancellationToken ct)
    {
        await YarnTask.WaitUntilCanceled(ct).SuppressCancellationThrow();
        optionSelectionSource?.TrySetResult(null);
    }

    private void ShowCurrentOption()
    {
        if (_options.Count == 0 || dialogueText == null)
            return;

        var current = _options[_currentOptionIndex];
        dialogueText.text = current.Line.Text.Text;

        // Opcional: se quiser mostrar o nome do personagem da última fala
        if (characterNameText != null)
            characterNameText.text = current.Line.CharacterName ?? "";
    }

    private void OnNextOptionClicked()
    {
        if (!_isShowingOptions || _options.Count == 0)
            return;

        _currentOptionIndex = (_currentOptionIndex + 1) % _options.Count;
        ShowCurrentOption();
    }

    private void OnConfirmClicked()
    {
        if (!_isShowingOptions || _options.Count == 0 || optionSelectionSource == null)
            return;

        var selected = _options[_currentOptionIndex];
        optionSelectionSource.TrySetResult(selected);
        _isShowingOptions = false;
    }

    // ================== HELPERS ==================

    private void HideOptionButtons()
    {
        if (nextOptionButton != null) nextOptionButton.gameObject.SetActive(false);
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
    }

    private void HideAllButtons()
    {
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        HideOptionButtons();
    }
}