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
    public Image fadeImage;

    public GameObject loadingScreen;
    public GameObject contentMenu;
    private string currentSceneName;
    
    protected override void Awake()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        base.Awake();
        Debug.Log("SceneTransition AWAKE — Singleton criado");
        DontDestroyOnLoad(gameObject);

        Animator anim = new();

        int animToInt = Animator.StringToHash("Normal");
        anim.SetTrigger(animToInt);
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

        if (loadingScreen != null && currentSceneName == "MainMenu")
        {
            Debug.Log("Activating loading screen");
            loadingScreen.SetActive(true);
            contentMenu.SetActive(false);
        }
        currentSceneName = sceneName;
        Debug.Log($"Starting scene change to {sceneName}");
        StartCoroutine(FadeOutAndLoadScene(sceneName));

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

    IEnumerator FadeOutAndLoadScene(string sceneName)
    {
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
        Debug.Log($"Loading scene: {sceneName}");
        if (sceneName == "MainMenu")
        {
            contentMenu.SetActive(true);
        }


        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        Debug.Log("Scene loaded");

        yield return StartCoroutine(FadeIn());

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
            Debug.Log("Loading screen deactivated");
        }
    }
}