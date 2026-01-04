using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneLoader : MonoBehaviour
{
 [SerializeField] public Object nextScene;   

    public void LoadNextScene()
    {
        Debug.Log($"Loading next scene: {nextScene.name} NXL");
        SceneManager.LoadScene(nextScene.name);
    }

}
