using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Yarn.Unity;

public class MiniGameController : MonoBehaviour
{
    public List<SlotDraggable> targetSlots;

    [Header("Draggables")]
    public List<DraggablePrefab> draggablePrefabs = new();
    public Transform objectStartParent;

    [Header("Audio")]
    public AudioClip successSfx;
    public AudioClip failSfx;

    [SerializeField] private bool _clared = false;

    private readonly List<DraggablePrefab> spawnedDraggables = new();
    private List<Transform> SpawnPoints => objectStartParent.Cast<Transform>().ToList();

    private IMiniGameScoring _scoringStrategy;    

    private void Awake()
    {
        _scoringStrategy = GetComponent<IMiniGameScoring>();
    }
    private void Start()
    {
        SpawnAllDraggables();
    }

    private void SpawnAllDraggables()
{
    List<Transform> sp = SpawnPoints;
    int spawnPointsAmount = sp.Count;

    for (int i = 0; i < draggablePrefabs.Count; i++)
    {
        if(i >= spawnPointsAmount)
        {
            break;
        }

        var instance = Instantiate(draggablePrefabs[i], sp[i]); 
        instance.OnBeginDragEvent += () => ClearDraggables(instance);
        instance.TargetSlots = targetSlots;
        instance.MiniGameController = this; 


        spawnedDraggables.Add(instance);
    }
}

    private void ClearDraggables(DraggablePrefab selectedOne)
    {
        if(_clared) return;
        _clared = true;

        foreach (var draggable in spawnedDraggables)
        {
            if (draggable == selectedOne) continue;

            // Destroy(draggable.gameObject);

            float timer = UnityEngine.Random.Range(0.1f, 0.6f);
            draggable.transform.DOScale(Vector2.zero, timer).OnComplete(() =>
            {
                Destroy(draggable.gameObject);
            });
        }
    }

    public void OnObjectDroppedInSlot(SlotDraggable slot, ItemsSO[] items)
{
    
    if (slot == null)
    {
        Debug.LogWarning("[MiniGameController] Slot nulo!");
        return;
    }

    Debug.Log($"[MiniGameController] _scoringStrategy é {(_scoringStrategy != null ? "não nulo" : "NULO")}");

    if (_scoringStrategy != null)
    {
        _scoringStrategy.OnObjectDropped(slot, items);
    } 
    else
    {
        Debug.Log("[MiniGameController] Usando lógica padrão (ApplyPointsByTrait)");
        CharactersManager.Instance.ApplyPointsByTrait(items);
    }
}
}
