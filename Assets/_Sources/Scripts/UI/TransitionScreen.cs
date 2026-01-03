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

    protected override void Awake()
    {
            base.Awake();
    Debug.Log("SceneTransition AWAKE — Singleton criado");
    DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(DelayedFadeIn());
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

        if (loadingScreen != null)
        {
            Debug.Log("Activating loading screen");
            loadingScreen.SetActive(true);
        }

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

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            Debug.Log($"Loading progress: {asyncLoad.progress}");
            yield return null;
        }

        Debug.Log("Scene almost loaded, activating...");
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        Debug.Log("Scene loaded");

        yield return StartCoroutine(FadeIn());
        Debug.Log("FadeIn completed");

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
            Debug.Log("Loading screen deactivated");
        }
    }
}