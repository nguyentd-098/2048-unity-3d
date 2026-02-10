using UnityEngine;
using System;

// ============================================
// TOUCH INPUT HANDLER - Chỉ cho Mobile/Simulator
// Dùng Input.touches - ĐƠN GIẢN NHẤT!
// ============================================

public class TouchInputHandler : MonoBehaviour, IInputService
{
    [Header("Swipe Settings")]
    [SerializeField] private float _minSwipeDistance = 50f;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLog = true;

    private Vector2 _startPos;
    private bool _isEnabled = true;

    public event Action<Vector2> OnSwipe;

    void Start()
    {
        Debug.Log("==============================================");
        Debug.Log("[TouchInput] READY - Swipe on screen to move!");
        Debug.Log("==============================================");
    }

    void Update()
    {
        if (!_isEnabled) return;

        // MOBILE: Touch input
        if (Input.touchCount > 0)
        {
            HandleTouch();
        }
        // EDITOR/SIMULATOR: Mouse as touch
        else
        {
            HandleMouseAsTouch();
        }
    }

    public void Enable()
    {
        _isEnabled = true;
        Debug.Log("[TouchInput] ✅ ENABLED - Ready for swipe");
    }

    public void Disable()
    {
        _isEnabled = false;
        Debug.Log("[TouchInput] ❌ DISABLED");
    }

    // ============================================
    // TOUCH INPUT (Real Device)
    // ============================================

    private void HandleTouch()
    {
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            _startPos = touch.position;

            if (_showDebugLog)
                Debug.Log($"[TouchInput] 👆 Touch START at {_startPos}");
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            Vector2 endPos = touch.position;

            if (_showDebugLog)
                Debug.Log($"[TouchInput] 👆 Touch END at {endPos}");

            ProcessSwipe(_startPos, endPos);
        }
    }

    // ============================================
    // MOUSE AS TOUCH (Editor/Simulator)
    // ============================================

    private void HandleMouseAsTouch()
    {
        // Mouse down = Touch began
        if (Input.GetMouseButtonDown(0))
        {
            _startPos = Input.mousePosition;

            if (_showDebugLog)
                Debug.Log($"[TouchInput] 🖱️ Mouse DOWN at {_startPos}");
        }

        // Mouse up = Touch ended
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 endPos = Input.mousePosition;

            if (_showDebugLog)
                Debug.Log($"[TouchInput] 🖱️ Mouse UP at {endPos}");

            ProcessSwipe(_startPos, endPos);
        }
    }

    // ============================================
    // SWIPE PROCESSING
    // ============================================

    private void ProcessSwipe(Vector2 start, Vector2 end)
    {
        Vector2 delta = end - start;
        float distance = delta.magnitude;

        Debug.Log($"[TouchInput] 📏 Swipe distance: {distance:F1} pixels (min: {_minSwipeDistance})");

        // Check minimum distance
        if (distance < _minSwipeDistance)
        {
            Debug.Log($"[TouchInput] ⚠️ Swipe TOO SHORT! Need at least {_minSwipeDistance} pixels");
            return;
        }

        // Determine direction
        Vector2 direction;
        string directionName;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // Horizontal swipe
            if (delta.x > 0)
            {
                direction = Vector2.right;
                directionName = "RIGHT →";
            }
            else
            {
                direction = Vector2.left;
                directionName = "LEFT ←";
            }
        }
        else
        {
            // Vertical swipe
            if (delta.y > 0)
            {
                direction = Vector2.up;
                directionName = "UP ↑";
            }
            else
            {
                direction = Vector2.down;
                directionName = "DOWN ↓";
            }
        }

        Debug.Log($"[TouchInput] ✅ SWIPE {directionName} detected!");

        // Trigger event
        if (OnSwipe == null)
        {
            Debug.LogError("[TouchInput] ❌ ERROR: OnSwipe event has NO listeners!");
            Debug.LogError("Did you forget to subscribe in GameManager?");
            return;
        }

        Debug.Log($"[TouchInput] 🎯 Invoking OnSwipe({direction})...");
        OnSwipe.Invoke(direction);
        Debug.Log($"[TouchInput] ✅ Event invoked successfully!");
    }
}