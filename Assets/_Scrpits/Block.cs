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
    private Color _baseColor;

    public Vector2 Pos => transform.position;

    private AudioManager audioManager;

    [Header("VFX By Level")]
    public GameObject vfxSmall;   // 2 – 64
    public GameObject vfxMedium;  // 128 – 512
    public GameObject vfxLarge;   // 1024 – 4096
    public GameObject vfxUltra;   // 8192 trở lên


    // INIT
    public void Init(int value, AudioManager am)
    {
        Value = value;
        audioManager = am;
        if (_renderer != null)
        {
            _mat = new Material(_renderer.material);
            _renderer.material = _mat;
        }
        _baseColor = GetColor(Value);
        if (_renderer != null) _renderer.color = _baseColor;
        if (_text != null) _text.text = Value.ToString();
        PlaySpawnAnimation();
        audioManager?.PlaySpawn();
    }
    public void SetBlock(Node node)
    {
        if (Node != null)
            Node.OccupiedBlock = null;

        Node = node;
        Node.OccupiedBlock = this;
    }
    public void MergeBlock(Block blockToMergeWith)
    {
        MergingBlock = blockToMergeWith;
        blockToMergeWith.Merging = true;
        audioManager?.PlayMerge();
    }

    public bool CanMerge(int value) =>
        Value == value && !Merging && MergingBlock == null;

    // ANIMATIONS
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
            _mat.DOColor(_baseColor, "_Color", 0.15f);
        });
    }
    public void PlayDestroyFade()
    {
        if (_mat == null) return;
        _mat.DOFade(0f, 0.12f);
    }

    // VFX SYSTEM - PHÁT HIỆU ỨNG KHI MERGE
    public void PlayMergeVFX()
    {
        GameObject vfxPrefab = GetVFXByValue(Value);

        if (vfxPrefab != null)
        {
            GameObject vfx = Instantiate(vfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }
    private GameObject GetVFXByValue(int value)
    {
        if (value >= 8192)
            return vfxUltra;
        else if (value >= 1024)
            return vfxLarge;
        else if (value >= 128)
            return vfxMedium;
        else if (value >= 2)
            return vfxSmall;
        return null;
    }
    // COLOR SYSTEM
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

        int level = (int)Mathf.Log(value, 2);
        float t = Mathf.InverseLerp(12, 18, level);
        t = Mathf.Clamp01(t);

        Color c1 = new Color32(237, 194, 46, 255);
        Color c2 = new Color32(180, 80, 80, 255);
        Color c3 = new Color32(120, 60, 150, 255);
        Color c4 = new Color32(60, 120, 200, 255);

        if (t < 0.33f)
            return Color.Lerp(c1, c2, t / 0.33f);
        if (t < 0.66f)
            return Color.Lerp(c2, c3, (t - 0.33f) / 0.33f);
        return Color.Lerp(c3, c4, (t - 0.66f) / 0.34f);
    }
}