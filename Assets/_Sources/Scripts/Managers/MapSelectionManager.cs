using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class MapSelectionManager : MonoBehaviour
{
    public static MapSelectionManager Instance { get; private set; }
    
    [Header("Map Data")]
    public MapData[] allMaps;
    [SerializeField] private MapButton[] SceneButtons;
    private MapData[] availableMaps;
    private List<MapData> selectedMaps = new List<MapData>();
    private MapButton currentSelectedButton;
    private MapData pendingMap;

    [Header("UI")]
    [SerializeField] private GameObject confirmButtonGO;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log("MapSelectionManager Awake");
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            return;
        }

        if (GameSessionManager.Instance != null)
        {
            availableMaps = allMaps
                .Where(m => !GameSessionManager.Instance.IsCompleted(m))
                .ToArray();
        }
        else
        {
            availableMaps = allMaps;
        }

        if (confirmButtonGO != null)
            confirmButtonGO.SetActive(false);
    }

    private void Start()
    {
        if (GameSessionManager.Instance != null)
        {
            availableMaps = allMaps
                .Where(m => !GameSessionManager.Instance.IsCompleted(m))
                .ToArray();
        }
        else
        {
            availableMaps = allMaps;
        }

        // Desabilitar botões de mapas completados
        foreach (var button in SceneButtons)
        {
            if (button == null || button.mapData == null) continue;

            bool isCompleted = GameSessionManager.Instance != null && GameSessionManager.Instance.IsCompleted(button.mapData);
            button.SetInteractable(!isCompleted);
        }

        if (confirmButtonGO != null)
            confirmButtonGO.SetActive(false);

    }

    private void OnEnable()
    {
        for (int i = 0; i < SceneButtons.Length; i++)
        {
            if (SceneButtons[i] == null) continue;
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

    public void OnMapButtonClicked(MapButton mapButton, MapData mapData)
    {
        if (!availableMaps.Contains(mapData))
        {
            Debug.Log("Mapa já foi completado: " + mapData.sceneName);
            return;
        }

        if (currentSelectedButton != null && currentSelectedButton != mapButton)
            currentSelectedButton.SetSelected(false);

        currentSelectedButton = mapButton;
        currentSelectedButton.SetSelected(true);
        pendingMap = mapData;

        if (confirmButtonGO != null)
            confirmButtonGO.SetActive(true);
    }

    public string GetPendingSceneName()
    {
        if (pendingMap != null)
        {
            return pendingMap.sceneName;
        }
        
        return null;
    }

    public void ConfirmSelection()
    {
        if (pendingMap == null || currentSelectedButton == null)
        {
            Debug.LogWarning("Nenhum mapa selecionado ao confirmar");
            return;
        }
        if (GameSessionManager.Instance != null && GameSessionManager.Instance.IsCompleted(pendingMap))
        {
            Debug.LogWarning("Mapa já completado, não pode selecionar novamente.");
            return;
        }

        Debug.Log($"Confirmando seleção: {pendingMap.sceneName}");
        ScoreManager.Instancia?.ApplyScoreForMapSelection(pendingMap);
        SelectMap(pendingMap);
    }

    public void SelectMap(MapData mapData)
    {
        if (mapData == null) return;
        
        if (availableMaps.Contains(mapData))
        {
            if (GameSessionManager.Instance != null)
            {
                GameSessionManager.Instance.SetCurrentMap(mapData);
            }

            removeMapFromAvailable(mapData);
            AddMapToSelection(mapData);
            ChangeSceneMap(mapData);
        }
        else
        {
            Debug.Log("Map already completed: " + mapData.sceneName);
        }
    }

    private void ChangeSceneMap(MapData mapData)
    {
        Debug.Log("Changing Scene to: " + mapData.sceneName);
        SceneManager.LoadScene(mapData.sceneName);
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