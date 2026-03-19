using UnityEngine;
using Yarn.Unity;

public class YarnMinigameHooks : MonoBehaviour
{
    [YarnCommand("finalizarMinigame")]
    public void FinalizarMinigame()
    {
        Debug.Log("Yarn: finalizarMinigame chamado");
    }
}
