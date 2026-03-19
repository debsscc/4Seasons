using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class SceneLoaderYarn : MonoBehaviour
{
    public string sceneToLoad;
    public DialogueRunner dialogueRunner;
    public YarnScoreCommands scoreCommands;

    void Awake()
    {
        Debug.Log("[SceneLoaderYarn] Awake chamado!");

        if (dialogueRunner == null)
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();

        if (scoreCommands == null)
            scoreCommands = FindFirstObjectByType<YarnScoreCommands>();

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
    }

    void OnDestroy()
    {
        if (dialogueRunner != null)
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
    }

    void OnDialogueComplete()
    {
        if (!Application.isPlaying)
            return;

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("[SceneLoaderYarn] sceneToLoad vazio.");
            return;
        }

        bool isEndOfDay = sceneToLoad == "MapSeletor";

        if (isEndOfDay && GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.MarkCurrentMapAsCompleted();

            int completed = System.Linq.Enumerable.Count(GameSessionManager.Instance.GetCompletedMaps());
            int needed = GameFlowManager.Instance != null ? GameFlowManager.Instance.mapsCompletedToEndGame : 3;

            if (completed >= needed)
            {
                if (GameFlowManager.Instance != null)
                    GameFlowManager.Instance.isGameOver = true;
                LoadScene("Credits");
                return;
            }
        }

        LoadScene(sceneToLoad);
    }

    void LoadScene(string sceneName)
    {
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.ChangeScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }
}
