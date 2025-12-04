using UnityEngine;

// codigo p pesquisa.

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }
    
    private MapData[] currentMapSequence;
    private int currentMapIndex = 0;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SetMapSequence(MapData[] sequence)
    {
        currentMapSequence = sequence;
        currentMapIndex = 0;
    }
    
    public MapData GetCurrentMap()
    {
        if (currentMapSequence == null || currentMapIndex >= currentMapSequence.Length)
            return null;
            
        return currentMapSequence[currentMapIndex];
    }
    
    public void CompleteCurrentMap()
    {
        currentMapIndex++;
        
        if (currentMapIndex < currentMapSequence.Length)
        {
            // Carrega o próximo mapa
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                currentMapSequence[currentMapIndex].sceneName
            );
        }
        else
        {
            // Todos os mapas completados
            ReturnToMapSelection();
        }
    }
    
    public void ReturnToMapSelection()
    {
        Destroy(gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MapSelectionScene");
    }
}