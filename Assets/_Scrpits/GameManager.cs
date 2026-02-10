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
        // Đăng ký sự kiện: Khi MoveController báo xong, thì chạy SyncBlocks
        _moveController.OnMoveComplete += OnMoveFinished;
    }

    void Start()
    {
        Debug.Log("==============================================");
        Debug.Log("[GameManager] Start - Initializing game...");
        Debug.Log("==============================================");

        InitializeGame();


    }

    private void OnMoveFinished()
    {
        // 1. Cập nhật lại danh sách "sạch" từ Grid
        SyncBlocksFromGrid();

        // 2. Sau đó mới thực hiện các logic tiếp theo như Spawn block mới
        SpawnAfterMove();

        // 3. Kiểm tra thắng/thua
        CheckGameStatus();
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
        // 1. Lọc bỏ các Block null hoặc đã bị destroy trước khi xử lý
        _blocks.RemoveAll(b => b == null);

        if (_blocks.Count == 0) return;

        // 2. Gọi move với danh sách sạch
        _moveController.Move(direction, _blocks);
    }

    private void OnMoveComplete()
    {
        Debug.Log("[GameManager] 🏁 Move complete - Spawning new block...");

        var newBlocks = _blockSpawner.SpawnBlocks(1);
        _blocks.AddRange(newBlocks);

        _blocks.RemoveAll(b => b == null);

        CheckGameStatus();
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

    private void CheckGameStatus()
    {
        // 1. Kiểm tra THẮNG: Tìm xem có block nào đạt giá trị 2048 chưa
        // Chỉ kiểm tra nếu chưa thắng trước đó
        if (!_hasWon)
        {
            foreach (var block in _blocks)
            {
                if (block != null && block.Value >= _winValue)
                {
                    _hasWon = true;
                    _state = GameState.Win;
                    _uiManager.ShowWinScreen();
                    _audioManager.PlayWin();
                    return; // Thoát hàm vì đã thắng
                }
            }
        }

        // 2. Kiểm tra THUA: Nếu hết ô trống VÀ không thể di chuyển được nữa
        if (_gridManager.EmptyNodeCount == 0 && !CanMove())
        {
            _state = GameState.GameOver;
            _uiManager.ShowLoseScreen();
            _audioManager.PlayGameOver();
            Debug.Log("Game Over!");
        }
        else
        {
            // Nếu vẫn còn chơi được, cho phép nhận Input tiếp theo
            _state = GameState.WaitingInput;
        }
    }
    private void SyncBlocksFromGrid()
    {
        _blocks.Clear(); // Xóa sạch danh sách cũ

        // Duyệt qua toàn bộ các ô (Node) trên Grid
        for (int x = 0; x < _gridManager.Width; x++)
        {
            for (int y = 0; y < _gridManager.Height; y++)
            {
                var node = _gridManager.GetNodeAt(new Vector2(x, y));

                // Nếu ô này có chứa Block, hãy thêm nó vào danh sách quản lý
                if (node != null && node.OccupiedBlock != null)
                {
                    _blocks.Add(node.OccupiedBlock);
                }
            }
        }

        if (_showDebugLog) Debug.Log($"[GameManager] Synced {_blocks.Count} blocks from grid.");
    }

    private void SpawnAfterMove()
    {
        // Chỉ spawn nếu Grid còn chỗ trống
        if (_gridManager.EmptyNodeCount > 0)
        {
            // Thường 2048 sẽ spawn 1 block mỗi lượt di chuyển
            var newBlocks = _blockSpawner.SpawnBlocks(1);

            // Thêm các block mới vào danh sách quản lý
            foreach (var b in newBlocks)
            {
                _blocks.Add(b);
            }

            if (_showDebugLog) Debug.Log("[GameManager] Spawned new block.");
        }
    }

    private bool CanMove()
    {
        // Duyệt qua từng ô trên Grid
        for (int x = 0; x < _gridManager.Width; x++)
        {
            for (int y = 0; y < _gridManager.Height; y++)
            {
                Node currentNode = _gridManager.GetNodeAt(new Vector2(x, y));
                if (currentNode == null || currentNode.OccupiedBlock == null) continue;

                int currentVal = currentNode.OccupiedBlock.Value;

                // Kiểm tra 4 hướng xung quanh ô hiện tại
                Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };

                foreach (Vector2 dir in directions)
                {
                    Node neighbor = _gridManager.GetNodeAt(new Vector2(x + dir.x, y + dir.y));

                    // Nếu có ô hàng xóm trống -> Vẫn di chuyển được
                    if (neighbor != null && neighbor.OccupiedBlock == null)
                        return true;

                    // Nếu ô hàng xóm có block cùng giá trị -> Vẫn merge được
                    if (neighbor != null && neighbor.OccupiedBlock != null)
                    {
                        if (neighbor.OccupiedBlock.Value == currentVal)
                            return true;
                    }
                }
            }
        }
        return false; // Không còn nước đi nào
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
    GameOver,
    Win
}