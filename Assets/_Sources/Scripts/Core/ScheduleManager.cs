using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ScheduleManager : MonoBehaviour
{
    public static ScheduleManager Instance { get; private set; }
    public List<LocationData> selectedLocations = new List<LocationData>();

    [Header("UI Slots")]
    public Image[] selectionSlots;

    [Header("UI - Slots")]
    public Image slot1Icon;
    public Image slot2Icon;
    public Image slot3Icon;

    [Header("Botão Confirmar")]
    public Button confirmButton;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        confirmButton.interactable = false;
        UpdateSlotsUI();
    }

    public void ToggleLocation(LocationData loc)
    {
        if (selectedLocations.Contains(loc)) return;
        if (selectedLocations.Count >= 3) return;

        selectedLocations.Add(loc);
        UpdateSlotsUI();
    }


    public void UpdateSlotsUI()
    {
        slot1Icon.sprite = null;
        slot2Icon.sprite = null;
        slot3Icon.sprite = null;

        slot1Icon.enabled = false;
        slot2Icon.enabled = false;
        slot3Icon.enabled = false;

        // preenche slots conforme o número de escolhas
        if (selectedLocations.Count > 0)
        {
            slot1Icon.sprite = selectedLocations[0].icon;
            slot1Icon.enabled = true;
        }

        if (selectedLocations.Count > 1)
        {
            slot2Icon.sprite = selectedLocations[1].icon;
            slot2Icon.enabled = true;
        }

        if (selectedLocations.Count > 2)
        {
            slot3Icon.sprite = selectedLocations[2].icon;
            slot3Icon.enabled = true;
        }

        // habilita o botão de confirmar com 3 lugares
        confirmButton.interactable = selectedLocations.Count == 3;

    }
    public void ConfirmSchedule()
    {
        if (selectedLocations.Count < 3) return; // impede start sem seleção

        GameFlowManager.Instance.StartWeek(selectedLocations);
        UnityEngine.SceneManagement.SceneManager.LoadScene("EventScene");
    }
}
