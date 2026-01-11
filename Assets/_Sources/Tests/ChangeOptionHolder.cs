using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Yarn.Unity;

public class ChangeOptionHolder : MonoBehaviour
{
    [SerializeField] private HorizontalLayoutGroup _layoutGroup;
    [SerializeField] private Button _leftArrowButton;
    [SerializeField] private Button _rightArrowButton;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private float _itemSize = 200f;

    private DialogueRunner _dialogueRunner;
    private float _currentScrollPosition = 0f;
    private int _currentIndex = 0;
    private float _posOffset = 0f;

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
        }
    }

    private OptionItem[] Options => _layoutGroup.GetComponentsInChildren<OptionItem>();

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
            var selectedOption = options[_currentIndex];
            selectedOption.InvokeOptionSelected();
            Debug.Log($"Selected option: {selectedOption}", selectedOption);
        }
    }

}
