using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;
using UnityEngine.EventSystems;

public class DialogueOptionItem : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Referências do Prefab")]
    public TMP_Text optionText;          // Bubble/Text (TMP)
    public Image arrowLeft;              // Bubble/Option_Arrow_Left
    public Image arrowRight;             // Bubble/Option_Arrow_Right
    public Image moodIcon;               // Name/MoodIcon
    public TMP_Text characterNameText;   // Name/Text (TMP)
    public Image bubbleBackground;       // Bubble (Image)

    [Header("Ícones de Mood")]
    public Sprite goodSprite;
    public Sprite badSprite;
    public Sprite neutralSprite;

    [Header("Cores")]
    public Color normalTextColor = Color.white;
    public Color highlightedTextColor = Color.yellow;
    public Color normalBubbleColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    public Color highlightedBubbleColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    private System.Action onClickCallback;
    private bool isHighlighted = false;

    public void Setup(DialogueOption option, int index, System.Action onClick)
    {
        // Define o texto da opção
        optionText.text = option.Line.TextWithoutCharacterName.Text;
        onClickCallback = onClick;

        // Define o nome do personagem (se houver)
        if (characterNameText != null && !string.IsNullOrEmpty(option.Line.CharacterName))
        {
            characterNameText.text = option.Line.CharacterName;
        }

        // Define o ícone de mood baseado em tags
        string moodTag = GetMoodTag(option);
        if (moodIcon != null)
        {
            switch (moodTag)
            {
                case "good":
                    moodIcon.sprite = goodSprite;
                    moodIcon.color = Color.green;
                    moodIcon.gameObject.SetActive(true);
                    break;
                case "bad":
                    moodIcon.sprite = badSprite;
                    moodIcon.color = Color.red;
                    moodIcon.gameObject.SetActive(true);
                    break;
                default:
                    moodIcon.gameObject.SetActive(false);
                    break;
            }
        }

        SetHighlighted(false);
    }

    private string GetMoodTag(DialogueOption option)
    {
        // Procura por tags #good ou #bad no texto da opção
        foreach (var attr in option.Line.Text.Attributes)
        {
            if (attr.Name == "good") return "good";
            if (attr.Name == "bad") return "bad";
        }
        return "neutral";
    }

    public void SetHighlighted(bool highlighted)
    {
        isHighlighted = highlighted;

        // Muda a cor do texto
        if (optionText != null)
            optionText.color = highlighted ? highlightedTextColor : normalTextColor;

        // Muda a cor do fundo do balão
        if (bubbleBackground != null)
            bubbleBackground.color = highlighted ? highlightedBubbleColor : normalBubbleColor;

        // Mostra/esconde as setas
        if (arrowLeft != null)
            arrowLeft.enabled = highlighted;
        if (arrowRight != null)
            arrowRight.enabled = highlighted;

        // Efeito de escala (opcional, tipo Night in the Woods)
        transform.localScale = highlighted ? Vector3.one * 1.05f : Vector3.one;
    }

    // Suporte para mouse hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHighlighted(true);
    }

    // Suporte para clique do mouse
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick();
    }

    public void OnClick()
    {
        onClickCallback?.Invoke();
    }
}