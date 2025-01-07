using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 10f;
    public float minX, maxX, minY, maxY;
    public float zoomSpeed = 2f;
    public float smoothSpeed = 0.125f;

    private Vector3 dragOrigin;
    private Vector3 targetPosition;
    private float targetZoom;

    // Kameranýn kontrol edilip edilmediðini belirleyen bayrak
    public bool isCameraControlEnabled = true;

    void Start()
    {
        targetPosition = transform.position;
        targetZoom = Camera.main.orthographicSize;
    }

    void Update()
    {
        if (isCameraControlEnabled)
        {
            HandleMovement();
            HandleZoom();
        }
    }

    void HandleMovement()
    {
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 difference = dragOrigin - currentPos;
            targetPosition = transform.position + difference;

            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            targetZoom -= scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, targetZoom, smoothSpeed);
    }

    // Kamerayý geçici olarak devre dýþý býrakmak için metod
    public void DisableCameraControl()
    {
        isCameraControlEnabled = false;
    }

    // Kamerayý yeniden etkinleþtirmek için metod
    public void EnableCameraControl()
    {
        isCameraControlEnabled = true;
    }
}
