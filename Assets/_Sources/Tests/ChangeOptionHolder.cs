using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ChangeOptionHolder : MonoBehaviour
{
    [SerializeField] private HorizontalLayoutGroup _layoutGroup;
    [SerializeField] private Button _leftArrowButton;
    [SerializeField] private Button _rightArrowButton;
    [SerializeField] private float _itemSize = 200f;

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
            Debug.Log($"current post set to {_posOffset}");
            Debug.Log($"scroll amount: {ScrollAmount}");
            CurrentLayoutPosition = -_currentIndex * ScrollAmount + _posOffset;
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

        _posOffset = _layoutGroup.transform.localPosition.x;
    }

}
