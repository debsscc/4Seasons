using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;


//o botao so leva o nome dos locais

public class MapButton : MonoBehaviour
{
    [Header("Map Button Settings")]
    [SerializeField] private Image mapIcon;
    [SerializeField] private TMP_Text mapNameText;
    [SerializeField] private Button mapButton;
    [SerializeField] public float waitTimer;

    [Header("Map Scenes")]
    //[ValueDropdown ("GetSceneNames")] //completar
    [SerializeField] private string sceneName;
    public event Action<string> OnMapSelected;

    public void PointerEnterFeedback()
    {
        transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);
    }

    public void PointerOutFeedback()
    {
        transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
    }

    public async void OnMapButtonClick()
    {
        Debug.Log("Map Button Clicked: " + sceneName);
        // await transform.DOShakePosition(waitTimer, 10f, 20, 90f, false, true);
        await UniTask.Delay(TimeSpan.FromSeconds(waitTimer));
        OnMapSelected?.Invoke(sceneName);
    }
}