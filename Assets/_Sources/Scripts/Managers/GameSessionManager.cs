using UnityEngine;
using System.Collections.Generic;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }
    
    private HashSet<MapData> completedMaps = new HashSet<MapData>();
    private MapData currentMap;
    
    [Header("Map Selection Scene")]
    [SerializeField] private string mapSelectionSceneName = "MapSelectionScene";
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameSessionManager criado e persistente");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SetCurrentMap(MapData map)
    {
        currentMap = map;
        Debug.Log($"GameSessionManager: Mapa atual = {map.sceneName}");
    }
    
    public MapData GetCurrentMap()
    {
        return currentMap;
    }
    
    public bool IsCompleted(MapData map)
    {
        return map != null && completedMaps.Contains(map);
    }
    
    public void MarkCurrentMapAsCompleted()
    {
        if (currentMap != null)
        {
            completedMaps.Add(currentMap);
            Debug.Log($"Mapa completado: {currentMap.sceneName}. Total completados: {completedMaps.Count}");
        }
        else
        {
            Debug.LogWarning("Tentou marcar mapa como completado, mas currentMap é null");
        }
    }
    
    public void ReturnToMapSelection()
    {
        Debug.Log("Voltando para cena de seleção de mapas...");
        SceneTransition.Instance.ChangeScene(mapSelectionSceneName);
       // UnityEngine.SceneManagement.SceneManager.LoadScene(mapSelectionSceneName);
    }

    public IEnumerable<MapData> GetCompletedMaps() => completedMaps;
}