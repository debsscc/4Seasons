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
        SceneTransition.Instance.ChangeScene("MapSeletor");
    }
    public void onOptionsClick()
    {
        if (optionsModal != null)
        {
            optionsModal.SetActive(true);
        }
    }
    public void onCloseOptionsModal()
    {
        if (optionsModal != null)
        {
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
