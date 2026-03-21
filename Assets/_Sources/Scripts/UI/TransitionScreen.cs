using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Sirenix.OdinInspector;

public class SceneTransition : Singleton<SceneTransition>
{
    [FoldoutGroup("Settings")]
    public float fadeTime = 1f;

    [FoldoutGroup("Settings")]
    public float minLoadingTime = 2f;

    [FoldoutGroup("Settings")]
    public Image fadeImage;

    public GameObject loadingScreen;
    public GameObject contentMenu;
    private string currentSceneName;
    
    protected override void Awake()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        base.Awake();
        Debug.Log("SceneTransition AWAKE — Singleton criado");
    }

    void Start()
    {
        // StartCoroutine(DelayedFadeIn());
    }

    IEnumerator DelayedFadeIn()
    {
        yield return null;
        StartCoroutine(FadeIn());
    }

    public void ChangeScene(string sceneName)
    {
        if (!gameObject.activeInHierarchy || !isActiveAndEnabled)
        {
            Debug.LogWarning("SceneTransition GameObject ou componente está inativo.");
            return;
        }

        bool useLoadingScreen = loadingScreen != null && currentSceneName == "MainMenu";
        if (useLoadingScreen)
        {
            Debug.Log("Activating loading screen");
            loadingScreen.SetActive(true);
            contentMenu.SetActive(false);
        }
        currentSceneName = sceneName;
        Debug.Log($"Starting scene change to {sceneName}");
        StartCoroutine(useLoadingScreen ? FadeOutAndLoadScene(sceneName) : SimpleFadeTransition(sceneName));

    }

    IEnumerator FadeIn()
    {
        Debug.Log("FadeIn started");
        float t = fadeTime;
        Color c = fadeImage.color;

        while (t > 0)
        {
            t -= Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeTime);
            fadeImage.color = c;
            yield return null;
        }
        Debug.Log("FadeIn finished");
    }

    IEnumerator SimpleFadeTransition(string sceneName)
    {
        // Fade out
        float t = 0;
        Color c = fadeImage.color;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeTime);
            fadeImage.color = c;
            yield return null;
        }

        if (sceneName == "MainMenu")
            contentMenu.SetActive(true);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
            yield return null;

        Debug.Log("Scene loaded");
        yield return StartCoroutine(FadeIn());
    }

    IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");

        // Inicia o carregamento em background enquanto a loading screen fica visível
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // Aguarda o tempo mínimo da loading screen e a cena estar pronta
        float loadTimer = 0f;
        while (asyncLoad.progress < 0.9f || loadTimer < minLoadingTime)
        {
            loadTimer += Time.deltaTime;
            yield return null;
        }

        // Fade out: esconde a loading screen
        Debug.Log("FadeOut started");
        float t = 0;
        Color c = fadeImage.color;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeTime);
            fadeImage.color = c;
            yield return null;
        }
        Debug.Log("FadeOut completed");

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
            Debug.Log("Loading screen deactivated");
        }

        if (sceneName == "MainMenu")
            contentMenu.SetActive(true);

        asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone)
            yield return null;

        Debug.Log("Scene loaded");

        // Fade in: revela a nova cena
        yield return StartCoroutine(FadeIn());
    }
}