using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dialogue/Node Metadata")]
public class DialogueNodeMetadata : ScriptableObject
{
    [TabGroup("General")]
    public string yarnNodeName;

    [TabGroup("Portraits")]
    public Sprite portrait;

    [TabGroup("Portraits")]
    public string expressionName;

    [TabGroup("Events")]
    public List<string> eventsBeforeNode;

    [TabGroup("Events")]
    public List<string> eventsAfterNode;

    [TabGroup("Choices")]
    public bool overrideChoicesUI;
}
