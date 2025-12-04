using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

//permitir ir pra cena, tranca os que ja foram completados
//troca de cena e mantém o controle do progresso do jogador no mapa

public class MapSelectionManager : MonoBehaviour
{
    public static MapSelectionManager Instance { get; private set; }
    
    [Header("Map Data")]
    [SerializeField] private MapData[] allMaps;
    [SerializeField] private MapButton[] SceneButtons;
    private MapData[] availableMaps;
    private List<MapData> selectedMaps = new List<MapData>();
    
    private void Awake()
    {
        Debug.Log("MapSelectionManager Awake");
        if (Instance == null)
        {
            Debug.Log("Setting Instance");
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("Destroying duplicate MapSelectionManager");
            //Destroy(gameObject);
        }
        availableMaps = allMaps;
    }
    
    public void SelectMap(string sceneName)
    {
        Debug.Log("Selecting Map: " + sceneName);
        MapData mapData = System.Array.Find(allMaps, map => map.sceneName == sceneName);
        if (mapData == null) return;
        
        if (availableMaps.Contains(mapData))
        {
            Debug.Log("Map is still in availableMaps list.");
            ChangeSceneMap(mapData);
            removeMapFromAvailable(mapData);
            AddMapToSelection(mapData);
            return;
        }
        else
        {
            Debug.Log("Map has already been completed." + sceneName);
        }
    }
    public void removeMapFromAvailable(MapData mapData)
    {
        List<MapData> tempList = new List<MapData>(availableMaps);
        tempList.Remove(mapData);
        availableMaps = tempList.ToArray();
    }
    private void AddMapToSelection(MapData mapData)
    {
        if (selectedMaps.Contains(mapData)) return;
        if (selectedMaps.Count >= 5) return;
        
        selectedMaps.Add(mapData);
    }
    private void ChangeSceneMap(MapData mapData)
    {
        Debug.Log("Changing Scene to: " + mapData.sceneName);
        SceneManager.LoadScene(mapData.sceneAsset.name);
    }

    private void OnEnable()
    {
        Debug.Log("MapSelectionManager OnEnable");
        for (int i = 0; i < SceneButtons.Length; i++)
        {
            Debug.Log("Subscribing to MapButton: " + SceneButtons[i].name);
            SceneButtons[i].OnMapSelected += SelectMap;
        }
    }
    private void OnDisable()
    {
      for (int i = 0; i < SceneButtons.Length; i++)
        {
            SceneButtons[i].OnMapSelected -= SelectMap;
        }  
    }

    public MapData[] GetSelectedMaps()
    {
        MapData[] result = new MapData[3];
        for (int i = 0; i < selectedMaps.Count; i++)
        {
            result[i] = selectedMaps[i];
        }
        return result;
    }
}