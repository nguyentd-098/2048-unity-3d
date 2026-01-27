using UnityEngine;
using TMPro;

// ============================================
// UI MANAGER - Tuân thủ SRP
// Chỉ quản lý UI
// ============================================

public class UIManager : MonoBehaviour, IUIService
{
    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _bestScoreText;

    [Header("Screen Panels")]
    [SerializeField] private GameObject _winScreen;
    [SerializeField] private GameObject _loseScreen;

    void Start()
    {
        HideAllScreens();
    }

    /// <summary>
    /// Cập nhật điểm hiển thị
    /// </summary>
    public void UpdateScore(int score)
    {
        if (_scoreText != null)
            _scoreText.text = score.ToString();
    }

    /// <summary>
    /// Cập nhật best score hiển thị
    /// </summary>
    public void UpdateBestScore(int bestScore)
    {
        if (_bestScoreText != null)
            _bestScoreText.text = bestScore.ToString();
    }

    /// <summary>
    /// Hiện màn hình thắng
    /// </summary>
    public void ShowWinScreen()
    {
        if (_winScreen != null)
            _winScreen.SetActive(true);
    }

    /// <summary>
    /// Ẩn màn hình thắng
    /// </summary>
    public void HideWinScreen()
    {
        if (_winScreen != null)
            _winScreen.SetActive(false);
    }

    /// <summary>
    /// Hiện màn hình thua
    /// </summary>
    public void ShowLoseScreen()
    {
        if (_loseScreen != null)
            _loseScreen.SetActive(true);
    }

    /// <summary>
    /// Ẩn màn hình thua
    /// </summary>
    public void HideLoseScreen()
    {
        if (_loseScreen != null)
            _loseScreen.SetActive(false);
    }

    /// <summary>
    /// Ẩn tất cả màn hình
    /// </summary>
    public void HideAllScreens()
    {
        HideWinScreen();
        HideLoseScreen();
    }
}