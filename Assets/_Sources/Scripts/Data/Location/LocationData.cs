using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "LocationData", menuName = "Scriptable Objects/LocationData")]
public class LocationData : SerializedScriptableObject
{
    public string locationName;
    public Sprite icon; //p ui do slot
    public Sprite dayBackground;
    public Sprite nightBackground;


    [TableList]
    public EventData[] possibleEvents;


    [Range(0, 100)]
    public int npcFavoriteChance = 30;
}