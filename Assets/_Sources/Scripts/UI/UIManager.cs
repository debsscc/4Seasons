using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform mapGridParent;
    [SerializeField] private Button backButton;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject mapButtonPrefab;
    
    [Header("Map Data")]
    [SerializeField] private MapData[] allMaps;
    
    private void Start()
    {
        InitializeUI();
        ConfigureBackButton();
    }
    
    private void InitializeUI()
    {
        
        // Cria botões para cada mapa
        foreach (MapData mapData in allMaps)
        {
            if (mapData == null) continue;
            
            GameObject buttonObj = Instantiate(mapButtonPrefab, mapGridParent);
            MapButton mapButton = buttonObj.GetComponent<MapButton>();
            if (mapButton != null)
            {
                //mapButton.Initialize(mapData);
            }
            else
            {
                Debug.LogError("Prefab do botão não tem componente MapButton!");
            }
        }
    }
    
    private void ConfigureBackButton()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("MainMenu");
            });
        }
    }
}