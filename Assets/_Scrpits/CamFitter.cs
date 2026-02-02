// CameraFitter.cs
using UnityEngine;

public class CameraFitter : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 4;
    [SerializeField] private int gridHeight = 4;
    [SerializeField] private float padding = 1f; // Khoảng cách viền

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        FitCameraToGrid();
    }

    void FitCameraToGrid()
    {
        float screenAspect = (float)Screen.width / Screen.height;

        // Tính size cần thiết cho grid
        float gridAspect = (float)gridWidth / gridHeight;

        float requiredHeight = gridHeight + padding * 2;
        float requiredWidth = gridWidth + padding * 2;

        if (screenAspect > gridAspect)
        {
            // Màn hình rộng hơn → fit theo height
            cam.orthographicSize = requiredHeight / 2f;
        }
        else
        {
            // Màn hình dài hơn → fit theo width
            cam.orthographicSize = requiredWidth / screenAspect / 2f;
        }

        Debug.Log($"Camera size: {cam.orthographicSize}, Screen aspect: {screenAspect}");
    }
}