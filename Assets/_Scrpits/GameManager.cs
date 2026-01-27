using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// ============================================
// GAME MANAGER - Refactored theo SOLID
// Chỉ điều phối game flow (Orchestrator)
// ============================================

public class GameManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private BlockSpawner _blockSpawner;
    [SerializeField] private MoveController _moveController;
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private AudioManager _audioManager;

    [Header("Game Settings")]
    [SerializeField] private int _winValue = 2048;

    private List<Block> _blocks = new List<Block>();
    private GameState _state;
    private bool _hasWon = false;

    // ============================================
    // LIFECYCLE
    // ============================================

    void Start()
    {
        InitializeGame();
    }

    /// <summary>
    /// Khởi tạo game (Dependency Injection)
    /// </summary>
    private void InitializeGame()
    {
        // Subscribe to services
        _inputHandler.OnSwipe += HandleSwipe;
        _moveController.OnMoveComplete += OnMoveComplete;
        _moveController.OnBlockMerged += OnBlockMerged;
        _scoreManager.OnScoreChanged += _uiManager.UpdateScore;
        _scoreManager.OnBestScoreChanged += _uiManager.UpdateBestScore;

        // Inject dependencies
        _blockSpawner.Initialize(_gridManager, _audioManager);
        _moveController.Initialize(_gridManager, _audioManager, _scoreManager, _blockSpawner);

        // Start game
        _gridManager.Initialize();
        _uiManager.UpdateBestScore(_scoreManager.BestScore);

        SpawnInitialBlocks();
        ChangeState(GameState.WaitingInput);
    }

    void OnDestroy()
    {
        // Unsubscribe
        if (_inputHandler != null) _inputHandler.OnSwipe -= HandleSwipe;
        if (_moveController != null)
        {
            _moveController.OnMoveComplete -= OnMoveComplete;
            _moveController.OnBlockMerged -= OnBlockMerged;
        }
        if (_scoreManager != null)
        {
            _scoreManager.OnScoreChanged -= _uiManager.UpdateScore;
            _scoreManager.OnBestScoreChanged -= _uiManager.UpdateBestScore;
        }
    }

    // ============================================
    // GAME FLOW
    // ============================================

    private void ChangeState(GameState newState)
    {
        _state = newState;

        switch (newState)
        {
            case GameState.WaitingInput:
                _inputHandler.Enable();
                break;

            case GameState.Moving:
                _inputHandler.Disable();
                break;

            case GameState.GameOver:
                _inputHandler.Disable();
                _uiManager.ShowLoseScreen();
                _audioManager?.PlayGameOver();
                break;
        }
    }

    private void SpawnInitialBlocks()
    {
        var spawned = _blockSpawner.SpawnBlocks(2);
        _blocks.AddRange(spawned);
    }

    private void HandleSwipe(Vector2 direction)
    {
        if (_state != GameState.WaitingInput) return;

        ChangeState(GameState.Moving);
        _moveController.Move(direction, _blocks);
    }

    private void OnMoveComplete()
    {
        // Spawn new blocks
        var newBlocks = _blockSpawner.SpawnBlocks(1);
        _blocks.AddRange(newBlocks);

        // Clean up destroyed blocks
        _blocks.RemoveAll(b => b == null);

        // Check win/lose
        CheckGameEnd();
    }

    private void OnBlockMerged(int newValue)
    {
        // Check win condition
        if (!_hasWon && newValue >= _winValue)
        {
            _hasWon = true;
            _uiManager.ShowWinScreen();
            _audioManager?.PlayWin();
        }
    }

    private void CheckGameEnd()
    {
        // Check lose condition
        if (_gridManager.EmptyNodeCount == 0 && !CanMove())
        {
            ChangeState(GameState.GameOver);
            return;
        }

        ChangeState(GameState.WaitingInput);
    }

    /// <summary>
    /// Kiểm tra còn nước đi không
    /// </summary>
    private bool CanMove()
    {
        // Check horizontal merges
        for (int y = 0; y < _gridManager.Height; y++)
        {
            for (int x = 0; x < _gridManager.Width - 1; x++)
            {
                var node1 = _gridManager.GetNodeAt(new Vector2(x, y));
                var node2 = _gridManager.GetNodeAt(new Vector2(x + 1, y));

                if (node1.OccupiedBlock != null && node2.OccupiedBlock != null)
                {
                    if (node1.OccupiedBlock.Value == node2.OccupiedBlock.Value)
                        return true;
                }
            }
        }

        // Check vertical merges
        for (int x = 0; x < _gridManager.Width; x++)
        {
            for (int y = 0; y < _gridManager.Height - 1; y++)
            {
                var node1 = _gridManager.GetNodeAt(new Vector2(x, y));
                var node2 = _gridManager.GetNodeAt(new Vector2(x, y + 1));

                if (node1.OccupiedBlock != null && node2.OccupiedBlock != null)
                {
                    if (node1.OccupiedBlock.Value == node2.OccupiedBlock.Value)
                        return true;
                }
            }
        }

        return false;
    }

    // ============================================
    // PUBLIC METHODS (UI Callbacks)
    // ============================================

    public void ContinueGame()
    {
        _uiManager.HideWinScreen();
        ChangeState(GameState.WaitingInput);
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}

// ============================================
// GAME STATE ENUM
// ============================================

public enum GameState
{
    WaitingInput,
    Moving,
    GameOver
}