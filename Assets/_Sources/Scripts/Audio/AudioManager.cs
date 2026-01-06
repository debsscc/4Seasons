using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
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

    [Tooltip("Se verdadeiro, essa música pode persistir entre cenas listadas abaixo")]
    public bool PersistentAcrossScenes = false;

    [Tooltip("Lista de nomes das cenas onde essa música deve continuar tocando")]
    [ShowIf(nameof(PersistentAcrossScenes))]
    public List<string> PersistentScenes = new List<string>();
}

public class AudioManager : Singleton<AudioManager>
{
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

    [FoldoutGroup("Settings")]
    [Tooltip("Duração do fade ao trocar música")]
    public float FadeDuration = 1f;
    

    private string currentMusicName = null;
    private Coroutine currentFadeCoroutine = null;

    private void Start()
    {
        AutoSetup();
        ApplyMixer();

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        if (string.IsNullOrEmpty(currentMusicName))
        {
            PlayMusicForScene(sceneName);
            return;
        }

        var currentClip = FindClip(MusicLibrary, currentMusicName);
        if (currentClip != null && currentClip.PersistentAcrossScenes)
        {
            if (currentClip.PersistentScenes.Contains(sceneName))
            {
                return;
            }
        }

        PlayMusicForScene(sceneName);
    }

    private void PlayMusicForScene(string sceneName)
    {
        foreach (var clip in MusicLibrary)
        {
            if (clip.PersistentAcrossScenes && clip.PersistentScenes.Contains(sceneName))
            {
                StartCoroutine(SwitchMusicCoroutine(clip.Name, FadeDuration));
                return;
            }
        }

        StartCoroutine(StopMusicWithFade(FadeDuration));
        currentMusicName = null;
    }

    private IEnumerator SwitchMusicCoroutine(string newMusicName, float fadeDuration)
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
        }

        if (MusicSource.isPlaying)
        {
            currentFadeCoroutine = StartCoroutine(FadeOut(MusicSource, fadeDuration));
            yield return currentFadeCoroutine;
            currentFadeCoroutine = null;
        }

        PlayMusic(newMusicName, instant: true);
        currentFadeCoroutine = StartCoroutine(FadeInToTargetVolume(MusicSource, GetClipTargetVolume(newMusicName), fadeDuration));
        yield return currentFadeCoroutine;
        currentFadeCoroutine = null;
    }

    private IEnumerator StopMusicWithFade(float fadeDuration)
    {
        if (!MusicSource.isPlaying)
            yield break;

        yield return StartCoroutine(FadeOut(MusicSource, fadeDuration));
        currentMusicName = null;
    }

    private IEnumerator FadeOut(AudioSource source, float duration)
    {
        if (source == null || !source.isPlaying || duration <= 0f)
        {
            source?.Stop();
            yield break;
        }

        float startVolume = source.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        source.Stop();
        source.volume = startVolume;
    }

    private IEnumerator FadeInToTargetVolume(AudioSource source, float targetVolume, float duration)
    {
        if (source == null)
            yield break;

        if (duration <= 0f)
        {
            source.volume = targetVolume;
            yield break;
        }

        float elapsed = 0f;
        source.volume = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }
        source.volume = targetVolume;
    }

    private float GetClipTargetVolume(string musicName)
    {
        var entry = FindClip(MusicLibrary, musicName);
        if (entry == null) return MusicVolume * MasterVolume;
        return entry.Volume * MusicVolume * MasterVolume;
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

    public void PlayMusic(string name, bool instant = false)
    {
        if (string.IsNullOrEmpty(name))
            return;

        if (MusicSource.isPlaying && MusicSource.clip != null && MusicSource.clip.name == name)
        {
            currentMusicName = name;
            return;
        }

        var entry = FindClip(MusicLibrary, name);
        if (entry == null || entry.Clip == null)
        {
            Debug.LogWarning($"[AudioManager] Música '{name}' não encontrada.");
            return;
        }

        MusicSource.clip = entry.Clip;
        MusicSource.loop = entry.Loop;
        float targetVolume = entry.Volume * MusicVolume * MasterVolume;
        MusicSource.volume = instant ? targetVolume : 0f;
        MusicSource.Play();
        currentMusicName = name;
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