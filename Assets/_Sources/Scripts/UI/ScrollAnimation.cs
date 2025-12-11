using UnityEngine;
using System.Collections;
using System; // Necessário para Action

public class ScrollAnimation : MonoBehaviour
{
    public Action OnScrollFinished; 

    public float scrollSpeed = 50f; //game designer pode alterar
    public float scrollDuration = 20f; // game designer pode alterar

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void StartScroll()
    {
        gameObject.SetActive(true);
        StartCoroutine(ScrollCoroutine());
    }

    private IEnumerator ScrollCoroutine()
    {
        float initialY = -rectTransform.rect.height; 
        float finalY = Screen.height + rectTransform.rect.height; 

        float timeElapsed = 0f;

        while (timeElapsed < scrollDuration)
        {
            timeElapsed += Time.deltaTime;
            
            float newY = Mathf.Lerp(initialY, finalY, timeElapsed / scrollDuration);
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newY);

            yield return null; 
        }

        // Garante que o objeto chegue à posição final
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, finalY);

        // A animação terminou! Notifica o CreditsManager.
        if (OnScrollFinished != null)
        {
            OnScrollFinished.Invoke();
        }
        
        // Desativa o próprio objeto se não for mais necessário
        gameObject.SetActive(false); 
    }
}