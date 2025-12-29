using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Game/Map Data")]
public class MapData : ScriptableObject
{
    public string sceneName;
    public string displayName;
    public Sprite icon;
    
    // Opcional: se quiser manter compatibilidade com enum
    public LugarFavorito lugarEnum;
}