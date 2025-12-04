using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "NewMapData", menuName = "Game/Map Data")]
public class MapData : ScriptableObject
{
    public string mapName;
    public Sprite mapIcon;
    public Sprite mapMiniature;
    
    [SerializeField] 
    public  Object sceneAsset; 
    public string sceneName
    {
        get
        {
            #if UNITY_EDITOR
            if (sceneAsset != null)
                return sceneAsset.name;
            #endif
            return _sceneName;
        }
    }
    
    [SerializeField]
    private string _sceneName;
        private void OnValidate()
    {
        if (sceneAsset != null)
        {
            _sceneName = sceneAsset.name;
        }
        else
        {
            _sceneName = string.Empty;
        }
    }
}