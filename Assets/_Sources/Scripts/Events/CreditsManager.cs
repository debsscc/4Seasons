using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class CreditsManager : MonoBehaviour
{
    public ScrollAnimation scrollAnimation; 
    public GameObject imagemAnuario;
    public GameObject fadePrefab; 

    public float tempoDeEspera = 5f; 
    public float duracaoDoFade = 1.5f; 
    public string nomeDaCenaMenu = "MenuPrincipal"; 

    private FadeController fadeControllerInstance;

    void Start()
    {
        if (imagemAnuario != null) imagemAnuario.SetActive(false);
        if (scrollAnimation != null) scrollAnimation.gameObject.SetActive(false);
        IniciarSequenciaDeCreditos();
    }
    
    public void IniciarSequenciaDeCreditos()
    {

        if (scrollAnimation != null)
        {
            scrollAnimation.OnScrollFinished += IniciarTransicaoParaMenu; 
        }
        if (imagemAnuario != null) 
        {
            imagemAnuario.SetActive(true);
        }
        StartCoroutine(SincronizarCreditos());
    }

    private IEnumerator SincronizarCreditos()
    {
        yield return new WaitForSeconds(tempoDeEspera);
        if (imagemAnuario != null)
        {
            imagemAnuario.SetActive(false);
        }

        if (scrollAnimation != null)
        {
            scrollAnimation.StartScroll();
        }

    }

    public void IniciarTransicaoParaMenu()
    {
        if (scrollAnimation != null)
        {
            scrollAnimation.OnScrollFinished -= IniciarTransicaoParaMenu;
        }

        if (fadePrefab != null)
        {
            GameObject fadeObject = Instantiate(fadePrefab);
            DontDestroyOnLoad(fadeObject); 

            fadeControllerInstance = fadeObject.GetComponent<FadeController>();

            if (fadeControllerInstance != null)
            {
                StartCoroutine(TransicionarParaMenu());
            }
            else
            {
                SceneManager.LoadScene(nomeDaCenaMenu); // Fallback
            }
        }
        else
        {
            SceneManager.LoadScene(nomeDaCenaMenu); // Fallback
        }
    }

    private IEnumerator TransicionarParaMenu()
    {

        yield return StartCoroutine(fadeControllerInstance.FadeOut(duracaoDoFade));

        SceneManager.LoadScene(nomeDaCenaMenu);
    }
}