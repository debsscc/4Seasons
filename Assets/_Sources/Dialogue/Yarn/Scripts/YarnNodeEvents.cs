using Yarn.Unity;
using UnityEngine;

public class YarnNodeEvents : MonoBehaviour
{
    public DialogueMetadataDatabase metadataDatabase;
    public DialogueUI dialogueUI;

    void Awake()
    {
        GetComponent<DialogueRunner>().onNodeStart.AddListener(OnNodeStart);
    }

    void OnNodeStart(string nodeName)
    {
        var meta = metadataDatabase.Get(nodeName);

        if (meta == null)
            return;

        dialogueUI.portrait.sprite = meta.portrait;
        dialogueUI.expression.sprite = meta.expression;
    }
}
