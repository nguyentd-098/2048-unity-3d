using UnityEngine;
using System.Collections.Generic;

// ============================================
// BLOCK SPAWNER - Tuân thủ SRP
// Chỉ spawn blocks
// ============================================

public class BlockSpawner : MonoBehaviour, IBlockFactory
{
    [Header("Prefab")]
    [SerializeField] private Block _blockPrefab;

    [Header("Configuration")]
    [SerializeField] private BlockConfig _blockConfig;

    [Header("Spawn Settings")]
    [SerializeField] private float _chanceOfFour = 0.2f; // 20% spawn số 4

    private IGridManager _gridManager;
    private IAudioService _audioService;
    private IScoreService _scoreService; // ← Thêm này!

    /// <summary>
    /// Khởi tạo spawner với dependencies
    /// </summary>
    public void Initialize(IGridManager gridManager, IAudioService audioService, IScoreService scoreService)
    {
        _gridManager = gridManager;
        _audioService = audioService;
        // Lưu scoreService nếu cần dùng để tính điểm khi spawn (nếu có logic đó)
    }

    /// <summary>
    /// Spawn blocks vào grid
    /// </summary>
    public List<Block> SpawnBlocks(int count)
    {
        List<Block> spawnedBlocks = new List<Block>();

        for (int i = 0; i < count; i++)
        {
            Node emptyNode = _gridManager.GetRandomEmptyNode();
            if (emptyNode == null) break;

            int value = GetRandomStartValue();
            Block block = CreateBlock(emptyNode.Pos, value);
            block.SetBlock(emptyNode);

            spawnedBlocks.Add(block);
        }

        return spawnedBlocks;
    }

    /// <summary>
    /// Tạo block mới (Factory Method)
    /// </summary>
    public Block CreateBlock(Vector2 position, int value)
    {
        Block block = Instantiate(_blockPrefab, position, Quaternion.identity, transform);
        block.Initialize(value, _blockConfig, _audioService);
        return block;
    }

    /// <summary>
    /// Hủy block
    /// </summary>
    public void DestroyBlock(Block block)
    {
        if (block != null)
        {
            block.Node.OccupiedBlock = null;
            Destroy(block.gameObject);
        }
    }

    /// <summary>
    /// Lấy giá trị khởi đầu ngẫu nhiên (2 hoặc 4)
    /// </summary>
    private int GetRandomStartValue()
    {
        return Random.value > _chanceOfFour ? 2 : 4;
    }
}