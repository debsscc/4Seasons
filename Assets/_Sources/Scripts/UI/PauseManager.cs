using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class PauseManager : MonoBehaviour
{
    public GameObject PAUSE;

    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject mainPausePanel;

    [Header("Credits Overlay")]
    [Tooltip("Prefab do Test_New_Credits (deve iniciar desativado).")]
    public GameObject creditsPrefab;

    private bool isPaused = false;
    private GameObject _creditsInstance;
    private CreditsManager _creditsManager;
    private AudioSource[] _pausedForCredits;

    private void Start()
    {
        if (PAUSE != null) PAUSE.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;
    }

    private void Update()
    {
        if (PAUSE == null) return;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (PAUSE == null) return;
        PAUSE.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        isPaused = true;

        if (mainPausePanel != null) mainPausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        // Se os créditos estão abertos, deixa o botão de fechar deles lidar com o retorno
        if (_creditsInstance != null) return;

        bool subPanelOpen = (settingsPanel != null && settingsPanel.activeSelf) ||
                            (creditsPanel != null && creditsPanel.activeSelf);

        if (subPanelOpen)
        {
            GoToMainPause();
            return;
        }

        if (PAUSE != null) PAUSE.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;
    }

    public void OpenSettings()
    {
        if (mainPausePanel != null) mainPausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void GoToMainPause()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (mainPausePanel != null) mainPausePanel.SetActive(true);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        GameSessionManager.Instance?.ResetSession();
        if (GameSessionManager.Instance != null) Destroy(GameSessionManager.Instance.gameObject);
        if (MapSelectionManager.Instance != null) Destroy(MapSelectionManager.Instance.gameObject);
        if (GameFlowManager.Instance != null) Destroy(GameFlowManager.Instance.gameObject);

        SceneTransition.Instance.ChangeScene("MainMenu");
    }

    public void OpenCredits()
    {
        if (creditsPrefab == null) return;
        if (mainPausePanel != null) mainPausePanel.SetActive(false);
        if (PAUSE != null) PAUSE.SetActive(false);

        // Restaura o tempo para os scrolls animarem corretamente
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Pausa todos os AudioSources ativos ANTES de instanciar os créditos
        var all = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        System.Collections.Generic.List<AudioSource> paused = new();
        foreach (var src in all)
        {
            if (src.isPlaying)
            {
                src.Pause();
                paused.Add(src);
            }
        }
        _pausedForCredits = paused.ToArray();

        _creditsInstance = Instantiate(creditsPrefab);
        var mgr = _creditsInstance.GetComponentInChildren<CreditsManager>(includeInactive: true);

        if (mgr != null)
        {
            mgr.isOverlay = true;
            mgr.OnOverlayClosed = OnCreditsClosed;
            _creditsManager = mgr;
        }

        // Fiação do botão de fechar (Button_Menu dentro do prefab)
        var closeBtn = _creditsInstance.GetComponentInChildren<Button>(includeInactive: true);
        if (closeBtn != null)
            closeBtn.onClick.AddListener(CloseCredits);

        _creditsInstance.SetActive(true);
    }

    public void CloseCredits()
    {
        if (_creditsManager != null)
            _creditsManager.IniciarTransicaoParaMenu();
        else
            OnCreditsClosed();
    }

    private void OnCreditsClosed()
    {
        _creditsManager = null;

        if (_creditsInstance != null)
            Destroy(_creditsInstance);
        _creditsInstance = null;

        // Restaura os AudioSources que foram pausados para os créditos
        if (_pausedForCredits != null)
        {
            foreach (var src in _pausedForCredits)
            {
                if (src != null)
                    src.UnPause();
            }
            _pausedForCredits = null;
        }

        // Volta ao estado de pause
        if (PAUSE != null) PAUSE.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        GoToMainPause();
    }
}
