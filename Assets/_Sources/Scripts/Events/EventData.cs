using UnityEngine;
using Yarn.Unity;

[CreateAssetMenu(fileName = "EventData", menuName = "Scriptable Objects/EventData")]
public class EventData : ScriptableObject
{
    public string EventID;
    // public NPCData mainNPC;
    public YarnProject yarnFile;

    public int selfPointsChange;
    public RelationshipImpactNPC impacts;
}

[System.Serializable]
public class RelationshipImpactNPC
{
    // public NPCData npc;
    public int pointsChange;
}
