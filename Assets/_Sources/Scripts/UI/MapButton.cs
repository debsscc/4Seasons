using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using Sirenix.OdinInspector;
using System;


//o botao so leva o nome dos locais

public class MapButton : MonoBehaviour
{
    [Header("Map Button Settings")]
    [SerializeField] private Image mapIcon;
    [SerializeField] private TMP_Text mapNameText;
    [SerializeField] private Button mapButton;
    
    [Header("Map Scenes")]
    //[ValueDropdown ("GetSceneNames")] //completar
    [SerializeField] private string sceneName;
    public event Action<string> OnMapSelected;
    
    private void Start()
    {
        if (mapButton == null)
            mapButton = GetComponent<Button>();
            
        mapButton.onClick.AddListener(OnMapButtonClick);
    }

    public void OnMapButtonClick()
    {
        Debug.Log("Map Button Clicked: " + sceneName);
        OnMapSelected?.Invoke(sceneName);
    }
}