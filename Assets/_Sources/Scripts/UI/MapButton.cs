using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MapButton : MonoBehaviour
{
    [Header("Map Button Settings")]
    [SerializeField] private Image mapIcon;
    [SerializeField] private Button mapButton;
    [SerializeField] public float waitTimer;

    [Header("Map Data")]
    [SerializeField] public MapData mapData;
    
    [Header("Selection Visual")]
    [SerializeField] private Outline outline; 
    
    public event Action<MapButton, MapData> OnMapSelected; 
    
    private bool isSelected = false;
    private bool isCompleted = false;

    private void Awake()
    {
        if (outline == null)
            outline = GetComponent<Outline>();
        
        SetSelected(false);
    }

    public void PointerEnterFeedback()
    {
        if (!isSelected && !isCompleted )
        {
            transform.DOScale(1.05f, 0.2f).SetEase(Ease.OutBack);
        }
    }

    public void PointerOutFeedback()
    {
        if (!isSelected && !isCompleted)
        {
            transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        }
    }

    public async void OnMapButtonClick()
    {
        if (mapData == null)
        {
            Debug.LogError("MapData nn ta configurado no MapButton!");
            return;
        }
        if (GameSessionManager.Instance != null && GameSessionManager.Instance.IsCompleted(mapData))
            {
                Debug.Log("Mapa já completado: " + mapData.sceneName);
                return;
            }
        Debug.Log("Map Button Clicked: " + mapData.sceneName);
        // await UniTask.Delay(TimeSpan.FromSeconds(waitTimer));
        OnMapSelected?.Invoke(this, mapData); 
    }
    
    public void SetSelected(bool selected)
    {
        if (isCompleted) return;

        isSelected = selected;
        
        if (outline != null)
        {
            // outline.enabled = selected;
            
            if (selected)
            {
                // outline.effectColor = Color.white;
                transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack);
            }
            else
            {
                transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
            }
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (mapButton != null)
            mapButton.interactable = interactable;

        var image = GetComponent<Image>();
        if (image != null)
        {
            if (!interactable)
            {
                isCompleted = true;
                image.color = new Color(1, 1, 1, 0.3f);
                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                }
            }
            else
            {
                isCompleted = false;
                image.color = Color.white;
            }
        }
    }
}