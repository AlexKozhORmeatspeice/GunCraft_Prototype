using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float moveSpeed = 10f;
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;
    [SerializeField] private float startZoom = 10f;
    
    [Header("Bounds Settings")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds = new Vector2(-10f, -10f);
    [SerializeField] private Vector2 maxBounds = new Vector2(10f, 10f);
    
    [Header("Cinemachine Settings")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    
    private float targetZoom;
    private Vector3 targetPosition;
    
    void Awake()
    {
        // Находим виртуальную камеру если не назначена
        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }
        
        if (virtualCamera != null)
        {
            // Устанавливаем начальный зум
            targetZoom = startZoom;
            SetCameraZoom(startZoom);
            
            // Устанавливаем начальную позицию с учетом bounds
            targetPosition = transform.position;
            if (useBounds)
            {
                targetPosition = ApplyBoundsToPosition(targetPosition);
                transform.position = targetPosition;
            }
        }
    }
    
    void Update()
    {
        HandleMovementInput();
        HandleZoomInput();
        
        // Применяем зум
        if (virtualCamera != null)
        {
            float currentZoom = virtualCamera.m_Lens.OrthographicSize;
            float newZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomSpeed);
            SetCameraZoom(newZoom);
        }
        
        // Применяем позицию с учетом bounds
        if (useBounds)
        {
            Vector3 boundedPosition = ApplyBoundsToPosition(targetPosition);
            boundedPosition.z = transform.position.z;
            transform.position = boundedPosition;
            
            // Обновляем целевую позицию, чтобы не было рывков
            if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                targetPosition = transform.position;
            }
        }
        else
        {
            transform.position = targetPosition;
        }
    }
    
    private void HandleMovementInput()
    {
        float horizontal = 0f;
        float vertical = 0f;
        
        // WASD управление
        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
        
        // Дополнительно: стрелки
        if (Input.GetKey(KeyCode.UpArrow)) vertical += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
        
        if (horizontal != 0f || vertical != 0f)
        {
            Vector3 move = new Vector3(horizontal, vertical, 0f) * moveSpeed * Time.deltaTime;
            targetPosition += move;
        }
    }
    
    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (scroll != 0f)
        {
            targetZoom -= scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
    }
    
    private void SetCameraZoom(float zoom)
    {
        if (virtualCamera != null)
        {
            virtualCamera.m_Lens.OrthographicSize = zoom;
        }
    }
    
    // Применяет ограничения к позиции с учетом текущего размера камеры
    private Vector3 ApplyBoundsToPosition(Vector3 position)
    {
        if (!useBounds) return position;
        
        // Получаем текущий размер камеры
        float currentZoom = virtualCamera != null ? virtualCamera.m_Lens.OrthographicSize : startZoom;
        
        // Вычисляем границы с учетом размера камеры
        // В ортографической камере высота = OrthographicSize * 2, ширина = высота * (ширина экрана / высота экрана)
        float cameraHeight = currentZoom * 2f;
        float cameraWidth = cameraHeight * Camera.main.aspect;
        
        // Вычисляем допустимые границы для позиции камеры
        float minX = minBounds.x + (cameraWidth / 2f);
        float maxX = maxBounds.x - (cameraWidth / 2f);
        float minY = minBounds.y + (cameraHeight / 2f);
        float maxY = maxBounds.y - (cameraHeight / 2f);
        
        // Если границы меньше размера камеры, центрируем камеру посередине
        float clampedX = position.x;
        float clampedY = position.y;
        
        if (minX <= maxX)
        {
            clampedX = Mathf.Clamp(position.x, minX, maxX);
        }
        else
        {
            clampedX = (minBounds.x + maxBounds.x) / 2f;
        }
        
        if (minY <= maxY)
        {
            clampedY = Mathf.Clamp(position.y, minY, maxY);
        }
        else
        {
            clampedY = (minBounds.y + maxBounds.y) / 2f;
        }
        
        return new Vector3(clampedX, clampedY, position.z);
    }
    
    // Публичный метод для телепортации камеры
    public void TeleportToPosition(Vector3 position)
    {
        targetPosition = position;
        
        if (useBounds)
        {
            targetPosition = ApplyBoundsToPosition(targetPosition);
        }
        
        transform.position = targetPosition;
        
        if (virtualCamera != null)
        {
            virtualCamera.OnTargetObjectWarped(transform, Vector3.zero);
        }
    }
    
    // Публичные методы для обновления границ во время выполнения
    public void SetBounds(Vector2 newMinBounds, Vector2 newMaxBounds)
    {
        minBounds = newMinBounds;
        maxBounds = newMaxBounds;
        
        // Переприменяем границы к текущей позиции
        if (useBounds)
        {
            targetPosition = ApplyBoundsToPosition(transform.position);
            transform.position = targetPosition;
        }
    }
    
    public void SetUseBounds(bool use)
    {
        useBounds = use;
        
        if (!useBounds && virtualCamera != null)
        {
            // Если отключаем границы, синхронизируем позицию
            targetPosition = transform.position;
        }
        else if (useBounds)
        {
            // Если включаем границы, применяем их к текущей позиции
            targetPosition = ApplyBoundsToPosition(transform.position);
            transform.position = targetPosition;
        }
    }
}