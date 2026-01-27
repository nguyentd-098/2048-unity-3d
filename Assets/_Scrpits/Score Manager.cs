using UnityEngine;
using System;

// ============================================
// SCORE MANAGER - Tuân thủ SRP
// Chỉ quản lý điểm số
// ============================================

public class ScoreManager : MonoBehaviour, IScoreService
{
    private const string BEST_SCORE_KEY = "BestScore";

    private int _currentScore;
    private int _bestScore;

    public int CurrentScore => _currentScore;
    public int BestScore => _bestScore;

    public event Action<int> OnScoreChanged;
    public event Action<int> OnBestScoreChanged;

    void Awake()
    {
        LoadBestScore();
    }

    /// <summary>
    /// Thêm điểm
    /// </summary>
    public void AddScore(int value)
    {
        _currentScore += value;
        OnScoreChanged?.Invoke(_currentScore);

        if (_currentScore > _bestScore)
        {
            _bestScore = _currentScore;
            OnBestScoreChanged?.Invoke(_bestScore);
            SaveBestScore();
        }
    }

    /// <summary>
    /// Reset điểm về 0
    /// </summary>
    public void ResetScore()
    {
        _currentScore = 0;
        OnScoreChanged?.Invoke(_currentScore);
    }

    private void LoadBestScore()
    {
        _bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
    }

    private void SaveBestScore()
    {
        PlayerPrefs.SetInt(BEST_SCORE_KEY, _bestScore);
        PlayerPrefs.Save();
    }
}