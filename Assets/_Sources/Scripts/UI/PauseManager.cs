using UnityEngine;
using UnityEngine.SceneManagement;


public class PauseManager : MonoBehaviour
{
    public GameObject PAUSE;

    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject mainPausePanel;

    private bool isPaused = false;

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
        if (Input.GetKeyDown(KeyCode.Escape))
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
        if (settingsPanel != null && creditsPanel != null &&
            (settingsPanel.activeSelf || creditsPanel.activeSelf))
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
        SceneTransition.Instance.ChangeScene("MainMenu");
    }

    public void OpenCredits()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneTransition.Instance.ChangeScene("Credits");
    }
}
