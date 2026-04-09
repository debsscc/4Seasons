using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Yarn.Unity;

public class ChangeOptionHolder : MonoBehaviour
{
    [SerializeField] private HorizontalLayoutGroup _layoutGroup;
    [SerializeField] private Button _leftArrowButton;
    [SerializeField] private Button _rightArrowButton;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private float _itemSize = 200f;
    [SerializeField] private DialogueEmotionController _emotionController;
    [SerializeField] private OptionEmotionIcon _emotionIcon;

    private DialogueRunner _dialogueRunner;
    private float _currentScrollPosition = 0f;
    private int _currentIndex = 0;
    private float _posOffset = 0f;
    private int _lastOptionCount = -1;

    private float ItensSpacing => _layoutGroup != null ? _layoutGroup.spacing : 0f;
    private float ScrollAmount => _itemSize + ItensSpacing;
    private float CurrentLayoutPosition
    {
        get
        {
            if (_layoutGroup == null) return 0f;
            return _layoutGroup.transform.localPosition.x;
        }
        set
        {
            if (_layoutGroup == null) return;

            transform.DOKill();
            _layoutGroup.transform.DOLocalMoveX(value, 0.3f).SetEase(Ease.OutCubic);
        }
    }

    private int CurrentIndex
    {
        get => _currentIndex;
        set
        {
            _currentIndex = Mathf.Clamp(value, 0, _layoutGroup.transform.childCount - 1);
            CurrentLayoutPosition = -_currentIndex * ScrollAmount + _posOffset;
            PreviewOptionEmotion();
        }
    }

    private OptionItem[] Options => _layoutGroup.GetComponentsInChildren<OptionItem>();

    void Update()
    {
        //Detecta opções ativas e reseta o índice se necessário
        var options = Options;
        int activeCount = 0;
        foreach (var o in options)
            if (o.gameObject.activeSelf) activeCount++;

        if (activeCount > 0 && activeCount != _lastOptionCount)
        {
            _lastOptionCount = activeCount;
            _currentIndex = 0;
            CurrentLayoutPosition = _posOffset;
            if (_emotionController != null)
                _emotionController.BeginOptionsPreview();
            PreviewOptionEmotion();
            // Desativa navegação do EventSystem para que as teclas A/D cheguem ao Update()
            if (EventSystem.current != null)
                EventSystem.current.sendNavigationEvents = false;
        }
        // se não houver opções ativas, reseta o contador para garantir que a próxima vez que opções forem ativadas, o sistema reconheça a mudança
        else if (activeCount == 0 && _lastOptionCount > 0)
        {
            _lastOptionCount = 0;
            // Restaura navegação do EventSystem
            if (EventSystem.current != null)
                EventSystem.current.sendNavigationEvents = true;
        }

        // Navegação por teclado
        if (activeCount > 0)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                CurrentIndex--;
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                CurrentIndex++;
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                ConfirmSelection();
        }
    }

    void Awake()
    {
        if (_dialogueRunner == null)
        {
            _dialogueRunner = GetComponentInParent<DialogueRunner>();
            if(!_dialogueRunner)
                Debug.LogError("DialogueRunner component not found in parent.");
        }
    }

    void Start()
    {
        _leftArrowButton.onClick.AddListener(() =>
        {
            CurrentIndex--;
        });

        _rightArrowButton.onClick.AddListener(() =>
        {
            CurrentIndex++;
        });


        _confirmButton.onClick.AddListener(ConfirmSelection);

        _posOffset = _layoutGroup.transform.localPosition.x;
    }

    void ConfirmSelection()
    {
        var options = Options;

        if (options.Length > 0 && _currentIndex >= 0 && _currentIndex < options.Length)
        {
            if (_emotionController != null)
                _emotionController.EndOptionsPreview();
            var selectedOption = options[_currentIndex];
            selectedOption.InvokeOptionSelected();
            Debug.Log($"Selected option: {selectedOption}", selectedOption);
        }
    }

    private void PreviewOptionEmotion()
    {
        var options = Options;
        if (_currentIndex < 0 || _currentIndex >= options.Length) return;

        var option = options[_currentIndex];

        string emotionTag = null;
        string characterName = null;
        try
        {
            characterName = option.Option.Line.CharacterName;
            foreach (var tag in option.Option.Line.Metadata)
            {
                if (tag.StartsWith("emotion:"))
                {
                    emotionTag = tag.Substring("emotion:".Length);
                    break;
                }
            }
        }
        catch (System.NullReferenceException)
        {
            // Option ainda não foi setado pelo OptionsPresenter neste frame
            return;
        }

        if (_emotionController != null && !string.IsNullOrEmpty(characterName) && !string.IsNullOrEmpty(emotionTag))
            _emotionController.PreviewEmotion(characterName, emotionTag);

        if (_emotionIcon != null)
        {
            if (!string.IsNullOrEmpty(emotionTag))
                _emotionIcon.Apply(emotionTag);
            else
                _emotionIcon.Hide();
        }
    }
}
