using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public TextMeshProUGUI textField;
    public Image portrait;
    public Image expression;
    public GameObject continueIcon;

    public float typeSpeed = 0.04f; //GameDesigner adjustable

    public void ShowLine(DialogueLine line)
    {
        //portrait.sprite = line.speaker.portrait;
        expression.sprite = line.expression;

        StopAllCoroutines();
        StartCoroutine(TypeLine(line.text, line.blipSound));
    }

    IEnumerator TypeLine(string text, AudioClip blipSound)
    {
        textField.text = "";
        continueIcon.SetActive(false);

        foreach(var c in text)
        {
            textField.text += c;
            if (!char.IsWhiteSpace(c) && blipSound != null)
                AudioSource.PlayClipAtPoint(blipSound, Vector3.zero);

            yield return new WaitForSeconds(typeSpeed);
        }

        continueIcon.SetActive(true);
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
