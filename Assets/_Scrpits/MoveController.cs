using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// ============================================
// MOVE CONTROLLER - Tuân thủ SRP
// Chỉ xử lý logic di chuyển và merge
// ============================================

public class MoveController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float _travelTime = 0.2f;

    private IGridManager _gridManager;
    private IAudioService _audioService;
    private IScoreService _scoreService;
    private IBlockFactory _blockFactory;

    public event Action OnMoveComplete;
    public event Action<int> OnBlockMerged; // Trả về giá trị block sau merge

    /// <summary>
    /// Khởi tạo với dependencies
    /// </summary>
    public void Initialize(
        IGridManager gridManager,
        IAudioService audioService,
        IScoreService scoreService,
        IBlockFactory blockFactory)
    {
        _gridManager = gridManager;
        _audioService = audioService;
        _scoreService = scoreService;
        _blockFactory = blockFactory;
    }

    /// <summary>
    /// Di chuyển blocks theo hướng
    /// </summary>
    public void Move(Vector2 direction, List<Block> blocks)
    {
        _audioService?.PlayMove();

        // Reset trạng thái merge
        foreach (var block in blocks)
        {
            block.ResetMergeState();
        }

        // Sắp xếp blocks theo hướng di chuyển
        var orderedBlocks = GetOrderedBlocks(blocks, direction);

        // Tính toán vị trí mới
        foreach (var block in orderedBlocks)
        {
            CalculateNewPosition(block, direction);
        }

        // Animate di chuyển
        AnimateMovement(orderedBlocks);
    }

    /// <summary>
    /// Sắp xếp blocks theo hướng di chuyển
    /// </summary>
    private IEnumerable<Block> GetOrderedBlocks(List<Block> blocks, Vector2 direction)
    {
        IEnumerable<Block> ordered = blocks.OrderBy(b => b.Position.x).ThenBy(b => b.Position.y);

        if (direction == Vector2.right || direction == Vector2.up)
            ordered = ordered.Reverse();

        return ordered;
    }

    /// <summary>
    /// Tính toán vị trí mới cho block
    /// </summary>
    private void CalculateNewPosition(Block block, Vector2 direction)
    {
        Node currentNode = block.Node;
        Node nextNode;

        while (true)
        {
            nextNode = _gridManager.GetNodeAt(currentNode.Pos + direction);
            if (nextNode == null) break;

            // Ô trống -> tiếp tục di chuyển
            if (nextNode.OccupiedBlock == null)
            {
                currentNode = nextNode;
                continue;
            }

            // Có thể merge
            if (nextNode.OccupiedBlock.CanMerge(block.Value))
            {
                block.SetMergingBlock(nextNode.OccupiedBlock);
                nextNode.OccupiedBlock.SetAsMergeTarget();
                currentNode = nextNode;
            }

            break;
        }

        // Cập nhật vị trí
        block.Node.OccupiedBlock = null;
        block.SetBlock(currentNode);
    }

    /// <summary>
    /// Animate di chuyển của tất cả blocks
    /// </summary>
    private void AnimateMovement(IEnumerable<Block> blocks)
    {
        var sequence = DOTween.Sequence();

        foreach (var block in blocks)
        {
            Vector2 targetPos = block.MergingBlock != null
                ? block.MergingBlock.Node.Pos
                : block.Node.Pos;

            sequence.Join(block.transform.DOMove(targetPos, _travelTime));
        }

        sequence.OnComplete(() => HandleMerges(blocks.ToList()));
    }

    /// <summary>
    /// Xử lý merge sau khi di chuyển xong
    /// </summary>
    private void HandleMerges(List<Block> blocks)
    {
        HashSet<Block> processedBlocks = new HashSet<Block>();

        foreach (var block in blocks)
        {
            if (block.MergingBlock != null && !processedBlocks.Contains(block.MergingBlock))
            {
                MergeBlocks(block, block.MergingBlock);

                processedBlocks.Add(block);
                processedBlocks.Add(block.MergingBlock);
            }
        }

        OnMoveComplete?.Invoke();
    }

    /// <summary>
    /// Merge 2 blocks
    /// </summary>
    private void MergeBlocks(Block blockA, Block blockB)
    {
        _audioService?.PlayMerge();

        Node targetNode = blockA.Node;
        int newValue = blockA.Value * 2;

        // Clear old blocks
        blockA.Node.OccupiedBlock = null;
        blockB.Node.OccupiedBlock = null;

        // Tạo block mới
        Block newBlock = _blockFactory.CreateBlock(targetNode.Pos, newValue);
        newBlock.SetBlock(targetNode);
        newBlock.PlayMergeAnimation();

        // Cập nhật điểm
        _scoreService?.AddScore(newValue);
        OnBlockMerged?.Invoke(newValue);

        // Hủy blocks cũ
        blockA.PlayDestroyAnimation();
        blockB.PlayDestroyAnimation();

        Destroy(blockA.gameObject, 0.15f);
        Destroy(blockB.gameObject, 0.15f);
    }
}