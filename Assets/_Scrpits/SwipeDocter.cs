using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeInput : MonoBehaviour
{
    public System.Action<Vector2> OnSwipe;

    private Vector2 startPos;
    private bool swiping;

    private float minSwipeDistance = 20f;

    void Update()
    {
        var touch = Touchscreen.current;

        if (touch != null && touch.primaryTouch.press.isPressed)
        {
            Vector2 pos = touch.primaryTouch.position.ReadValue();

            if (!swiping)
            {
                swiping = true;
                startPos = pos;
            }
            else
            {
                Vector2 delta = pos - startPos;
                if (delta.magnitude > minSwipeDistance)
                {
                    DetectSwipe(delta);
                    swiping = false;
                }
            }
        }
        else
        {
            swiping = false;
        }
    }

    void DetectSwipe(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 0) OnSwipe?.Invoke(Vector2.right);
            else OnSwipe?.Invoke(Vector2.left);
        }
        else
        {
            if (delta.y > 0) OnSwipe?.Invoke(Vector2.up);
            else OnSwipe?.Invoke(Vector2.down);
        }
    }
}
