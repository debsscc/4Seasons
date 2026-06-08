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

    private void Awake()
    {
        // Desabilita autoStart no Awake (antes de qualquer Start()) para
        // garantir que DialogueRunner.Start() não dispare o diálogo
        // antes que LinePresenter e LineAdvancer terminem seus Start().
        if (dialogueRunner != null)
            dialogueRunner.autoStart = false;
    }

    private IEnumerator Start()
    {
        yield return null; // aguarda 1 frame para todos os Start() completarem
        if (dialogueRunner != null)
            dialogueRunner.StartDialogue(dialogueRunner.startNode);
    }
}
