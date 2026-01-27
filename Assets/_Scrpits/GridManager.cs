using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// ============================================
// GRID MANAGER - Tuân thủ SRP
// Chỉ quản lý grid và nodes
// ============================================

public class GridManager : MonoBehaviour, IGridManager
{
    [Header("Grid Settings")]
    [SerializeField] private int _width = 4;
    [SerializeField] private int _height = 4;
    [SerializeField] private Node _nodePrefab;
    [SerializeField] private SpriteRenderer _boardRenderer;

    private List<Node> _nodes;
    private Dictionary<Vector2, Node> _nodeCache;

    public int Width => _width;
    public int Height => _height;
    public int EmptyNodeCount => _nodes.Count(n => n.OccupiedBlock == null);

    /// <summary>
    /// Khởi tạo grid
    /// </summary>
    public void Initialize()
    {
        _nodes = new List<Node>();
        _nodeCache = new Dictionary<Vector2, Node>();

        GenerateNodes();
        SetupBoard();
        PositionCamera();
    }

    /// <summary>
    /// Lấy node tại vị trí
    /// </summary>
    public Node GetNodeAt(Vector2 position)
    {
        return _nodeCache.TryGetValue(position, out var node) ? node : null;
    }

    /// <summary>
    /// Lấy node trống ngẫu nhiên
    /// </summary>
    public Node GetRandomEmptyNode()
    {
        var emptyNodes = _nodes.Where(n => n.OccupiedBlock == null).ToList();
        if (emptyNodes.Count == 0) return null;

        int randomIndex = Random.Range(0, emptyNodes.Count);
        return emptyNodes[randomIndex];
    }

    /// <summary>
    /// Lấy tất cả nodes trống
    /// </summary>
    public List<Node> GetEmptyNodes()
    {
        return _nodes.Where(n => n.OccupiedBlock == null).ToList();
    }

    /// <summary>
    /// Clear toàn bộ grid
    /// </summary>
    public void Clear()
    {
        foreach (var node in _nodes)
        {
            if (node != null)
                Destroy(node.gameObject);
        }

        _nodes.Clear();
        _nodeCache.Clear();
    }

    private void GenerateNodes()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Vector2 position = new Vector2(x, y);
                Node node = Instantiate(_nodePrefab, position, Quaternion.identity, transform);
                _nodes.Add(node);
                _nodeCache[position] = node;
            }
        }
    }

    private void SetupBoard()
    {
        if (_boardRenderer == null) return;

        Vector2 center = new Vector2(
            (float)_width / 2 - 0.5f,
            (float)_height / 2 - 0.5f
        );

        var board = Instantiate(_boardRenderer, center, Quaternion.identity, transform);
        board.size = new Vector2(_width, _height);
    }

    private void PositionCamera()
    {
        if (Camera.main == null) return;

        Vector2 center = new Vector2(
            (float)_width / 2 - 0.5f,
            (float)_height / 2 - 0.5f
        );

        Camera.main.transform.position = new Vector3(center.x, center.y, -10);
    }
}