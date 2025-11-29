using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class NamedClip
{
    public string Name;
    public AudioClip Clip;

    [Range(0f, 1f)]
    public float Volume = 1f;

    public bool Loop = false;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [FoldoutGroup("Sources"), ReadOnly]
    public AudioSource MusicSource;

    [FoldoutGroup("Sources"), ReadOnly]
    public AudioSource SFXSource;

    [FoldoutGroup("Sources"), ReadOnly]
    public AudioSource UISource;

    [FoldoutGroup("Mixer")]
    [Range(0f, 1f)]
    public float MasterVolume = 1f;

    [FoldoutGroup("Mixer")]
    [Range(0f, 1f)]
    public float MusicVolume = 1f;

    [FoldoutGroup("Mixer")]
    [Range(0f, 1f)]
    public float SFXVolume = 1f;

    [FoldoutGroup("Mixer")]
    [Range(0f, 1f)]
    public float UIVolume = 1f;

    [FoldoutGroup("Libraries")]
    public List<NamedClip> MusicLibrary = new();

    [FoldoutGroup("Libraries")]
    public List<NamedClip> SFXLibrary = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        AutoSetup();
        ApplyMixer();
    }
    [FoldoutGroup("Setup Tools")]
    [Button("Gerar AudioSources Automaticamente")]
    private void AutoSetup()
    {
        CreateSourceIfMissing(ref MusicSource, "MusicSource", loop: false);
        CreateSourceIfMissing(ref SFXSource, "SFXSource", loop: false);
        CreateSourceIfMissing(ref UISource, "UISource", loop: false);
    }

    private void CreateSourceIfMissing(ref AudioSource source, string name, bool loop)
    {
        if (source == null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform);

            source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;

#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create AudioSource");
#endif
        }
    }
    private IEnumerable<string> MusicNames()
    {
        foreach (var m in MusicLibrary)
            yield return m.Name;
    }

    private IEnumerable<string> SFXNames()
    {
        foreach (var s in SFXLibrary)
            yield return s.Name;
    }
    [FoldoutGroup("Debug")]
    [ValueDropdown("MusicNames")]
    public string DebugMusic;

    [FoldoutGroup("Debug")]
    [Button("Tocar Música (Debug)")]
    private void DebugPlayMusic()
    {
        PlayMusic(DebugMusic);
    }

    [FoldoutGroup("Debug")]
    [Button("Pausar/Despausar Música")]
    private void DebugTogglePause()
    {
        if (MusicSource.isPlaying)
            MusicSource.Pause();
        else
            MusicSource.UnPause();
    }

    [FoldoutGroup("Debug")]
    [Button("Parar Música")]
    private void DebugStopMusic()
    {
        MusicSource.Stop();
    }

    [FoldoutGroup("Debug")]
    [ValueDropdown("SFXNames")]
    public string DebugSFX;

    [FoldoutGroup("Debug")]
    [Button("Tocar SFX (Debug)")]
    private void DebugPlaySFX()
    {
        PlaySFX(DebugSFX);
    }

    [FoldoutGroup("Debug")]
    [Button("Resetar DebugMusic para null")]
    private void ResetDebugMusic() => DebugMusic = null;

    [FoldoutGroup("Debug")]
    [Button("Resetar DebugSFX para null")]
    private void ResetDebugSFX() => DebugSFX = null;
    private void ApplyMixer()
    {
        MusicSource.volume = MasterVolume * MusicVolume;
        SFXSource.volume = MasterVolume * SFXVolume;
        UISource.volume = MasterVolume * UIVolume;
    }

    private NamedClip FindClip(List<NamedClip> list, string name)
    {
        return list.Find(c => c.Name == name);
    }

    public void PlayMusic(string name)
    {
        var entry = FindClip(MusicLibrary, name);
        if (entry == null || entry.Clip == null)
        {
            Debug.LogWarning($"[AudioManager] Música '{name}' não encontrada.");
            return;
        }

        MusicSource.clip = entry.Clip;
        MusicSource.loop = entry.Loop;
        MusicSource.volume = entry.Volume * MusicVolume * MasterVolume;
        MusicSource.Play();
    }

    public void PlaySFX(string name)
    {
        var entry = FindClip(SFXLibrary, name);
        if (entry == null || entry.Clip == null)
        {
            Debug.LogWarning($"[AudioManager] SFX '{name}' não encontrado.");
            return;
        }

        SFXSource.PlayOneShot(entry.Clip, entry.Volume * SFXVolume * MasterVolume);
    }

    public void PlayUI(string name)
    {
        var entry = FindClip(SFXLibrary, name);
        if (entry == null || entry.Clip == null)
        {
            Debug.LogWarning($"[AudioManager] UI '{name}' não encontrado.");
            return;
        }

        UISource.PlayOneShot(entry.Clip, entry.Volume * UIVolume * MasterVolume);
    }
}
