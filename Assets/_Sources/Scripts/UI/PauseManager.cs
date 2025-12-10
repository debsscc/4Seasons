using UnityEngine;
using UnityEngine.SceneManagement;


public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;

    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject mainPausePanel;

    public GameObject popUpQuit;

    private bool isPaused = false;

    private void Start()
    {
        pauseMenuUI.SetActive(false);
        popUpQuit.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (popUpQuit.activeSelf) return;

        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        mainPausePanel.SetActive(true);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        if (settingsPanel.activeSelf || creditsPanel.activeSelf || popUpQuit.activeSelf)
        {
            GoToMainPause();
            return;
        }

        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OpenSettings()
    {
        mainPausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void GoToMainPause()
    {
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        popUpQuit.SetActive(false); 
        mainPausePanel.SetActive(true);
    }

    public void ConfirmQuit()
    {
        mainPausePanel.SetActive(false);
        popUpQuit.SetActive(true);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitApplication()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OpenCredits()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Credits");
    }
}