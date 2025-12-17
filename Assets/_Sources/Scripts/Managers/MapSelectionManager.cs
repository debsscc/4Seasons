using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class MapSelectionManager : MonoBehaviour
{
    public static MapSelectionManager Instance { get; private set; }
    
    [Header("Map Data")]
    [SerializeField] private MapData[] allMaps;
    [SerializeField] private MapButton[] SceneButtons;
    private MapData[] availableMaps;
    private List<MapData> selectedMaps = new List<MapData>();

    [Header("UI")]
    [SerializeField] private GameObject confirmButtonGO;

    [Header("Selection State")]
    private MapButton currentSelectedButton;
    private string pendingSceneName;

    private void Awake()
    {
        Debug.Log("MapSelectionManager Awake");
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            return;
        }

        availableMaps = allMaps;

        if (confirmButtonGO != null)
        {
            confirmButtonGO.SetActive(false);
        }
    }

    private void OnEnable()
    {
        Debug.Log("MapSelectionManager OnEnable");
        for (int i = 0; i < SceneButtons.Length; i++)
        {
            if (SceneButtons[i] == null) continue;
            
            Debug.Log("Subscribing to MapButton: " + SceneButtons[i].name);
            SceneButtons[i].OnMapSelected += OnMapButtonClicked;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < SceneButtons.Length; i++)
        {
            if (SceneButtons[i] == null) continue;
            SceneButtons[i].OnMapSelected -= OnMapButtonClicked;
        }
    }

    public void OnMapButtonClicked(MapButton mapButton, string sceneName)
    {
        Debug.Log($"OnMapButtonClicked: {sceneName} via {mapButton.name}");

        var mapData = System.Array.Find(allMaps, map => map.sceneName == sceneName);
        if (mapData == null)
        {
            Debug.LogWarning("MapData não encontrado para: " + sceneName);
            return;
        }

        if (!availableMaps.Contains(mapData))
        {
            Debug.Log("Mapa já foi completado: " + sceneName);
            return;
        }

        if (currentSelectedButton != null && currentSelectedButton != mapButton)
            currentSelectedButton.SetSelected(false);

        currentSelectedButton = mapButton;
        currentSelectedButton.SetSelected(true);
        pendingSceneName = sceneName;

        if (confirmButtonGO != null)
        {
            confirmButtonGO.SetActive(true);
        }
    }

    public string GetPendingSceneName()
    {
        return pendingSceneName;
    }

    public void ConfirmSelection()
    {
        if (string.IsNullOrEmpty(pendingSceneName) || currentSelectedButton == null)
        {
            Debug.LogWarning("MapSelectionManager: Nenhum mapa selecionado ao confirmar");
            return;
        }

        Debug.Log($"MapSelectionManager: Confirmando seleção: {pendingSceneName}");
        SelectMap(pendingSceneName);
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
        var tempList = new List<MapData>(availableMaps);
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
        Debug.Log(">>> Changing Scene via MapSelectionManager to: " + mapData.sceneAsset.name);
        SceneManager.LoadScene(mapData.sceneAsset.name);
    }

    public MapData[] GetSelectedMaps()
    {
        MapData[] result = new MapData[3];
        for (int i = 0; i < selectedMaps.Count && i < result.Length; i++)
        {
            result[i] = selectedMaps[i];
        }
        return result;
    }
}