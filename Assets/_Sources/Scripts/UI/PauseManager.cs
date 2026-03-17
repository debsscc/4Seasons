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
        PAUSE.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
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

        PAUSE.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        isPaused = true;

        mainPausePanel.SetActive(true);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        if (settingsPanel.activeSelf || creditsPanel.activeSelf)
        {
            GoToMainPause();
            return;
        }

        PAUSE.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
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
        mainPausePanel.SetActive(true);
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