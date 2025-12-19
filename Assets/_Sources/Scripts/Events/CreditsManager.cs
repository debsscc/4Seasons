using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsManager : MonoBehaviour
{
    [Header("Scrolls")]
    public ScrollAnimation imagemAnuarioScroll;  
    public ScrollAnimation creditosScroll;       

    [Header("Referências")]
    public GameObject imagemAnuario;         
    public GameObject fadePrefab;

    [Header("Config")]
    public float duracaoDoFade = 1.5f;
    public string nomeDaCenaMenu = "MenuPrincipal";

    private FadeController fadeControllerInstance;

    void Start()
    {
        if (imagemAnuario != null) imagemAnuario.SetActive(false);
        if (imagemAnuarioScroll != null) imagemAnuarioScroll.gameObject.SetActive(false);
        if (creditosScroll != null) creditosScroll.gameObject.SetActive(false);

        IniciarSequenciaDeCreditos();
    }

    public void IniciarSequenciaDeCreditos()
    {
        // 1) Quando o SCROLL da imagem acabar, começa o SCROLL dos créditos
        if (imagemAnuarioScroll != null)
        {
            imagemAnuarioScroll.OnScrollFinished += OnScrollImagemFinalizado;
        }

        if (creditosScroll != null)
        {
            creditosScroll.OnScrollFinished += IniciarTransicaoParaMenu;
        }

        if (imagemAnuario != null)
        {
            imagemAnuario.SetActive(true);
        }

        if (imagemAnuarioScroll != null)
        {
            imagemAnuarioScroll.StartScroll();
        }
    }

    private void OnScrollImagemFinalizado()
    {
        if (imagemAnuarioScroll != null)
        {
            imagemAnuarioScroll.OnScrollFinished -= OnScrollImagemFinalizado;
        }

        if (imagemAnuario != null)
        {
            imagemAnuario.SetActive(false);
        }

        if (creditosScroll != null)
        {
            creditosScroll.StartScroll();
        }
    }

    public void IniciarTransicaoParaMenu()
    {
        if (creditosScroll != null)
        {
            creditosScroll.OnScrollFinished -= IniciarTransicaoParaMenu;
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
                SceneManager.LoadScene(nomeDaCenaMenu);
            }
        }
        else
        {
            SceneManager.LoadScene(nomeDaCenaMenu);
        }
    }

    private IEnumerator TransicionarParaMenu()
    {
        yield return StartCoroutine(fadeControllerInstance.FadeOut(duracaoDoFade));
        SceneManager.LoadScene(nomeDaCenaMenu);
    }
}