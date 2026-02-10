using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

// ============================================
// BLOCK - Refactored theo SOLID
// ============================================

public class Block : MonoBehaviour, IMergeable, IMovable
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private TextMeshPro _text;

    private Material _material;
    private Color _baseColor;
    private BlockConfig _config;
    private IAudioService _audioService;

    // IMergeable
    public int Value { get; private set; }
    public Block MergingBlock { get; private set; }
    public bool IsMergeTarget { get; private set; }

    // IMovable
    public Vector2 Position => transform.position;

    // Node reference
    public Node Node { get; private set; }

    /// <summary>
    /// Khởi tạo block với dependencies (DIP)
    /// </summary>
    public void Initialize(int value, BlockConfig config, IAudioService audioService)
    {
        Value = value;
        _config = config;
        _audioService = audioService;

        // DEBUG: Check config
        if (_config == null)
        {
            Debug.LogError("[Block] BlockConfig is NULL! Assign BlockConfig in BlockSpawner.");
        }

        SetupVisuals();
        PlaySpawnAnimation();
        _audioService?.PlaySpawn();
    }

    /// <summary>
    /// Gán block vào node
    /// </summary>
    public void SetBlock(Node node)
    {
        if (Node != null)
            Node.OccupiedBlock = null;

        Node = node;
        Node.OccupiedBlock = this;
    }

    /// <summary>
    /// Kiểm tra có thể merge không
    /// </summary>
    public bool CanMerge(int value)
    {
        return Value == value && !IsMergeTarget && MergingBlock == null;
    }

    /// <summary>
    /// Được gọi khi merge xảy ra
    /// </summary>
    public void OnMerge()
    {
        _audioService?.PlayMerge();
    }

    /// <summary>
    /// Set block sẽ merge vào
    /// </summary>
    public void SetMergingBlock(Block target)
    {
        MergingBlock = target;
    }

    /// <summary>
    /// Đánh dấu block là target của merge
    /// </summary>
    public void SetAsMergeTarget()
    {
        IsMergeTarget = true;
    }

    /// <summary>
    /// Reset trạng thái merge
    /// </summary>
    public void ResetMergeState()
    {
        IsMergeTarget = false;
        MergingBlock = null;
    }

    /// <summary>
    /// Di chuyển đến vị trí (IMovable)
    /// </summary>
    public void MoveTo(Vector2 target, float duration, Action onComplete = null)
    {
        transform.DOMove(target, duration).OnComplete(() => onComplete?.Invoke());
    }

    // ============================================
    // ANIMATIONS
    // ============================================

    public void PlaySpawnAnimation()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }

    public void PlayMergeAnimation()
    {
        DOTween.Sequence()
            .Append(transform.DOScale(1.2f, 0.12f))
            .Append(transform.DOScale(1f, 0.12f));

        PlayHighlight();
        PlayMergeVFX();
    }

    public void PlayHighlight()
    {
        if (_material == null) return;

        _material.DOColor(Color.white, "_Color", 0.1f).OnComplete(() =>
        {
            _material.DOColor(_baseColor, "_Color", 0.15f);
        });
    }

    public void PlayDestroyAnimation()
    {
        if (_material == null) return;
        _material.DOFade(0f, 0.12f);
    }

    public void PlayMergeVFX()
    {
        if (_config == null) return;

        GameObject vfxPrefab = _config.GetVFXForValue(Value);
        if (vfxPrefab != null)
        {
            GameObject vfx = Instantiate(vfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }

    // ============================================
    // PRIVATE METHODS
    // ============================================

    private void SetupVisuals()
    {
        // Setup material
        if (_renderer != null)
        {
            _material = new Material(_renderer.material);
            _renderer.material = _material;
        }
        else
        {
            Debug.LogError("[Block] SpriteRenderer is NULL! Assign in Block prefab.");
        }

        // Setup color
        if (_config != null)
        {
            _baseColor = _config.GetColorForValue(Value);
        }
        else
        {
            // FALLBACK: Màu mặc định nếu không có config
            Debug.LogWarning($"[Block] No BlockConfig! Using fallback color for value {Value}");
            _baseColor = GetFallbackColor(Value);
        }

        if (_renderer != null)
            _renderer.color = _baseColor;

        // Setup text
        if (_text != null)
        {
            _text.text = Value.ToString();
        }
        else
        {
            Debug.LogError("[Block] TextMeshPro is NULL! Assign in Block prefab.");
        }
    }

    /// <summary>
    /// Màu fallback nếu không có BlockConfig
    /// </summary>
    private Color GetFallbackColor(int value)
    {
        switch (value)
        {
            case 2: return new Color32(238, 228, 218, 255);
            case 4: return new Color32(237, 224, 200, 255);
            case 8: return new Color32(242, 177, 121, 255);
            case 16: return new Color32(245, 149, 99, 255);
            case 32: return new Color32(246, 124, 95, 255);
            case 64: return new Color32(246, 94, 59, 255);
            case 128: return new Color32(237, 207, 114, 255);
            case 256: return new Color32(237, 204, 97, 255);
            case 512: return new Color32(237, 200, 80, 255);
            case 1024: return new Color32(237, 197, 63, 255);
            case 2048: return new Color32(237, 194, 46, 255);
            default: return Color.gray;
        }
    }
}