using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class StartMenuController : MonoBehaviour
{
    public GameObject optionsModal;
    public GameObject sceneLoadingScreen;

    [BoxGroup("Settings")]
    [LabelText("Loading Delay")]
    [SuffixLabel("s", Overlay = true)]
    [MinValue(0f)]
    public float loadingDelay = 1f;

    

    void Start()
    {
        if (optionsModal != null)
            optionsModal.SetActive(false);

        if (sceneLoadingScreen != null)
            sceneLoadingScreen.SetActive(false);
    }

    public void OnStartClick()
    {
        StartCoroutine(StartGameWithDelay());
    }

    private IEnumerator StartGameWithDelay()
    {
        // Mostra a tela de carregamento
        if (sceneLoadingScreen != null)
            sceneLoadingScreen.SetActive(true);

        // Espera o tempo configurável
        if (loadingDelay > 0)
            yield return new WaitForSeconds(loadingDelay);

        // Inicia a transição de cena com fade out
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.ChangeScene("MapSeletor");
    }

    public void onOptionsClick()
    {
        if (optionsModal != null)
            optionsModal.SetActive(true);
    }

    public void onCloseOptionsModal()
    {
        if (optionsModal != null)
            optionsModal.SetActive(false);
    }

    public void onExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}