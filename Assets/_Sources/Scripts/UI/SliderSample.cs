using NUnit.Framework;
using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;

public class SliderSample : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private CharacterData _character;
    [SerializeField] List<Color> colorTresholds;

    public int CurrentValue => (int)_slider.value;

    public void Start()
    {
        ChangeSliderValue(_character.RelationshipScore);
        _slider.maxValue = _character._maxRelationshipScore;
        _slider.minValue = 0;
        _character.RelationshipScore = _character.RelationshipScore;
        _slider.fillRect.GetComponent<Image>().color = colorTresholds[0]; 
        ChangeSliderValue(_character.RelationshipScore);
    }

    void OnEnable()
    {
        _character.OnRelationshipChanged += ChangeSliderValue;
    }

    void OnDisable()
    {
        _character.OnRelationshipChanged -= ChangeSliderValue;
    }

    void ChangeSliderValue(int newValue)
    {
        _slider.DOValue(newValue, 0.3f).SetEase(Ease.OutCubic);
        ApplyColorBasedOnValue(newValue);
    }

    public void ApplyColorBasedOnValue(int value)
    {
        int colorDenied = 0;
        Image fillImage = _slider.fillRect?.GetComponent<Image>();
        if (fillImage == null)
        {
            Debug.LogError("O Slider não possui um componente Image no Fill Rect. Verifique a configuração.");
            return;
        }


        for (int i = _character._relationshipTresholds.Count - 1; i >= 0; i--)
        {
            if (value <= _character._relationshipTresholds[i])
            {
                colorDenied++;
                continue;
            }
            break;
        }

        int colorId = colorTresholds.Count - 1 - colorDenied;
        colorId = Mathf.Clamp(colorId, 0, colorTresholds.Count - 1);

        _slider.fillRect.GetComponent<Image>().DOKill(); // Cancela tweens anteriores
        _slider.fillRect.GetComponent<Image>().DOColor(colorTresholds[colorId], 1f);
    }
}