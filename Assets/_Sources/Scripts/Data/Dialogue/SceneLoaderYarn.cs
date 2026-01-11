using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class SceneLoaderYarn : MonoBehaviour
{
    public string sceneToLoad;
    public DialogueRunner dialogueRunner; // arraste no Inspector ou deixe nulo para achar automaticamente
    public YarnScoreCommands scoreCommands;

    void Awake()
    {
        Debug.Log("[SceneLoaderYarn] Awake chamado!");

        if (dialogueRunner == null)
        {
            Debug.Log("[SceneLoaderYarn] Procurando DialogueRunner...");
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        }

        if (dialogueRunner != null)
        {
            Debug.Log("[SceneLoaderYarn] DialogueRunner encontrado! Registrando evento...");
            dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
        }
        if (scoreCommands == null)
        {
            Debug.Log("[SceneLoaderYarn] Procurando YarnScoreCommands...");
            scoreCommands = FindFirstObjectByType<YarnScoreCommands>();
        }
        if (dialogueRunner || scoreCommands == null)
        {
            Debug.LogWarning("[SceneLoaderYarn] DialogueRunner ou YarnScoreCommands não encontrado!");
            return;
        }

        dialogueRunner.AddCommandHandler<string>(
            "ApplyEventPart",
            scoreCommands.ApplyEventPart
        );
        Debug.Log("[SceneLoaderYarn] Comando ApplyEventPart registrado!");
    }

    void OnDestroy()
    {
        if (dialogueRunner != null)
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
    }

    void OnDialogueComplete()
    {   
        if (!string.IsNullOrEmpty(sceneToLoad)) {
            GameSessionManager.Instance.MarkCurrentMapAsCompleted();
            SceneTransition.Instance.ChangeScene(sceneToLoad);
        } else
            Debug.LogWarning("[SceneOnDialogueComplete] sceneToLoad vazio.");
    }   
}
