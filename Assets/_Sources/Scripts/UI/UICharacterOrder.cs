using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class UICharacterOrder : MonoBehaviour
{
    [SerializeField] private CharacterData character;
    [SerializeField] private TextMeshProUGUI orderText;
    [SerializeField] private Image characterImage;
    [Space]
    [SerializeField] private Image heartImage;
    [SerializeField] private Sprite positiveHeartSprite;
    [SerializeField] private Sprite negativeHeartSprite;

    public CharacterData Character => character;

    void Start()
    {
        MiniGameFeedbackManager.Instance.uiCharacterOrders.RemoveAll(x => x == this);
        MiniGameFeedbackManager.Instance.uiCharacterOrders.Add(this);

        UpdateExpresionBasedOnItem(null);

        MiniGameController miniGameController = FindFirstObjectByType<MiniGameController>();
        var draggables = miniGameController.draggablePrefabs;
        string order = null;
        var seen = new System.Collections.Generic.HashSet<string>();
        Debug.Log("Dragganle amount " + draggables.Count);
        foreach (var draggable in draggables)
        {
            var itemsHolder = draggable.GetComponent<IItemHolder>();

            if (itemsHolder == null) continue;
            if (itemsHolder.Items == null) continue;

            foreach (var item in itemsHolder.Items)
            {
                if (character.LikesItem(item) && !string.IsNullOrEmpty(item.OrderDescription) && seen.Add(item.OrderDescription))
                {
                    order = order == null ? item.OrderDescription : $"{order}, {item.OrderDescription}";
                }
            }
        }

        if (!string.IsNullOrEmpty(order))
            orderText.text = order;
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

    public void UpdateExpressionBasedOnCharacter(int expressionID)
    {
        ExpressionFeedbackSprite feedbackSprites = character.ExpressionFeedbackSprite;
        Sprite newExpression = expressionID switch
        {
             1=> feedbackSprites.happySprite,
            -1 => feedbackSprites.sadSprite,
            0=> feedbackSprites.neutralSprite
        };
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

    public float heartDisplayDuration = 3f;

    public void PunchScale()
    {
        characterImage.transform.DOKill();
        characterImage.transform.localScale = Vector3.one;
        characterImage.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
    }

    public void ShowHeart(bool positive)
    {
        Debug.Log("ShowHeart called for " + (character ? character.name : "null") + " positive: " + positive);
        if (heartImage == null) 
        {
            Debug.Log("heartImage is null for " + (character ? character.name : "null"));
            return;
        }
        if (positiveHeartSprite == null || negativeHeartSprite == null)
        {
            Debug.Log("Heart sprites not set for " + (character ? character.name : "null"));
            return;
        }
        heartImage.sprite = positive ? positiveHeartSprite : negativeHeartSprite;
        heartImage.gameObject.SetActive(true);
        var color = heartImage.color;
        color.a = 1f;
        heartImage.color = color;
        heartImage.DOKill();
        heartImage.DOFade(0f, heartDisplayDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            if (heartImage != null) heartImage.gameObject.SetActive(false);
        });
    }
}
