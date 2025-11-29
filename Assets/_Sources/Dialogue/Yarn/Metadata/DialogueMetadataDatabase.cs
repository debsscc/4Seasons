using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Metadata Database")]
public class DialogueMetadataDatabase : ScriptableObject
{
    public List<DialogueMetadata> entries;

    public DialogueMetadata Get(string nodeName)
    {
        return entries.Find(e => e.nodeName == nodeName);
    }
}

[System.Serializable]
public class DialogueMetadata
{
    public string nodeName;

    public Sprite portrait;
    public Sprite expression;

    public string preEvent;
    public string postEvent;
}
