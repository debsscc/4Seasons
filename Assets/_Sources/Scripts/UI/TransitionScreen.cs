using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Sirenix.OdinInspector;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    [FoldoutGroup("Settings")]
    public float fadeTime = 1f;

    [FoldoutGroup("Settings")]
    public Image fadeImage;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine((DelayedFadeIn()));
    }

        IEnumerator DelayedFadeIn()
    {
        yield return null; // espera um frame
        StartCoroutine(FadeIn());
    }

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(FadeOut(sceneName));
    }

    IEnumerator FadeIn()
    {
        float t = fadeTime;
        Color c = fadeImage.color;
        
        while (t > 0)
        {
            t -= Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeTime);
            fadeImage.color = c;
            yield return null;
        }
    }

    IEnumerator FadeOut(string sceneName)
    {
        float t = 0;
        Color c = fadeImage.color;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeTime);
            fadeImage.color = c;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }
}

// to use in another scripts : SceneTransition.Instance.ChangeScene("nomeDaCena");
