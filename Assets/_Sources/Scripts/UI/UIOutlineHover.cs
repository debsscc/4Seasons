using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(UIOutline))]
public class UIOutlineHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float _fadeDuration = 0.2f;
    [SerializeField] private float _hoverAlpha = 1f;

    private UIOutline _outline;
    private Tween _tween;

    private void Awake()
    {
        // Ensure we can receive pointer events.
        var graphic = GetComponent<Graphic>();
        if (graphic != null)
            graphic.raycastTarget = true;

        _outline = GetComponent<UIOutline>();
        if (_outline == null) return;

        _outline.enabled = true;
        var c = _outline.effectColor;
        c.a = 0f;
        _outline.effectColor = c;
    }

    private void OnDestroy()
    {
        _tween?.Kill();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        FadeTo(_hoverAlpha);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FadeTo(0f);
    }

    private void FadeTo(float alpha)
    {
        if (_outline == null) return;

        _tween?.Kill();
        var target = _outline.effectColor;
        target.a = alpha;

        _tween = DOTween.To(
                () => _outline.effectColor,
                x => _outline.effectColor = x,
                target,
                _fadeDuration
            )
            .SetEase(Ease.OutQuad);
    }
}
