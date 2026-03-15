using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseMoveArea : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform viewport;
    [SerializeField] public RectTransform content;
    [SerializeField] private float mouseSens = 1f;
    
    private Vector2 lastMousePosition;
    private bool canDrag = false;
    private Vector2 minPosition;
    private Vector2 maxPosition;

    void Start()
    {
        CalculateBounds();
    }

    void Update()
    {
        UpdateLayoutGroup();
        CalculateBounds();
    }

    private void CalculateBounds()
    {
        if (viewport == null || content == null) return;

        // Получаем размеры
        Vector2 viewportSize = viewport.rect.size;
        Vector2 contentSize = content.rect.size;

        // Проверяем, нужно ли вообще двигать контент
        bool needHorizontalDrag = contentSize.x > viewportSize.x;
        bool needVerticalDrag = contentSize.y > viewportSize.y;
        canDrag = needHorizontalDrag || needVerticalDrag;

        // Вычисляем абсолютные границы для контента
        // Контент может двигаться только так, чтобы его края не заходили внутрь viewport
        
        // По X: от (viewportWidth - contentWidth) до 0
        float minX = (viewportSize.x - contentSize.x) / 2.0f; // Отрицательное число или 0
        float maxX = -minX;
        
        // По Y: от (viewportHeight - contentHeight) до 0
        float minY = viewportSize.y - contentSize.y; // Отрицательное число или 0
        float maxY = 0.0f;

        // Если контент меньше по ширине, центрируем и запрещаем движение по X
        if (!needHorizontalDrag)
        {
            minX = 0.0f;
            maxX = 0.0f;
        }

        // Если контент меньше по высоте, центрируем и запрещаем движение по Y
        if (!needVerticalDrag)
        {
            minY = (viewportSize.y - contentSize.y) / 2;
            maxY = minY;
        }

        minPosition = new Vector2(minX, minY);
        maxPosition = new Vector2(maxX, maxY);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canDrag) return;
        
        lastMousePosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag) return;

        // Конвертируем позицию мыши в локальные координаты родителя content
        Vector2 currentMousePosition = eventData.position;
        Vector2 delta = currentMousePosition - lastMousePosition;
        
        // Применяем чувствительность
        delta *= mouseSens;

        // Новая позиция контента
        Vector2 newPosition = content.anchoredPosition + delta;

        // Ограничиваем позицию в пределах допустимых границ
        newPosition.x = Mathf.Clamp(newPosition.x, minPosition.x, maxPosition.x);
        newPosition.y = Mathf.Clamp(newPosition.y, minPosition.y, maxPosition.y);

        // Применяем новую позицию
        content.anchoredPosition = newPosition;
        Debug.Log(minPosition);
        Debug.Log(maxPosition);

        lastMousePosition = currentMousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Можно добавить инерцию
    }
    private void UpdateLayoutGroup()
    {
        LayoutRebuilder.MarkLayoutForRebuild(content);
        LayoutGroup[] parentLayoutGroups = gameObject.GetComponentsInChildren<LayoutGroup>();

        foreach (LayoutGroup group in parentLayoutGroups) {
          LayoutRebuilder.MarkLayoutForRebuild((RectTransform)group.transform);
        }
    }
}