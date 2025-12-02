using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("SFX Clips")]
    public AudioClip spawnClip;
    public AudioClip mergeClip;
    public AudioClip moveClip;
    public AudioClip winClip;
    public AudioClip gameOverClip;

    [Header("Audio States")]
    public bool isMusicOn = true;
    public bool isSfxOn = true;

    // --- MUSIC ---
    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        if (musicSource != null)
            musicSource.mute = !isMusicOn;
        Debug.Log("Music: " + (isMusicOn ? "ON" : "OFF"));
    }

    // --- SFX ---
    public void ToggleSFX()
    {
        isSfxOn = !isSfxOn;
        Debug.Log("SFX: " + (isSfxOn ? "ON" : "OFF"));
    }

    public void PlaySpawn() => Play(sfxSource, spawnClip);
    public void PlayMerge() => Play(sfxSource, mergeClip);
    public void PlayMove() => Play(sfxSource, moveClip);
    public void PlayWin() => Play(sfxSource, winClip);
    public void PlayGameOver() => Play(sfxSource, gameOverClip);

    private void Play(AudioSource source, AudioClip clip)
    {
        if (clip == null || source == null || !isSfxOn) return;

        source.PlayOneShot(clip);
        Debug.Log("SFX: " + clip.name);
    }
}
