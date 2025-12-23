using UnityEngine;
using Yarn.Unity;
using UnityEngine.SceneManagement;

public class YarnMinigameHooks : MonoBehaviour
{
    [YarnCommand("finalizarMinigame")]
    public void FinalizarMinigame()
    {
        Debug.Log("Yarn: finalizarMinigame chamado");

        // Marca mapa atual como completo
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.MarkCurrentMapAsCompleted();
            GameSessionManager.Instance.ReturnToMapSelection();
        }
        else
        {
            // Fallback
            SceneManager.LoadScene("MapSeletor");
        }
    }
}