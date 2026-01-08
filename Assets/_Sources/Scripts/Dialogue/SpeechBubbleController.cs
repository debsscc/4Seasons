using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeechBubbleController : MonoBehaviour
{
    public Image backgroundImage;
    public Image emotionImage;
    public Animator emotionAnimator;
    public TextMeshProUGUI dialogueText;

    public void ShowSpeech(string text, Sprite emotionSprite, AnimationClip emotionAnim, bool useAnimation)
    {
        dialogueText.text = text;

        if (useAnimation && emotionAnim != null && emotionAnimator != null)
        {
            emotionAnimator.enabled = true;
            emotionAnimator.Play(emotionAnim.name);
            emotionImage.enabled = false;
        }
        else if (emotionSprite != null)
        {
            emotionAnimator.enabled = false;
            emotionImage.sprite = emotionSprite;
            emotionImage.enabled = true;
        }
        else
        {
            emotionAnimator.enabled = false;
            emotionImage.enabled = false;
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}