using System.Collections;
using UnityEngine;
using Yarn.Unity;

/// <summary>
/// Substitui o autoStart do DialogueRunner com um delay de 1 frame.
/// Isso garante que todos os Start() (incluindo LinePresenter.Start() que
/// popula o ActionMarkupHandlers com os handlers de botão) sejam executados
/// antes do diálogo começar. Sem isso, o primeiro balão de diálogo não
/// responde a cliques do mouse.
/// </summary>
public class DialogueDelayedStarter : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string startNode = "Start";

    private IEnumerator Start()
    {
        yield return null; // aguarda 1 frame para todos os Start() completarem
        dialogueRunner.StartDialogue(startNode);
    }
}
