using UnityEngine;

public class cameraZoom : MonoBehaviour
{

    public float zoomSpeed = 10f; // Speed of zooming
    public float minZoom = 5f; // Minimum zoom distance
    public float maxZoom = 20f; // Maximum zoom distance

    Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel"); // Get scroll input
        cam.orthographicSize -= scroll * zoomSpeed; // Adjust the orthographic size based on scroll input
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom); // Clamp the orthographic size to the defined limits
    }
}
