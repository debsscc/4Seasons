using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class LocationButton : MonoBehaviour
{
    [FoldoutGroup("Configuração")]
    public LocationData locationData;

    [FoldoutGroup("UI")]
    public Image highlightImage;

    [FoldoutGroup("UI")]
    public Image iconImage;

    [FoldoutGroup("Som")]
    public AudioSource audioSource;

    [FoldoutGroup("Som")]
    public AudioClip clickSfx;

    private bool isSelect = false;

    public void OnButtonLocationClick()
    {
        //tocando o sound
        if (audioSource && clickSfx)
            audioSource.PlayOneShot(clickSfx);

        ScheduleManager.Instance.ToggleLocation(locationData);

        //atualizar highlight visual
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        isSelect = ScheduleManager.Instance.selectedLocations.Contains(locationData);

        if (highlightImage)
            highlightImage.enabled = isSelect;
    }


}
