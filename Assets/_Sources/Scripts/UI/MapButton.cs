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

    [Header("Map Scenes")]
    [SerializeField] private string sceneName;
    
    [Header("Selection Visual")]
    [SerializeField] private Outline outline; 
    
    public event Action<MapButton, string> OnMapSelected;
    
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
    Debug.Log("Map Button Clicked: " + sceneName);
    await UniTask.Delay(TimeSpan.FromSeconds(waitTimer));
    OnMapSelected?.Invoke(this, sceneName);
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
}