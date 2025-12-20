using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;
using Yarn.Unity.Legacy;

public class CustomOptionView : DialogueViewBase
{
    [Header("Referências UI")]
    public GameObject bubbleRoot;
    public TextMeshProUGUI dialogueText;
    public Button nextOptionButton;
    public Button confirmButton;

    private List<DialogueOption> _options = new List<DialogueOption>();
    private Action<int> _onOptionSelected;
    private int _currentIndex = 0;
    private bool _isShowingOptions = false;

    private void Awake()
    {
        if (nextOptionButton != null)
            nextOptionButton.onClick.AddListener(OnNextOptionClicked);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);

        // Começa com os botões desligados
        if (nextOptionButton != null) nextOptionButton.gameObject.SetActive(false);
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
    }

    public override void RunOptions(DialogueOption[] dialogueOptions, Action<int> onOptionSelected)
    {
        _options = new List<DialogueOption>(dialogueOptions);
        _onOptionSelected = onOptionSelected;
        _currentIndex = 0;
        _isShowingOptions = true;

        if (bubbleRoot != null)
            bubbleRoot.SetActive(true);

        if (nextOptionButton != null)
            nextOptionButton.gameObject.SetActive(true);

        if (confirmButton != null)
            confirmButton.gameObject.SetActive(true);

        ShowCurrentOption();
    }

    private void ShowCurrentOption()
    {
        if (_options.Count == 0)
            return;

        var current = _options[_currentIndex];
        dialogueText.text = current.Line.Text.Text;
    }

    private void OnNextOptionClicked()
    {
        if (!_isShowingOptions || _options.Count == 0)
            return;

        _currentIndex = (_currentIndex + 1) % _options.Count;
        ShowCurrentOption();
    }

    private void OnConfirmClicked()
    {
        if (!_isShowingOptions)
            return;

        _isShowingOptions = false;

        if (nextOptionButton != null)
            nextOptionButton.gameObject.SetActive(false);
        if (confirmButton != null)
            confirmButton.gameObject.SetActive(false);

        _onOptionSelected?.Invoke(_currentIndex);
    }

    public override void DismissLine(Action onDismissalComplete)
    {
        _isShowingOptions = false;

        if (nextOptionButton != null)
            nextOptionButton.gameObject.SetActive(false);
        if (confirmButton != null)
            confirmButton.gameObject.SetActive(false);

        onDismissalComplete();
    }

    public override void DialogueStarted()
    {
        if (nextOptionButton != null) nextOptionButton.gameObject.SetActive(false);
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
    }

    public override void DialogueComplete()
    {
        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
    }
}