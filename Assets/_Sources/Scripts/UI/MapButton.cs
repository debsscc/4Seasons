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
    [SerializeField] private TMP_Text mapNameText;
    [SerializeField] private Button mapButton;
    [SerializeField] public float waitTimer;

    [Header("Map Data")]
    [SerializeField] public MapData mapData;
    
    [Header("Selection Visual")]
    [SerializeField] private Outline outline; 
    
    public event Action<MapButton, MapData> OnMapSelected; // Mudou de string para MapData
    
    private bool isSelected = false;

    private void Awake()
    {
        if (outline == null)
            outline = GetComponent<Outline>();
        
        SetSelected(false);
    }

    public void PointerEnterFeedback()
    {
        if (!isSelected)
        {
            transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);
        }
    }

    public void PointerOutFeedback()
    {
        if (!isSelected)
        {
            transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        }
    }

    public async void OnMapButtonClick()
    {
        if (mapData == null)
        {
            Debug.LogError("MapData não está configurado no MapButton!");
            return;
        }
        if (GameSessionManager.Instance != null && GameSessionManager.Instance.IsCompleted(mapData))
            {
                Debug.Log("Mapa já completado: " + mapData.sceneName);
                return;
            }
        Debug.Log("Map Button Clicked: " + mapData.sceneName);
        await UniTask.Delay(TimeSpan.FromSeconds(waitTimer));
        OnMapSelected?.Invoke(this, mapData); 
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (outline != null)
        {
            outline.enabled = selected;
            
            if (selected)
            {
                outline.effectColor = Color.white;
                transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);
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
            image.color = interactable ? Color.white : new Color(1, 1, 1, 0.5f);
        }
    }

}