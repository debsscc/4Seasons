using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class Settings_Menu : MonoBehaviour
{
    public Slider masterVol, musicVol, sfxVol, voiceVol;
    public AudioMixer mainAudioMixer;

    public void ChangeMasterVolume()
    {
        mainAudioMixer.SetFloat("MasterVol", masterVol.value);
    }
    public void ChangeMusicVolume()
    {
        mainAudioMixer.SetFloat("MusicVol", musicVol.value);
    }
    public void ChangeSFXVolume()
    {
        mainAudioMixer.SetFloat("SFXVol", sfxVol.value);
    }
    public void ChangeVoiceVolume()
    {
        mainAudioMixer.SetFloat("VoiceVol", voiceVol.value);
    }

    // Chamado pelo botão PT/BR  → passa "pt"
    // Chamado pelo botão ENG    → passa "en"
    public void SetLanguage(string lang)
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.SetLanguage(lang);
    }
}
