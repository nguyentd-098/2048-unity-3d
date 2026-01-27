using UnityEngine;

// ============================================
// AUDIO MANAGER - Implement IAudioService
// Tuân thủ DIP và ISP
// ============================================

public class AudioManager : MonoBehaviour, IAudioService
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip _spawnClip;
    [SerializeField] private AudioClip _mergeClip;
    [SerializeField] private AudioClip _moveClip;
    [SerializeField] private AudioClip _winClip;
    [SerializeField] private AudioClip _gameOverClip;

    private bool _isMusicOn = true;
    private bool _isSfxOn = true;

    public bool IsMusicOn => _isMusicOn;
    public bool IsSfxOn => _isSfxOn;

    void Awake()
    {
        LoadSettings();
    }

    /// <summary>
    /// Bật/tắt nhạc
    /// </summary>
    public void ToggleMusic()
    {
        _isMusicOn = !_isMusicOn;

        if (_musicSource != null)
            _musicSource.mute = !_isMusicOn;

        SaveSettings();
    }

    /// <summary>
    /// Bật/tắt SFX
    /// </summary>
    public void ToggleSFX()
    {
        _isSfxOn = !_isSfxOn;
        SaveSettings();
    }

    // ============================================
    // PLAY SOUNDS
    // ============================================

    public void PlaySpawn() => PlaySFX(_spawnClip);
    public void PlayMerge() => PlaySFX(_mergeClip);
    public void PlayMove() => PlaySFX(_moveClip);
    public void PlayWin() => PlaySFX(_winClip);
    public void PlayGameOver() => PlaySFX(_gameOverClip);

    private void PlaySFX(AudioClip clip)
    {
        if (!_isSfxOn || clip == null || _sfxSource == null) return;

        _sfxSource.PlayOneShot(clip);
    }

    // ============================================
    // SAVE/LOAD
    // ============================================

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("MusicOn", _isMusicOn ? 1 : 0);
        PlayerPrefs.SetInt("SfxOn", _isSfxOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        _isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        _isSfxOn = PlayerPrefs.GetInt("SfxOn", 1) == 1;

        if (_musicSource != null)
            _musicSource.mute = !_isMusicOn;
    }
}