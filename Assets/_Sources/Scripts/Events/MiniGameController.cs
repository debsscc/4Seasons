using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Yarn.Unity;

public class MiniGameController : MonoBehaviour
{
    [Header("Slots de Alvo")]
    public List<SlotDraggable> targetSlots;

    [Header("Draggables (Spawn automático)")]
    public bool useSceneDraggables = false;         
    public List<DraggablePrefab> draggablePrefabs = new();
    public Transform objectStartParent;

    [Header("Audio")]
    public AudioClip successSfx;
    public AudioClip failSfx;

    [SerializeField] private bool _clared = false;

    private readonly List<DraggablePrefab> spawnedDraggables = new();
    private List<Transform> SpawnPoints => objectStartParent
        ? objectStartParent.Cast<Transform>().ToList()
        : new List<Transform>();

    private IMiniGameScoring _scoringStrategy;
    

    private void Awake()
    {
        _scoringStrategy = GetComponent<IMiniGameScoring>();
        if (_scoringStrategy == null)
        {
            _scoringStrategy = FindFirstObjectByType<MonoBehaviour>() as IMiniGameScoring;
        }
    }

    private void Start()
    {   
        if (useSceneDraggables)
        {
            RegisterSceneDraggables();
        }
        else
        {
            SpawnAllDraggables();
        }
    }

    private void SpawnAllDraggables()
    {
        if (objectStartParent == null)
        {
            Debug.LogWarning("[MiniGameController] objectStartParent não definido.");
            return;
        }

        List<Transform> sp = SpawnPoints;
        int spawnPointsAmount = sp.Count;

        var miniGame2 = GetComponent<MiniGame2Scoring>();

        for (int i = 0; i < draggablePrefabs.Count; i++)
        {
            if (i >= spawnPointsAmount)
                break;

            var instance = Instantiate(draggablePrefabs[i], sp[i]);
            instance.OnBeginDragEvent += () => ClearDraggables(instance);
            instance.TargetSlots = targetSlots;
            instance.MiniGameController = this;

            spawnedDraggables.Add(instance);

            if (miniGame2 != null)
        {
            var drink = instance.GetComponent<DrinksINFO>();
            if (drink != null)
            {
                miniGame2.RegisterDrink(drink);
            }
        }
        }

    }

    private void ClearDraggables(DraggablePrefab selectedOne)
    {
        if (_clared) return;
        _clared = true;

        foreach (var draggable in spawnedDraggables)
        {
            if (draggable == selectedOne) continue;

            float timer = UnityEngine.Random.Range(0.1f, 0.6f);
            draggable.transform.DOScale(Vector2.zero, timer).OnComplete(() =>
            {
                Destroy(draggable.gameObject);
            });
        }
    }

    private void RegisterSceneDraggables()
    {
        var sceneDraggables = FindObjectsByType<DraggablePrefab>(FindObjectsSortMode.None);

        var miniGame2 = GetComponent<MiniGame2Scoring>();

        foreach (var drag in sceneDraggables)
        {
            drag.TargetSlots = targetSlots;
            drag.MiniGameController = this;

            Debug.Log($"[MiniGameController] Drag '{drag.name}' recebeu {targetSlots.Count} TargetSlots.");

            if (miniGame2 != null)
        {
            var drink = drag.GetComponent<DrinksINFO>();
            if (drink != null)
            {
                miniGame2.RegisterDrink(drink);
            }
        }
        }

        Debug.Log($"[MiniGameController] Registrados {sceneDraggables.Length} DraggablePrefabs via FindObjectsByType.");
    }

    public void ShowNPCReactions(ItemsSO[] items)
    {
        foreach (var npc in CharactersManager.Instance.npcs)
        {
            bool isFavorite = false;
            foreach (var item in items)
            {
                if (npc.favoriteItems.Contains(item))
                {
                    isFavorite = true;
                    break;
                }
            }

            Debug.Log($"{npc.name} reage: {(isFavorite ? "FELIZ" : "TRISTE")}");
        }
    }

    public void OnObjectDroppedInSlot(SlotDraggable slot, ItemsSO[] items)
{
    if (_scoringStrategy != null)
    {
        _scoringStrategy.OnObjectDropped(slot, items);
    }
    else
    {
        CharactersManager.Instance.ApplyPointsByTrait(items);
        ShowNPCReactions(items); 
    }
}
}