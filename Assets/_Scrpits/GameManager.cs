using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// ============================================
// GAME MANAGER - Touch Input Version
// Dùng TouchInputHandler
// ============================================

public class GameManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private BlockSpawner _blockSpawner;
    [SerializeField] private MoveController _moveController;
    [SerializeField] private TouchInputHandler _inputHandler; // ← Touch only!
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private AudioManager _audioManager;

    [Header("Game Settings")]
    [SerializeField] private int _winValue = 2048;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLog = true;

    private List<Block> _blocks = new List<Block>();
    private GameState _state;
    private bool _hasWon = false;

    void Awake()
    {
        Debug.Log("==============================================");
        Debug.Log("[GameManager] Awake - Finding dependencies...");
        Debug.Log("==============================================");

        AutoFindDependencies();
    }

    void Start()
    {
        Debug.Log("==============================================");
        Debug.Log("[GameManager] Start - Initializing game...");
        Debug.Log("==============================================");

        InitializeGame();
    }

    private void AutoFindDependencies()
    {
        if (_gridManager == null)
        {
            _gridManager = GetComponent<GridManager>();
            if (_gridManager == null)
                _gridManager = FindFirstObjectByType<GridManager>();

            Debug.Log(_gridManager != null
                ? "[GameManager] ✅ GridManager found"
                : "[GameManager] ❌ GridManager NOT FOUND!");
        }

        if (_blockSpawner == null)
        {
            _blockSpawner = GetComponent<BlockSpawner>();
            if (_blockSpawner == null)
                _blockSpawner = FindFirstObjectByType<BlockSpawner>();

            Debug.Log(_blockSpawner != null
                ? "[GameManager] ✅ BlockSpawner found"
                : "[GameManager] ❌ BlockSpawner NOT FOUND!");
        }

        if (_moveController == null)
        {
            _moveController = GetComponent<MoveController>();
            if (_moveController == null)
                _moveController = FindFirstObjectByType<MoveController>();

            Debug.Log(_moveController != null
                ? "[GameManager] ✅ MoveController found"
                : "[GameManager] ❌ MoveController NOT FOUND!");
        }

        if (_inputHandler == null)
        {
            _inputHandler = GetComponent<TouchInputHandler>();
            if (_inputHandler == null)
                _inputHandler = FindFirstObjectByType<TouchInputHandler>();

            Debug.Log(_inputHandler != null
                ? "[GameManager] ✅ TouchInputHandler found"
                : "[GameManager] ❌ TouchInputHandler NOT FOUND!");
        }

        if (_scoreManager == null)
        {
            _scoreManager = GetComponent<ScoreManager>();
            if (_scoreManager == null)
                _scoreManager = FindFirstObjectByType<ScoreManager>();

            Debug.Log(_scoreManager != null
                ? "[GameManager] ✅ ScoreManager found"
                : "[GameManager] ❌ ScoreManager NOT FOUND!");
        }

        if (_uiManager == null)
        {
            _uiManager = GetComponent<UIManager>();
            if (_uiManager == null)
                _uiManager = FindFirstObjectByType<UIManager>();

            Debug.Log(_uiManager != null
                ? "[GameManager] ✅ UIManager found"
                : "[GameManager] ❌ UIManager NOT FOUND!");
        }

        if (_audioManager == null)
        {
            _audioManager = GetComponent<AudioManager>();
            if (_audioManager == null)
                _audioManager = FindFirstObjectByType<AudioManager>();

            Debug.Log(_audioManager != null
                ? "[GameManager] ✅ AudioManager found"
                : "[GameManager] ⚠️ AudioManager not found (optional)");
        }
    }

    private void InitializeGame()
    {
        if (!ValidateDependencies())
        {
            Debug.LogError("==============================================");
            Debug.LogError("[GameManager] ❌ FAILED - Missing dependencies!");
            Debug.LogError("Check Inspector and assign missing components!");
            Debug.LogError("==============================================");
            return;
        }

        Debug.Log("[GameManager] 🔗 Subscribing to events...");

        // Subscribe to input
        _inputHandler.OnSwipe += HandleSwipe;
        Debug.Log("[GameManager] ✅ Subscribed to TouchInput.OnSwipe");

        // Subscribe to move controller
        _moveController.OnMoveComplete += OnMoveComplete;
        _moveController.OnBlockMerged += OnBlockMerged;
        Debug.Log("[GameManager] ✅ Subscribed to MoveController events");

        // Subscribe to score
        _scoreManager.OnScoreChanged += _uiManager.UpdateScore;
        _scoreManager.OnBestScoreChanged += _uiManager.UpdateBestScore;
        Debug.Log("[GameManager] ✅ Subscribed to ScoreManager events");

        // Inject dependencies
        Debug.Log("[GameManager] 💉 Injecting dependencies...");
        _blockSpawner.Initialize(_gridManager, _audioManager, _scoreManager);
        _moveController.Initialize(_gridManager, _audioManager, _scoreManager, _blockSpawner);

        // Start game
        Debug.Log("[GameManager] 🎮 Starting game...");
        _gridManager.Initialize();
        _uiManager.UpdateBestScore(_scoreManager.BestScore);

        SpawnInitialBlocks();
        ChangeState(GameState.WaitingInput);

        Debug.Log("==============================================");
        Debug.Log("[GameManager] ✅ GAME READY - Swipe to play!");
        Debug.Log("==============================================");
    }

    private bool ValidateDependencies()
    {
        bool isValid = true;

        if (_gridManager == null)
        {
            Debug.LogError("[GameManager] GridManager is NULL!");
            isValid = false;
        }

        if (_blockSpawner == null)
        {
            Debug.LogError("[GameManager] BlockSpawner is NULL!");
            isValid = false;
        }

        if (_moveController == null)
        {
            Debug.LogError("[GameManager] MoveController is NULL!");
            isValid = false;
        }

        if (_inputHandler == null)
        {
            Debug.LogError("[GameManager] TouchInputHandler is NULL!");
            isValid = false;
        }

        if (_scoreManager == null)
        {
            Debug.LogError("[GameManager] ScoreManager is NULL!");
            isValid = false;
        }

        if (_uiManager == null)
        {
            Debug.LogError("[GameManager] UIManager is NULL!");
            isValid = false;
        }

        return isValid;
    }

    void OnDestroy()
    {
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

    private void ChangeState(GameState newState)
    {
        if (_showDebugLog)
            Debug.Log($"[GameManager] 🔄 State: {_state} → {newState}");

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

        Debug.Log($"[GameManager] 🎲 Spawned {spawned.Count} blocks. Total: {_blocks.Count}");
    }

    private void HandleSwipe(Vector2 direction)
    {
        Debug.Log("==============================================");
        Debug.Log($"[GameManager] 👆 HandleSwipe called!");
        Debug.Log($"Direction: {direction}");
        Debug.Log($"State: {_state}");
        Debug.Log($"Blocks count: {_blocks.Count}");
        Debug.Log("==============================================");

        if (_state != GameState.WaitingInput)
        {
            Debug.LogWarning($"[GameManager] ⚠️ Cannot move! State is {_state}, not WaitingInput");
            return;
        }

        if (_blocks.Count == 0)
        {
            Debug.LogError("[GameManager] ❌ Blocks list is EMPTY!");
            return;
        }

        Debug.Log($"[GameManager] ✅ Calling MoveController.Move() with {_blocks.Count} blocks");

        ChangeState(GameState.Moving);
        _moveController.Move(direction, _blocks);
    }

    private void OnMoveComplete()
    {
        Debug.Log("[GameManager] 🏁 Move complete - Spawning new block...");

        var newBlocks = _blockSpawner.SpawnBlocks(1);
        _blocks.AddRange(newBlocks);

        _blocks.RemoveAll(b => b == null);

        CheckGameEnd();
    }

    private void OnBlockMerged(int newValue)
    {
        Debug.Log($"[GameManager] 🎉 Block merged! New value: {newValue}");

        if (!_hasWon && newValue >= _winValue)
        {
            _hasWon = true;
            _uiManager.ShowWinScreen();
            _audioManager?.PlayWin();
            Debug.Log($"[GameManager] 🏆 YOU WIN! Reached {newValue}");
        }
    }

    private void CheckGameEnd()
    {
        if (_gridManager.EmptyNodeCount == 0 && !CanMove())
        {
            Debug.Log("[GameManager] 💀 GAME OVER - No moves left!");
            ChangeState(GameState.GameOver);
            return;
        }

        ChangeState(GameState.WaitingInput);
    }

    private bool CanMove()
    {
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

    public void ContinueGame()
    {
        Debug.Log("[GameManager] ▶️ Continue game");
        _uiManager.HideWinScreen();
        ChangeState(GameState.WaitingInput);
    }

    public void RestartGame()
    {
        Debug.Log("[GameManager] 🔄 Restart game");
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}

public enum GameState
{
    WaitingInput,
    Moving,
    GameOver
}