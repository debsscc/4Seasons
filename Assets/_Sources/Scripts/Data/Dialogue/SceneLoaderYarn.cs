using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class SceneLoaderYarn : MonoBehaviour
{
    public string sceneToLoad;
    public DialogueRunner dialogueRunner; // arraste no Inspector ou deixe nulo para achar automaticamente

    void Awake()
    {
        Debug.Log("[SceneLoaderYarn] Awake chamado!");

        if (dialogueRunner == null)
        {
            //dialogueRunner = FindObjectOfType<DialogueRunner>();
            Debug.Log("[SceneLoaderYarn] Procurando DialogueRunner...");
        }

        if (dialogueRunner != null)
        {
            Debug.Log("[SceneLoaderYarn] DialogueRunner encontrado! Registrando evento...");
            dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
        }
        else
        {
            Debug.LogWarning("[SceneLoaderYarn] DialogueRunner NÃO encontrado!");
        }
    }

    void OnDestroy()
    {
        if (dialogueRunner != null)
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
    }

    void OnDialogueComplete()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
        else
            Debug.LogWarning("[SceneOnDialogueComplete] sceneToLoad vazio.");
    }
}
