using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    public GameObject optionsModal;

    void Start()
    {
        if (optionsModal != null) {
            optionsModal.SetActive(false);
        }
    }

    public void OnStartClick()
    {
        SceneManager.LoadScene("MapSeletor");
    }
    public void onOptionsClick()
    {
        Debug.Log("Options clicked");
        if (optionsModal != null)
        {
            Debug.Log("Isnt null clicked");
            optionsModal.SetActive(true);
        }
    }
    public void onCloseOptionsModal()
    {
        Debug.Log("Close options clicked");
        if (optionsModal != null)
        {
            Debug.Log("Isnt null  CLOSE clicked");
            optionsModal.SetActive(false);
        }
    }

    public void onExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
