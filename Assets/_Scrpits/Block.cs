using UnityEngine;
using DG.Tweening;
using TMPro;

public class Block : MonoBehaviour
{
    public int Value;
    public Node Node;
    public Block MergingBlock;
    public bool Merging;

    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private TextMeshPro _text;

    private Material _mat;
    private Color _baseColor;   // màu gốc theo value

    public Vector2 Pos => transform.position;

    // -----------------------------------------
    // INIT (KHÔNG dùng BlockType)
    // -----------------------------------------
    public void Init(int value)
    {
        Value = value;

        // Clone material để DOTween an toàn
        if (_renderer != null)
        {
            _mat = new Material(_renderer.material);
            _renderer.material = _mat;
        }

        // Set màu theo giá trị
        _baseColor = GetColor(Value);
        if (_renderer != null) _renderer.color = _baseColor;

        // Set text
        if (_text != null) _text.text = Value.ToString();

        PlaySpawnAnimation();
    }

    // -----------------------------------------
    // GÁN NODE
    // -----------------------------------------
    public void SetBlock(Node node)
    {
        if (Node != null)
            Node.OccupiedBlock = null;

        Node = node;
        Node.OccupiedBlock = this;
    }

    // -----------------------------------------
    // MERGE CONTROL
    // -----------------------------------------
    public void MergeBlock(Block blockToMergeWith)
    {
        MergingBlock = blockToMergeWith;
        blockToMergeWith.Merging = true;
    }

    public bool CanMerge(int value) =>
        Value == value && !Merging && MergingBlock == null;

    // -----------------------------------------
    // ANIMATIONS
    // -----------------------------------------
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
    }

    public void PlayHighlight()
    {
        if (_mat == null) return;

        _mat.DOColor(Color.white, "_Color", 0.1f).OnComplete(() =>
        {
            // revert lại màu gốc
            _mat.DOColor(_baseColor, "_Color", 0.15f);
        });
    }

    // BlockManager sẽ gọi Destroy sau ~0.15s
    public void PlayDestroyFade()
    {
        if (_mat == null) return;
        _mat.DOFade(0f, 0.12f);
    }

    // -----------------------------------------
    // COLOR TABLE (chuẩn 2048 nhưng auto expand)
    // -----------------------------------------
    private Color GetColor(int value)
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
        }

        // nếu > 2048 → màu cao hơn (tự tính theo hàm)
        float t = Mathf.Clamp01(Mathf.Log(value / 2048f + 1f));
        return Color.Lerp(new Color32(237, 194, 46, 255), new Color32(60, 60, 60, 255), t);
    }
}
