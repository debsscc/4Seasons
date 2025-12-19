#nullable enable

using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using TMPro;

public class FancyOptionsPresenter : DialoguePresenterBase
{
    [Header("Referências")]
    public CanvasGroup canvasGroup;
    public Transform optionsContainer; // onde as opções vão aparecer
    public DialogueOptionItem optionItemPrefab; // prefab de cada opção

    [Header("Configuração")]
    public float fadeTime = 0.2f;
    public TMP_Text lastLineText; // opcional: mostra a última fala antes das opções

    private List<DialogueOptionItem> optionItems = new List<DialogueOptionItem>();
    private int selectedIndex = 0;
    private bool optionSelected = false;

    void Awake()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public override YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
    {
        return RunOptionsInternalAsync(dialogueOptions, cancellationToken);
    }

    private async YarnTask<DialogueOption?> RunOptionsInternalAsync(DialogueOption[] options, CancellationToken token)
    {
        // Limpa opções antigas
        foreach (var item in optionItems)
        {
            if (item != null) Destroy(item.gameObject);
        }
        optionItems.Clear();

        // Fade in
        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            await Effects.FadeAlphaAsync(canvasGroup, 0, 1, fadeTime, token);
        }

        // Cria as opções
        var tcs = new YarnTaskCompletionSource<DialogueOption?>();
        selectedIndex = 0;
        optionSelected = false;

        for (int i = 0; i < options.Length; i++)
        {
            if (!options[i].IsAvailable) continue; // pula opções indisponíveis

            var item = Instantiate(optionItemPrefab, optionsContainer);
            item.Setup(options[i], i, () => OnOptionSelected(options[i], tcs));
            optionItems.Add(item);
        }

        if (optionItems.Count > 0)
        {
            optionItems[0].SetHighlighted(true);
        }

        // Loop de input (navegação por teclado/gamepad)
        var inputTask = HandleInputAsync(tcs, token);
        
        // Espera seleção (ou cancelamento)
        DialogueOption? result = null;
        try
        {
            result = await tcs.Task;
        }
        catch (System.OperationCanceledException)
        {
            // Cancelado
        }

        // Fade out
        if (canvasGroup != null)
        {
            await Effects.FadeAlphaAsync(canvasGroup, 1, 0, fadeTime, token);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        return result;
    }

    private async YarnTask HandleInputAsync(YarnTaskCompletionSource<DialogueOption?> tcs, CancellationToken token)
    {
        while (!optionSelected && !token.IsCancellationRequested)
        {
            // Navegação por teclado/gamepad
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                ChangeSelection(1);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                ChangeSelection(-1);
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                if (optionItems.Count > 0 && selectedIndex >= 0 && selectedIndex < optionItems.Count)
                {
                    optionItems[selectedIndex].OnClick();
                }
            }

            await YarnTask.Delay(10);
        }
    }

    private void OnOptionSelected(DialogueOption option, YarnTaskCompletionSource<DialogueOption?> tcs)
    {
        optionSelected = true;
        tcs.TrySetResult(option);
    }

    private void ChangeSelection(int delta)
    {
        if (optionItems.Count == 0) return;

        optionItems[selectedIndex].SetHighlighted(false);
        selectedIndex = (selectedIndex + delta + optionItems.Count) % optionItems.Count;
        optionItems[selectedIndex].SetHighlighted(true);
    }

    public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        // Não mostra linhas (deixa pro LinePresenter padrão)
        return YarnTask.CompletedTask;
    }

    public override YarnTask OnDialogueStartedAsync()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0;
        return YarnTask.CompletedTask;
    }

    public override YarnTask OnDialogueCompleteAsync()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0;
        return YarnTask.CompletedTask;
    }
}