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
    private bool _musicPausedForCredits;

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
        if (_creditsInstance != null)
        {
            Destroy(_creditsInstance);
            _creditsInstance = null;
            _creditsManager = null;
        }

        // Para o MusicSource. AudioListener.pause permanece true durante o fade;
        // SceneTransition.OnSceneLoaded o libera após a cena carregar.
        AudioManager.Instance?.StopMusic();

        Time.timeScale = 1f;

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

        // Quando AudioListener.pause = true, isPlaying retorna false para todas as sources.
        // Pausamos as fontes de música explicitamente antes de soltar o listener.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseMusic();
            _musicPausedForCredits = true;
        }

        AudioListener.pause = false;

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

        if (_musicPausedForCredits && AudioManager.Instance != null)
        {
            AudioManager.Instance.UnpauseMusic();
            _musicPausedForCredits = false;
        }

        // Volta ao estado de pause
        if (PAUSE != null) PAUSE.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        GoToMainPause();
    }
}
