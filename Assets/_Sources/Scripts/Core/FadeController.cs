using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

public class FadeController : MonoBehaviour
{
    public Image telaDeFade;

    void Awake()
    {
        telaDeFade = GetComponent<Image>();
        if (telaDeFade == null)
        {
            Debug.LogError("FadeController requer um componente Image no mesmo GameObject.");
        }
    }

    public IEnumerator FadeOut(float duration)
    {
        float tempoInicial = Time.time;
        float alphaAtual = telaDeFade.color.a;

        while (alphaAtual < 1f)
        {
            float tempoDecorrido = Time.time - tempoInicial;
            alphaAtual = Mathf.Clamp01(tempoDecorrido / duration);
            
            telaDeFade.color = new Color(telaDeFade.color.r, telaDeFade.color.g, telaDeFade.color.b, alphaAtual); 

            yield return null;
        }
    }
    
    public IEnumerator FadeIn(float duration)
    {
        float tempoInicial = Time.time;
        float alphaAtual = telaDeFade.color.a; 

        while (alphaAtual > 0f)
        {
            float tempoDecorrido = Time.time - tempoInicial;
            alphaAtual = 1f - Mathf.Clamp01(tempoDecorrido / duration); 
            
            telaDeFade.color = new Color(telaDeFade.color.r, telaDeFade.color.g, telaDeFade.color.b, alphaAtual); 

            yield return null;
        }
    }
}