using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instancia { get; private set; }

    [SerializeField] private int favoritePlaceScore = 1;

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ApplyScoreForMapSelection(MapData selectedMap)
    {
        if (selectedMap == null)
        {
            Debug.LogWarning("ScoreManager: selectedMap é nulo.");
            return;
        }

        var charsManager = CharactersManager.Instance;
        if (charsManager == null)
        {
            Debug.LogWarning("ScoreManager: CharactersManager.Instance é nulo.");
            return;
        }

        foreach (var npc in charsManager.npcs)
        {
            if (npc == null) continue;

            if (npc.favoritePlace == selectedMap)
            {
                npc.RelationshipScore += favoritePlaceScore;
                Debug.Log($"ScoreManager: +{favoritePlaceScore} para {npc.name} (mapa favorito: {selectedMap.displayName})");
            }
        }
    }
}