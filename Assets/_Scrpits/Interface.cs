using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// Interface cho audio service
/// </summary>
public interface IAudioService
{
    bool IsMusicOn { get; }
    bool IsSfxOn { get; }
    void PlaySpawn();
    void PlayMerge();
    void PlayMove();
    void PlayWin();
    void PlayGameOver();
    void ToggleMusic();
    void ToggleSFX();
}

/// <summary>
/// Interface cho score service
/// </summary>
public interface IScoreService
{
    int CurrentScore { get; }
    int BestScore { get; }
    void AddScore(int value);
    void ResetScore();
    event Action<int> OnScoreChanged;
    event Action<int> OnBestScoreChanged;
}

/// <summary>
/// Interface cho input service
/// </summary>
public interface IInputService
{
    event Action<Vector2> OnSwipe;
    void Enable();
    void Disable();
}

/// <summary>
/// Interface cho UI service
/// </summary>
public interface IUIService
{
    void ShowWinScreen();
    void ShowLoseScreen();
    void HideWinScreen();
    void HideLoseScreen();
    void UpdateScore(int score);
    void UpdateBestScore(int bestScore);
}

/// <summary>
/// Interface cho các object có thể merge
/// </summary>
public interface IMergeable
{
    int Value { get; }
    bool CanMerge(int value);
    void OnMerge();
}

/// <summary>
/// Interface cho các object có thể di chuyển
/// </summary>
public interface IMovable
{
    Vector2 Position { get; }
    void MoveTo(Vector2 target, float duration, Action onComplete = null);
}

/// <summary>
/// Interface cho Block factory
/// </summary>
public interface IBlockFactory
{
    Block CreateBlock(Vector2 position, int value);
    void DestroyBlock(Block block);
}

/// <summary>
/// Interface cho grid manager
/// </summary>
public interface IGridManager
{
    Node GetNodeAt(Vector2 position);
    Node GetRandomEmptyNode();
    int EmptyNodeCount { get; }
}