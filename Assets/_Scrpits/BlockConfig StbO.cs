using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// ============================================
// SCRIPTABLE OBJECT - Tuân thủ OCP
// Thêm block mới không cần sửa code
// ============================================

[CreateAssetMenu(fileName = "BlockConfig", menuName = "Game/Block Configuration")]
public class BlockConfig : ScriptableObject
{
    [System.Serializable]
    public class BlockTheme
    {
        public int value;
        public Color color;
        public GameObject vfxPrefab;

        [Header("Optional Custom Settings")]
        public float spawnAnimDuration = 0.25f;
        public float mergeAnimDuration = 0.24f;
    }

    [Header("Block Themes")]
    public List<BlockTheme> themes = new List<BlockTheme>();

    [Header("Default VFX Tiers")]
    public GameObject defaultVfxSmall;   // 2-64
    public GameObject defaultVfxMedium;  // 128-512
    public GameObject defaultVfxLarge;   // 1024-2048
    public GameObject defaultVfxUltra;   // 4096+

    private Dictionary<int, BlockTheme> _themeCache;

    void OnEnable()
    {
        BuildCache();
    }

    private void BuildCache()
    {
        _themeCache = themes.ToDictionary(t => t.value);
    }

    /// <summary>
    /// Lấy color cho giá trị block
    /// </summary>
    public Color GetColorForValue(int value)
    {
        if (_themeCache == null) BuildCache();

        if (_themeCache.TryGetValue(value, out var theme))
            return theme.color;

        // Fallback: gradient động cho giá trị lớn
        return GenerateDynamicColor(value);
    }

    /// <summary>
    /// Lấy VFX prefab cho giá trị block
    /// </summary>
    public GameObject GetVFXForValue(int value)
    {
        if (_themeCache == null) BuildCache();

        // Ưu tiên custom VFX
        if (_themeCache.TryGetValue(value, out var theme) && theme.vfxPrefab != null)
            return theme.vfxPrefab;

        // Fallback: default VFX tiers
        if (value >= 4096) return defaultVfxUltra;
        if (value >= 1024) return defaultVfxLarge;
        if (value >= 128) return defaultVfxMedium;
        if (value >= 2) return defaultVfxSmall;

        return null;
    }

    /// <summary>
    /// Tạo màu động cho giá trị cao
    /// </summary>
    private Color GenerateDynamicColor(int value)
    {
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