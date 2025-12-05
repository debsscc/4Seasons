using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System;


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

    public async void OnMapButtonClick()
    {
        await UniTask.Delay((int)(waitTimer * 1000));
        Debug.Log("Map Button Clicked: " + sceneName);
        OnMapSelected?.Invoke(sceneName);
    }
}