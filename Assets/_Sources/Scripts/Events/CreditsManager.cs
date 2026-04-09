using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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

    [Header("Audio")]
    [Tooltip("Trilha específica dos créditos (opcional). Se atribuída, ela não será parada ao entrar na cena.")]
    [SerializeField] private AudioSource creditosMusic;

    [Header("Overlay Mode")]
    [Tooltip("Se true, ao final não troca de cena — apenas fecha o overlay e invoca OnOverlayClosed.")]
    public bool isOverlay = false;
    public System.Action OnOverlayClosed;
    public float overlayFadeOutDuration = 0.4f;

    [Header("Overlap")]
    [Tooltip("Seconds before the anuario scroll ends when the credits scroll should begin.")]
    public float creditosStartBeforeAnuarioEnd = 2f;

    private FadeController fadeControllerInstance;
    private bool creditosStarted;
    private bool _transitionStarted;

    void Start()
    {
        // Ao entrar na cena de créditos, parar qualquer música anterior que ainda esteja tocando,
        // exceto a trilha configurada especificamente para os créditos (se houver).
        if (!isOverlay)
        {
            var allAudioSources = FindObjectsOfType<AudioSource>();
            foreach (var src in allAudioSources)
            {
                if (src == null) continue;
                if (creditosMusic != null && src == creditosMusic) continue;
                if (src.isPlaying)
                    src.Stop();
            }
        }

        if (imagemAnuario != null) imagemAnuario.SetActive(false);
        if (imagemAnuarioScroll != null) imagemAnuarioScroll.gameObject.SetActive(false);
        if (creditosScroll != null) creditosScroll.gameObject.SetActive(false);

        if (isOverlay && creditosMusic != null)
            creditosMusic.Play();

        IniciarSequenciaDeCreditos();
    }

    public void IniciarSequenciaDeCreditos()
    {
        creditosStarted = false;

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

    if (isOverlay)
    {
        if (_transitionStarted) return;
        StartCoroutine(FadeOutOverlay());
        return;
    }

    if (fadePrefab != null)
    {
        GameObject fadeObject = Instantiate(fadePrefab);
        fadeObject.SetActive(true);
        DontDestroyOnLoad(fadeObject);
        fadeControllerInstance = fadeObject.GetComponent<FadeController>();
        if (fadeControllerInstance != null)
        {
            StartCoroutine(TransicionarParaMenu());
        }
        else
        {
            GameSessionManager.Instance?.ResetSession();
            SceneManager.LoadScene(nomeDaCenaMenu);
        }
    }
    else
    {
        GameSessionManager.Instance?.ResetSession();
        SceneManager.LoadScene(nomeDaCenaMenu);
    }
}

    private IEnumerator FadeOutOverlay()
    {
        _transitionStarted = true;

        var canvases = GetComponentsInChildren<Canvas>(true);
        var cgs = new List<CanvasGroup>();
        foreach (var c in canvases)
        {
            var cg = c.GetComponent<CanvasGroup>();
            if (cg == null) cg = c.gameObject.AddComponent<CanvasGroup>();
            cgs.Add(cg);
        }

        if (cgs.Count > 0)
        {
            var seq = DOTween.Sequence().SetUpdate(true);
            foreach (var cg in cgs)
                seq.Join(cg.DOFade(0f, overlayFadeOutDuration).SetUpdate(true));
            yield return seq.WaitForCompletion();
        }

        OnOverlayClosed?.Invoke();
    }

    private IEnumerator TransicionarParaMenu()
    {
        // Faz o fade
    yield return StartCoroutine(fadeControllerInstance.FadeOut(duracaoDoFade));
    // Reseta sessão
    GameSessionManager.Instance?.ResetSession();
    // Destroi os singletons/persistentes que não queremos no menu
    if (GameSessionManager.Instance != null)
        Destroy(GameSessionManager.Instance.gameObject);
    if (MapSelectionManager.Instance != null)
        Destroy(MapSelectionManager.Instance.gameObject);
    if (GameFlowManager.Instance != null)
        Destroy(GameFlowManager.Instance.gameObject);
    if (fadeControllerInstance != null)
        Destroy(fadeControllerInstance.gameObject);
    // Carrega o menu limpo
    SceneManager.LoadScene(nomeDaCenaMenu);
}
}
