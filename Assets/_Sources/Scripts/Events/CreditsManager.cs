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

    [Header("Overlap")]
    [Tooltip("Seconds before the anuario scroll ends when the credits scroll should begin.")]
    public float creditosStartBeforeAnuarioEnd = 2f;

    private FadeController fadeControllerInstance;
    private bool creditosStarted;

    void Start()
    {
        if (imagemAnuario != null) imagemAnuario.SetActive(false);
        if (imagemAnuarioScroll != null) imagemAnuarioScroll.gameObject.SetActive(false);
        if (creditosScroll != null) creditosScroll.gameObject.SetActive(false);

        IniciarSequenciaDeCreditos();
    }

    public void IniciarSequenciaDeCreditos()
    {
        creditosStarted = false;

        // 1) Quando o SCROLL da imagem acabar, faz o cleanup e garante que os créditos estejam rodando
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
            StartCoroutine(StartCreditsWithOverlap());
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

        // Se os créditos ainda não começaram (por algum motivo), garante que comecem agora.
        if (!creditosStarted)
        {
            StartCredits();
        }
    }

    private IEnumerator StartCreditsWithOverlap()
    {
        if (imagemAnuarioScroll == null || creditosScroll == null)
            yield break;

        float anuarioDuration = imagemAnuarioScroll.GetEstimatedScrollDuration();
        float delay = Mathf.Max(0f, anuarioDuration - creditosStartBeforeAnuarioEnd);
        yield return new WaitForSeconds(delay);

        StartCredits();
    }

    private void StartCredits()
    {
        if (creditosStarted)
            return;

        creditosStarted = true;

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
            fadeObject.SetActive(true); // garante que o objeto e o Image possam ser vistos e o coroutine rode
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