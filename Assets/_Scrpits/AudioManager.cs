using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource sfxSource;

    [Header("SFX Clips")]
    public AudioClip spawnClip;
    public AudioClip mergeClip;
    public AudioClip moveClip;
    public AudioClip winClip;
    public AudioClip gameOverClip;

    public void PlaySpawn() => Play(spawnClip);
    public void PlayMerge() => Play(mergeClip);
    public void PlayMove() => Play(moveClip);
    public void PlayWin() => Play(winClip);
    public void PlayGameOver() => Play(gameOverClip);

    private void Play(AudioClip clip)
    {
        if (clip == null) return;
        if (sfxSource == null) return;

        sfxSource.PlayOneShot(clip);
        Debug.Log("SFX: " + clip.name);
    }
}
