using UnityEngine;

public class SceneMusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip _musicClip;
    [SerializeField] private bool _loop = true;
    [SerializeField, Range(0f, 1f)] private float _volume = 1f;

    void Start()
    {
        PlayMusicIfNotPlaying();
    }

    private void PlayMusicIfNotPlaying()
    {
        AudioManager audioManager = AudioManager.Instance;
        if(audioManager == null)
        {
            Debug.LogWarning("AudioManager instance not found.");
            return;
        }

        if (audioManager.CurrentMusicClip != _musicClip)
            audioManager.PlayMusic(_musicClip, _loop, _volume);
    }
}