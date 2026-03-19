using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

public class FadeController : MonoBehaviour
{
    public Image telaDeFade;

    void Awake()
    {
        // Allow the Image to be on the same GameObject or in a child (common for UI wrappers).
        telaDeFade = GetComponent<Image>();
        if (telaDeFade == null)
            telaDeFade = GetComponentInChildren<Image>(includeInactive: true);

        if (telaDeFade == null)
        {
            Debug.LogError("FadeController requer um componente Image no mesmo GameObject ou em um filho.");
        }
    }

    public IEnumerator FadeOut(float duration)
    {
        if (telaDeFade == null)
            telaDeFade = GetComponent<Image>();

        if (telaDeFade == null)
            telaDeFade = GetComponentInChildren<Image>(includeInactive: true);

        if (telaDeFade == null)
        {
            Debug.LogError("FadeController requer um componente Image no mesmo GameObject ou em um filho.");
            yield break;
        }

        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        if (!telaDeFade.enabled)
            telaDeFade.enabled = true;

        // Ensure we start from fully transparent so the fade out is visible.
        telaDeFade.color = new Color(telaDeFade.color.r, telaDeFade.color.g, telaDeFade.color.b, 0f);

        float tempoInicial = Time.time;
        float alphaAtual = 0f;

        float duracao = Mathf.Max(0.01f, duration);

        while (alphaAtual < 1f)
        {
            float tempoDecorrido = Time.time - tempoInicial;
            alphaAtual = Mathf.Clamp01(tempoDecorrido / duracao);
            
            telaDeFade.color = new Color(telaDeFade.color.r, telaDeFade.color.g, telaDeFade.color.b, alphaAtual); 

            yield return null;
        }
    }
    
    public IEnumerator FadeIn(float duration)
    {
        if (telaDeFade == null)
            telaDeFade = GetComponent<Image>();

        if (telaDeFade == null)
            telaDeFade = GetComponentInChildren<Image>(includeInactive: true);

        if (telaDeFade == null)
        {
            Debug.LogError("FadeController requer um componente Image no mesmo GameObject ou em um filho.");
            yield break;
        }

        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        if (!telaDeFade.enabled)
            telaDeFade.enabled = true;

        // Ensure we start from fully opaque so the fade in actually shows.
        telaDeFade.color = new Color(telaDeFade.color.r, telaDeFade.color.g, telaDeFade.color.b, 1f);

        float tempoInicial = Time.time;
        float alphaAtual = 1f; 

        float duracao = Mathf.Max(0.01f, duration);

        while (alphaAtual > 0f)
        {
            float tempoDecorrido = Time.time - tempoInicial;
            alphaAtual = 1f - Mathf.Clamp01(tempoDecorrido / duracao); 
            
            telaDeFade.color = new Color(telaDeFade.color.r, telaDeFade.color.g, telaDeFade.color.b, alphaAtual); 

            yield return null;
        }
    }
}