using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneLoader : MonoBehaviour
{
 [SerializeField] public Object nextScene;   

    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextScene.name);
    }

}
