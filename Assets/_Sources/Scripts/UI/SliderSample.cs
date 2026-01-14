using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SliderSample : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;

    [Header("Data")]
    [SerializeField] private CharacterData character;

    [Header("Visual")]
    [SerializeField] private List<Color> colorTresholds;

    public int CurrentValue => character != null ? character.RelationshipScore : 0;

    private void Start()
    {

        if (fillImage == null || character == null)
        {
            Debug.LogError("[FillBarSample] Referências não atribuídas.");
            enabled = false;
            return;
        }

        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        UpdateFill(character.RelationshipScore);
    }

    private void OnEnable()
    {
        if (character != null)
            character.OnRelationshipChanged += UpdateFill;
    }

    private void OnDisable()
    {
        if (character != null)
            character.OnRelationshipChanged -= UpdateFill;
    }

    private void UpdateFill(int newValue)
    {
        float normalizedValue = Mathf.InverseLerp(
            0,
            character._maxRelationshipScore,
            newValue
        );

        fillImage.fillAmount = normalizedValue;

        ApplyValueColor(newValue);
    }

    private void ApplyValueColor(int value)
    {
        int colorDenied = 0;

        for (int i = character._relationshipTresholds.Count - 1; i >= 0; i--)
        {
            if (value <= character._relationshipTresholds[i])
            {
                colorDenied++;
                continue;
            }
            break;
        }

        int colorId = colorTresholds.Count - 1 - colorDenied;
        colorId = Mathf.Clamp(colorId, 0, colorTresholds.Count - 1);

        fillImage.color = colorTresholds[colorId];
    }
}
