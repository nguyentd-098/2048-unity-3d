using UnityEngine;
using UnityEngine.InputSystem;
using System;

// ============================================
// INPUT HANDLER - Tuân thủ SRP
// Chỉ xử lý input
// ============================================

public class InputHandler : MonoBehaviour, IInputService
{
    [Header("Swipe Settings")]
    [SerializeField] private float _minSwipeDistance = 80f;

    private Vector2 _startPos;
    private bool _isEnabled = true;

    public event Action<Vector2> OnSwipe;

    void Update()
    {
        if (!_isEnabled) return;

        HandleInput();
    }

    /// <summary>
    /// Bật input
    /// </summary>
    public void Enable()
    {
        _isEnabled = true;
    }

    /// <summary>
    /// Tắt input
    /// </summary>
    public void Disable()
    {
        _isEnabled = false;
    }

    /// <summary>
    /// Xử lý input (touch hoặc mouse)
    /// </summary>
    private void HandleInput()
    {
        Vector2 currentPos = Vector2.zero;
        bool pressed = false;
        bool released = false;

        // Touch input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                _startPos = Touchscreen.current.primaryTouch.position.ReadValue();

            currentPos = Touchscreen.current.primaryTouch.position.ReadValue();
            released = Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;
        }
        // Mouse input (fallback)
        else if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                _startPos = Mouse.current.position.ReadValue();

            currentPos = Mouse.current.position.ReadValue();
            released = Mouse.current.leftButton.wasReleasedThisFrame;
        }

        if (!released) return;

        DetectSwipe(currentPos);
    }

    /// <summary>
    /// Phát hiện hướng swipe
    /// </summary>
    private void DetectSwipe(Vector2 endPos)
    {
        Vector2 delta = endPos - _startPos;

        if (delta.magnitude < _minSwipeDistance) return;

        Vector2 direction = GetSwipeDirection(delta);
        OnSwipe?.Invoke(direction);
    }

    /// <summary>
    /// Xác định hướng swipe
    /// </summary>
    private Vector2 GetSwipeDirection(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            return delta.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            return delta.y > 0 ? Vector2.up : Vector2.down;
        }
    }
}