using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NUnit.Framework;

public class UICharacterOrder : MonoBehaviour
{
    [SerializeField] private CharacterData character;
    [SerializeField] private TextMeshProUGUI orderText;
    [SerializeField] private Image characterImage;
    [Space]
    [SerializeField] private Image heartImage;
    [SerializeField] private Sprite positiveHeartSprite;
    [SerializeField] private Sprite negativeHeartSprite;

    void Start()
    {
        UpdateExpresionBasedOnItem(null);

        MiniGameController miniGameController = FindFirstObjectByType<MiniGameController>();
        var draggables = miniGameController.draggablePrefabs;
        string order = null;
        foreach (var draggable in draggables)
        {
            if (draggable.Items == null) continue;
            foreach (var item in draggable.Items)
            {
                if (character.LikesItem(item))
                {
                    if (order == null)
                        order = item.OrderDescription;
                    else
                        order += $", {item.OrderDescription}";

                    orderText.text = order;
                }
            }
        }

        MiniGameFeedbackManager.Instance.uiCharacterOrders.Add(this);
    }

    public void UpdateExpresionBasedOnItem(ItemsSO item)
    {
        ExpressionFeedbackSprite feedbackSprites = character.ExpressionFeedbackSprite;
        if (item == null)
        {
            characterImage.sprite = feedbackSprites.neutralSprite;
            return;
        }

        Sprite newExpression = character.LikesItem(item) ? feedbackSprites.happySprite : feedbackSprites.sadSprite;
        characterImage.sprite = newExpression;
    }

    public void DisplayHeartFeedbackBasedOnItem(ItemsSO item)
    {
        if (item == null || heartImage == null)
        {
            heartImage.gameObject.SetActive(false);
            return;
        }

        heartImage.gameObject.SetActive(true);

        Sprite heartSprite = character.LikesItem(item) ? positiveHeartSprite : negativeHeartSprite;
        heartImage.sprite = heartSprite;
    }

    public bool CharacterLikesItem(ItemsSO item)
    {
        return character.LikesItem(item);
    }
}
