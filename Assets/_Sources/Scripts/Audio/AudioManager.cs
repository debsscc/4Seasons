using UnityEngine;
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
    public AudioSource MusicSourceB;

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
    public List<NamedClip> SFXLibrary = new();

    [FoldoutGroup("Settings")]
    [Tooltip("Duração do crossfade ao trocar música")]
    public float FadeDuration = 1f;

    private string currentMusicName = null;
    private Coroutine currentFadeCoroutine = null;

    private AudioSource _activeMusicSource;
    private AudioClip _currentClip;
    private float _currentTrackVolume = 1f;
    private bool _currentLoop = true;

    public AudioClip CurrentMusicClip => _currentClip;
    public AudioClip CurrentSFXClip => SFXSource != null ? SFXSource.clip : null;
    public AudioClip CurrentUIClip => UISource != null ? UISource.clip : null;

    private AudioSource InactiveMusicSource =>
        _activeMusicSource == MusicSource ? MusicSourceB : MusicSource;

    private void Start()
    {
        AutoSetup();
        ApplyMixer();
        InitializeMusicState();
    }

    [FoldoutGroup("Setup Tools")]
    [Button("Gerar AudioSources Automaticamente")]
    private void AutoSetup()
    {
        CreateSourceIfMissing(ref MusicSource, "MusicSource", loop: true);
        CreateSourceIfMissing(ref MusicSourceB, "MusicSourceB", loop: true);
        CreateSourceIfMissing(ref SFXSource, "SFXSource", loop: false);
        CreateSourceIfMissing(ref UISource, "UISource", loop: false);
    }

    private void InitializeMusicState()
    {
        _activeMusicSource = MusicSource;

        if (MusicSource != null && MusicSource.isPlaying && MusicSource.clip != null)
        {
            _currentClip = MusicSource.clip;
            _currentLoop = MusicSource.loop;
            currentMusicName = MusicSource.clip.name;
            return;
        }

        if (MusicSourceB != null && MusicSourceB.isPlaying && MusicSourceB.clip != null)
        {
            _activeMusicSource = MusicSourceB;
            _currentClip = MusicSourceB.clip;
            _currentLoop = MusicSourceB.loop;
            currentMusicName = MusicSourceB.clip.name;
        }
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

            if (MusicSource != null && source != MusicSource)
                source.outputAudioMixerGroup = MusicSource.outputAudioMixerGroup;

#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create AudioSource");
#endif
        }
    }

    private void ApplyMixer()
    {
        if (MusicSource != null)
            MusicSource.volume = MasterVolume * MusicVolume;
        if (MusicSourceB != null)
            MusicSourceB.volume = MasterVolume * MusicVolume;
        if (SFXSource != null)
            SFXSource.volume = MasterVolume * SFXVolume;
        if (UISource != null)
            UISource.volume = MasterVolume * UIVolume;
    }

    private NamedClip FindClip(List<NamedClip> list, string name)
    {
        return list.Find(c => c.Name == name);
    }

    private float TargetVolume(float trackVolume)
    {
        return trackVolume * MusicVolume * MasterVolume;
    }

    private bool HasAudibleMusic()
    {
        return _currentClip != null
            && _activeMusicSource != null
            && _activeMusicSource.clip == _currentClip
            && (_activeMusicSource.isPlaying || _activeMusicSource.time > 0f)
            && _activeMusicSource.volume > 0.001f;
    }

    public void PlayMusic(AudioClip audioClip, bool loop = true, float volume = 1, bool instant = false)
    {
        if (audioClip == null)
            return;

        CancelActiveFade();

        _currentLoop = loop;
        _currentTrackVolume = volume;
        float targetVolume = TargetVolume(volume);

        if (audioClip == _currentClip && HasAudibleMusic()
            && Mathf.Approximately(_activeMusicSource.volume, targetVolume))
        {
            _activeMusicSource.loop = loop;
            return;
        }

        if (instant)
        {
            PlayInstant(audioClip, loop, targetVolume);
            return;
        }

        if (_currentClip == null || !HasAudibleMusic())
        {
            currentFadeCoroutine = StartCoroutine(FadeInMusic(audioClip, loop, targetVolume));
            return;
        }

        if (audioClip == _currentClip)
        {
            _activeMusicSource.loop = loop;
            currentFadeCoroutine = StartCoroutine(FadeVolumeTo(_activeMusicSource, targetVolume));
            return;
        }

        currentFadeCoroutine = StartCoroutine(CrossfadeTo(audioClip, loop, targetVolume));
    }

    public void StopMusic()
    {
        CancelActiveFade();
        StopMusicSource(MusicSource);
        StopMusicSource(MusicSourceB);

        _activeMusicSource = MusicSource;
        _currentClip = null;
        currentMusicName = null;
    }

    public void PauseMusic()
    {
        MusicSource?.Pause();
        MusicSourceB?.Pause();
    }

    public void UnpauseMusic()
    {
        MusicSource?.UnPause();
        MusicSourceB?.UnPause();
    }

    private void PlayInstant(AudioClip audioClip, bool loop, float targetVolume)
    {
        StopMusicSource(MusicSource);
        StopMusicSource(MusicSourceB);

        _activeMusicSource = MusicSource;
        MusicSource.clip = audioClip;
        MusicSource.loop = loop;
        MusicSource.volume = targetVolume;
        MusicSource.Play();

        _currentClip = audioClip;
        currentMusicName = audioClip.name;
    }

    private IEnumerator FadeInMusic(AudioClip audioClip, bool loop, float targetVolume)
    {
        StopMusicSource(MusicSource);
        StopMusicSource(MusicSourceB);

        _activeMusicSource = MusicSource;
        _activeMusicSource.clip = audioClip;
        _activeMusicSource.loop = loop;
        _activeMusicSource.volume = 0f;
        _activeMusicSource.Play();

        _currentClip = audioClip;
        currentMusicName = audioClip.name;

        yield return FadeVolumeTo(_activeMusicSource, targetVolume);
        currentFadeCoroutine = null;
    }

    private IEnumerator CrossfadeTo(AudioClip audioClip, bool loop, float targetVolume)
    {
        AudioSource outgoing = _activeMusicSource;
        AudioSource incoming = InactiveMusicSource;

        float duration = Mathf.Max(0.01f, FadeDuration);
        float startOutgoingVolume = outgoing != null ? outgoing.volume : 0f;

        incoming.clip = audioClip;
        incoming.loop = loop;
        incoming.volume = 0f;
        incoming.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (outgoing != null && outgoing.isPlaying)
                outgoing.volume = Mathf.Lerp(startOutgoingVolume, 0f, t);

            incoming.volume = Mathf.Lerp(0f, targetVolume, t);
            yield return null;
        }

        if (outgoing != null)
            StopMusicSource(outgoing);

        incoming.volume = targetVolume;
        _activeMusicSource = incoming;
        _currentClip = audioClip;
        currentMusicName = audioClip.name;
        currentFadeCoroutine = null;
    }

    private IEnumerator FadeVolumeTo(AudioSource source, float targetVolume)
    {
        if (source == null)
            yield break;

        float duration = Mathf.Max(0.01f, FadeDuration);
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            source.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        source.volume = targetVolume;
    }

    private void CancelActiveFade()
    {
        if (currentFadeCoroutine == null)
            return;

        StopCoroutine(currentFadeCoroutine);
        currentFadeCoroutine = null;

        AudioSource incoming = InactiveMusicSource;
        if (incoming != null && incoming.isPlaying && incoming.clip != _currentClip)
            StopMusicSource(incoming);
    }

    private static void StopMusicSource(AudioSource source)
    {
        if (source == null)
            return;

        source.Stop();
        source.clip = null;
        source.volume = 0f;
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
