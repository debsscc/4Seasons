using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRelations : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] CharacterSlider[] characterSliders;

    public float inYPosition;
    public float outYPosition;

    public void OnPointerEnter(PointerEventData eventData)
    {
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.DOKill();
        rectTransform.DOAnchorPosY(inYPosition, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.DOKill();
        rectTransform.DOAnchorPosY(outYPosition, 0.2f);
    }

    void Update()
    {
        foreach (var characterSlider in characterSliders)
        {
            float maxValue = characterSlider.character._maxRelationshipScore;
            float currentValue = characterSlider.character.RelationshipScore;
            float fillAmount = Mathf.Clamp01(currentValue / maxValue);

            characterSlider.sliderImage.fillAmount = fillAmount;
        }
    }
}

[System.Serializable]
public class CharacterSlider
{
    public CharacterData character;
    public Image sliderImage;
}