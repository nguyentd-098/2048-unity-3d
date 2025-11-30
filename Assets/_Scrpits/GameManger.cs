using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private int _width = 4;
    [SerializeField] private int _height = 4;
    [SerializeField] private Node _nodePrefab;
    [SerializeField] private Block _blockPrefab;
    [SerializeField] private SpriteRenderer _boardRenderer;
    [SerializeField] private float _travelTime = 0.2f;

    [Header("Score")]
    [SerializeField] private int Score;
    [SerializeField] private int BestScore;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;

    private Vector2 startPos;
    private float minDistance = 80f;

    [Header("UI Screens")]
    [SerializeField] private GameObject _winScreen, _loseScreen;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private bool _winShown = false;

    private List<Node> _nodes;
    private List<Block> _blocks;
    private GameState _state;
    private int _round;

    void Start()
    {
        BestScore = PlayerPrefs.GetInt("BestScore", 0);
        bestScoreText.text = BestScore.ToString();
        //// ---------- Vuốt ---------------
        FindFirstObjectByType<SwipeInput>().OnSwipe = dir =>
        {
            if (_state == GameState.WaitingInput)
                Shift(dir);
        };
        ChangeState(GameState.GenerateLevel);
    }

    private void ChangeState(GameState newstate)
    {
        _state = newstate;

        switch (newstate)
        {
            case GameState.GenerateLevel:
                GenerateGrid();
                break;

            case GameState.SpawningBlocks:
                SpawnBlocks(_round++ == 0 ? 2 : 1);
                break;

            case GameState.WaitingInput:
                break;

            case GameState.Moving:
                break;

            case GameState.Win:
                _winScreen.SetActive(true);
                break;

            case GameState.Lose:
                _loseScreen.SetActive(true);
                break;
        }
    }

    void Update()
    {
        if (_state != GameState.WaitingInput) return;

        HandleSwipe();
        //// ----------ấn nút---------------
        //if (Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);
        //if (Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);
        //if (Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);
        //if (Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);

    }

    void HandleSwipe()
    {
        Vector2 pos = Vector2.zero;
        bool released = false;

            // Touch
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.isPressed)
        {
            if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                startPos = Touchscreen.current.primaryTouch.position.ReadValue();

            pos = Touchscreen.current.primaryTouch.position.ReadValue();
            released = Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;
        }
        else
        {
            // Mouse
            if (Mouse.current.leftButton.wasPressedThisFrame)
                startPos = Mouse.current.position.ReadValue();

            pos = Mouse.current.position.ReadValue();
            released = Mouse.current.leftButton.wasReleasedThisFrame;
        }

        if (!released) return;

        Vector2 delta = pos - startPos;
        if (delta.magnitude < minDistance) return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 0) Shift(Vector2.right);
            else Shift(Vector2.left);
        }
        else
        {
            if (delta.y > 0) Shift(Vector2.up);
            else Shift(Vector2.down);
        }
    }

        // -------------------------
        // GRID
        // -------------------------
    void GenerateGrid()
    {
        _round = 0;
        _nodes = new List<Node>();
        _blocks = new List<Block>();

        for (int x = 0; x < _width; x++)
        for (int y = 0; y < _height; y++)
            _nodes.Add(Instantiate(_nodePrefab, new Vector2(x, y), Quaternion.identity));

        var center = new Vector2(
            (float)_width / 2 - 0.5f,
            (float)_height / 2 - 0.5f);

        var board = Instantiate(_boardRenderer, center, Quaternion.identity);
        board.size = new Vector2(_width, _height);

        Camera.main.transform.position = new Vector3(center.x, center.y, -10);
        ChangeState(GameState.SpawningBlocks);
    }

    // -------------------------
    // SPAWN BLOCK && Win/Lose
    // -------------------------
    void SpawnBlocks(int amount)
    {
        var freeNodes = _nodes
            .Where(n => n.OccupiedBlock == null)
            .OrderBy(n => UnityEngine.Random.value)
            .ToList();

        foreach (var node in freeNodes.Take(amount))
            SpawnBlock(node, UnityEngine.Random.value > 0.8f ? 4 : 2);

        if (freeNodes.Count == 1)
        {
            ChangeState(GameState.Lose);
            return;
        }

        if (!_winShown && _blocks.Any(b => b.Value == 2048))
        {
            _winShown = true;
            winPanel.SetActive(true);
            _state = GameState.WaitingInput;
            return;
        }

        ChangeState(GameState.WaitingInput);
    }

    void SpawnBlock(Node node, int value)
    {
        var block = Instantiate(_blockPrefab, node.Pos, Quaternion.identity);
        block.Init(value);
        block.SetBlock(node);
        _blocks.Add(block);
    }

    // -------------------------
    // MOVEMENT + MERGE FIXED
    // -------------------------
    void Shift(Vector2 dir)
    {
        ChangeState(GameState.Moving);

        IEnumerable<Block> ordered = _blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y);

        if (dir == Vector2.right || dir == Vector2.up)
            ordered = ordered.Reverse();

        foreach (var b in _blocks)
        {
            b.Merging = false;
            b.MergingBlock = null;
        }

        foreach (var block in ordered)
        {
            Node current = block.Node;
            Node next;

            while (true)
            {
                next = GetNodeAtPosition(current.Pos + dir);
                if (next == null) break;

                if (next.OccupiedBlock == null)
                {
                    current = next;
                    continue;
                }

                if (next.OccupiedBlock.CanMerge(block.Value))
                {
                    block.MergingBlock = next.OccupiedBlock;
                    next.OccupiedBlock.Merging = true;
                    current = next;
                }

                break;
            }

            block.Node.OccupiedBlock = null;
            block.SetBlock(current);
        }

        var seq = DOTween.Sequence();
        foreach (var block in ordered)
        {
            Vector2 target = block.MergingBlock != null
                ? block.MergingBlock.Node.Pos
                : block.Node.Pos;

            seq.Join(block.transform.DOMove(target, _travelTime));
        }

        seq.OnComplete(() =>
        {
            var merged = new List<Block>();

            foreach (var block in ordered)
            {
                if (block.MergingBlock != null && !merged.Contains(block.MergingBlock))
                {
                    HandleMerge(block, block.MergingBlock);
                    merged.Add(block);
                    merged.Add(block.MergingBlock);
                }
            }

            ChangeState(GameState.SpawningBlocks);
        });
    }

    // -------------------------
    // CLEAN MERGE LOGIC 
    // -------------------------
    private void HandleMerge(Block main, Block merging)
    {
        if (merging == null) return;

        Node node = main.Node;

        // Remove old blocks from Node
        if (main.Node.OccupiedBlock == main)
            main.Node.OccupiedBlock = null;
        if (merging.Node.OccupiedBlock == merging)
            merging.Node.OccupiedBlock = null;

        int newValue = main.Value * 2;

        // create merged block
        var newBlock = Instantiate(_blockPrefab, node.Pos, Quaternion.identity);
        newBlock.Init(newValue);
        newBlock.SetBlock(node);
        _blocks.Add(newBlock);

        // score
        AddScore(newValue);

        // animations
        newBlock.PlayMergeAnimation();
        newBlock.PlayHighlight();

        // fade old blocks
        main.PlayDestroyFade();
        merging.PlayDestroyFade();

        Destroy(main.gameObject, 0.12f);
        Destroy(merging.gameObject, 0.12f);

        _blocks.Remove(main);
        _blocks.Remove(merging);
    }

    // -------------------------
    // SCORE SYSTEM
    // -------------------------
    void AddScore(int value)
    {
        Score += value;
        scoreText.text = Score.ToString();

        if (Score > BestScore)
        {
            BestScore = Score;
            bestScoreText.text = Score.ToString();
            PlayerPrefs.SetInt("BestScore", Score);
        }
    }
    // -------------------------
    // UI Panel
    // -------------------------
    public void __________ContinueGame()
    {
        winPanel.SetActive(false);
        _state = GameState.WaitingInput;
        Debug.Log("Continue clicked.");
    }

    public void __________RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    Node GetNodeAtPosition(Vector2 pos)
        => _nodes.FirstOrDefault(n => n.Pos == pos);
}

public enum GameState
{
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}
