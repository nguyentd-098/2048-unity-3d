using DG.Tweening;
using System;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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
        if (_config == null) return;

        // Setup material
        if (_renderer != null)
        {
            _material = new Material(_renderer.material);
            _renderer.material = _material;
        }

        // Setup color (OCP - từ config)
        _baseColor = _config.GetColorForValue(Value);
        if (_renderer != null)
            _renderer.color = _baseColor;

        // Setup text
        if (_text != null)
            _text.text = Value.ToString();
    }
}