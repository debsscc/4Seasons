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

        // Buscar DialogueRunner
        if (dialogueRunner == null)
        {
            Debug.Log("[SceneLoaderYarn] Procurando DialogueRunner...");
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        }

        // Buscar YarnScoreCommands
        if (scoreCommands == null)
        {
            Debug.Log("[SceneLoaderYarn] Procurando YarnScoreCommands...");
            scoreCommands = FindFirstObjectByType<YarnScoreCommands>();
        }

        // Validar dependências
        if (dialogueRunner == null || scoreCommands == null)
        {
            Debug.LogError("[SceneLoaderYarn] DialogueRunner ou YarnScoreCommands não encontrado!");
            return;
        }

        dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);

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
        // Editor ou jogo já está sendo encerrado
        if (!Application.isPlaying)
            return;

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("[SceneOnDialogueComplete] sceneToLoad vazio.");
            return;
        }

        if (GameSessionManager.Instance == null || SceneTransition.Instance == null)
            return;

        GameSessionManager.Instance.MarkCurrentMapAsCompleted();
        SceneTransition.Instance.ChangeScene(sceneToLoad);
    }

} 
